using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

public class MyBootstrapManager : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(300, 10, 600, 300));

        var networkManager = NetworkManager.Singleton;
        if (!networkManager.IsClient && !networkManager.IsServer)
        {
            if (GUILayout.Button("Host"))
            {
                networkManager.StartHost();
            }

            if (GUILayout.Button("Client"))
            {
                networkManager.StartClient();
            }

            if (GUILayout.Button("Server"))
            {
                networkManager.StartServer();
            }
        }
        else
        {
            GUILayout.Label($"Mode: {(networkManager.IsHost ? "Host" : networkManager.IsServer ? "Server" : "Client")}");

            // "Random Teleport" button will only be shown to clients
            if (networkManager.IsClient)
            {
                if (GUILayout.Button("Random Teleport"))
                {
                    if (networkManager.LocalClient != null)
                    {

                        // Get `BootstrapPlayer` component from the player's `PlayerObject`
                        if (networkManager.LocalClient.PlayerObject.TryGetComponent(out Networked3rdPersonInput bootstrapPlayer))
                        {
                            // Invoke a `ServerRpc` from client-side to teleport player to a random position on the server-side
                            bootstrapPlayer.RandomTeleportServerRpc();
                        }
                    }
                }
            }
        }

        GUILayout.EndArea();
    }

}

