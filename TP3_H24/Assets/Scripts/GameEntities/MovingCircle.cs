using System;
using System.Collections;
using System.Collections.Generic;
using DataStruct;
using Unity.Netcode;
using UnityEngine;

public class MovingCircle : NetworkBehaviour
{
    [SerializeField]
    private float m_Radius = 1;

    [SerializeField]
    private bool DebugPrint;

    public Vector2 Position
    {
        get
        {
            if (IsClient)
            {
                return m_ClientPosition;
            }

            return m_Position.Value.Vector;
        }
    }

    public Vector2 Velocity => m_Velocity.Value.Vector;

    public Vector2 InitialPosition;
    public Vector2 InitialVelocity;

    private NetworkVariable<Vector2Payload> m_Position = new NetworkVariable<Vector2Payload>();
    private NetworkVariable<Vector2Payload> m_Velocity = new NetworkVariable<Vector2Payload>();

    private CircleBuffer<Vector2> m_PositionBuffer;
    private CircleBuffer<Vector2> m_VelocityBuffer;
    
    private Vector2 m_ClientPosition;
    private Vector2 m_ClientVelocity;

    private GameState m_GameState;

    private void Awake()
    {
        m_GameState = FindObjectOfType<GameState>();
        m_PositionBuffer = new CircleBuffer<Vector2>(2 * (int)NetworkUtility.GetLocalTickRate());
        m_VelocityBuffer = new CircleBuffer<Vector2>(2 * (int)NetworkUtility.GetLocalTickRate());
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            m_Position.Value = new Vector2Payload(){ Vector = InitialPosition, Tick = 0};
            m_Velocity.Value = new Vector2Payload() { Vector = InitialVelocity, Tick = 0 };
        }

        if (IsClient)
        {
            m_ClientPosition = m_Position.Value.Vector;
            m_ClientVelocity = m_Velocity.Value.Vector;
        }
    }

    private int m_lastTick = 0;
    private void FixedUpdate()
    {
        var currentTick = NetworkUtility.GetLocalTick();

        if (m_lastTick == currentTick)
        {
            return;
        }
        // Si le stun est active, rien n'est mis a jour.
        if (m_GameState.IsStunned)
        {
            return;
        }

        // Seul le serveur peut mettre a jour la position et la vitesse des cercles.
        if (IsServer)
        {
            MoveServer();
        }

        if (IsClient)
        {
            MoveClient();
            CheckServerPosition();
        }

    }

    private void CheckServerPosition()
    {
        var serverPos = m_Position.Value.Vector;
        var tick = m_Position.Value.Tick;

        var clientPos = m_PositionBuffer.Get(tick);

        if (!PositionEqualWithTolerance(serverPos, clientPos))
        {
            //TODO make some reconciliation;
            // Debug.Log("Server tick: " + tick + " Server Pos: " + serverPos + " Client pos: " +clientPos);
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

    private void MoveServer()
    {
        Vector2 pos = m_Position.Value.Vector;
        Vector2 vel = m_Velocity.Value.Vector;
        pos += vel * Time.deltaTime;
 
        // Gestion des collisions avec l'exterieur de la zone de simulation
        var size = m_GameState.GameSize;
        if (pos.x - m_Radius < -size.x)
        {
            pos = new Vector2(-size.x + m_Radius, pos.y);
            vel *= new Vector2(-1, 1);
        }
        else if (pos.x + m_Radius > size.x)
        {
            pos = new Vector2(size.x - m_Radius, pos.y);
            vel *= new Vector2(-1, 1);
        }
 
        if (pos.y + m_Radius > size.y)
        {
            pos = new Vector2(pos.x, size.y - m_Radius);
            vel *= new Vector2(1, -1);
        }
        else if (pos.y - m_Radius < -size.y)
        {
            pos = new Vector2(pos.x, -size.y + m_Radius);
            vel *= new Vector2(1, -1);
        }

        m_Position.Value = new Vector2Payload() { Vector = pos, Tick = NetworkUtility.GetLocalTick() };

        if (DebugPrint)
        {
            Debug.Log("Server tick: " + NetworkUtility.GetLocalTick() + " Server Pos: " + pos);
        }
        m_Velocity.Value = new Vector2Payload() { Vector = vel, Tick = NetworkUtility.GetLocalTick() };
    }

    private void MoveClient()
    {
        m_ClientPosition += m_ClientVelocity * Time.deltaTime;
                    
        // Gestion des collisions avec l'exterieur de la zone de simulation
        var size = m_GameState.GameSize;
        if (m_ClientPosition.x - m_Radius < -size.x)
        {
            m_ClientPosition = new Vector2(-size.x + m_Radius, m_ClientPosition.y);
            m_ClientVelocity *= new Vector2(-1, 1);
        }
        else if (m_ClientPosition.x + m_Radius > size.x)
        {
            m_ClientPosition = new Vector2(size.x - m_Radius, m_ClientPosition.y);
            m_ClientVelocity *= new Vector2(-1, 1);
        }
                    
        if (m_ClientPosition.y + m_Radius > size.y)
        {
            m_ClientPosition = new Vector2(m_ClientPosition.x, size.y - m_Radius);
            m_ClientVelocity *= new Vector2(1, -1);
        }
        else if (m_ClientPosition.y - m_Radius < -size.y)
        {
            m_ClientPosition = new Vector2(m_ClientPosition.x, -size.y + m_Radius);
            m_ClientVelocity *= new Vector2(1, -1);
        }
                    
        m_PositionBuffer.Put(m_ClientPosition, NetworkUtility.GetLocalTick());
        
         if (DebugPrint)
         {
             Debug.Log("Client tick: " + NetworkUtility.GetLocalTick() + " Client Pos: " + m_ClientPosition);
         }
        m_VelocityBuffer.Put(m_ClientVelocity, NetworkUtility.GetLocalTick());
    }
}
