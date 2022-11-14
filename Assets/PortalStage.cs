using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PortalStage : NetworkBehaviour
{
    private NetworkVariable<int> playerCount = new NetworkVariable<int>();

    public void Awake()
    {
        this.NetworkObject.Spawn();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer || IsHost)
        {
            playerCount.Value = 0;
        }
    }

    private void Update()
    {
        
        Debug.Log(playerCount.Value);
        Debug.Log(NetworkManager.Singleton.ConnectedClients.Count);
        if (IsServer || IsHost)
        {
            if(playerCount.Value == NetworkManager.Singleton.ConnectedClients.Count)
            {
                GameObject.Destroy(GameObject.Find("Level"));
                SceneManager.LoadScene("Stage2", LoadSceneMode.Additive);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        NetworkObject player = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject;

        player.transform.position = transform.position;

        this.playerCount.Value++;
    }

}
