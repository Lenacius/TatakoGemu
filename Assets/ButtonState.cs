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

        //Debug.LogWarning($"PLAYER {(int)NetworkManager.Singleton.LocalClientId} >>>>>>>>>>>>>>>>>>>>>>>>");
        //Debug.LogWarning($"Player: {lobby.playersConnected.Value}");
        if (lobby.playersInLobby.ContainsKey(buttonNumber)) {
            //Debug.LogWarning($"PLAYER {(int)NetworkManager.Singleton.LocalClientId} is trying to ACCESS {buttonNumber} KEY.");
            //Debug.LogWarning($"KEY VALUE {lobby.playersInLobby[buttonNumber]}");
            playerState = lobby.playersInLobby[buttonNumber];
            notReadyImg.SetActive(true);
            if (playerState) readyImg.SetActive(true);
            else readyImg.SetActive(false);
        }
        else notReadyImg.SetActive(false);
    }

}
