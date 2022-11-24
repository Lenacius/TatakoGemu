using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PositionRandomizer : NetworkBehaviour
{
    private void Awake()
    {
        NetworkObject player = NetworkManager.Singleton.LocalClient.PlayerObject;
        float x = 0;
        float y = 0;
        while(ValueInBound(x) && ValueInBound(y))
        {
            x = Random.Range(-30, 30);
            y = Random.Range(-30, 30);
        }
        player.transform.position = new Vector3(x, 0, y);
    }

    private bool ValueInBound(float x)
    {
        if (x <= 5 && x >= -5)
            return true;
        return false;
    }
}
