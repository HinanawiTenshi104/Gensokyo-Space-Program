using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField, Header("ʱ�䱶��"), Range(0, 10)]
    public float timeScale = 1;
    [SerializeField, Header("����ģ��dt(ģ�⾫��)") ,Range(0.001f,0.02f)]
    public float physicisDeltaTime = 0.02f;

    private void Update()
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = physicisDeltaTime;
    }
}
