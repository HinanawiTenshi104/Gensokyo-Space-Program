using UnityEngine;

namespace _Scripts.Managers
{
    public class ScenesTracker : MonoBehaviour
    {
        public bool inStarMap;


        private void Start()
        {
            inStarMap = false;
        }
        private void Update()
        {
            if (Input.GetKeyDown("m"))
            {
                inStarMap = !inStarMap;
            }
        }
    }
}