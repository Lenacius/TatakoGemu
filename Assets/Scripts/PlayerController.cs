using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private const float SPEED = 1.0f;

    private Vector3 player_movement = new Vector3();

    private void Update()
    {
        if (IsOwner) { Move(); }
    }

    private void Move()
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

        transform.position += player_movement * Time.deltaTime;
    }
}
