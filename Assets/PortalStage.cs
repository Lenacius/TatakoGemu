using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PortalStage : NetworkBehaviour
{
    [SerializeField] private NetworkSceneController sceneController;

    public void Start()
    {
        //this.NetworkObject.Spawn();
        sceneController = GameObject.Find("NetworkSceneController").GetComponent<NetworkSceneController>();
        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        
        //Debug.Log(playerCount.Value);
        //Debug.Log(NetworkManager.Singleton.ConnectedClients.Count);
        if (IsServer || IsHost)
        {
            if(PlayersReady())
            {
                sceneController.GotoStage2();
            }
        }

        if(NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().is_ready.Value)
        {
            Debug.LogWarning($"Client {NetworkManager.Singleton.LocalClientId} locked in portal");
            NetworkManager.Singleton.LocalClient.PlayerObject.transform.position = this.transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsClient)
        {
            Debug.LogWarning("PORTAL ACTIVATED");
            other.GetComponent<PlayerController>().SetPlayerReadyServerRpc(true);
        }
    }

    private bool PlayersReady()
    {
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (!client.PlayerObject.GetComponent<PlayerController>().is_ready.Value)
                return false;
        }

        return true;
    }
}
