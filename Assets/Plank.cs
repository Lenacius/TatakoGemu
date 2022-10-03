using Unity.Netcode;
using UnityEngine;

public class Plank : NetworkBehaviour
{
    void Update()
    {
        GetComponent<Rigidbody>().isKinematic = true;
    }

}
