using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;

public class PositionRandomizer : MonoBehaviour
{
    [SerializeField] private GameObject portal;

    private async void Awake()
    {
        NetworkObject player = NetworkManager.Singleton.LocalClient.PlayerObject;
        float x = 0;
        float y = 0;
        while(ValueInBound(x) && ValueInBound(y))
        {
            x = Random.Range(-15, 15);
            y = Random.Range(-15, 15);
        }
        Debug.LogWarning($"Character position V3({x}, 0, {y})");
        player.transform.position = new Vector3(x, 0, y);

        await StartPortal();
    }

    private bool ValueInBound(float x)
    {
        if (x <= 13 && x >= -13)
            return true;
        return false;
    }

    private async Task StartPortal()
    {
        Debug.LogWarning("Awaiting portal");
        await Task.Delay(5000);
        portal.SetActive(true);
    }
}
