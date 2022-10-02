using Unity.Netcode;
using UnityEngine;

public class BallSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject ballPrefab;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        if (!Input.GetMouseButtonDown(0)) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }

        SpawnBallServerRpc(hit.point);
    }

    [ServerRpc]
    private void SpawnBallServerRpc(Vector3 spawnPos)
    {
        NetworkObject ballInstance = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

        ballInstance.SpawnWithOwnership(OwnerClientId);
    }
}
