using Unity.Netcode;
using UnityEngine;

public class PlayerController : CharacterController
{
    void Update()
    {
        if (IsOwner)
        {
            PlayerMovement();
        }
    }

    private void PlayerMovement()
    {
        if (Input.GetKey(KeyCode.W)) Move(Direction.UP);
        else if (Input.GetKey(KeyCode.S)) Move(Direction.DOWN);
        
        if (Input.GetKey(KeyCode.A)) Move(Direction.LEFT);
        else if (Input.GetKey(KeyCode.D)) Move(Direction.RIGHT);

        if (Input.GetKey(KeyCode.Space)) Jump();

        if (Input.GetAxis("Mouse X") > 0) Rotate(Direction.RIGHT);
        else if (Input.GetAxis("Mouse X") < 0) Rotate(Direction.LEFT);
    }
}
