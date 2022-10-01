using Unity.Netcode;
using UnityEngine;

public class ParticleSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject particlePrefab;

    private void Update()
    {
        if (!IsOwner) { return; }

        if (!Input.GetKeyDown(KeyCode.Space)) { return; }

        SpawnParticleServerRpc();

        Instantiate(particlePrefab, transform.position, transform.rotation);
    }

    [ServerRpc(Delivery = RpcDelivery.Unreliable)] // Unreliable means that if some issue happens to the communication of the RPC the method is not executed
    private void SpawnParticleServerRpc()
    {
        SpawnParticleClientRpc();
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    private void SpawnParticleClientRpc()
    {
        if (IsOwner) { return; }

        Instantiate(particlePrefab, transform.position, transform.rotation);
    }
}
