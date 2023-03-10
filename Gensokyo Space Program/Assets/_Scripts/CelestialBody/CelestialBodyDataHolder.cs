using _Scripts.CelestialBody.ScriptableObjects;
using UnityEngine;

namespace _Scripts.CelestialBody
{
    public class CelestialBodyDataHolder : MonoBehaviour
    {
        [field: SerializeField, Header("������Ϣ")]
        public CelestialBodyDataSO celestialBodyData;
        public GameObject orbitingObject;

        private void Start()
        {
            orbitingObject = GameObject.Find(celestialBodyData.OrbitingBodyName);
        }
    }
}
