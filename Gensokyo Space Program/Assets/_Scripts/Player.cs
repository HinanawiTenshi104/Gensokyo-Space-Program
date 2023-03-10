using UnityEngine;

namespace GensokyoSpaceProgram
{
    public class Player : MonoBehaviour
    {
        public Vector3 playerPosition;
        public Vector3 playerVelocity;

        public bool playerState;//目前只有在飞船模式(true)和不在飞船模式(false)

        private Rigidbody _rigidbody;
        private void Start()
        {
            playerState = true;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (playerState)
            {
                playerPosition = _rigidbody.position;
                playerVelocity = _rigidbody.velocity;
            }
        }
    }
}
