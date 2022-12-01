using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ButtonState : MonoBehaviour
{
    [SerializeField] private LobbyController lobby;
    [SerializeField] private int buttonNumber;
    [SerializeField] private bool playerState;
    [SerializeField] private GameObject notReadyImg;
    [SerializeField] private GameObject readyImg;

    // Update is called once per frame
    void Update()
    {
        Debug.LogWarning($"Number of connected players: {lobby.playersConnected.Value}");
        if(buttonNumber < lobby.playersConnected.Value) {
            notReadyImg.SetActive(true);
            playerState = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().is_ready.Value;
            if (NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().is_ready.Value) readyImg.SetActive(true);
            else readyImg.SetActive(false);
        }
        else notReadyImg.SetActive(false);

    }
}
