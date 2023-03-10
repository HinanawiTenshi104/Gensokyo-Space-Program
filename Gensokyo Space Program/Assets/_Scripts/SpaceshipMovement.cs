using UnityEngine;

namespace GensokyoSpaceProgram._Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class SpaceshipMovement : MonoBehaviour
    {
        private Rigidbody _rigidbody;

        public float shipMass;
        public float enginePower;
        public float throttleChangeSpeed = 50; //每秒throttle可以改变多少
        public float throttle;
        public float pitchPower = 100; //每秒pitch可以改变多少度
        public float rollPower = 100;
        public float yawPower = 100;

        private void Awake()
        {
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.Log("飞船控制器未找到Rigidbody组件！");
            }

            _rigidbody.drag = 0;
            _rigidbody.mass = shipMass;
            _rigidbody.useGravity = false;
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(enginePower * throttle * transform.forward);

            float pitch = -Input.GetAxisRaw("Pitch") * pitchPower * Time.fixedDeltaTime;
            float roll = -Input.GetAxisRaw("Roll") * rollPower * Time.fixedDeltaTime;
            float yaw = Input.GetAxisRaw("Yaw") * yawPower * Time.fixedDeltaTime;

            _rigidbody.MoveRotation(_rigidbody.rotation * Quaternion.Euler(pitch, yaw, roll));
            if ((pitch==0)&&(roll==0)&&(yaw==0))//直走的时候可以向后飞
            {
                _rigidbody.velocity = Quaternion.FromToRotation(_rigidbody.velocity, transform.forward) * _rigidbody.velocity;
            }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) &&(throttle<100))
            {
                throttle += throttleChangeSpeed * Time.deltaTime;
                if (throttle > 100)
                {
                    throttle = 100;
                }
            }
            if (Input.GetKey(KeyCode.LeftControl) &&(throttle>-50))
            {
                throttle -= throttleChangeSpeed * Time.deltaTime;
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
                _rigidbody.velocity = Vector3.zero;
            }
        }
    }
}
