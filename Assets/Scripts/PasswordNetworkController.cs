using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class PasswordNetworkController : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private GameObject passwordEntryUI;
    [SerializeField] private GameObject leaveButton;
    [SerializeField] private GameObject teamPicker;

    //private static Dictionary<ulong, string> roomData;

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

    public void Host() { // host does not count in the connected client count
        //roomData = new Dictionary<ulong, string>();
        //roomData[NetworkManager.Singleton.LocalClientId] = passwordInputField.text;

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        //Debug.Log("Numero de jogadores: " + NetworkManager.Singleton.ConnectedClients.Count);
    }

    struct Player
    {
        public string name;
        public string password;
    }
    public void Client() {
        //Player player = new Player();
        //player.name = nameInputField.text;
        //player.password = passwordInputField.text;
        //byte[] data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(player));
        //Debug.Log(JsonUtility.ToJson(player));
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(passwordInputField.text);
        //NetworkManager.Singleton.NetworkConfig.ConnectionData = data;
        NetworkManager.Singleton.StartClient();
        //Debug.Log("Numero de jogadores: " + NetworkManager.Singleton.ConnectedClients.Count);
    }

    public void Leave() {
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
        teamPicker.SetActive(false);
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
            //teamPicker.SetActive(true);
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            passwordEntryUI.SetActive(true);
            leaveButton.SetActive(false);
            //teamPicker.SetActive(false);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        //if (NetworkManager.Singleton.IsHost)
        //{
        //    response.Approved = true;
        //    response.CreatePlayerObject = true;
        //    response.PlayerPrefabHash = null;
        //    response.Position = new Vector3(0f, 1f, -6f);
        //    response.Rotation = Quaternion.Euler(0f, 180f, 0f);
        //    response.Pending = false;
        //    return;
        //}

        var clientId = request.ClientNetworkId;
        var connectionData = request.Payload;
        //byte[] connectionData = request.Payload;
        //Player payload = new Player();
        //payload = JsonUtility.FromJson<Player>(Encoding.ASCII.GetString(connectionData));
        //Debug.Log(roomData[NetworkManager.Singleton.LocalClientId]);
        bool approveConnection = Encoding.ASCII.GetString(connectionData) == passwordInputField.text;
        //bool approveConnection = payload.password == passwordInputField.text;
        Debug.Log(Encoding.ASCII.GetString(connectionData));

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
}
