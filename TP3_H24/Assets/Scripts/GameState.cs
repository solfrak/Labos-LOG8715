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

    private NetworkVariable<bool> m_IsStunned = new NetworkVariable<bool>();

    private bool m_ClientIsStunned;

    private CircleBuffer<bool> m_StateBuffer;

    public bool IsStunned
    {
        get
        {
            if (IsClient)
            {
                return m_ClientIsStunned;
            }
            return m_IsStunned.Value;
        }
    }

    private Coroutine m_StunCoroutine;

    private float m_CurrentRtt;

    public float CurrentRTT { get => m_CurrentRtt / 1000f; }

    public NetworkVariable<float> ServerTime;

    private void Start()
    {
        m_GameArea.transform.localScale = new Vector3(m_GameSize.x * 2, m_GameSize.y * 2, 1);
    }


    private void Awake()
    {
        m_StateBuffer = new CircleBuffer<bool>(2 * (int)NetworkUtility.GetLocalTick());
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

    public void Stun()
    {
        if (m_StunCoroutine != null)
        {
            StopCoroutine(m_StunCoroutine);
        }
        if (IsServer)
        {
            m_StunCoroutine = StartCoroutine(StunCoroutine());
        }
        if (IsClient)
        {
            Debug.Log("Activating coroutine");
            m_StunCoroutine = StartCoroutine(LocalStunCoroutine());
        }
    }

    private IEnumerator StunCoroutine()
    {
        m_IsStunned.Value = true;
        yield return new WaitForSeconds(m_StunDuration);
        m_IsStunned.Value = false;
    }
    private IEnumerator LocalStunCoroutine()
    {
        m_ClientIsStunned = true;
        yield return new WaitForSeconds(m_StunDuration);
        m_ClientIsStunned = false;
    }
}
