using Unity.Netcode;
using UnityEngine;
using TMPro;
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

    public void Host() {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
    }

    struct Player
    {
        public string name;
        public string password;
    }
    public void Client() {
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

        bool approveConnection = Encoding.ASCII.GetString(connectionData) == passwordInputField.text;

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
