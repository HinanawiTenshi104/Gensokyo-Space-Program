using UnityEngine;

namespace _Scripts.Managers
{
    public class TimeManager : MonoBehaviour
    {
        [SerializeField, Header("时间倍数"), Range(0, 10)]
        public float timeScale = 1;
        [SerializeField, Header("物理模拟dt(模拟精度)") ,Range(0.001f,0.02f)]
        public float physicsDeltaTime = 0.02f;

        private void Update()
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = physicsDeltaTime;
        }
    }
}
