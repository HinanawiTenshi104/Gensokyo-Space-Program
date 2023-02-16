using UnityEngine;

public class CelestialBodyDataHolder : MonoBehaviour
{
    [field: SerializeField, Header("������Ϣ")]
    public CelestialBodyDataSO celestialBodyData;
    public GameObject OrbitingObject;

    private void Start()
    {
        OrbitingObject = GameObject.Find(celestialBodyData.OrbitingBodyName);
    }
}
