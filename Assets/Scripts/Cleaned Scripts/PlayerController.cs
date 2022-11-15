using Unity.Netcode;
using UnityEngine;

public class PlayerController : CharacterController
{
    [SerializeField] private NetworkVariable<bool> is_ready = new NetworkVariable<bool>(false);

    private void Start()
    {
        if (IsOwner) ChangeCameraToPlayer();
    }

    void Update()
    {
        if (IsOwner) PlayerMovement();
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

    private void ChangeCameraToPlayer()
    {
        Transform main_camera = GameObject.Find("Main Camera").transform;
        main_camera.parent = transform;
        main_camera.localPosition = new Vector3(0, 6, 4);
        main_camera.localRotation = Quaternion.Euler(new Vector3(45, -180, 0));
    }

    public void ChangeCameraToWorld()
    {
        Transform main_camera = GameObject.Find("Main Camera").transform;
        main_camera.parent = null;
        main_camera.localPosition = new Vector3(0, 6, 4);
        main_camera.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }

}
