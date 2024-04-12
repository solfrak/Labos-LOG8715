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
            return m_ClientPosition.Value.Vector;
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

    private CircleBuffer<VectorTick> m_InputBuffer;
    private CircleBuffer<VectorTick> m_PositionBuffer;

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
        m_PositionBuffer = new CircleBuffer<VectorTick>(bufferSize);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_ServerPosition = new NetworkVariable<Vector2Payload>();
        m_ServerPosition.OnValueChanged += (previous, current) =>
        {
            CheckServerPosition(current);
        };

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
            int tick = NetworkUtility.GetLocalTick();
            UpdateInputClient();
            UpdatePositionClient(tick);
        }

        if (IsOwner)
        {
            m_ClientPosition.Value = new Vector2Payload { Vector = m_Position, Tick = NetworkUtility.GetLocalTick() };
        }
    }

    private void CheckServerPosition(Vector2Payload positionPayload)
    {
        var serverPosition = positionPayload.Vector;
        var serverTick = positionPayload.Tick;
        var positionState = m_PositionBuffer.Get(serverTick);

        if(!PositionEqualWithTolerance(serverPosition, positionState.Vector, 0.5f) && positionState.Tick == serverTick)
        {
            Debug.LogWarning($"PlayerPositionError serverTick: {serverTick} selfTick: {NetworkUtility.GetLocalTick()} biggestTick: {m_BiggestClientTick} serverPos: {serverPosition} selfPos: {positionState.Vector}");

            // Set the serverPosition to the server's serverPosition
            m_Position = serverPosition;

            int endTick = NetworkUtility.GetLocalTick();
            while(serverTick < endTick)
            {
                UpdatePositionClient(serverTick++);
            }
        }
    }

    private void UpdatePositionClient(int tick)
    {
        if(!IsClient)
        {
            return;
        }

        var inputState = m_InputBuffer.Get(tick);
        if(inputState != null && inputState.Tick == tick)
        {
            Vector2 movedPosition = ApplyMove(m_Position, inputState.Vector, Time.deltaTime);
            m_Position = movedPosition;
        }

        var positionState = new VectorTick() { Tick = tick, Vector = m_Position };
        m_PositionBuffer.Put(positionState, tick);
    }

    private void UpdatePositionServer()
    {
        if(!IsServer)
        {
            return;
        }

        // Mise a jour de la serverPosition selon dernier inputState reçu, puis consommation de l'inputState
        Vector2 pos = m_ServerPosition.Value.Vector;
        int tick = 0;
        bool changed = false;
        while(m_InputQueue.Count > 0)
        {
            changed = true;
            var inputPayload = m_InputQueue.Dequeue();
            if(!GameState.IsStunned)
            {
                pos = ApplyMove(pos, inputPayload.Vector, Time.deltaTime);
            }
            tick = inputPayload.Tick;

            var positionState = new VectorTick() { Tick = tick, Vector = pos };
            m_PositionBuffer.Put(positionState, tick);
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

        var inputState = new VectorTick() { Tick = tick, Vector = inputDirection };
        m_InputBuffer.Put(inputState, tick);
        Vector2Payload inputPayload = new Vector2Payload() { Vector = inputDirection.normalized, Tick = tick };
        SendInputServerRpc(inputPayload);
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