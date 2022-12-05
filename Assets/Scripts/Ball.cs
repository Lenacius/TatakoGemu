using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        if(!IsServer) { return; }
    }


    [ServerRpc]
    private void DestroyBallServerRpc()
    {
        //GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }


}
