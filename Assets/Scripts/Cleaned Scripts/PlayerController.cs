using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;

public class PlayerController : CharacterController
{
    [SerializeField] public NetworkVariable<bool> is_ready = new NetworkVariable<bool>(false);
    [SerializeField] public List<bool> client_ready;
    [SerializeField] public List<ulong> client_id;

    public GameObject ready;
    private void Start()
    {
        if (IsOwner)
        {
            ChangeCameraToPlayer();
            if (IsClient && !IsHost)
            {
                ready = GameObject.Find("ReadyButton");
                ready.GetComponent<Button>().onClick.AddListener(TooglePlayerReadyServerRpc);
            }
        }
    }


    void Update()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1",  // The IP address is a string
                                                                                    (ushort)12345, // The port number is an unsigned short
                                                                                    "0.0.0.0" // The server listen address is a string.
                                                                                    );
        Debug.Log(NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address);
        Debug.Log(NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port);
        Debug.Log(NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.ServerListenAddress);

        //Debug.Log(NetworkManager.Singleton.GetComponent<UnityTransport>().);
        //Debug.Log(NetworkManager.Singleton.GetComponent<UnityTransport>().ServerListenPort);

        if (IsHost)
        {
            foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClients.Values)
            {
                if (!client_id.Contains(client.ClientId))
                {
                    client_id.Add(client.ClientId);
                    client_ready.Add(client.PlayerObject.GetComponent<PlayerController>().is_ready.Value);
                }
                else
                {
                    client_ready[client_id.IndexOf(client.ClientId)] = client.PlayerObject.GetComponent<PlayerController>().is_ready.Value;
                }
            }
        }
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

    [ServerRpc(RequireOwnership = false)]
    public void TooglePlayerReadyServerRpc()
    {
            is_ready.Value = !is_ready.Value;
    }


}
