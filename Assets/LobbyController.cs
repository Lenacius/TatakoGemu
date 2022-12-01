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

public class LobbyController : MonoBehaviour
{
    [SerializeField] private GameObject lobbyCanvas;
    [SerializeField] private GameObject preGameCanvas;
    [SerializeField] private TextMeshProUGUI loobyCodeDisplay;
    [SerializeField] private TMP_InputField loobyCodeInput;
    [SerializeField] private TMP_InputField nicknameInput;

    private Lobby connectedLobby;
    private QueryResponse lobbies;
    private UnityTransport transport;
    private const string JoinCodeKey = "j";
    private string playerId;

    private void Start() {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        throw new NotImplementedException(); // add a verification if it is host
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsClient)
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().SetPlayerNameServerRpc(nicknameInput.text.ToUpper());
    }

    private void HandleServerStarted() {
        if (NetworkManager.Singleton.IsHost)
            HandleClientConnected(NetworkManager.Singleton.LocalClientId);
    }

    private async void Awake() {
        await Authenticate();
        transport = FindObjectOfType<UnityTransport>();
    }

    public async void CreateOrJoinLobby(int mode) {
        switch (mode)
        {
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
            preGameCanvas.SetActive(true);
            lobbyCanvas.SetActive(false);
        }

    }

    private async Task Authenticate() {
        var options = new InitializationOptions();

        await UnityServices.InitializeAsync(options);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerId = AuthenticationService.Instance.PlayerId;
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
            var lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(loobyCodeInput.text.ToUpper());//Change to lobby code

            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

            SetTransformAsClient(a);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes("XIX");
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
            loobyCodeDisplay.text = joinCode;

            var options = new CreateLobbyOptions {
                Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };
            var lobby = await Lobbies.Instance.CreateLobbyAsync("Useless Lobby Name", maxPlayers, options);

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

    private void OnDestroy() {
        try {
            StopAllCoroutines();
            // todo: add a check to see if you're host
            if(connectedLobby != null) {
                if (connectedLobby.HostId == playerId) Lobbies.Instance.DeleteLobbyAsync(connectedLobby.Id);
                else Lobbies.Instance.RemovePlayerAsync(connectedLobby.Id, playerId);
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
}
