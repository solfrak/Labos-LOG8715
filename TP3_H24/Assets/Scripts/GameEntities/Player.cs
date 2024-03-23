using System;
using System.Collections;
using System.Collections.Generic;
using DataStruct;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
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
    
    private NetworkVariable<Vector2> m_Position = new NetworkVariable<Vector2>();
    private NetworkVariable<PositionPayload> m_Position2 = new NetworkVariable<PositionPayload>();

    public Vector2 Position => m_Position2.Value.pos;

    private Queue<Vector2> m_InputQueue = new Queue<Vector2>();

    private void Awake()
    {
        m_InputBuffer = new CircleBuffer<Vector2>(2 * (int)NetworkUtility.GetLocalTickRate());
        m_PositionBuffer = new CircleBuffer<Vector2>(2 * (int)NetworkUtility.GetLocalTickRate());
        m_Position2.Value = new PositionPayload();
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
            UpdateInputClient();
            UpdatePositionClient();
            CheckServerPosition();
        }
    }

    private void CheckServerPosition()
    {
        var position = m_Position2.Value.pos;
        var tick = m_Position2.Value.tick;
        
        var cachedPosition = m_PositionBuffer.Get(tick);
        
        if (!PositionEqualWithTolerance(position, cachedPosition))
        {
            //TODO make some position correction 
            Debug.LogWarning("Position are not the same at tick: " + tick );
            Debug.LogWarning(position + " " + cachedPosition);
            transform.position = position;
        }
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

    private void UpdatePositionClient()
    {
        var input = m_InputBuffer.Get(NetworkUtility.GetLocalTick());
        if (input != null)
        {
            transform.position = new Vector3(transform.position.x + input.x * m_Velocity * Time.deltaTime,
                transform.position.y + input.y * m_Velocity * Time.deltaTime, 0);

            // Gestion des collisions avec l'exterieur de la zone de simulation
            var size = GameState.GameSize;
            if (this.transform.position.x - m_Size < -size.x)
            {
                transform.position= new Vector2(-size.x + m_Size, transform.position.y);
            }
            else if (transform.position.x + m_Size > size.x)
            {
                transform.position = new Vector2(size.x - m_Size, transform.position.y);
            }

            if (transform.position.y + m_Size > size.y)
            {
                transform.position = new Vector2(transform.position.x, size.y - m_Size);
            }
            else if (transform.position.y - m_Size < -size.y)
            {
                transform.position= new Vector2(transform.position.x, -size.y + m_Size);
            }
        } 
        
        m_PositionBuffer.Put(transform.position, NetworkUtility.GetLocalTick());
    }

    private void UpdatePositionServer()
    {
        // Mise a jour de la position selon dernier input reçu, puis consommation de l'input
        if (m_InputQueue.Count > 0)
        {
            var input = m_InputQueue.Dequeue();
            var pos = m_Position2.Value.pos;
            pos += input * (m_Velocity * Time.deltaTime);

            // Gestion des collisions avec l'exterieur de la zone de simulation
            var size = GameState.GameSize;
            if (pos.x - m_Size < -size.x)
            {
                pos = new Vector2(-size.x + m_Size, pos.y);
            }
            else if (pos.x + m_Size > size.x)
            {
                pos = new Vector2(size.x - m_Size, pos.y);
            }

            if (pos.y + m_Size > size.y)
            {
                pos = new Vector2(pos.x, size.y - m_Size);
            }
            else if (pos.y - m_Size < -size.y)
            {
                pos = new Vector2(pos.x, -size.y + m_Size);
            }
            m_Position2.Value = new PositionPayload { pos = pos, tick = NetworkUtility.GetLocalTick() };
        }
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
        
        m_InputBuffer.Put(inputDirection, NetworkUtility.GetLocalTick());
        SendInputServerRpc(inputDirection.normalized);
    }


    [ServerRpc]
    private void SendInputServerRpc(Vector2 input)
    {
        // On utilise une file pour les inputs pour les cas ou on en recoit plusieurs en meme temps.
        m_InputQueue.Enqueue(input);
    }



}
