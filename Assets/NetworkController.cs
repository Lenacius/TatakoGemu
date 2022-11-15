using UnityEngine;
using Unity.Netcode;

public class NetworkController : NetworkManager
{
    private void OnPlayerConnected(NetworkClient player)
    {
        foreach(NetworkClient client in ConnectedClients.Values)
        {
            if (client != player)
                client.PlayerObject.GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }
    }
}
