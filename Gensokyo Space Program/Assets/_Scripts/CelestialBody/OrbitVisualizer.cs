using System;
using UnityEngine;

//参考：https://www.youtube.com/watch?v=mQKGRoV_jBc

public class OrbitVisualizer : MonoBehaviour
{
    [Range(100, 10000), Header("拟合段数")]
    public int segments = 10000;
    [field: SerializeField, Header("LineRender材质")]
    public Material material;
    #region 本地变量
    private LineRenderer lineRenderer;
    private GameObject LineRendererObject;
    private GameObject orbitingCelestialBody;
    private CelestialBodyDataHolder celestialBodyDataHolder;
    private CelestialBodyDataSO celestialBodyData;

    private float OrbitalEccentricit;
    private double EtimesP;
    private float ArgumentOfPeriapsis;
    private Vector3 planeNormalVector;
    #endregion

    private void Start()
    {
        celestialBodyDataHolder = gameObject.GetComponent<CelestialBodyDataHolder>();
        celestialBodyData = celestialBodyDataHolder.celestialBodyData;
        if (material == null)
        {
            material = Resources.Load("Materials/OrbitIndicator", typeof(Material)) as Material;
        }
        #region LineRenderer设置
        LineRendererObject = new GameObject(celestialBodyData.CelestialBodyName + "轨道可视化");
        LineRendererObject.transform.parent = GameObject.Find("OrbitVisualizers").transform;
        LineRendererObject.AddComponent<LineRenderer>();
        lineRenderer = LineRendererObject.GetComponent<LineRenderer>();
        lineRenderer.material = material;
        lineRenderer.useWorldSpace = false;
        #endregion
        #region 轨道设置
        orbitingCelestialBody = celestialBodyDataHolder.OrbitingObject;
        OrbitalEccentricit = celestialBodyData.OrbitalEccentricit;
        EtimesP = celestialBodyData.EtimesP;
        ArgumentOfPeriapsis = celestialBodyData.ArgumentOfPeriapsis;
        planeNormalVector = celestialBodyData.PlaneNormalVector;
        #endregion

        LineRendererObject.transform.rotation = Quaternion.identity;
        OrbitalMechanicsFunctions.SetupOrbitVisuaizer(lineRenderer, segments, OrbitalEccentricit, EtimesP, ArgumentOfPeriapsis, planeNormalVector);
    }
    private void Update()
    {
        LineRendererObject.transform.position = orbitingCelestialBody.transform.position;
    }
}