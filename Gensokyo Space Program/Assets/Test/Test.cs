using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] Vector3 RotationAxis;
    [SerializeField] float vector_length = 10f;
    Vector3 RotationAxis_old;
    Vector3 LocalAxis;
    GameObject surface;
    GameObject worldaxis;
    GameObject localaxis;
    Quaternion original;

    private void Start()
    {
        surface = gameObject.transform.Find("surface").gameObject;
        worldaxis = ObjectCreator.CreateSphere("Rotation Axis", 1, gameObject);
        localaxis = ObjectCreator.CreateSphere("Local Axis", 1, gameObject);

        original = surface.transform.rotation;
        //GameObject localaxis = ObjectCreator.CreateSphere("Local Axis", 1, surface);
        //localaxis.transform.position = gameObject.transform.position + 5 * Vector3.forward;
    }

    void Update()
    {
        if (RotationAxis != RotationAxis_old)
        {
            RotationAxis_old = RotationAxis;
            RotationAxis = RotationAxis.normalized;

            surface.transform.rotation = original;
            LocalAxis = Quaternion.FromToRotation(surface.transform.forward, gameObject.transform.forward) * RotationAxis;
            surface.transform.rotation = surface.transform.rotation * Quaternion.FromToRotation(gameObject.transform.forward, RotationAxis);
        }

        worldaxis.transform.position = gameObject.transform.position + vector_length * RotationAxis;
        localaxis.transform.position = gameObject.transform.position + vector_length * LocalAxis;
        surface.transform.Rotate(LocalAxis, -120 * Time.deltaTime);
    }
}