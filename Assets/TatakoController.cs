using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TatakoController : NetworkBehaviour
{
    [SerializeField] private GameObject tatakoCenter;
    [SerializeField] private NetworkSceneController sceneController;
    [SerializeField] private LobbyController lobbyController;

    private void Start()
    {
        sceneController = GameObject.Find("NetworkSceneController").GetComponent<NetworkSceneController>();
        lobbyController = GameObject.Find("LobbyController").GetComponent<LobbyController>();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        this.tatakoCenter.transform.Rotate(new Vector3(0, 10 * Time.deltaTime, 0));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsHost)
            sceneController.GotoEnding();
        lobbyController.LeaveLobby();
        DestroyObjectServerRpc();
    }

    [ServerRpc]
    private void DestroyObjectServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
