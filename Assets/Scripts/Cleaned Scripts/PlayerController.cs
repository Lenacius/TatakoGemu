using Unity.Netcode;
using UnityEngine;

public class PlayerController : CharacterController
{
    public override void OnNetworkSpawn()
    {
        //base.OnNetworkSpawn();
        DeleteNonPlayerCameras();
    }

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

    [SerializeField] private Camera player_camera;
    private void DeleteNonPlayerCameras()
    {
        GetPlayerCamera();
        foreach(Camera camera in Camera.allCameras)
        {
            if (camera != player_camera)
                Destroy(camera.gameObject);
        }
    }

    private void GetPlayerCamera()
    {
        player_camera = GetComponentInChildren<Camera>();
    }
}
