using NetWork;
using PlayerInfo;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    CombatEntity entity;
    [SerializeField]
    PlayerInput input;
    [SerializeField]
    Rigidbody rb;
    Animator anim;
    Vector3 rota = Vector3.zero;
    public float rotaSpeed = 200;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        entity = GetComponent<CombatEntity>();
        input = new PlayerInput();
        input.EnableTable();
        input.Onmove += Move;
        input.Onstop += Stop;
        anim = GetComponent<Animator>();
    }

    private void Stop(Vector2 arg0)
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0) * entity.numberBox.Speed.Value;
        //rigidbody2d.velocity = transform.forward* speed;

    }
    private void Move(Vector2 arg0)
    {
        rb.velocity = new Vector3(0, 0, -arg0.y) * entity.numberBox.Speed.Value;
        rb.velocity = -transform.forward * entity.numberBox.Speed.Value * arg0.y;
    }
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
            rb.velocity = transform.forward * entity.numberBox.Speed.Value;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            //rigidbody2d.AddForce(-transform.forward * -speed);
            rb.velocity = -transform.forward * entity.numberBox.Speed.Value;
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
        transform.transform.rotation = Quaternion.Euler(rota);
        //Debug.Log(rb.velocity);

    }
}
