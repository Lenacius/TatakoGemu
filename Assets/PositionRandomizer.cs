using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PositionRandomizer : NetworkBehaviour
{
    private void Awake()
    {
        NetworkObject player = NetworkManager.Singleton.LocalClient.PlayerObject;
        float x = Random.Range(-30, 30);
        float y = Random.Range(-30, 30);
        player.transform.position = new Vector3(x, 0, y);
    }
}
