using _Scripts.CelestialBody.Function;
using _Scripts.CelestialBody.ScriptableObjects;
using UnityEngine;

//参考：https://www.youtube.com/watch?v=mQKGRoV_jBc

namespace _Scripts.CelestialBody
{
    public class OrbitVisualizer : MonoBehaviour
    {
        [Range(100, 10000), Header("拟合段数")]
        public int segments = 10000;
        [field: SerializeField, Header("LineRender材质")]
        public Material material;
        #region 本地变量
        private LineRenderer _lineRenderer;
        private GameObject _lineRendererObject;
        private GameObject _orbitingCelestialBody;
        private CelestialBodyDataHolder _celestialBodyDataHolder;
        private CelestialBodyDataSO _celestialBodyDataSo;

        private float _orbitalEccentricity;
        private double _etimesP;
        private float _argumentOfPeriapsis;
        private Vector3 _planeNormalVector;
        #endregion

        private void Start()
        {
            _celestialBodyDataHolder = gameObject.GetComponent<CelestialBodyDataHolder>();
            _celestialBodyDataSo = _celestialBodyDataHolder.celestialBodyData;
            if (material == null)
            {
                material = Resources.Load("Materials/OrbitIndicator", typeof(Material)) as Material;
            }
            #region LineRenderer设置
            _lineRendererObject = new GameObject(_celestialBodyDataSo.CelestialBodyName + "轨道可视化");
            _lineRendererObject.transform.parent = GameObject.Find("OrbitVisualizers").transform;
            _lineRendererObject.AddComponent<LineRenderer>();
            _lineRenderer = _lineRendererObject.GetComponent<LineRenderer>();
            _lineRenderer.material = material;
            _lineRenderer.useWorldSpace = false;
            #endregion
            #region 轨道设置
            _orbitingCelestialBody = _celestialBodyDataHolder.orbitingObject;
            _orbitalEccentricity = _celestialBodyDataSo.OrbitalEccentricity;
            _etimesP = _celestialBodyDataSo.EtimesP;
            _argumentOfPeriapsis = _celestialBodyDataSo.ArgumentOfPeriapsis;
            _planeNormalVector = _celestialBodyDataSo.PlaneNormalVector;
            #endregion

            _lineRendererObject.transform.rotation = Quaternion.identity;
            OrbitalMechanicsFunctions.SetupOrbitVisuaizer(_lineRenderer, segments, _orbitalEccentricity, _etimesP, _argumentOfPeriapsis, _planeNormalVector);
        }
        private void Update()
        {
            _lineRendererObject.transform.position = _orbitingCelestialBody.transform.position;
        }
    }
}