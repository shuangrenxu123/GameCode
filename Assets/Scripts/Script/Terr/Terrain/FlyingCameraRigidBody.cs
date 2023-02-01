using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FlyingCameraRigidBody : MonoBehaviour
{
    private Rigidbody rb;

    public float movementSpeed = 20f;

    public float rotaSpeed = 300f;
    Vector3 rota = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.dKey.isPressed)
        {
            rota += new Vector3(0, 1, 0) * rotaSpeed * Time.deltaTime;
        }
        if (Keyboard.current.aKey.isPressed)
        {
            rota -= new Vector3(0, 1, 0) * rotaSpeed * Time.deltaTime;
        }
        if (Keyboard.current.wKey.isPressed)
        {
            //rigidbody2d.AddForce(-transform.forward * speed);
            rb.velocity = transform.forward * movementSpeed;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            //rigidbody2d.AddForce(-transform.forward * -speed);
            rb.velocity = -transform.forward * movementSpeed;
        }
        else if (Keyboard.current.spaceKey.isPressed)
        {
            rb.velocity = new Vector3(rb.velocity.x, movementSpeed, rb.velocity.z);
        }
        else if (Keyboard.current.leftShiftKey.isPressed)
        {
            rb.velocity = new Vector3(rb.velocity.x, -movementSpeed, rb.velocity.z);
        }
        else
        {
            rb.velocity = new Vector3(0, 0, 0);
        }
        transform.transform.rotation = Quaternion.Euler(rota);
    }
}
