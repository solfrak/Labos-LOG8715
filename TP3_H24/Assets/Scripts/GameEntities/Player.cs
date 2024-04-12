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

    private NetworkVariable<Vector2Payload> m_Position = new NetworkVariable<Vector2Payload>();

    private Vector2 m_ClientPosition;

    public Vector2 Position
    {
        get
        {
            if (IsClient && IsOwner)
            {
                return m_ClientPosition;
            }
            return m_Position.Value.Vector;
        }
        
    }

    private Queue<(Vector2, int)> m_InputQueue = new Queue<(Vector2, int)>();



    private CircleBuffer<Vector2> m_PositionBuffer;
    private CircleBuffer<Vector2> m_InputBuffer;
    private void Awake()
    {
        m_GameState = FindObjectOfType<GameState>();

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            m_Position.Value = new Vector2Payload();
        }
        
        m_PositionBuffer = new CircleBuffer<Vector2>(2 * (int)NetworkUtility.GetLocalTickRate());
        m_InputBuffer = new CircleBuffer<Vector2>(2 * (int)NetworkUtility.GetLocalTickRate());

        for (int i = 0; i < 2 * (int)NetworkUtility.GetLocalTickRate(); i++)
        {
            m_PositionBuffer[i] = m_Position.Value.Vector; 
            m_InputBuffer[i] = Vector2.zero;
        }
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
            UpdateInputClient(NetworkUtility.GetLocalTick());
            UpdatePositionClient(NetworkUtility.GetLocalTick());
        }
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

            if (GameState.IsStunned)
            {
                m_Position.Value = new Vector2Payload()
                {
                    Vector = m_PositionBuffer[tick - 1],
                    Tick = tick
                };
                m_PositionBuffer[tick] = m_PositionBuffer[tick - 1];
                return;
            }
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

            m_Position.Value = new Vector2Payload() { Vector = pos, Tick = tick };

            m_PositionBuffer[tick] = pos;
        }
    }

    private void UpdatePositionClient(int tick, int endTick = 0)
    {
        // Mise a jour de la position selon dernier input reçu, puis consommation de l'input
        var input = m_InputBuffer[tick];

        var pos = m_ClientPosition;

        if (GameState.IsStunned || (endTick != 0 && tick < endTick))
        {
            m_PositionBuffer[tick] = m_PositionBuffer[tick - 1];
            m_ClientPosition = pos;
            return;
        }
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

        m_PositionBuffer[tick] = pos;
        m_ClientPosition = pos;
    }

    private void UpdateInputClient(int tick)
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

        m_InputBuffer[tick] = inputDirection.normalized;
        SendInputServerRpc(inputDirection.normalized, tick);
    }

    public void StunRollback(int tick, int endTick)
    {
        m_ClientPosition = m_PositionBuffer[tick];
        int currentTick = NetworkUtility.GetLocalTick();
        while (tick < currentTick)
        {
            UpdatePositionClient(tick++, endTick);
        }
    }


    [ServerRpc]
    private void SendInputServerRpc(Vector2 input, int tick)
    {
        // On utilise une file pour les inputs pour les cas ou on en recoit plusieurs en meme temps.
        m_InputQueue.Enqueue((input, tick));
    }



}
