using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    [SerializeField] private const float SPEED = 2.0f;
    [SerializeField] private bool is_swimming = false;
    [SerializeField] private bool was_swimming = false;
    [SerializeField] private bool is_midair = false;

    [SerializeField] private const float ANG_SPEED = 180.0f;

    private Vector3 player_movement = new Vector3();

    private void Update()
    {
        if (IsOwner) {

            //Cursor.lockState = CursorLockMode.Locked;

            Move();
            Rotate();
        }
    }

    private void Move()
    {
        player_movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            player_movement += -this.transform.forward * SPEED;
        else if (Input.GetKey(KeyCode.S))
            player_movement += this.transform.forward * SPEED;

        if (Input.GetKey(KeyCode.A))
            player_movement += this.transform.right * SPEED;
        else if (Input.GetKey(KeyCode.D))
            player_movement += -this.transform.right * SPEED;

        if (is_swimming || was_swimming)
            player_movement /= 2;

        if (Input.GetKeyDown(KeyCode.Space) && !is_midair)
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(0, 200, 0));
            is_midair = true;
        }
            

        transform.position += player_movement * Time.deltaTime;
    }

    private void Rotate()
    {

        if(Input.GetAxis("Mouse X") > 0) transform.Rotate(new Vector3(0, ANG_SPEED * Time.deltaTime, 0));
        else if(Input.GetAxis("Mouse X") < 0) transform.Rotate(new Vector3(0, -ANG_SPEED * Time.deltaTime, 0));

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
            is_swimming = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Water")
        {
            is_swimming = false;
            was_swimming = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Ground")
        {
            is_midair = false;
            was_swimming = false;
        }
    }
}
