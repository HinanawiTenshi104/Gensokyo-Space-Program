using UnityEngine;

namespace GensokyoSpaceProgram._Scripts
{
    public class CoordinateSystemOrigin : MonoBehaviour
    {
        public string coordinateSystemName;
        public Vector3 position;
        public Vector3 velocity;

        private void FixedUpdate()
        {
            position += velocity * Time.fixedDeltaTime;
        }
        private void Update()
        {
            gameObject.transform.position = position;
        }
    }
}
