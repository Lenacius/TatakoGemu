using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private const float SPEED = 1.0f;

    private Vector3 player_movement = new Vector3();

    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    private void Update()
    {
        if(IsClient)
            MoveServerRpc();

        transform.position = Position.Value;
    }

    [ServerRpc]
    private void MoveServerRpc(ServerRpcParams rpcParams = default)
    {
        player_movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            player_movement += new Vector3(0f, 0f, SPEED);
        else if(Input.GetKey(KeyCode.S))
            player_movement += new Vector3(0f, 0f, -SPEED);

        if (Input.GetKey(KeyCode.A))
            player_movement += new Vector3(-SPEED, 0f, 0f);
        else if (Input.GetKey(KeyCode.D))
            player_movement += new Vector3(SPEED, 0f, 0f);

        Position.Value += player_movement * Time.deltaTime;
    }
}
