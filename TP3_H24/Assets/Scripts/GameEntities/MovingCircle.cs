using System;
using DataStruct;
using Unity.Netcode;
using UnityEngine;

public class MovingCircle : NetworkBehaviour
{
    public class StatePayload : INetworkSerializable
    {
        public State State;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref State.Tick);
            serializer.SerializeValue(ref State.Position);
            serializer.SerializeValue(ref State.Velocity);
        }
    }

    public struct State
    {
        public int Tick;
        public Vector2 Position;
        public Vector2 Velocity;

        public State(int tick, Vector2 position, Vector2 velocity)
        {
            Tick = tick;
            Position = position;
            Velocity = velocity;
        }

        public override string ToString()
        {
            return $"Tick: {Tick} Position: {Position} Velocity: {Velocity}";
        }
    }

    [SerializeField]
    private float m_Radius = 1;

    public Vector2 InitialPosition;
    public Vector2 InitialVelocity;

    public Vector2 Position { get => m_Position; }


    private NetworkVariable<StatePayload> m_ServerState = new NetworkVariable<StatePayload>();

    private CircleBuffer<State> m_StateBuffer;

    private Vector2 m_Position;
    private Vector2 m_Velocity;

    private int m_LastBiggestClientTick = 0;
    private GameState m_GameState;

    private void Awake()
    {
        m_GameState = FindObjectOfType<GameState>();

        int bufferSize = 8 * (int)NetworkUtility.GetLocalTickRate();
        m_StateBuffer = new CircleBuffer<State>(bufferSize);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            m_Position = InitialPosition;
            m_Velocity = InitialVelocity;
            m_ServerState.Value = new StatePayload() { State = new State(0, InitialPosition, InitialVelocity) };
        }

        if (IsClient)
        {
            int expectedEndOfInitTick = NetworkUtility.GetLocalTick() + (int) (NetworkUtility.GetCurrentRtt(OwnerClientId) / (1000.0f * NetworkUtility.GetLocalTickDeltaTime()));
            ReconcileState(m_ServerState.Value.State, expectedEndOfInitTick);

            m_ServerState.OnValueChanged += (StatePayload previousValue, StatePayload newValue) =>
            {
                VerifyWithState(newValue.State);
            };
        }

    }


    private void FixedUpdate()
    {
        // Si le stun est active, rien n'est mis a jour.
        if (!m_GameState.IsStunned)
        {
            Move(Time.deltaTime);
        }
    }

    private void VerifyWithState(State state)
    {
        State clientState = m_StateBuffer.Get(state.Tick);
        Vector2 clientPos = clientState.Position;

        if(!PositionEqualWithTolerance(state.Position, clientPos, 0.5f) && clientState.Tick == state.Tick)
        {
            //Debug.LogWarning($"newState: {state}\nclientState: {clientState}");

            ReconcileState(state, m_LastBiggestClientTick);
        }
    }
    
    private void Move(float deltaTime)
    {
        // Gestion des collisions avec l'exterieur de la zone de simulation
        ApplyMove(ref m_Position, ref m_Velocity, deltaTime);

        int tick = NetworkUtility.GetLocalTick();
        if(IsServer)
        {
            m_ServerState.Value = new StatePayload() { State = new State(tick, m_Position, m_Velocity) };
        }

        // With a high enough latency, the client's tick from its local time can be behing a previous tick
        if(tick >= m_LastBiggestClientTick)
        {
            m_StateBuffer.Put(new State(tick, m_Position, m_Velocity), tick);
            m_LastBiggestClientTick = tick;
        }


        if(tick < m_LastBiggestClientTick)
        {
            //Debug.LogError($"PreviousTick: {m_LastBiggestClientTick} CurrentTick {NetworkUtility.GetLocalTick()}\nState: {m_StateBuffer.Get(tick)} time: {NetworkManager.Singleton.NetworkTickSystem.LocalTime.Time}  selfTime: {Time.time}");
        }
    }

    private void ReconcileState(State state, int endTick)
    {
        m_StateBuffer.Put(state, state.Tick);

        int curTick = state.Tick;
        Vector2 pos = state.Position;
        Vector2 vel = state.Velocity;
        float deltaTime = 1.0f / NetworkUtility.GetLocalTickRate();
        while(curTick <= endTick)
        {
            curTick++;
            ApplyMove(ref pos, ref vel, deltaTime);
            m_StateBuffer.Put(new State(curTick, pos, vel), curTick);
        }

        m_Position = pos;
        m_Velocity = vel;
    }


    private void ApplyMove(ref Vector2 pos, ref Vector2 vel, float deltaTime)
    {
        pos += vel * deltaTime;

        // Gestion des collisions avec l'exterieur de la zone de simulation
        var size = m_GameState.GameSize;
        if(pos.x - m_Radius < -size.x)
        {
            pos = new Vector2(-size.x + m_Radius, pos.y);
            vel *= new Vector2(-1, 1);
        }
        else if(pos.x + m_Radius > size.x)
        {
            pos = new Vector2(size.x - m_Radius, pos.y);
            vel *= new Vector2(-1, 1);
        }

        if(pos.y + m_Radius > size.y)
        {
            pos = new Vector2(pos.x, size.y - m_Radius);
            vel *= new Vector2(1, -1);
        }
        else if(pos.y - m_Radius < -size.y)
        {
            pos = new Vector2(pos.x, -size.y + m_Radius);
            vel *= new Vector2(1, -1);
        }
    }


    private bool PositionEqualWithTolerance(Vector2 pos1, Vector2 pos2, float tolerance)
    {
        return Math.Abs(pos1.x - pos2.x) + Math.Abs(pos1.y - pos2.y) < tolerance;
    }
}
