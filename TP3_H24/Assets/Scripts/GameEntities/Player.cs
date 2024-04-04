using System;
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

    private int previousTick = 0;

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

    

   
    private NetworkVariable<Vector2Payload> m_Position = new NetworkVariable<Vector2Payload>();

    public Vector2 Position => m_Position.Value.Vector;

    private Queue<(Vector2, int)> m_InputQueue = new Queue<(Vector2, int)>();
    

    private void Awake()
    {
        m_InputBuffer = new CircleBuffer<Vector2>(10 * (int)NetworkUtility.GetLocalTickRate());
        m_PositionBuffer = new CircleBuffer<Vector2>(10 * (int)NetworkUtility.GetLocalTickRate());
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            m_Position.Value = new Vector2Payload();
        }
        base.OnNetworkSpawn();
    }

    private void FixedUpdate()
    {
        int currentTick = NetworkUtility.GetLocalTick();
        if (previousTick < currentTick) 
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
                CheckServerPosition(currentTick);
                UpdateInputClient();
                UpdatePositionClient(currentTick);
            }

            previousTick = currentTick;
        }
    }

    private void CheckServerPosition(int currentTick)
    {
        var position = m_Position.Value.Vector;
        var tick = m_Position.Value.Tick;
        
        var cachedPosition = m_PositionBuffer.Get(tick);
        
        if (!PositionEqualWithTolerance(position, cachedPosition))
        {
            
            Debug.LogWarning(position +" " + cachedPosition + " tick: " + tick);
            //TODO maybe we'll need to change this
            transform.position = position;
            while (tick <= currentTick)
            {
                UpdatePositionClient(tick);
                tick++;
            }
        }
    }

    private bool PositionEqualWithTolerance(Vector2 pos1, Vector2 pos2)
    {
        if (Math.Abs(pos1.x - pos2.x) < 0.5f && Math.Abs(pos1.y - pos2.y) < 0.5f)
        {
            return true;
        }
        return false;
    }

    private void UpdatePositionClient(int tick)
    {
        var input = m_InputBuffer.Get(tick);
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

        m_PositionBuffer.Put(transform.position, tick);
    }

    private void UpdatePositionServer()
    {
        // Mise a jour de la position selon dernier input reçu, puis consommation de l'input
        if (m_InputQueue.Count > 0)
        {
            var data = m_InputQueue.Dequeue();
            var input = data.Item1;
            var tick = data.Item2;
            var pos = m_Position.Value.Vector;
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
            m_Position.Value = new Vector2Payload { Vector = pos, Tick = tick };
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
        
        m_InputBuffer.Put(inputDirection.normalized, NetworkUtility.GetLocalTick());
        SendInputServerRpc(inputDirection.normalized, NetworkUtility.GetLocalTick());
    }


    [ServerRpc]
    private void SendInputServerRpc(Vector2 input, int tick)
    {
        // On utilise une file pour les inputs pour les cas ou on en recoit plusieurs en meme temps.
        m_InputQueue.Enqueue((input, tick));
    }



}
