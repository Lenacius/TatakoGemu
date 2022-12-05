using Unity.Netcode;
using UnityEngine;

public class CharacterController : NetworkBehaviour
{
    [SerializeField] private float mov_speed = 2.0f;
    [SerializeField] private bool is_swimming = false;
    [SerializeField] private bool was_swimming = false;

    [SerializeField] private float rot_speed = 180.0f;

    [SerializeField] private bool is_jumping = false;

    [SerializeField] private Vector3 character_movement = new Vector3();

    public enum Direction { UP, DOWN, LEFT, RIGHT };

    public void Move(Direction direction)
    {
        character_movement = Vector3.zero;

        if(direction == Direction.UP)           character_movement -= this.transform.forward * mov_speed;
        else if(direction == Direction.DOWN)    character_movement += this.transform.forward * mov_speed;

        if (direction == Direction.LEFT)        character_movement += this.transform.right * mov_speed;
        else if (direction == Direction.RIGHT)  character_movement -= this.transform.right * mov_speed;

        if (is_swimming || was_swimming)
            character_movement /= 2;

        transform.position += character_movement * Time.deltaTime;
    }

    public void Rotate(Direction direction)
    {
        switch (direction)
        {
            case Direction.RIGHT:
                transform.Rotate(new Vector3(0, rot_speed * Time.deltaTime, 0));
            break;
            case Direction.LEFT:
                transform.Rotate(new Vector3(0, -rot_speed * Time.deltaTime, 0));
            break;
            default:
                // Do nothing
            break;
        }
    }

    public void Jump()
    {
        if (!is_jumping)
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(0, 250, 0));
            is_jumping = true;
        }
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
            is_jumping = false;
            was_swimming = false;
        }
    }

    public bool IsSwimming() {
        return is_swimming;
    }
}
