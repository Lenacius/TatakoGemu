using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    [SerializeField] private const float SPEED = 2.0f;
    [SerializeField] private bool is_swimming = false;
    [SerializeField] private bool is_midair = false;

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

        if (is_swimming)
            player_movement /= 2;

        if (Input.GetKeyDown(KeyCode.Space) && !is_midair)
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(0, 200, 0));
            is_midair = true;
        }
            

        transform.position += player_movement * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
            is_swimming = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Water")
            is_swimming = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Ground")
            is_midair = false;
    }
}
