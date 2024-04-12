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
                int currentTick = NetworkUtility.GetLocalTick();
                int endTick = currentTick + (int)(m_GameState.StunDuration * NetworkUtility.GetLocalTickRate());
                ActivateStunServerRpc(currentTick);
                m_GameState.Stun(endTick);
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void ActivateStunServerRpc(int startTick)
    {
        int endTick = startTick + (int)(m_GameState.StunDuration * NetworkUtility.GetLocalTickRate());
        ActivatStunClientRpc(startTick, endTick);
        m_GameState.Stun(endTick);
        
    }

    [ClientRpc]
    private void ActivatStunClientRpc(int startTick, int endTick)
    {
        m_GameState.Stun(endTick);

        foreach (var player in FindObjectsOfType < Player>())
        {
            if (player.IsClient && player.IsOwner)
            {
                player.StunRollback(startTick, endTick);
            }
        }
    }
}
