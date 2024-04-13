using System;
using System.Collections;
using System.Collections.Generic;
using DataStruct;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public class VectorTick
    { 
        public Vector2 Vector;
        public int Tick;
    }

    [SerializeField]
    private float m_Velocity;

    [SerializeField]
    private float m_Size = 1;

    private GameState m_GameState;
    public Vector2 Position
    {
        get
        {
            if(IsClient && IsOwner)
            {
                return m_Position;
            }
            return m_ServerPosition.Value.Vector;
        }
    }

    // GameState peut etre nul si l'entite joueur est instanciee avant de charger MainScene
    private GameState GameState
    {
        get
        {
            if(m_GameState == null)
            {
                m_GameState = FindObjectOfType<GameState>();
            }
            return m_GameState;
        }
    }

    // The input buffer can contain inputs which are added up from multiple different update frames
    // So they are not always normalized
    private CircleBuffer<VectorTick> m_InputBuffer;
    private CircleBuffer<List<VectorTick>> m_PositionBuffer;

    private NetworkVariable<Vector2Payload> m_ServerPosition = new NetworkVariable<Vector2Payload>();
    private Vector2 m_Position;

    private NetworkVariable<Vector2Payload> m_ClientPosition = new NetworkVariable<Vector2Payload>(writePerm: NetworkVariableWritePermission.Owner);

    private int m_BiggestClientTick = 0;

    private Queue<Vector2Payload> m_InputQueue;

    private void Awake()
    {
        m_InputQueue = new Queue<Vector2Payload>();

        int bufferSize = 8 * (int) NetworkUtility.GetLocalTickRate();
        m_InputBuffer = new CircleBuffer<VectorTick>(bufferSize);
        m_PositionBuffer = new CircleBuffer<List<VectorTick>>(bufferSize);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsServer)
        {
            m_ServerPosition.Value = new Vector2Payload { Vector = Vector2.zero, Tick = 0 };
        }
        if(IsClient)
        {
            m_ServerPosition.OnValueChanged += (previous, current) =>
            {
                CheckServerPosition(current);
            };
        }

        m_BiggestClientTick = NetworkUtility.GetLocalTick();
    }

    private void FixedUpdate()
    {
        if(m_BiggestClientTick <= NetworkUtility.GetLocalTick())
        {
            m_BiggestClientTick = NetworkUtility.GetLocalTick();
        }

        // Si le stun est active, rien n'est mis a jour.
        if(GameState == null || GameState.IsStunned)
        {
            return;
        }


        // Seul le serveur met à jour la serverPosition de l'entite.
        if(IsServer)
        {
            UpdatePositionServer();
        }

        // Seul le client qui possede cette entite peut envoyer ses inputs. 
        if(IsClient && IsOwner)
        {
            int tick = m_BiggestClientTick;
            UpdateInputClient();
            UpdatePositionClient(tick, GetInputDirection());
        }

        // Remove this code block after testing
        if (IsOwner)
        {
            m_ClientPosition.Value = new Vector2Payload { Vector = m_Position, Tick = NetworkUtility.GetLocalTick() };
        }
    }

    private void CheckServerPosition(Vector2Payload positionPayload)
    {
        if(!IsClient)
        {
            return;
        }
        var serverPosition = positionPayload.Vector;
        var serverTick = positionPayload.Tick;
        var positionStates = m_PositionBuffer.Get(serverTick);

        if (positionStates == null || positionStates.Count == 0 || positionStates[0].Tick != serverTick)
        {
            return;
        }

        bool isClientInSyncWithServer = false;
        foreach(var positionState in positionStates)
        {
            if (PositionEqualWithTolerance(serverPosition, positionState.Vector, 0.5f))
            {
                isClientInSyncWithServer = true;
                break;
            }
        }

        if(!isClientInSyncWithServer)
        {
            ReconciliateClient(serverPosition, serverTick);
        }
    }

    private void ReconciliateClient(Vector2 serverPosition, int serverTick)
    {
        Debug.LogWarning($"PlayerPositionError serverTick: {serverTick} selfTick: {NetworkUtility.GetLocalTick()} biggestTick: {m_BiggestClientTick} serverPos: {serverPosition} selfPos: {PositionOfTick(serverTick)}");

        // Set the serverPosition to the server's serverPosition
        m_Position = serverPosition;

        int endTick = NetworkUtility.GetLocalTick();
        serverTick++;
        while(serverTick < endTick)
        {
            VectorTick inputState = m_InputBuffer.Get(serverTick);
            if(inputState != null && inputState.Tick == serverTick)
            {
                m_Position = ApplyMove(m_Position, inputState.Vector, Time.deltaTime);
                UpdatePositionBuffer(new VectorTick() { Vector = m_Position, Tick = serverTick });
            }
            serverTick++;
        }
    }

    private void UpdatePositionClient(int tick, Vector2 inputDirection)
    {
        if(!IsClient)
        {
            return;
        }

        Vector2 movedPosition = ApplyMove(m_Position, inputDirection, Time.deltaTime);
        m_Position = movedPosition;

        var positionState = new VectorTick() { Tick = tick, Vector = m_Position };
        UpdatePositionBuffer(positionState);
    }

    private void UpdatePositionServer()
    {
        if(!IsServer)
        {
            return;
        }

        // Mise a jour de la serverPosition selon dernier inputState reçu, puis consommation de l'inputState
        Vector2 pos = m_ServerPosition.Value.Vector;
        int tick = m_ServerPosition.Value.Tick;
        bool changed = false;

        while(m_InputQueue.Count > 0)
        {
            changed = true;
            var inputPayload = m_InputQueue.Dequeue();
            pos = ApplyMove(pos, inputPayload.Vector, Time.deltaTime);
            tick = inputPayload.Tick;

            var positionState = new VectorTick() { Tick = tick, Vector = pos };
            UpdatePositionBuffer(positionState);
        }

        if(changed)
        {
            m_ServerPosition.Value = new Vector2Payload { Vector = pos, Tick = tick };
        }
    }

    private void UpdateInputClient()
    {
        if(!IsClient || !IsOwner)
        {
            return;
        }

        Vector2 inputDirection = GetInputDirection();
        int tick = m_BiggestClientTick;

        VectorTick inputState = m_InputBuffer.Get(tick);
        if(inputState != null && inputState.Tick == tick)
        {
            inputState.Vector += inputDirection;
        }
        else
        {
            inputState = new VectorTick() { Tick = tick, Vector = inputDirection };
        }

        m_InputBuffer.Put(inputState, tick);
        Vector2Payload inputPayload = new Vector2Payload() { Vector = inputDirection.normalized, Tick = tick };
        SendInputServerRpc(inputPayload);
    }


    private void UpdatePositionBuffer(VectorTick positionState)
    {
        if(positionState == null)
            return;

        var positionList = m_PositionBuffer.Get(positionState.Tick);
        if(positionList == null || positionList.Count == 0 || positionList[0].Tick != positionState.Tick)
        {
            positionList = new List<VectorTick>();
            m_PositionBuffer.Put(positionList, positionState.Tick);
        }

        positionList.Add(positionState);
    }

    private Vector2 PositionOfTick(int tick)
    {
        var posBuffer = m_PositionBuffer.Get(tick);
        if(posBuffer == null || posBuffer.Count == 0 || posBuffer[0].Tick != tick)
            return Vector2.zero;
        return posBuffer[posBuffer.Count - 1].Vector;
    }

    private Vector2 GetInputDirection()
    {
        Vector2 inputDirection = new Vector2(0, 0);
        if(Input.GetKey(KeyCode.W))
        {
            inputDirection += Vector2.up;
        }
        if(Input.GetKey(KeyCode.A))
        {
            inputDirection += Vector2.left;
        }
        if(Input.GetKey(KeyCode.S))
        {
            inputDirection += Vector2.down;
        }
        if(Input.GetKey(KeyCode.D))
        {
            inputDirection += Vector2.right;
        }

        return inputDirection.normalized;
    }

    private Vector2 ApplyMove(Vector2 pos, Vector2 input, float deltaTime)
    {
        pos += input * (m_Velocity * deltaTime);

        // Gestion des collisions avec l'exterieur de la zone de simulation
        var size = GameState.GameSize;
        if(pos.x - m_Size < -size.x)
        {
            pos = new Vector2(-size.x + m_Size, pos.y);
        }
        else if(pos.x + m_Size > size.x)
        {
            pos = new Vector2(size.x - m_Size, pos.y);
        }

        if(pos.y + m_Size > size.y)
        {
            pos = new Vector2(pos.x, size.y - m_Size);
        }
        else if(pos.y - m_Size < -size.y)
        {
            pos = new Vector2(pos.x, -size.y + m_Size);
        }

        return pos;
    }

    private bool PositionEqualWithTolerance(Vector2 pos1, Vector2 pos2, float tolerance)
    {
        return Math.Abs(pos1.x - pos2.x) + Math.Abs(pos1.y - pos2.y) < tolerance;
    }


    [ServerRpc]
    private void SendInputServerRpc(Vector2Payload input)
    {
        // On utilise une file pour les inputs pour les cas ou on en recoit plusieurs en meme temps.
        m_InputQueue.Enqueue(input);
    }

}