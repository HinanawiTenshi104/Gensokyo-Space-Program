using System;
using System.Linq;
using _Scripts.CelestialBody.Function;
using _Scripts.CelestialBody.ScriptableObjects;
using _Scripts.GeneralFunctions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace _Scripts.CelestialBody
{

//升降交点计算参考：https://zh.wikipedia.org/wiki/%E5%8D%87%E4%BA%A4%E9%BB%9E%E9%BB%83%E7%B6%93
//赤经赤纬参考：https://image.baidu.com/search/detail?ct=503316480&z=0&ipn=d&word=%E8%B5%A4%E7%BA%AC&step_word=&hs=0&pn=6&spn=0&di=7169026086108397569&pi=0&rn=1&tn=baiduimagedetail&is=0%2C0&istype=0&ie=utf-8&oe=utf-8&in=&cl=2&lm=-1&st=undefined&cs=1214928430%2C2017643492&os=2337508916%2C3410988845&simid=1214928430%2C2017643492&adpicid=0&lpn=0&ln=1255&fr=&fmq=1675245236488_R&fm=&ic=undefined&s=undefined&hd=undefined&latest=undefined&copyright=undefined&se=&sme=&tab=0&width=undefined&height=undefined&face=undefined&ist=&jit=&cg=&bdtype=0&oriquery=&objurl=https%3A%2F%2Finews.gtimg.com%2Fnewsapp_bt%2F0%2F7906124687%2F1000&fromurl=ippr_z2C%24qAzdH3FAzdH3Fgjo_z%26e3Bqq_z%26e3Bv54AzdH3F54gAzdH3Fda8lanamAzdH3Fda8lanamA8FTmP_z%26e3Bip4s%3Frv&gsm=1e&rpstart=0&rpnum=0&islist=&querylist=&nojc=undefined&dyTabStr=MCwyLDMsNiw0LDEsNSw3LDgsOQ%3D%3D

    public class CelestialBodyDataEditor : MonoBehaviour
    {
        #region 天体本身的信息
        [NonSerialized] public GameObject OrbitingCelestialBody;
        [NonSerialized] public GameObject CelestialBody;
        [NonSerialized] public CelestialBodyDataSO OrbitingCelestialBodyData;
        [NonSerialized] public CelestialBodyDataSO CelestialBodyData;
        #endregion
        #region 星球轨道特性
        [NonSerialized] public string OrbitingBodyName;
        [NonSerialized] public double Apoapsis; //meter
        [NonSerialized] public double Periapsis; //meter
        [NonSerialized] public double SemiMajorAxis; //meter
        [NonSerialized] public float OrbitalEccentricity;
        [NonSerialized] public double EtimesP;
        [NonSerialized] public float OrbitalInclination;//degree
        [NonSerialized] public float ArgumentOfPeriapsis;//degree
        [NonSerialized] public float LongitudeOfTheAscendingNode;//degree
        [NonSerialized] public Vector3 planeVector;
        [NonSerialized] public Vector3 AscendingNode;
        [NonSerialized] public Vector3 DescendingNode;
        [NonSerialized] public double OrbitalPeriod; //second
        [NonSerialized] public double OrbitalAngularMomentum;   //kg*m^2*s^-1
        [NonSerialized] public double SpecificAngularMomentum;  //m^2*s^-1
        [NonSerialized] public double OrbitalMechanicalEnergy;  //kg*m^2*s^-2
        [NonSerialized] public float TrueAnomalyAtT0; //degree
        [NonSerialized] public float MeanAnomalyAtT0; //degree
        [NonSerialized] public float TrueAnomaly;//rad
        [NonSerialized] public float EccentricAnomaly;//rad
        [NonSerialized] public float MeanAnomaly;//rad
        #endregion
        #region 星球物理特性
        [NonSerialized] public double MeanRadius; //meter
        [NonSerialized] public double Mass; //kg
        [NonSerialized] public double StandardGravitationalParameter; //m^3*s^-2
        [NonSerialized] public double Density;
        [NonSerialized] public float Obliquity; //degree
        [NonSerialized] public float ObliquityLongitude; //degree
        [NonSerialized] public Vector3 RotationAxis;
        [NonSerialized] private Vector3 RotationAxis_old;
        [NonSerialized] public double SiderealRotationPeriod; //second
        [NonSerialized] public double SphereOfInfluence; //meter
        #endregion
        #region 编辑器设置
        [NonSerialized] public float BallSize = 5;
        [NonSerialized] public float PlaneVectorLength = 200;
        [NonSerialized] public float RotationAxisLength = 50;
        [NonSerialized] public float OrbitalPlaneWidth = 500;
        [NonSerialized] public float OrbitalPlaneHeight = 500;
        [NonSerialized] public int Segments = 10000;
        [NonSerialized] public Material OrbitalPlaneMaterial;
        [NonSerialized] public Material ADNodeMaterial;
        [NonSerialized] public Material LineRendererMaterial;
        #endregion
        #region 私有变量定义
        [NonSerialized] public bool ChangeData = true;
        [NonSerialized] public bool IsCenterBody = false;
        [NonSerialized] public bool UseApPe = true;
        [NonSerialized] public bool UseTAat0 = true;
        [NonSerialized] public GameObject EquatorialPlane;
        [NonSerialized] public GameObject Longitude0Plane;
        [NonSerialized] public GameObject Longitude90Plane;
        private double centralBodySGP;
        LineRenderer _lineRenderer;
        GameObject _lineRendererObject;
        private GameObject _orbitalPlane;
        private GameObject _ascendingNodeObject;
        private GameObject _descendingNodeObject;
        private GameObject _satellite;
        private GameObject _planeVectorObject;
        private GameObject _rotationAxisObject;
        #endregion

        private void Awake()
        {
            CelestialBodyUpdate();
            Apoapsis = 1;
            Periapsis = 1;
            SiderealRotationPeriod = 1;
        }
        private void Start()
        {
            #region null值处理
            if (OrbitalPlaneMaterial == null)
            {
                OrbitalPlaneMaterial = Resources.Load("Materials/OrbitalPlane", typeof(Material)) as Material;
            }
            if (ADNodeMaterial == null)
            {
                ADNodeMaterial = Resources.Load("Materials/ADNode", typeof(Material)) as Material;
            }
            if (LineRendererMaterial == null)
            {
                LineRendererMaterial = Resources.Load("Materials/OrbitIndicator", typeof(Material)) as Material;
            }
            #endregion
            #region LineRenderer初始化
            _lineRendererObject = new GameObject("轨道可视化");
            _lineRendererObject.transform.parent = gameObject.transform;
            _lineRendererObject.AddComponent<LineRenderer>();
            _lineRenderer = _lineRendererObject.GetComponent<LineRenderer>();
            _lineRenderer.material = LineRendererMaterial;
            _lineRenderer.useWorldSpace = false;
            #endregion
            #region 各种平面初始化
            _orbitalPlane = ObjectCreator.CreatePlane("轨道平面", OrbitalPlaneWidth, OrbitalPlaneHeight, OrbitalPlaneMaterial);
            _orbitalPlane.hideFlags = HideFlags.DontSave;
            _orbitalPlane.transform.parent = gameObject.transform;
            EquatorialPlane = ObjectCreator.CreatePlane("赤道平面", OrbitalPlaneWidth, OrbitalPlaneHeight, OrbitalPlaneMaterial);
            EquatorialPlane.hideFlags = HideFlags.DontSave;
            EquatorialPlane.transform.parent = gameObject.transform;
            EquatorialPlane.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.forward);
            Longitude0Plane = ObjectCreator.CreatePlane("0经度平面", OrbitalPlaneWidth, OrbitalPlaneHeight, OrbitalPlaneMaterial);
            Longitude0Plane.hideFlags = HideFlags.DontSave;
            Longitude0Plane.transform.parent = gameObject.transform;
            Longitude0Plane.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.down);
            Longitude90Plane = ObjectCreator.CreatePlane("90经度平面", OrbitalPlaneWidth, OrbitalPlaneHeight, OrbitalPlaneMaterial);
            Longitude90Plane.hideFlags = HideFlags.DontSave;
            Longitude90Plane.transform.parent = gameObject.transform;
            Longitude90Plane.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.right);
            #endregion
            #region 各种物体初始化
            _ascendingNodeObject = ObjectCreator.CreateSphere("升交点", 1, gameObject, ADNodeMaterial);
            _descendingNodeObject = ObjectCreator.CreateSphere("降交点", 1, gameObject, ADNodeMaterial);
            _satellite = ObjectCreator.CreateSphere("小卫星", 1, gameObject);
            _planeVectorObject = ObjectCreator.CreateSphere("平面向量", 1, gameObject);
            _rotationAxisObject = ObjectCreator.CreateSphere("自转轴向量", 1, gameObject);
            #endregion
        }
        public void Update()
        {
            if (ChangeData)
            {
                ChangeData = false;
                SubUpdate();
            }

            if (CelestialBody != null && (OrbitingCelestialBody != null || IsCenterBody))
            {
                #region 设置各种物体的位置和大小

                if (!IsCenterBody)
                {
                    Vector3 orbitingBodyPosition = OrbitingCelestialBody.transform.position;

                    _orbitalPlane.transform.position = orbitingBodyPosition;
                    EquatorialPlane.transform.position = orbitingBodyPosition;
                    Longitude0Plane.transform.position = orbitingBodyPosition;
                    Longitude90Plane.transform.position = orbitingBodyPosition;
                    _ascendingNodeObject.transform.position = orbitingBodyPosition + AscendingNode;
                    _descendingNodeObject.transform.position = orbitingBodyPosition + DescendingNode;
                    _planeVectorObject.transform.position = orbitingBodyPosition + planeVector * PlaneVectorLength;

                    _ascendingNodeObject.transform.localScale = BallSize * Vector3.one;
                    _descendingNodeObject.transform.localScale = BallSize * Vector3.one;
                    _planeVectorObject.transform.localScale = BallSize * Vector3.one;

                    if (EtimesP != 0)
                    {
                        _satellite.transform.position = OrbitingCelestialBody.transform.position +
                                                       OrbitalMechanicsFunctions.TranslatePoints(
                                                           OrbitalMechanicsFunctions.CalculatePointOnTheEllipse(
                                                               OrbitalEccentricity, EtimesP, ArgumentOfPeriapsis,
                                                               TrueAnomaly), planeVector.normalized);
                    }
                }

                _rotationAxisObject.transform.position =
                    CelestialBody.transform.position + RotationAxis * RotationAxisLength;
                if (SiderealRotationPeriod != 0)
                {
                    CelestialBody.transform.Rotate(Vector3.forward,
                        (float)(-360f / SiderealRotationPeriod * Time.deltaTime));
                }

                #endregion
            }
        }
        private void CelestialBodyUpdate()
        {
            if (CelestialBodyData != null)
            {
                GameObject[] celestialBodys = GameObject.FindGameObjectsWithTag("CelestialBody")
                    .Concat(GameObject.FindGameObjectsWithTag("Sun")).ToArray();
                foreach (GameObject tempcb in celestialBodys)
                {

                    if (tempcb.name == CelestialBodyData.name)
                    {
                        CelestialBody = tempcb;
                    }
                    else if (!IsCenterBody)
                    {
                        if (tempcb.name == OrbitingCelestialBodyData.name)
                        {
                            OrbitingCelestialBody = tempcb;
                        }
                    }
                    else
                    {
                        if (!tempcb.CompareTag("Sun"))
                        {
                            tempcb.SetActive(false); //暂时让其他物体失效，让界面干净一点
                        }
                    }
                }

                #region Null值警告

                if (!IsCenterBody)
                {
                    if (OrbitingCelestialBody == null)
                    {
                        Debug.Log("轨道编辑器未能找到中心物体！");
                        return;
                    }
                }
                if (CelestialBody == null)
                {
                    Debug.Log("轨道编辑器未能找到需要修改的天体！");
                    return;
                }

                #endregion

                GameObject temp = GameObject.Find("CelestialBodyManager");
                if (temp != null)
                {
                    temp.SetActive(false);
                }

                OrbitVisualizer temp2 = CelestialBody.GetComponent<OrbitVisualizer>();
                if (temp2 != null)
                {
                    temp2.enabled = false;
                }
            }
        }
        private void SubUpdate()
        {
            CelestialBodyUpdate();
            if (CelestialBody != null && (OrbitingCelestialBody != null || IsCenterBody))
            {
                if (!IsCenterBody)
                {
                    #region 轨道信息计算

                    OrbitingBodyName = OrbitingCelestialBody.name;
                    centralBodySGP = OrbitingCelestialBodyData.StandardGravitationalParameter;

                    #region 轨道平面轴的计算

                    planeVector = new Vector3(
                        -(float)(Math.Cos(LongitudeOfTheAscendingNode * Mathf.Deg2Rad) *
                                 Math.Sin(OrbitalInclination * Mathf.Deg2Rad)),
                        (float)(Math.Sin(LongitudeOfTheAscendingNode * Mathf.Deg2Rad) *
                                Math.Sin(OrbitalInclination * Mathf.Deg2Rad)),
                        (float)Math.Cos(OrbitalInclination * Mathf.Deg2Rad));
                    _orbitalPlane.transform.rotation = Quaternion.FromToRotation(Vector3.forward, planeVector);

                    #endregion

                    #region 升降交点的计算

                    //升降交点就在轨道平面的旋转轴上，所以不能用TranslatePoints();
                    float angle;
                    if (OrbitalInclination == 0)
                    {
                        angle = 0;
                    }
                    else
                    {
                        if (planeVector.x <= 0) //看看经度相对于x轴有没有相差大于180°（垃圾Angle函数）
                        {
                            angle = 2 * Mathf.PI -
                                    Vector3.Angle(Vector3.right, new Vector3(-planeVector.y, planeVector.x, 0)) *
                                    Mathf.Deg2Rad;
                        }
                        else
                        {
                            angle = Vector3.Angle(Vector3.right, new Vector3(-planeVector.y, planeVector.x, 0)) *
                                    Mathf.Deg2Rad;
                        }
                    }

                    AscendingNode =
                        OrbitalMechanicsFunctions.CalculatePointOnTheEllipse(OrbitalEccentricity, EtimesP,
                            ArgumentOfPeriapsis, angle);
                    DescendingNode = OrbitalMechanicsFunctions.CalculatePointOnTheEllipse(OrbitalEccentricity, EtimesP,
                        ArgumentOfPeriapsis, angle + Mathf.PI); //降交点一定在升交点对面(也就是+pi)

                    #endregion

                    #region 轨道形状计算

                    if (UseApPe)
                    {
                        SemiMajorAxis = (Apoapsis + Periapsis) / 2.0f;
                        OrbitalEccentricity = (float)((SemiMajorAxis - Periapsis) / SemiMajorAxis);
                    }
                    else
                    {
                        Periapsis = SemiMajorAxis * (1 - OrbitalEccentricity);
                        Apoapsis = 2 * SemiMajorAxis - Periapsis;
                    }

                    EtimesP = SemiMajorAxis * (1 - Math.Pow(OrbitalEccentricity, 2));

                    #endregion

                    #region 轨道位置计算

                    if (UseTAat0)
                    {
                        TrueAnomaly = TrueAnomalyAtT0 * Mathf.Deg2Rad;
                        EccentricAnomaly =
                            OrbitalMechanicsFunctions.TAnomalyToEAnomaly(OrbitalEccentricity, TrueAnomaly);
                        MeanAnomaly =
                            OrbitalMechanicsFunctions.EAnomalyToMAnomaly(OrbitalEccentricity, EccentricAnomaly);
                        MeanAnomalyAtT0 = MeanAnomaly * Mathf.Rad2Deg;
                    }
                    else
                    {
                        MeanAnomaly = MeanAnomalyAtT0 * Mathf.Deg2Rad;
                        EccentricAnomaly =
                            OrbitalMechanicsFunctions.MAnomalyToEAnomaly(OrbitalEccentricity, MeanAnomaly);
                        TrueAnomaly =
                            OrbitalMechanicsFunctions.EAnomalyToTAnomaly(OrbitalEccentricity, EccentricAnomaly);
                        TrueAnomalyAtT0 = TrueAnomaly * Mathf.Rad2Deg;
                    }

                    #endregion

                    #region 其他计算

                    OrbitalPeriod = 2 * Math.PI * SemiMajorAxis * Math.Sqrt(SemiMajorAxis / centralBodySGP);
                    SpecificAngularMomentum = Math.Sqrt(EtimesP * centralBodySGP);
                    OrbitalAngularMomentum = SpecificAngularMomentum * Mass;
                    OrbitalMechanicalEnergy =
                        (Mathf.Pow(OrbitalEccentricity, 2) - 1) * centralBodySGP * Mass / (2 * EtimesP);

                    #endregion

                    #endregion
                }

                #region 物理信息计算

                #region 自转轴的计算

                RotationAxis = new Vector3(
                    (float)(Math.Cos(ObliquityLongitude * Mathf.Deg2Rad) * Math.Sin(Obliquity * Mathf.Deg2Rad)),
                    (float)(Math.Sin(ObliquityLongitude * Mathf.Deg2Rad) * Math.Sin(Obliquity * Mathf.Deg2Rad)),
                    (float)Math.Cos(Obliquity * Mathf.Deg2Rad));
                if (RotationAxis_old != RotationAxis)
                {
                    RotationAxis_old = RotationAxis;
                    CelestialBody.transform.rotation =
                        Quaternion.FromToRotation(CelestialBody.transform.forward, RotationAxis);
                }

                #endregion

                #region 其他计算

                StandardGravitationalParameter = Constants.G * Mass;
                Density = Mass / (4.0f * Mathf.PI * Math.Pow(MeanRadius, 3) / 3.0f);

                if (!IsCenterBody)
                {
                    SphereOfInfluence =
                        SemiMajorAxis /
                        (Math.Sqrt(OrbitingCelestialBodyData.Mass / Mass) +
                         1); //这里的a应该是orbitingCelestialBody和celestialBody自身之间的距离，但是用半长轴代替一下好了（用近拱点好像也不错？）
                }

                #endregion

                #endregion

                if (!IsCenterBody)
                {
                    OrbitalMechanicsFunctions.SetupOrbitVisuaizer(_lineRenderer, Segments, OrbitalEccentricity, EtimesP,
                        ArgumentOfPeriapsis, planeVector);
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CelestialBodyDataEditor))]
    class CelestialBodyDataEditorEditor : Editor
    {
        #region 本地变量
        [NonSerialized] private bool _openOrbitChara = true;
        [NonSerialized] private bool _openPhysicsChara = true;
        [NonSerialized] private bool _openEditorChara = false;
        [NonSerialized] private bool _equatorialPlaneOn = false;
        [NonSerialized] private bool _longitude0PlaneOn = false;
        [NonSerialized] private bool _longitude90PlaneOn = false;
        [NonSerialized] private readonly float  _titleLengthFactor = 15;
        [NonSerialized] private readonly float _dataBoxLength = 100;
        [NonSerialized] private float _temp = 0;
        #endregion
        public override void OnInspectorGUI()
        {
            CelestialBodyDataEditor celestialBodyDataEditor = (CelestialBodyDataEditor)target;
            if (celestialBodyDataEditor == null) return;
            
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();

            _temp = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            celestialBodyDataEditor.IsCenterBody = EditorGUILayout.Toggle("是否为中心天体（有无轨道信息）",
                celestialBodyDataEditor.IsCenterBody);
            EditorGUIUtility.labelWidth = _temp;
            
            #region 读取天体信息
            
            if (!celestialBodyDataEditor.IsCenterBody)
            {
                celestialBodyDataEditor.OrbitingCelestialBodyData =
                    EditCelestialBodyData("中心天体的天体信息", celestialBodyDataEditor.OrbitingCelestialBodyData);
            }
            celestialBodyDataEditor.CelestialBodyData =
                EditCelestialBodyData("需要更改的天体信息", celestialBodyDataEditor.CelestialBodyData);

            #endregion

            if (celestialBodyDataEditor.CelestialBodyData != null)
            {
                EditorUtility.SetDirty(celestialBodyDataEditor.CelestialBodyData);//不setdirty的话data编辑完可能保存不了
            }

            if (!celestialBodyDataEditor.IsCenterBody)
            {
                #region 星球轨道特性编辑
                EditorGUILayout.Space();
                _openOrbitChara = EditorGUILayout.BeginFoldoutHeaderGroup(_openOrbitChara, "星球轨道特性");
                if (_openOrbitChara)
                {
                    #region 轨道形状
                    
                    _temp = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 150;
                    celestialBodyDataEditor.UseApPe = EditorGUILayout.Toggle("用远，近拱点控制", celestialBodyDataEditor.UseApPe);
                    celestialBodyDataEditor.UseApPe = !EditorGUILayout.Toggle("用半长轴——离心率控制", !celestialBodyDataEditor.UseApPe);
                    EditorGUIUtility.labelWidth = _temp;
                    
                    EditorGUILayout.BeginHorizontal();
                    if (celestialBodyDataEditor.UseApPe)
                    {
                        celestialBodyDataEditor.Apoapsis = EditDouble("远拱点", celestialBodyDataEditor.Apoapsis);
                        celestialBodyDataEditor.Periapsis = EditDouble("近拱点", celestialBodyDataEditor.Periapsis);
                    }
                    else
                    {
                        celestialBodyDataEditor.SemiMajorAxis = EditDouble("半长轴", celestialBodyDataEditor.SemiMajorAxis);
                        celestialBodyDataEditor.OrbitalEccentricity = EditFloat("离心率", celestialBodyDataEditor.OrbitalEccentricity);
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion
                    #region 轨道方位
                    celestialBodyDataEditor.OrbitalInclination = EditFloatSlider("轨道倾角", celestialBodyDataEditor.OrbitalInclination, 0, 180);
                    celestialBodyDataEditor.ArgumentOfPeriapsis = EditFloatSlider("近心点辐角", celestialBodyDataEditor.ArgumentOfPeriapsis, 0, 360);
                    celestialBodyDataEditor.LongitudeOfTheAscendingNode = EditFloatSlider("升交点经度", celestialBodyDataEditor.LongitudeOfTheAscendingNode, 0, 360);
                    #endregion
                    #region 星球初始位置
                    
                    _temp = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 150;
                    celestialBodyDataEditor.UseTAat0 = EditorGUILayout.Toggle("用T0时刻的真近点角控制", celestialBodyDataEditor.UseTAat0);
                    celestialBodyDataEditor.UseTAat0 = !EditorGUILayout.Toggle("用T0时刻的平近点角控制", !celestialBodyDataEditor.UseTAat0);
                    EditorGUIUtility.labelWidth = _temp;
                    
                    if (celestialBodyDataEditor.UseTAat0)
                    {
                        celestialBodyDataEditor.TrueAnomalyAtT0 = EditFloatSlider("T0时刻的真近点角", celestialBodyDataEditor.TrueAnomalyAtT0, 0, 360);
                    }
                    else
                    {
                        celestialBodyDataEditor.MeanAnomalyAtT0 = EditFloatSlider("T0时刻的平近点角", celestialBodyDataEditor.MeanAnomalyAtT0, 0, 360);
                    }
                    #endregion
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                #endregion 
            }
            #region 星球物理特性编辑
            EditorGUILayout.Space();
            _openPhysicsChara = EditorGUILayout.BeginFoldoutHeaderGroup(_openPhysicsChara, "星球物理特性");
            if (_openPhysicsChara)
            {
                celestialBodyDataEditor.MeanRadius = EditDouble("平均半径", celestialBodyDataEditor.MeanRadius);
                celestialBodyDataEditor.Mass = EditDouble("质量", celestialBodyDataEditor.Mass);
                celestialBodyDataEditor.Obliquity = EditFloatSlider("自转轴倾角", celestialBodyDataEditor.Obliquity, 0, 180);
                celestialBodyDataEditor.ObliquityLongitude = EditFloatSlider("自转轴初始经度", celestialBodyDataEditor.ObliquityLongitude, 0, 360);
                celestialBodyDataEditor.SiderealRotationPeriod = EditDouble("自转周期", celestialBodyDataEditor.SiderealRotationPeriod);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion
            
            if (EditorGUI.EndChangeCheck())
            {
                celestialBodyDataEditor.ChangeData = true;
            }
            #region 编辑器设置
            EditorGUILayout.Space();
            _openEditorChara = EditorGUILayout.BeginFoldoutHeaderGroup(_openEditorChara, "编辑器设置");
            if (_openEditorChara)
            {
                #region 平面显示
                _equatorialPlaneOn = EditorGUILayout.Toggle("显示赤道平面", _equatorialPlaneOn);
                _longitude0PlaneOn = EditorGUILayout.Toggle("显示0经度平面", _longitude0PlaneOn);
                _longitude90PlaneOn = EditorGUILayout.Toggle("显示90经度平面", _longitude90PlaneOn);
                if (celestialBodyDataEditor.EquatorialPlane != null)
                {
                    celestialBodyDataEditor.EquatorialPlane.SetActive(_equatorialPlaneOn);
                }
                if (celestialBodyDataEditor.Longitude0Plane != null)
                {
                    celestialBodyDataEditor.Longitude0Plane.SetActive(_longitude0PlaneOn);
                }
                if (celestialBodyDataEditor.Longitude90Plane != null)
                {
                    celestialBodyDataEditor.Longitude90Plane.SetActive(_longitude90PlaneOn);
                }
                #endregion
                #region 参数编辑
                celestialBodyDataEditor.BallSize = EditFloat("示意小球大小", celestialBodyDataEditor.BallSize);
                celestialBodyDataEditor.PlaneVectorLength = EditFloat("平面向量长度系数", celestialBodyDataEditor.PlaneVectorLength);
                celestialBodyDataEditor.RotationAxisLength = EditFloat("自转轴向量长度系数", celestialBodyDataEditor.RotationAxisLength);
                celestialBodyDataEditor.OrbitalPlaneWidth = EditFloat("平面宽度", celestialBodyDataEditor.OrbitalPlaneWidth);
                celestialBodyDataEditor.OrbitalPlaneHeight = EditFloat("平面高度", celestialBodyDataEditor.OrbitalPlaneHeight);
                celestialBodyDataEditor.Segments = EditInt("轨道拟合段数", celestialBodyDataEditor.Segments);
                celestialBodyDataEditor.OrbitalPlaneMaterial = EditMaterial("平面材质", celestialBodyDataEditor.OrbitalPlaneMaterial);
                celestialBodyDataEditor.ADNodeMaterial = EditMaterial("升降点标记材质", celestialBodyDataEditor.ADNodeMaterial);
                celestialBodyDataEditor.LineRendererMaterial = EditMaterial("LineRenderer材质", celestialBodyDataEditor.LineRendererMaterial);
                #endregion
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion
            if (GUILayout.Button("从需要更改的星球信息中读入celestialBodyData"))
            {
                if (!celestialBodyDataEditor.IsCenterBody)
                {
                    #region 读入星球轨道信息
                    celestialBodyDataEditor.SemiMajorAxis = celestialBodyDataEditor.CelestialBodyData.SemiMajorAxis;
                    celestialBodyDataEditor.OrbitalEccentricity = celestialBodyDataEditor.CelestialBodyData.OrbitalEccentricity;
                    celestialBodyDataEditor.OrbitalInclination = celestialBodyDataEditor.CelestialBodyData.OrbitalInclination;
                    celestialBodyDataEditor.ArgumentOfPeriapsis = celestialBodyDataEditor.CelestialBodyData.ArgumentOfPeriapsis;
                    celestialBodyDataEditor.LongitudeOfTheAscendingNode = celestialBodyDataEditor.CelestialBodyData.LongitudeOfTheAscendingNode;
                    celestialBodyDataEditor.MeanAnomalyAtT0 = celestialBodyDataEditor.CelestialBodyData.MeanAnomalyAtT0;
                    #endregion
                }
                #region 读入星球物理信息
                celestialBodyDataEditor.MeanRadius = celestialBodyDataEditor.CelestialBodyData.MeanRadius;
                celestialBodyDataEditor.Mass = celestialBodyDataEditor.CelestialBodyData.Mass;
                celestialBodyDataEditor.Obliquity = celestialBodyDataEditor.CelestialBodyData.Obliquity;
                celestialBodyDataEditor.ObliquityLongitude = celestialBodyDataEditor.CelestialBodyData.ObliquityLongitude;
                celestialBodyDataEditor.SiderealRotationPeriod = celestialBodyDataEditor.CelestialBodyData.SiderealRotationPeriod;
                #endregion
                celestialBodyDataEditor.ChangeData = true;
            }
            if (GUILayout.Button("写入celestialBodyData"))
            {
                celestialBodyDataEditor.CelestialBodyData.CelestialBodyName = celestialBodyDataEditor.CelestialBodyData.name;
                if (!celestialBodyDataEditor.IsCenterBody)
                {
                    #region 写入星球轨道信息
                    celestialBodyDataEditor.CelestialBodyData.OrbitingBodyName = celestialBodyDataEditor.OrbitingBodyName;
                    celestialBodyDataEditor.CelestialBodyData.Apoapsis = celestialBodyDataEditor.Apoapsis;
                    celestialBodyDataEditor.CelestialBodyData.Periapsis = celestialBodyDataEditor.Periapsis;
                    celestialBodyDataEditor.CelestialBodyData.SemiMajorAxis = celestialBodyDataEditor.SemiMajorAxis;
                    celestialBodyDataEditor.CelestialBodyData.OrbitalEccentricity = celestialBodyDataEditor.OrbitalEccentricity;
                    celestialBodyDataEditor.CelestialBodyData.EtimesP = celestialBodyDataEditor.EtimesP;
                    celestialBodyDataEditor.CelestialBodyData.OrbitalInclination = celestialBodyDataEditor.OrbitalInclination;
                    celestialBodyDataEditor.CelestialBodyData.ArgumentOfPeriapsis = celestialBodyDataEditor.ArgumentOfPeriapsis;
                    celestialBodyDataEditor.CelestialBodyData.LongitudeOfTheAscendingNode = celestialBodyDataEditor.LongitudeOfTheAscendingNode;
                    celestialBodyDataEditor.CelestialBodyData.PlaneNormalVector = celestialBodyDataEditor.planeVector.normalized;
                    celestialBodyDataEditor.CelestialBodyData.AscendingNode = celestialBodyDataEditor.AscendingNode;
                    celestialBodyDataEditor.CelestialBodyData.DescendingNode = celestialBodyDataEditor.DescendingNode;
                    celestialBodyDataEditor.CelestialBodyData.OrbitalPeriod = celestialBodyDataEditor.OrbitalPeriod;
                    celestialBodyDataEditor.CelestialBodyData.OrbitalAngularMomentum = celestialBodyDataEditor.OrbitalAngularMomentum;
                    celestialBodyDataEditor.CelestialBodyData.SpecificAngularMomentum = celestialBodyDataEditor.SpecificAngularMomentum;
                    celestialBodyDataEditor.CelestialBodyData.OrbitalMechanicalEnergy = celestialBodyDataEditor.OrbitalMechanicalEnergy;
                    celestialBodyDataEditor.CelestialBodyData.MeanAnomalyAtT0 = celestialBodyDataEditor.MeanAnomalyAtT0;
                    celestialBodyDataEditor.CelestialBodyData.TrueAnomalyAtT0 = celestialBodyDataEditor.TrueAnomalyAtT0;
                    celestialBodyDataEditor.CelestialBodyData.TrueAnomaly = celestialBodyDataEditor.TrueAnomaly;
                    celestialBodyDataEditor.CelestialBodyData.EccentricAnomaly = celestialBodyDataEditor.EccentricAnomaly;
                    celestialBodyDataEditor.CelestialBodyData.MeanAnomaly = celestialBodyDataEditor.MeanAnomaly;
                    #endregion
                }
                #region 写入星球物理信息
                celestialBodyDataEditor.CelestialBodyData.MeanRadius = celestialBodyDataEditor.MeanRadius;
                celestialBodyDataEditor.CelestialBodyData.Mass = celestialBodyDataEditor.Mass;
                celestialBodyDataEditor.CelestialBodyData.StandardGravitationalParameter = celestialBodyDataEditor.StandardGravitationalParameter;
                celestialBodyDataEditor.CelestialBodyData.Density = celestialBodyDataEditor.Density;
                celestialBodyDataEditor.CelestialBodyData.Obliquity = celestialBodyDataEditor.Obliquity;
                celestialBodyDataEditor.CelestialBodyData.ObliquityLongitude = celestialBodyDataEditor.ObliquityLongitude;
                celestialBodyDataEditor.CelestialBodyData.RotationAxis = celestialBodyDataEditor.RotationAxis;
                celestialBodyDataEditor.CelestialBodyData.SiderealRotationPeriod = celestialBodyDataEditor.SiderealRotationPeriod;
                celestialBodyDataEditor.CelestialBodyData.SphereOfInfluence = celestialBodyDataEditor.SphereOfInfluence;
                #endregion
            }
        }
        #region 本地函数

        private int EditInt(string title, int data)
        {
            EditorGUILayout.LabelField(title, GUILayout.MaxWidth(title.Length * _titleLengthFactor));
            return EditorGUILayout.IntField(data, GUILayout.MaxWidth(_dataBoxLength));
        }
        private float EditFloat(string title, float data)
        {
            EditorGUILayout.LabelField(title, GUILayout.MaxWidth(title.Length * _titleLengthFactor));
            return EditorGUILayout.FloatField(data, GUILayout.MaxWidth(_dataBoxLength));
        }
        private double EditDouble(string title, double data)
        {
            EditorGUILayout.LabelField(title, GUILayout.MaxWidth(title.Length * _titleLengthFactor));
            return EditorGUILayout.DoubleField(data, GUILayout.MaxWidth(_dataBoxLength));
        }
        private float EditFloatSlider(string title, float data, float left, float right)
        {
            EditorGUILayout.LabelField(title, GUILayout.MaxWidth(title.Length * _titleLengthFactor));
            return EditorGUILayout.Slider(data, left, right);
        }
        private Material EditMaterial(string title, Material data)
        {
            EditorGUILayout.LabelField(title, GUILayout.MaxWidth(title.Length * _titleLengthFactor));
            return (Material)EditorGUILayout.ObjectField(data, typeof(Material), true);
        }
        private CelestialBodyDataSO EditCelestialBodyData(string title, CelestialBodyDataSO data)
        {
            EditorGUILayout.LabelField(title, GUILayout.MaxWidth(title.Length * _titleLengthFactor));
            return (CelestialBodyDataSO)EditorGUILayout.ObjectField(data, typeof(CelestialBodyDataSO), true);
        }

        #endregion
    }
#endif
}