using UnityEditor.PackageManager.UI;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceshipMovement : MonoBehaviour
{
    private Rigidbody rb;

    public float shipMass;
    public float enginePower;
    public float throttle_change_speed = 50; //ÿ��throttle���Ըı����
    public float throttle = 0;
    public float pitchPower = 100; //ÿ��pitch���Ըı���ٶ�
    public float rollPower = 100;
    public float yawPower = 100;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.Log("�ɴ�������δ�ҵ�Rigidbody�����");
        }

        rb.drag = 0;
        rb.mass = shipMass;
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        rb.AddForce(transform.forward * enginePower * throttle);

        float pitch = -Input.GetAxisRaw("Pitch") * pitchPower * Time.fixedDeltaTime;
        float roll = -Input.GetAxisRaw("Roll") * rollPower * Time.fixedDeltaTime;
        float yaw = Input.GetAxisRaw("Yaw") * yawPower * Time.fixedDeltaTime;

        rb.MoveRotation(rb.rotation * Quaternion.Euler(pitch, yaw, roll));
        if ((pitch==0)&&(roll==0)&&(yaw==0))//ֱ�ߵ�ʱ���������
        {
            rb.velocity = Quaternion.FromToRotation(rb.velocity, transform.forward) * rb.velocity;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) &&(throttle<100))
        {
            throttle += throttle_change_speed * Time.deltaTime;
            if (throttle > 100)
            {
                throttle = 100;
            }
        }
        if (Input.GetKey(KeyCode.LeftControl) &&(throttle>-50))
        {
            throttle -= throttle_change_speed * Time.deltaTime;
            if (throttle < -50)
            {
                throttle = -50;
            }
        }
        if (Input.GetKey("z"))
        {
            throttle = 100;
        }

        if (Input.GetKey("x"))
        {
            throttle = 0;
        }

        if (Input.GetKey("c"))
        {
            rb.velocity = Vector3.zero;
        }
    }
}
