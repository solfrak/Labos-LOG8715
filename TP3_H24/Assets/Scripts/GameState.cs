using System;
using System.Collections;
using System.Collections.Generic;
using DataStruct;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : NetworkBehaviour
{
    [SerializeField]
    private GameObject m_GameArea;

    [SerializeField]
    private float m_StunDuration = 1.0f;

    [SerializeField]
    private Vector2 m_GameSize;

    public Vector2 GameSize { get => m_GameSize; }

    private bool m_IsStunned = false;

    public bool IsStunned
    {
        get
        {
            return m_IsStunned;
        }
    }

    private CircleBuffer<BoolPayload> m_StunBuffer;

    private Coroutine m_StunCoroutine;

    private float m_CurrentRtt;

    public float CurrentRTT { get => m_CurrentRtt / 1000f; }

    public NetworkVariable<float> ServerTime;


    private void Awake()
    {
        int bufferSize = 8 * (int) NetworkUtility.GetLocalTickRate();
        m_StunBuffer = new CircleBuffer<BoolPayload>(bufferSize);
    }

    private void Start()
    {
        m_GameArea.transform.localScale = new Vector3(m_GameSize.x * 2, m_GameSize.y * 2, 1);
    }


    private void FixedUpdate()
    {
        if (IsSpawned)
        {
            m_CurrentRtt = NetworkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId);
        }

        if (IsServer)
        {
            ServerTime.Value = Time.time;
        }
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (!IsServer)
        {
            // si on est un client, retourner au menu principal
            SceneManager.LoadScene("StartupScene");
        }
    }

    public bool IsStunnedAtTick(int tick)
    {
        if(tick == NetworkUtility.GetLocalTick())
        {
            return m_IsStunned;
        }

        var stunned = m_StunBuffer.Get(tick);
        if (stunned == null || stunned.Tick != tick)
        {
            return false;
        }
        return stunned.Value;
    }

    public void Stun(int startTick)
    {
        if(IsStunnedAtTick(startTick))
        {
            return;
        }

        if(IsServer)
        {
            StunClientRpc(startTick);
        }

        if (m_StunCoroutine != null)
        {
            StopCoroutine(m_StunCoroutine);
        }




        int endTick = GetEndTickStun(startTick);

        for (int tick = startTick; tick <= endTick; tick++)
        {
            m_StunBuffer.Put(new BoolPayload() { Tick = tick, Value = true }, tick);
        }
        Debug.Log("Stun from " + startTick + " to " + endTick + " at tick " + NetworkUtility.GetLocalTick());

        var circles = FindObjectsOfType<MovingCircle>();
        foreach(var circle in circles)
        {
            circle.ReconcileState(startTick, endTick);
        }
        // TODO reconcialiate players
        m_StunCoroutine = StartCoroutine(StunCoroutine(endTick));
    }

    private IEnumerator StunCoroutine(int endTick)
    {
        m_IsStunned = true;
        while(NetworkUtility.GetLocalTick() <= endTick)
        {
            yield return null;
        }
        m_IsStunned = false;
    }

    [ClientRpc]
    public void StunClientRpc(int startTick)
    {
        Stun(startTick);
    }

    public int GetEndTickStun(int startTick)
    {
        return startTick + (int)(m_StunDuration / NetworkUtility.GetLocalTickDeltaTime());
    }
}
