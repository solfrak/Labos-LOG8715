using System;
using System.Collections;
using System.Collections.Generic;
using DataStruct;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private class PositionPayload : INetworkSerializable
    {
        public Vector2 pos;
        public int tick;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref pos);
            serializer.SerializeValue(ref tick);
        }
    }

    private class InputPayload : INetworkSerializable
    {
        public Vector2 input;
        public int tick;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref input);
            serializer.SerializeValue(ref tick);
        }
    }

    [SerializeField]
    private float m_Velocity;

    [SerializeField]
    private float m_Size = 1;

    private GameState m_GameState;

    // GameState peut etre nul si l'entite joueur est instanciee avant de charger MainScene
    private GameState GameState
    {
        get
        {
            if (m_GameState == null)
            {
                m_GameState = FindObjectOfType<GameState>();
            }
            return m_GameState;
        }
    }

    private CircleBuffer<Vector2> m_InputBuffer;
    private CircleBuffer<Vector2> m_PositionBuffer;

    private NetworkVariable<Vector2> m_Position = new NetworkVariable<Vector2>();
    private NetworkVariable<PositionPayload> m_Position2 = new NetworkVariable<PositionPayload>();
    private bool m_Position2Verified = true;

    public Vector2 Position => m_Position2.Value.pos;

    private Queue<InputPayload> m_InputQueue = new Queue<InputPayload>();

    private void Awake()
    {
        m_InputBuffer = new CircleBuffer<Vector2>(8 * (int)NetworkUtility.GetLocalTickRate());
        m_PositionBuffer = new CircleBuffer<Vector2>(8 * (int)NetworkUtility.GetLocalTickRate());
        m_Position2.Value = new PositionPayload();

        m_Position2.OnValueChanged += (previous, current) =>
        {
            print(m_Position2.Value.tick + ": " + m_Position2.Value.pos + " | " + m_PositionBuffer.Get(m_Position2.Value.tick));
            m_Position2Verified = false;
        };
        m_Position2Verified = true;
    }

    private void FixedUpdate()
    {
        // Si le stun est active, rien n'est mis a jour.
        if (GameState == null || GameState.IsStunned)
        {
            return;
        }

        // Seul le serveur met à jour la position de l'entite.
        if (IsServer)
        {
            UpdatePositionServer();
        }

        // Seul le client qui possede cette entite peut envoyer ses inputs. 
        if (IsClient && IsOwner)
        {
            int tick = NetworkUtility.GetLocalTick();
            CheckServerPosition();
            UpdateInputClient();
            UpdatePositionClient(tick);
        }
    }

    private void CheckServerPosition()
    {
        if(m_Position2Verified)
        {
            return;
        }

        var position = m_Position2.Value.pos;
        var tick = m_Position2.Value.tick;

        var cachedPosition = m_PositionBuffer.Get(tick);
        
        if (!PositionEqualWithTolerance(position, cachedPosition))
        {
            Debug.LogWarning("Position are not the same at tick: " + tick );
            Debug.LogWarning(position + " " + cachedPosition);

            // Set the position to the server's position
            transform.position = position;

            while(tick < NetworkUtility.GetLocalTick())
            {
                UpdatePositionClient(tick++);
            }
        }

        m_Position2Verified = true;
    }

    private bool PositionEqualWithTolerance(Vector2 pos1, Vector2 pos2)
    {
        //TODO make a better implementation
        if (Math.Abs(pos1.x - pos2.x) < 0.5f && Math.Abs(pos1.y - pos2.y) < 0.5f)
        {
            return true;
        }

        return false;
    }

    private void UpdatePositionClient(int tick)
    {
        if(!IsClient)
        {
            return;
        }

        var input = m_InputBuffer.Get(tick);
        if (input != null)
        {
            Vector2 movedPosition = ApplyMove(transform.position, input);
            transform.position = movedPosition;
        } 
        
        m_PositionBuffer.Put(transform.position, tick);
    }

    private void UpdatePositionServer()
    {
        if(!IsServer)
        {
            return;
        }
        // Mise a jour de la position selon dernier input reçu, puis consommation de l'input
        var pos = m_Position2.Value.pos;
        bool changed = false;
        while (m_InputQueue.Count > 0)
        {
            changed = true;
            var inputPayload = m_InputQueue.Dequeue();
            pos = ApplyMove(pos, inputPayload.input);
            m_PositionBuffer.Put(pos, inputPayload.tick);
        }

        if(changed)
        {
            m_Position2.Value = new PositionPayload { pos = pos, tick = NetworkUtility.GetLocalTick() };
        }
    }

    private Vector2 ApplyMove(Vector2 pos, Vector2 input)
    {
        pos += input * (m_Velocity * Time.deltaTime);

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

    private void UpdateInputClient()
    {
        Vector2 inputDirection = new Vector2(0, 0);
        if (Input.GetKey(KeyCode.W))
        {
            inputDirection += Vector2.up;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputDirection += Vector2.left;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputDirection += Vector2.down;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputDirection += Vector2.right;
        }

        inputDirection = inputDirection.normalized;
        m_InputBuffer.Put(inputDirection, NetworkUtility.GetLocalTick());
        InputPayload inputPayload = new InputPayload() { input = inputDirection.normalized, tick = NetworkUtility.GetLocalTick() };
        SendInputServerRpc(inputPayload);
    }


    [ServerRpc]
    private void SendInputServerRpc(InputPayload input)
    {
        // On utilise une file pour les inputs pour les cas ou on en recoit plusieurs en meme temps.
        m_InputQueue.Enqueue(input);
    }

}
