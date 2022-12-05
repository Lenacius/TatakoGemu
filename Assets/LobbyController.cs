using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyController : NetworkBehaviour
{
    [SerializeField] private GameObject lobbyCanvas;
    [SerializeField] private GameObject preGameCanvas;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private TextMeshProUGUI loobyCodeDisplay;
    [SerializeField] private TMP_InputField loobyCodeInput;
    [SerializeField] private TMP_InputField nicknameInput;

    [SerializeField] public Dictionary<int, bool> playersInLobby = new Dictionary<int, bool>();

    private Lobby connectedLobby;
    private QueryResponse lobbies;
    private UnityTransport transport;
    private const string JoinCodeKey = "j";
    private string playerId;

    private void Update() //DEBUG ONLY
    {
        if (IsHost)
        {
            foreach(var client in NetworkManager.ConnectedClients.Values)
            {
                playersInLobby[(int)client.ClientId] = client.PlayerObject.GetComponent<PlayerController>().is_ready.Value;
            }
        }

        //// DEBUG FOR DICTIONARY VIEW
        //foreach (var i in playersInLobby)
        //{
        //    Debug.LogWarning($"| {i.Key} | {i.Value} |");
        //}

        //// GENERAL DEBUG
        //Debug.LogWarning($"PlayerID: {NetworkManager.Singleton.LocalClientId}");
        //if (connectedLobby != null)
        //{
        //    Debug.LogWarning($"Lobby id: {connectedLobby.Id}");
        //    Debug.LogWarning($"Player count in lobby: {connectedLobby.Players.Count}");
        //    if (NetworkManager.Singleton.IsHost)
        //        Debug.LogWarning($"Player count in network: {NetworkManager.Singleton.ConnectedClients.Count}");
        //}
    }

    private async void Awake() {
        transport = FindObjectOfType<UnityTransport>();
        await Authenticate();
    }

    private async Task Authenticate() {
        var options = new InitializationOptions();
        await UnityServices.InitializeAsync(options);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerId = AuthenticationService.Instance.PlayerId;
    }

    private void Start() {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    public async void CreateOrJoinLobby(int mode) {
        switch (mode) {
            case 0:
                connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
                break;
            case 1:
                connectedLobby = await CreateLobby();
                break;
            case 2:
                connectedLobby = await JoinLobby();
                break;
        }

        if (connectedLobby != null)
        {
            loobyCodeDisplay.text = connectedLobby.LobbyCode;

            lobbyCanvas.SetActive(false);
            preGameCanvas.SetActive(true);
            if (NetworkManager.Singleton.IsHost) startButton.SetActive(true);
            else readyButton.SetActive(true);
        }

    }

    private async Task<Lobby> QuickJoinLobby() {
        try {
            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

            SetTransformAsClient(a);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes("XIX");
            NetworkManager.Singleton.StartClient();
            return lobby;
        }
        catch (Exception e) {
            Debug.Log($"No lobbies available via quick join");
            return null;
        }
    }

    private async Task<Lobby> JoinLobby() {
        try {
            var lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(loobyCodeInput.text.ToUpper());

            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

            SetTransformAsClient(a);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes("DummyData");
            NetworkManager.Singleton.StartClient();
            return lobby;
        }
        catch (Exception e) {
            Debug.Log($"Lobby with code {loobyCodeInput.text.ToUpper()} doesn't exists");
            return null;
        }
    }

    private async Task<Lobby> CreateLobby() {
        try { 
            const int maxPlayers = 5;

            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            var options = new CreateLobbyOptions {
                Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };
            var lobby = await Lobbies.Instance.CreateLobbyAsync("Useless Lobby Name", maxPlayers, options);
            loobyCodeDisplay.text = lobby.LobbyCode;

            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost();
            return lobby;
        }
        catch (Exception e) {
            Debug.LogFormat($"Failed creating a lobby: {e}");
            return null;
        }
    }

    public void Quit()
    {
        LeaveLobby();
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().ChangeCameraToWorld();
        if (NetworkManager.Singleton.IsHost)
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        NetworkManager.Singleton.Shutdown();
    }

    public void LeaveLobby()
    {
        if (connectedLobby != null)
            try {
                StopAllCoroutines();

                if (connectedLobby.HostId == playerId) Lobbies.Instance.DeleteLobbyAsync(connectedLobby.Id);
                else {
                    Debug.Log($"Removing Player < {playerId} > \n from lobby: {connectedLobby.Id}");
                    Lobbies.Instance.RemovePlayerAsync(connectedLobby.Id, playerId);
                }
                
                connectedLobby = null;
                lobbyCanvas.SetActive(true);
                preGameCanvas.SetActive(false);
                startButton.SetActive(false);
                readyButton.SetActive(false);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
    }

    private void SetTransformAsClient(JoinAllocation a) {
        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
    }

    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds) {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true) {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnDestroy() { // provavelmente n precisa mais desse código
        try {
            if (NetworkManager.Singleton.IsHost)
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;

            StopAllCoroutines();
            // todo: add a check to see if you're host
            if(connectedLobby != null) {
                if (connectedLobby.HostId == playerId) Lobbies.Instance.DeleteLobbyAsync(connectedLobby.Id);
                else Lobbies.Instance.RemovePlayerAsync(connectedLobby.Id, playerId);
                connectedLobby = null;
            }
        }
        catch (Exception e) {
            Debug.Log($"Error shutting down lobby: {e}");
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

    private void HandleServerStarted() {
        if (NetworkManager.Singleton.IsHost)
            HandleClientConnected(NetworkManager.Singleton.LocalClientId);
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!playersInLobby.ContainsKey((int)clientId)) playersInLobby.Add((int)clientId, false);

        PropagateToClients();

        if (NetworkManager.Singleton.IsClient)
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().SetPlayerNameServerRpc(nicknameInput.text.ToUpper());
    }

    private void HandleClientDisconnect(ulong clientId) {
        // Handle locally
        if (playersInLobby.ContainsKey((int)clientId)) playersInLobby.Remove((int)clientId);

        // Propagate all clients
        RemovePlayerClientRpc(clientId);


        //Debug.Log($"Client {clientId} disconnected");

    }

    public void PropagateToClients()
    {
        foreach (var player in playersInLobby) {
            UpdatePlayerClientRpc((ulong)player.Key, player.Value);
        }
    }

    [ClientRpc]
    private void UpdatePlayerClientRpc(ulong clientId, bool isReady)
    {
        if (IsServer) return;

        if (!playersInLobby.ContainsKey((int)clientId)) playersInLobby.Add((int)clientId, isReady);
        else playersInLobby[(int)clientId] = isReady;
    }
    
    [ServerRpc]
    public void UpdatePlayerServerRpc(ulong clientId, bool isReady)
    {
        if (!playersInLobby.ContainsKey((int)clientId)) playersInLobby.Add((int)clientId, isReady);
        else playersInLobby[(int)clientId] = isReady;
    }


    [ClientRpc]
    private void RemovePlayerClientRpc(ulong clientId)
    {
        if (IsServer) return;

        if (playersInLobby.ContainsKey((int)clientId)) playersInLobby.Remove((int)clientId);
    }

}
