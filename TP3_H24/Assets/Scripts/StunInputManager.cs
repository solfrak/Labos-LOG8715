using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StunInputManager : NetworkBehaviour
{
    [SerializeField]
    private GameState m_GameState;

    private void Update()
    {
        // Seuls les clients peuvent envoyer des inputs.
        if (IsClient)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                int startTick = NetworkUtility.GetLocalTick();
                m_GameState.Stun(startTick);
                ActivateStunServerRpc(startTick);
            }
        }
    }
    

    [ServerRpc (RequireOwnership = false)]
    private void ActivateStunServerRpc(int startTick)
    {
        m_GameState.Stun(startTick);
    }
}
