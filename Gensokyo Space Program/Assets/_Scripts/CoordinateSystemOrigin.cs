using UnityEngine;

public class CoordinateSystemOrigin : MonoBehaviour
{
    public string CoordinateSystemName;
    public Vector3 Position;
    public Vector3 Velocity;

    private void FixedUpdate()
    {
        Position += Velocity * Time.fixedDeltaTime;
    }
    private void Update()
    {
        gameObject.transform.position = Position;
    }
}
