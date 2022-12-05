using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class StartButtonController : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton.IsHost)
            if (PlayersReady()) this.gameObject.GetComponent<Button>().interactable = true;
            else this.gameObject.GetComponent<Button>().interactable = false;
    }

    private bool PlayersReady()
    {
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (client.ClientId != 0 && !client.PlayerObject.GetComponent<PlayerController>().is_ready.Value)
                return false;
        }
        return true;
    }
}
