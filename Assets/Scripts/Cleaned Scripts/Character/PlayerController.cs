using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;
using Unity.Collections;
using TMPro;
using System;

public class PlayerController : CharacterController
{
    [SerializeField] public NetworkVariable<bool> is_ready = new NetworkVariable<bool>(false);
    [SerializeField] public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>();
    [SerializeField] public string playerNamePlaceholder;
    [SerializeField] private Transform Body;
    [SerializeField] private TextMeshPro playerNameDisplay;

    public GameObject ready;
    private void Start()
    {
        if (IsOwner)
        {
            ChangeCameraToPlayer();
            if (IsClient && !IsHost)
            {
                ready = GameObject.Find("ReadyButton");
                ready.GetComponent<Button>().onClick.AddListener(TooglePlayerReadyServerRpc);
                //ready.GetComponent<Button>().onClick.AddListener(TooglePlayerReadyDebug);
                is_ready.OnValueChanged += UpdateDictionary;
                //playerName.OnValueChanged += DisplayName;
            }
        }
    }

    //private void DisplayName(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    //{
    //    playerNameDisplay.text = newValue.ToString();
    //}

    private void UpdateDictionary(bool previousValue, bool newValue)
    {
        //Debug.Log($"PLAYER {(int)NetworkManager.LocalClientId} HAS BECOME: {newValue}");
        var lobby = GameObject.Find("LobbyController").GetComponent<LobbyController>();
        lobby.playersInLobby[(int)NetworkManager.LocalClientId] = newValue;
        //lobby.UpdatePlayerServerRpc(NetworkManager.LocalClientId, newValue);
        lobby.PropagateToClients();
    }

    void Update()
    {
        if (IsOwner) PlayerMovement();
    }

    private void PlayerMovement()
    {
        Body.rotation = transform.rotation;
        if (IsSwimming()) Body.Rotate(new Vector3(90, 0, 0));

        if (Input.GetKey(KeyCode.W)) Move(Direction.UP);
        else if (Input.GetKey(KeyCode.S)) Move(Direction.DOWN);
        
        if (Input.GetKey(KeyCode.A)) Move(Direction.LEFT);
        else if (Input.GetKey(KeyCode.D)) Move(Direction.RIGHT);

        if (Input.GetKey(KeyCode.Space)) Jump();

        if (Input.GetAxis("Mouse X") > 0) Rotate(Direction.RIGHT);
        else if (Input.GetAxis("Mouse X") < 0) Rotate(Direction.LEFT);
    }

    private void ChangeCameraToPlayer()
    {
        Transform main_camera = GameObject.Find("Main Camera").transform;
        main_camera.parent = transform;
        main_camera.localPosition = new Vector3(0, 5, 4);
        main_camera.localRotation = Quaternion.Euler(new Vector3(30, -180, 0));
    }

    public void ChangeCameraToWorld()
    {
        Transform main_camera = GameObject.Find("Main Camera").transform;
        main_camera.parent = null;
        main_camera.localPosition = new Vector3(0, 6, 4);
        main_camera.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }

    [ServerRpc(RequireOwnership = false)]
    public void TooglePlayerReadyServerRpc() {
        is_ready.Value = !is_ready.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(bool state)
    {
        is_ready.Value = state;
    }

    //public void TooglePlayerReadyDebug()
    //{
    //    var lobby = GameObject.Find("LobbyController").GetComponent<LobbyController>();
    //    Debug.LogWarning($"KEY {(int)NetworkManager.Singleton.LocalClientId} NEW VALUE {is_ready.Value}");
    //}

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerNameServerRpc(string name) {
        //Debug.LogFormat("Name given to player" + name);
        playerName.Value = name;
        SetPlayerNameClientRpc(name);
    }

    [ClientRpc]
    private void SetPlayerNameClientRpc(string name)
    {
        playerNamePlaceholder = name;
        playerNameDisplay.text = name;
    }


}
