using System;
using System.Collections.Generic;
using _Scripts.CelestialBody.Function;
using _Scripts.CelestialBody.ScriptableObjects;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace _Scripts.CelestialBody
{
    public class CelestialBodyManager : MonoBehaviour
    {
        #region 变量
        GameObject _sun;
        GameObject[] _planets;
        List<GameObject> _celestialBodies;//sun+planets
        CelestialBodyDataSO _sunData;
        List<CelestialBodyDataSO> _planetcbdatas = new List<CelestialBodyDataSO>();
        List<CelestialBodyDataSO> _celestialBodiescbdatas;
        List<GameObject> _surfaces;
        readonly List<Vector3> _localRotationAxes = new List<Vector3>();
        private List<float> _planetOriginalSize = new List<float>();
        #endregion
        #region 编辑器变量

        [NonSerialized] public bool ChangeData = true;
        [NonSerialized] public bool ResizePlanet = true;
        [NonSerialized] public bool UpdateMeanAnomaly = true;
        [NonSerialized] public bool ManagingOrbitMotion = true;
        [NonSerialized] public bool ManagingPlanetRotation = true;

        #endregion
        void Start()
        {
            #region 获取物体和数据
            _sun = GameObject.FindGameObjectWithTag("Sun");
            _planets = GameObject.FindGameObjectsWithTag("CelestialBody");
            _celestialBodies = new List<GameObject> { _sun };
            _celestialBodies.AddRange(_planets);

            _sunData = _sun.GetComponent<CelestialBodyDataHolder>().celestialBodyData;
            _planetcbdatas = OrbitalMechanicsFunctions.GetCelestialBodyDatas(_planets);
            _celestialBodiescbdatas = new List<CelestialBodyDataSO> { _sunData };
            _celestialBodiescbdatas.AddRange(_planetcbdatas);

            foreach (GameObject celestialBody in _celestialBodies)
            {
                _planetOriginalSize.Add(celestialBody.transform.localScale.x);
            }
            #endregion
            #region 星球位置初始化
            _sunData.Velocity = Vector3.zero;
            _sunData.Position = Vector3.zero;
            foreach (var cbdata in _planetcbdatas)
            {
                cbdata.MeanAnomaly = cbdata.MeanAnomalyAtT0;
            }
            #endregion

            if (ResizePlanet)
            {
                CelestialBodyResizer(true);
            }
            if (ManagingPlanetRotation)
            {
                #region 星球自转
                _surfaces = new List<GameObject>();
                for (int i = 0; i < _celestialBodies.Count; i++)
                {
                    Vector3 rotationAxis = _celestialBodiescbdatas[i].RotationAxis;

                    GameObject surface = _celestialBodies[i].transform.Find("Appearance").Find("Surface").gameObject;
                    if (surface == null)
                    {
                        Debug.Log("未能找到" + _celestialBodies[i].name +"的surface子物体");
                    }

                    _localRotationAxes.Add(Quaternion.FromToRotation(surface.transform.forward, _celestialBodies[i].transform.forward) * rotationAxis);  //因为transform.Rotate的Axis好像是local的，所以要把相对星球的自转轴改成相对地面贴图的自转轴
                    surface.transform.rotation = surface.transform.rotation * Quaternion.FromToRotation(Vector3.forward, rotationAxis); //把旋转后的地面贴图的[视觉上的Z轴]与自转轴对齐

                    _surfaces.Add(surface);
                }
                
                ////DEBUG 用
                //GameObject[] rotateaxesballs_world = new GameObject[celestialBodies.Count];
                //for (int i = 0; i < rotateaxesballs_world.Length; i++)
                //{
                //    rotateaxesballs_world[i] = ObjectCreator.CreateSphere(celestialBodies[i].name+" Rotation Axis", 100, celestialBodies[i]);
                //    rotateaxesballs_world[i].transform.position = celestialBodies[i].transform.position + 1000 * celestialBodiescbdatas[i].RotationAxis;
                //}
                //GameObject[] rotateaxesballs_local = new GameObject[celestialBodies.Count];
                //for (int i = 0; i < rotateaxesballs_local.Length; i++)
                //{
                //    rotateaxesballs_local[i] = ObjectCreator.CreateSphere(celestialBodies[i].name + " Local Rotation Axis", 100, celestialBodies[i].transform.Find("Appearance").Find("Surface").gameObject);
                //    rotateaxesballs_local[i].transform.position = celestialBodies[i].transform.position + 1000 * LocalRotationAxes[i];
                //}
                //GameObject[] surfaceforward = new GameObject[celestialBodies.Count];
                //for (int i = 0; i < surfaceforward.Length; i++)
                //{
                //    surfaceforward[i] = ObjectCreator.CreateSphere(celestialBodies[i].name + " Surface Forward Axis", 100, celestialBodies[i].transform.Find("Appearance").Find("Surface").gameObject);
                //    surfaceforward[i].transform.position = celestialBodies[i].transform.position + 1000 * Vector3.forward;
                //}
                ////#DEBUG 用
                #endregion
            }
        }
        void Update()
        {
            if (ChangeData)
            {
                ChangeData = false;
                if (ResizePlanet)
                {
                    CelestialBodyResizer(true);
                }
                else
                {
                    CelestialBodyResizer(false);
                }
            }

            if (UpdateMeanAnomaly)
            {
                UpdateMeanAnomalys();
            }
            if (ManagingOrbitMotion)
            {
                UpdatePositions();
                MoveCelestialBodys();
            }
            if (ManagingPlanetRotation)
            {
                RotateCelestialBodys();
            }
        }
        public void CelestialBodyResizer(bool resize)
        {
            if (resize)
            {
                for (int i = 0; i < _celestialBodies.Count; i++)
                {
                    _celestialBodies[i].transform.localScale = (float)(_celestialBodiescbdatas[i].MeanRadius * Constants.PlanetRadiusShrinkFactor) * Vector3.one;
                }
            }
            else
            {
                for (int i = 0; i < _celestialBodies.Count; i++)
                {
                    _celestialBodies[i].transform.localScale = _planetOriginalSize[i] * Vector3.one;
                }
            }
        }
        public void UpdateMeanAnomalys()
        {
            for (int i = 0; i < _planets.Length; i++)
            {
                //Debug.Log("planet " + planets[i].name + "'s MeanAnomaly updating!");
                OrbitalMechanicsFunctions.UpdateMeanAnomaly(_planetcbdatas[i], Time.time);
            }
        }
        public void UpdatePositions()
        {
            for (int i = 0; i < _planets.Length; i++)
            {
                //Debug.Log("planet " + planets[i].name + "'s Position updating!");
                _planetcbdatas[i].Position = OrbitalMechanicsFunctions.CalculatePlanetWorldPosition(_planets[i]);
            }
        }
        public void MoveCelestialBodys()
        {
            for (int i = 0; i < _planets.Length; i++)
            {
                //Debug.Log("Moving planet " + planets[i].name + "!");
                _planets[i].transform.position = _planetcbdatas[i].Position;
            }
        }
        public void RotateCelestialBodys()
        {
            for (int i = 0; i < _celestialBodies.Count; i++)
            {
                double siderealRotationPeriod = _celestialBodiescbdatas[i].SiderealRotationPeriod;
                _surfaces[i].transform.Rotate(_localRotationAxes[i], (float)(-360f / (siderealRotationPeriod * Constants.TShrinkFactor) * Time.deltaTime));
                //Debug.Log(celestialBodies[i].name + " Rotated!\nRotated by " + (float)(-360f / (SiderealRotationPeriod * Constants.TShrinkFactor) * Time.deltaTime) + "degree!");
            }
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(CelestialBodyManager))]
    [CanEditMultipleObjects]
    class CelestialBodyManagerEditor : Editor
    {
        private float _temp;
        public override void OnInspectorGUI()
        {
            CelestialBodyManager celestialBodyManager = (CelestialBodyManager)target;
            if (celestialBodyManager == null) return;

            DrawDefaultInspector();
            
            EditorGUI.BeginChangeCheck();
            
            _temp = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            
            celestialBodyManager.ResizePlanet = EditorGUILayout.Toggle("按照Data重设星球大小",
                celestialBodyManager.ResizePlanet);
            celestialBodyManager.UpdateMeanAnomaly = EditorGUILayout.Toggle("更新平进角点",
                celestialBodyManager.UpdateMeanAnomaly);
            celestialBodyManager.ManagingOrbitMotion = EditorGUILayout.Toggle("管理轨道运动",
                celestialBodyManager.ManagingOrbitMotion);
            celestialBodyManager.ManagingPlanetRotation = EditorGUILayout.Toggle("管理星球自转",
                celestialBodyManager.ManagingPlanetRotation);

            EditorGUIUtility.labelWidth = _temp;
            
            if (EditorGUI.EndChangeCheck())
            {
                celestialBodyManager.ChangeData = true;
            }
        }
    }
#endif
}