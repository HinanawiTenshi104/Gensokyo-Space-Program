using UnityEngine;

public class CelestialBodyDataHolder : MonoBehaviour
{
    [field: SerializeField, Header("天体信息")]
    public CelestialBodyDataSO celestialBodyData;
    public GameObject OrbitingObject;

    private void Start()
    {
        OrbitingObject = GameObject.Find(celestialBodyData.OrbitingBodyName);
    }
}
