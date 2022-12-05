using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Cannon : NetworkBehaviour
{

    [SerializeField] private NetworkObject ballPrefab;
    [SerializeField] private GameObject CannonTip;

    private void OnTriggerEnter(Collider other)
    {
        ShootServerRpc();
    }

    NetworkObject ballInstance;
    [ServerRpc]
    private void ShootServerRpc() { 
        ballInstance = Instantiate(ballPrefab, CannonTip.transform.position, Quaternion.identity, transform);

        ballInstance.Spawn();
        ShootClientRpc();
    }

    [ClientRpc]
    private void ShootClientRpc()
    {
        ballInstance.GetComponent<Rigidbody>().AddForce(CannonTip.transform.forward * 1000);
    }

}
