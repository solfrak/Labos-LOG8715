using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameState : NetworkBehaviour
{
    [SerializeField]
    private GameObject m_GameArea;

    [FormerlySerializedAs("m_StunDuration")] [SerializeField]
    public float StunDuration = 1.0f;

    [SerializeField]
    private Vector2 m_GameSize;

    public Vector2 GameSize { get => m_GameSize; }

    private NetworkVariable<bool> m_IsStunned = new NetworkVariable<bool>();
    private bool m_ClientIsStunned = false;

    public bool IsStunned
    {
        get
        {
            if (IsClient)
            {
                return m_ClientIsStunned;
            }
            else
            {
                return m_IsStunned.Value;
            }
        }

        set
        {
            if (IsClient)
            {
                m_ClientIsStunned = value;
            }

            else
            {
                m_IsStunned.Value = value;
            }
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

    public void Stun(int endTick)
    {
        if (m_StunCoroutine != null)
        {
            StopCoroutine(m_StunCoroutine);
        }
        m_StunCoroutine = StartCoroutine(StunCoroutine(endTick));
    }

    private IEnumerator StunCoroutine(int endTick)
    {
        IsStunned = true;
        while (NetworkUtility.GetLocalTick() < endTick)
        {
            yield return null;
        }
        IsStunned = false;
    }
}
