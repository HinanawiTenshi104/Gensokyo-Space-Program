using _Scripts.CelestialBody.ScriptableObjects;
using UnityEngine;

namespace _Scripts.CelestialBody
{
    public class CelestialBodyDataHolder : MonoBehaviour
    {
        [field: SerializeField, Header("星球信息")]
        public CelestialBodyDataSO celestialBodyData;
        public GameObject orbitingObject;

        private void Start()
        {
            orbitingObject = GameObject.Find(celestialBodyData.OrbitingBodyName);
        }
    }
}
