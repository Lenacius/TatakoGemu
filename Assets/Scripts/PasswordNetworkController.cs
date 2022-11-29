using System;
using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;
using TMPro;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;

public class PasswordNetworkController : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private GameObject passwordEntryUI;
    [SerializeField] private GameObject leaveButton;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject readyStatePanel;
    [SerializeField] private GameObject joinCodePanel;

    private UnityTransport transport;
    private const int MaxPlayers = 5;

    //private async void Awake()
    //{
    //    transport = FindObjectOfType<UnityTransport>();
    //    await Authenticate();
    //}

    //private async Task Authenticate()
    //{
    //    await UnityServices.InitializeAsync();
    //    await AuthenticationService.Instance.SignInAnonymouslyAsync();
    //}

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    public async void Host() {
        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        joinCodePanel.GetComponentInChildren<TextMeshProUGUI>().text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
    }

    struct Player
    {
        public string name;
        public string password;
    }
    public async void Client() {
        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(passwordInputField.text);
        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(passwordInputField.text);
        NetworkManager.Singleton.StartClient();
    }

    public void Leave() {
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().ChangeCameraToWorld();
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }

        passwordEntryUI.SetActive(true);
        leaveButton.SetActive(false);
        readyButton.SetActive(false);
        startButton.SetActive(false);
    }

    private void HandleServerStarted()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HandleClientConnected(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        if(clientId == NetworkManager.Singleton.LocalClientId)
        {
            passwordEntryUI.SetActive(false);
            leaveButton.SetActive(true);
            if (NetworkManager.Singleton.IsHost)
            {
                startButton.SetActive(true);
                readyStatePanel.SetActive(true);
            }
            else if (NetworkManager.Singleton.IsClient) readyButton.SetActive(true);
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            passwordEntryUI.SetActive(true);
            leaveButton.SetActive(false);
            readyButton.SetActive(false);
            startButton.SetActive(false);
            readyStatePanel.SetActive(false);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var clientId = request.ClientNetworkId;
        var connectionData = request.Payload;

        //bool approveConnection = Encoding.ASCII.GetString(connectionData) == passwordInputField.text;
        bool approveConnection = true;
        

        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        switch (NetworkManager.Singleton.ConnectedClients.Count) // comment switch when official
        {
            case 0:
                spawnPos = new Vector3(0f, 1f, -6f);
                spawnRot = Quaternion.Euler(0f, 180f, 0f);
                break;
            case 1:
                spawnPos = new Vector3(2f, 1f, -6f);
                spawnRot = Quaternion.Euler(0f, 225f, 0f);
                break;
            case 2:
                spawnPos = new Vector3(-2f, 1f, -6f);
                spawnRot = Quaternion.Euler(0f, 135f, 0f);
                break;
            case 3:
                spawnPos = new Vector3(4f, 1f, -6f);
                spawnRot = Quaternion.Euler(0f, 135f, 0f);
                break;
            case 4:
                spawnPos = new Vector3(-4f, 1f, -6f);
                spawnRot = Quaternion.Euler(0f, 135f, 0f);
                break;
        }

        response.Approved = approveConnection;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;
        response.Position = spawnPos;
        response.Rotation = spawnRot;
        response.Pending = false;
    }


    private void Update()
    {
        
        if (NetworkManager.Singleton.IsHost)
        {
            TextMeshProUGUI message = GameObject.Find("PlayersReadyMessage").GetComponent<TextMeshProUGUI>();
            if (PlayersReady())
            {
                startButton.GetComponent<Button>().interactable = true;
                message.text = "All players ready!";
            }
            else
            {
                startButton.GetComponent<Button>().interactable = false;
                message.text = "Not all players ready...";
            }
        }
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
