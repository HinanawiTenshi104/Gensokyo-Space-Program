using System;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

//升降交点计算参考：https://zh.wikipedia.org/wiki/%E5%8D%87%E4%BA%A4%E9%BB%9E%E9%BB%83%E7%B6%93
//赤经赤纬参考：https://image.baidu.com/search/detail?ct=503316480&z=0&ipn=d&word=%E8%B5%A4%E7%BA%AC&step_word=&hs=0&pn=6&spn=0&di=7169026086108397569&pi=0&rn=1&tn=baiduimagedetail&is=0%2C0&istype=0&ie=utf-8&oe=utf-8&in=&cl=2&lm=-1&st=undefined&cs=1214928430%2C2017643492&os=2337508916%2C3410988845&simid=1214928430%2C2017643492&adpicid=0&lpn=0&ln=1255&fr=&fmq=1675245236488_R&fm=&ic=undefined&s=undefined&hd=undefined&latest=undefined&copyright=undefined&se=&sme=&tab=0&width=undefined&height=undefined&face=undefined&ist=&jit=&cg=&bdtype=0&oriquery=&objurl=https%3A%2F%2Finews.gtimg.com%2Fnewsapp_bt%2F0%2F7906124687%2F1000&fromurl=ippr_z2C%24qAzdH3FAzdH3Fgjo_z%26e3Bqq_z%26e3Bv54AzdH3F54gAzdH3Fda8lanamAzdH3Fda8lanamA8FTmP_z%26e3Bip4s%3Frv&gsm=1e&rpstart=0&rpnum=0&islist=&querylist=&nojc=undefined&dyTabStr=MCwyLDMsNiw0LDEsNSw3LDgsOQ%3D%3D

public class CelestialBodyDataEditor : MonoBehaviour
{
    #region 天体本身的信息
    [NonSerialized] public GameObject orbitingCelestialBody;
    [NonSerialized] public GameObject celestialBody;
    [field: SerializeField, Header("中心天体的天体信息")]
    public CelestialBodyDataSO orbitingCelestialBodyData;
    [field: SerializeField, Header("需要更改的天体信息")]
    public CelestialBodyDataSO celestialBodyData;
    #endregion
    #region 星球轨道特性
    [NonSerialized] public string OrbitingBodyName;
    [NonSerialized] public double Apoapsis; //meter
    [NonSerialized] public double Periapsis; //meter
    [NonSerialized] public double SemiMajorAxis; //meter
    [NonSerialized] public float OrbitalEccentricit;
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
    [NonSerialized] public float planeVectorLength = 200;
    [NonSerialized] public float RotationAxisLength = 50;
    [NonSerialized] public float orbitalPlaneWidth = 500;
    [NonSerialized] public float orbitalPlaneHeight = 500;
    [NonSerialized] public int segments = 10000;
    [NonSerialized] public Material orbitalPlaneMaterial;
    [NonSerialized] public Material ADNodeMaterial;
    [NonSerialized] public Material LineRendererMaterial;
    #endregion
    #region 私有变量定义
    [NonSerialized] public bool changeData = true;
    [NonSerialized] public bool UseApPe = true;
    [NonSerialized] public bool UseTAat0 = true;
    [NonSerialized] public GameObject EquatorialPlane;
    [NonSerialized] public GameObject Longitude0Plane;
    [NonSerialized] public GameObject Longitude90Plane;
    private double centralBodySGP;
    LineRenderer lineRenderer;
    GameObject LineRendererObject;
    private GameObject orbitalPlane;
    private GameObject AscendingNodeObject;
    private GameObject DescendingNodeObject;
    private GameObject Satellite;
    private GameObject planeVectorObject;
    private GameObject RotationAxisObject;
    #endregion

    private void Awake()
    {
        CelestialBody();
        Apoapsis = 1;
        Periapsis = 1;
        SiderealRotationPeriod = 1;
    }
    private void Start()
    {
        #region null值处理
        if (orbitalPlaneMaterial == null)
        {
            orbitalPlaneMaterial = Resources.Load("Materials/OrbitalPlane", typeof(Material)) as Material;
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
        LineRendererObject = new GameObject("轨道可视化");
        LineRendererObject.transform.parent = gameObject.transform;
        LineRendererObject.AddComponent<LineRenderer>();
        lineRenderer = LineRendererObject.GetComponent<LineRenderer>();
        lineRenderer.material = LineRendererMaterial;
        lineRenderer.useWorldSpace = false;
        #endregion
        #region 各种平面初始化
        orbitalPlane = ObjectCreator.CreatePlane("轨道平面", orbitalPlaneWidth, orbitalPlaneHeight, orbitalPlaneMaterial);
        orbitalPlane.hideFlags = HideFlags.DontSave;
        orbitalPlane.transform.parent = gameObject.transform;
        EquatorialPlane = ObjectCreator.CreatePlane("赤道平面", orbitalPlaneWidth, orbitalPlaneHeight, orbitalPlaneMaterial);
        EquatorialPlane.hideFlags = HideFlags.DontSave;
        EquatorialPlane.transform.parent = gameObject.transform;
        EquatorialPlane.transform.rotation = Quaternion.identity;
        EquatorialPlane.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.forward);
        Longitude0Plane = ObjectCreator.CreatePlane("0经度平面", orbitalPlaneWidth, orbitalPlaneHeight, orbitalPlaneMaterial);
        Longitude0Plane.hideFlags = HideFlags.DontSave;
        Longitude0Plane.transform.parent = gameObject.transform;
        Longitude0Plane.transform.rotation = Quaternion.identity;
        Longitude0Plane.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.down);
        Longitude90Plane = ObjectCreator.CreatePlane("90经度平面", orbitalPlaneWidth, orbitalPlaneHeight, orbitalPlaneMaterial);
        Longitude90Plane.hideFlags = HideFlags.DontSave;
        Longitude90Plane.transform.parent = gameObject.transform;
        Longitude90Plane.transform.rotation = Quaternion.identity;
        Longitude90Plane.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.right);
        #endregion
        #region 各种物体初始化
        AscendingNodeObject = ObjectCreator.CreateSphere("升交点", 5, gameObject, ADNodeMaterial);
        DescendingNodeObject = ObjectCreator.CreateSphere("降交点", 5, gameObject, ADNodeMaterial);
        Satellite = ObjectCreator.CreateSphere("小卫星", 5, gameObject);
        planeVectorObject = ObjectCreator.CreateSphere("平面向量", 5, gameObject);
        RotationAxisObject = ObjectCreator.CreateSphere("自转轴向量", 5, gameObject);
        #endregion
    }
    public void Update()
    {
        if (changeData)
        {
            changeData = false;
            SubUpdate();
        }
        #region 设置各种物体的位置
        orbitalPlane.transform.position = orbitingCelestialBody.transform.position;
        EquatorialPlane.transform.position = orbitingCelestialBody.transform.position;
        Longitude0Plane.transform.position = orbitingCelestialBody.transform.position;
        Longitude90Plane.transform.position = orbitingCelestialBody.transform.position;
        AscendingNodeObject.transform.position = orbitingCelestialBody.transform.position + AscendingNode;
        DescendingNodeObject.transform.position = orbitingCelestialBody.transform.position + DescendingNode;
        planeVectorObject.transform.position = orbitingCelestialBody.transform.position + planeVector * planeVectorLength;
        RotationAxisObject.transform.position = celestialBody.transform.position + RotationAxis * RotationAxisLength;
        if (EtimesP != 0)
        {
            Satellite.transform.position = orbitingCelestialBody.transform.position + OrbitalMechanicsFunctions.TranslatePoints(OrbitalMechanicsFunctions.CalculatePointOnTheEllipse(OrbitalEccentricit, EtimesP, ArgumentOfPeriapsis, TrueAnomaly), planeVector.normalized);
        }
        if (SiderealRotationPeriod != 0)
        {
            celestialBody.transform.Rotate(Vector3.forward, (float)(-360f / SiderealRotationPeriod * Time.deltaTime));
        }
        #endregion
    }
    private void CelestialBody()
    {
        GameObject[] celestialBodys = GameObject.FindGameObjectsWithTag("CelestialBody").Concat(GameObject.FindGameObjectsWithTag("Sun")).ToArray();
        for (int i = 0; i < celestialBodys.Length; i++)
        {
            if (celestialBodys[i].name == orbitingCelestialBodyData.name)
            {
                orbitingCelestialBody = celestialBodys[i];
            }
            else if (celestialBodys[i].name == celestialBodyData.name)
            {
                celestialBody = celestialBodys[i];
            }
            else
            {
                if (celestialBodys[i].tag != "Sun")
                {
                    celestialBodys[i].SetActive(false); //暂时让其他物体失效，让界面干净一点
                }
            }
        }
        if (orbitingCelestialBody == null)
        {
            Debug.Log("轨道编辑器未能找到中心物体！");
            return;
        }
        if (celestialBody == null)
        {
            Debug.Log("轨道编辑器未能找到绕行物体！");
            return;
        }
        GameObject.Find("CelestialBodyManager").SetActive(false);
        celestialBody.GetComponent<OrbitVisualizer>().enabled = false;
    }
    private void SubUpdate()
    {
        CelestialBody();
        OrbitingBodyName = orbitingCelestialBody.name;
        centralBodySGP = orbitingCelestialBodyData.StandardGravitationalParameter;
        #region 轨道平面轴和自转轴的计算
        planeVector = new Vector3(-(float)(Math.Cos(LongitudeOfTheAscendingNode * Mathf.Deg2Rad) * Math.Sin(OrbitalInclination * Mathf.Deg2Rad)), (float)(Math.Sin(LongitudeOfTheAscendingNode * Mathf.Deg2Rad) * Math.Sin(OrbitalInclination * Mathf.Deg2Rad)), (float)Math.Cos(OrbitalInclination * Mathf.Deg2Rad));
        orbitalPlane.transform.rotation = Quaternion.FromToRotation(Vector3.forward, planeVector);

        RotationAxis = new Vector3((float)(Math.Cos(ObliquityLongitude * Mathf.Deg2Rad) * Math.Sin(Obliquity * Mathf.Deg2Rad)), (float)(Math.Sin(ObliquityLongitude * Mathf.Deg2Rad) * Math.Sin(Obliquity * Mathf.Deg2Rad)), (float)Math.Cos(Obliquity * Mathf.Deg2Rad));
        if (RotationAxis_old != RotationAxis)
        {
            RotationAxis_old = RotationAxis;
            celestialBody.transform.rotation = Quaternion.FromToRotation(celestialBody.transform.forward, RotationAxis);
        }
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
            if (planeVector.x <= 0)  //看看经度相对于x轴有没有相差大于180°（垃圾Angle函数）
            {
                angle = 2 * Mathf.PI - Vector3.Angle(Vector3.right, new Vector3(-planeVector.y, planeVector.x, 0)) * Mathf.Deg2Rad;
            }
            else
            {
                angle = Vector3.Angle(Vector3.right, new Vector3(-planeVector.y, planeVector.x, 0)) * Mathf.Deg2Rad;
            }
        }
        AscendingNode = OrbitalMechanicsFunctions.CalculatePointOnTheEllipse(OrbitalEccentricit, EtimesP, ArgumentOfPeriapsis, angle);
        DescendingNode = OrbitalMechanicsFunctions.CalculatePointOnTheEllipse(OrbitalEccentricit, EtimesP, ArgumentOfPeriapsis, angle + Mathf.PI); //降交点一定在升交点对面(也就是+pi)
        #endregion
        #region 轨道形状计算
        if (UseApPe)
        {
            SemiMajorAxis = (Apoapsis + Periapsis) / 2.0f;
            OrbitalEccentricit = (float)((SemiMajorAxis - Periapsis) / SemiMajorAxis);
        }
        else
        {
            Periapsis = SemiMajorAxis * (1 - OrbitalEccentricit);
            Apoapsis = 2 * SemiMajorAxis - Periapsis;
        }
        EtimesP = SemiMajorAxis * (1 - Math.Pow(OrbitalEccentricit, 2));
        #endregion
        #region 轨道位置计算
        if (UseTAat0)
        {
            TrueAnomaly = TrueAnomalyAtT0 * Mathf.Deg2Rad;
            EccentricAnomaly = OrbitalMechanicsFunctions.TAnomalyToEAnomaly(OrbitalEccentricit, TrueAnomaly);
            MeanAnomaly = OrbitalMechanicsFunctions.EAnomalyToMAnomaly(OrbitalEccentricit, EccentricAnomaly);
            MeanAnomalyAtT0 = MeanAnomaly * Mathf.Rad2Deg;
        }
        else
        {
            MeanAnomaly = MeanAnomalyAtT0 * Mathf.Deg2Rad;
            EccentricAnomaly = OrbitalMechanicsFunctions.MAnomalyToEAnomaly(OrbitalEccentricit, MeanAnomaly);
            TrueAnomaly = OrbitalMechanicsFunctions.EAnomalyToTAnomaly(OrbitalEccentricit, EccentricAnomaly);
            TrueAnomalyAtT0 = TrueAnomaly * Mathf.Rad2Deg;
        }
        #endregion
        #region 其他计算
        OrbitalPeriod = 2 * Math.PI * SemiMajorAxis * Math.Sqrt(SemiMajorAxis / centralBodySGP);
        SpecificAngularMomentum = Math.Sqrt(EtimesP * centralBodySGP);
        OrbitalAngularMomentum = SpecificAngularMomentum * Mass;
        OrbitalMechanicalEnergy = (Mathf.Pow(OrbitalEccentricit, 2) - 1) * centralBodySGP * Mass / (2 * EtimesP);
        StandardGravitationalParameter = Constants.G * Mass;
        Density = Mass / (4.0f * Mathf.PI * Math.Pow(MeanRadius, 3) / 3.0f);
        SphereOfInfluence = SemiMajorAxis / (Math.Sqrt(orbitingCelestialBodyData.Mass / Mass) + 1); //这里的a应该是orbitingCelestialBody和celestialBody自身之间的距离，但是用半长轴代替一下好了（用近拱点好像也不错？）
        #endregion
        OrbitalMechanicsFunctions.SetupOrbitVisuaizer(lineRenderer, segments, OrbitalEccentricit, EtimesP, ArgumentOfPeriapsis, planeVector);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CelestialBodyDataEditor))]
class celestialBodyDataEditorEditor : Editor
{
    #region 本地变量
    [NonSerialized] private bool OpenOrbitChara = true;
    [NonSerialized] private bool OpenPhysicsChara = true;
    [NonSerialized] private bool OpenEditorChara = false;
    [NonSerialized] private bool EquatorialPlaneOn = false;
    [NonSerialized] private bool Longitude0PlaneOn = false;
    [NonSerialized] private bool Longitude90PlaneOn = false;
    [NonSerialized] private float TitleLengthFactor = 15;
    [NonSerialized] private float DataBoxLength = 100;
    #endregion
    public override void OnInspectorGUI()
    {
        CelestialBodyDataEditor celestialBodyDataEditor = (CelestialBodyDataEditor)target;
        if (celestialBodyDataEditor == null) return;
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        EditorUtility.SetDirty(celestialBodyDataEditor.celestialBodyData);
        #region 星球轨道特性编辑
        EditorGUILayout.Space();
        OpenOrbitChara = EditorGUILayout.BeginFoldoutHeaderGroup(OpenOrbitChara, "星球轨道特性");
        if (OpenOrbitChara)
        {
            #region 轨道形状
            celestialBodyDataEditor.UseApPe = EditorGUILayout.Toggle("用远，近拱点控制", celestialBodyDataEditor.UseApPe);
            celestialBodyDataEditor.UseApPe = !EditorGUILayout.Toggle("用半长轴——离心率控制", !celestialBodyDataEditor.UseApPe);
            EditorGUILayout.BeginHorizontal();
            if (celestialBodyDataEditor.UseApPe)
            {
                celestialBodyDataEditor.Apoapsis = EditDouble("远拱点", celestialBodyDataEditor.Apoapsis);
                celestialBodyDataEditor.Periapsis = EditDouble("近拱点", celestialBodyDataEditor.Periapsis);
            }
            else
            {
                celestialBodyDataEditor.SemiMajorAxis = EditDouble("半长轴", celestialBodyDataEditor.SemiMajorAxis);
                celestialBodyDataEditor.OrbitalEccentricit = EditFloat("离心率", celestialBodyDataEditor.OrbitalEccentricit);
            }
            EditorGUILayout.EndHorizontal();
            #endregion
            #region 轨道方位
            celestialBodyDataEditor.OrbitalInclination = EditFloatSlider("轨道倾角", celestialBodyDataEditor.OrbitalInclination, 0, 180);
            celestialBodyDataEditor.ArgumentOfPeriapsis = EditFloatSlider("近心点辐角", celestialBodyDataEditor.ArgumentOfPeriapsis, 0, 360);
            celestialBodyDataEditor.LongitudeOfTheAscendingNode = EditFloatSlider("升交点经度", celestialBodyDataEditor.LongitudeOfTheAscendingNode, 0, 360);
            #endregion
            #region 星球初始位置
            celestialBodyDataEditor.UseTAat0 = EditorGUILayout.Toggle("用T0时刻的真近点角控制", celestialBodyDataEditor.UseTAat0);
            celestialBodyDataEditor.UseTAat0 = !EditorGUILayout.Toggle("用T0时刻的平近点角控制", !celestialBodyDataEditor.UseTAat0);
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
        #region 星球物理特性编辑
        EditorGUILayout.Space();
        OpenPhysicsChara = EditorGUILayout.BeginFoldoutHeaderGroup(OpenPhysicsChara, "星球物理特性");
        if (OpenPhysicsChara)
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
            celestialBodyDataEditor.changeData = true;
        }
        #region 编辑器设置
        EditorGUILayout.Space();
        OpenEditorChara = EditorGUILayout.BeginFoldoutHeaderGroup(OpenEditorChara, "编辑器设置");
        if (OpenEditorChara)
        {
            #region 平面显示
            EquatorialPlaneOn = EditorGUILayout.Toggle("显示赤道平面", EquatorialPlaneOn);
            Longitude0PlaneOn = EditorGUILayout.Toggle("显示0经度平面", Longitude0PlaneOn);
            Longitude90PlaneOn = EditorGUILayout.Toggle("显示90经度平面", Longitude90PlaneOn);
            celestialBodyDataEditor.EquatorialPlane.SetActive(EquatorialPlaneOn);
            celestialBodyDataEditor.Longitude0Plane.SetActive(Longitude0PlaneOn);
            celestialBodyDataEditor.Longitude90Plane.SetActive(Longitude90PlaneOn);
            #endregion
            #region 参数编辑
            celestialBodyDataEditor.planeVectorLength = EditFloat("平面向量长度系数", celestialBodyDataEditor.planeVectorLength);
            celestialBodyDataEditor.RotationAxisLength = EditFloat("自转轴向量长度系数", celestialBodyDataEditor.RotationAxisLength);
            celestialBodyDataEditor.orbitalPlaneWidth = EditFloat("平面宽度", celestialBodyDataEditor.orbitalPlaneWidth);
            celestialBodyDataEditor.orbitalPlaneHeight = EditFloat("平面高度", celestialBodyDataEditor.orbitalPlaneHeight);
            celestialBodyDataEditor.segments = EditInt("轨道拟合段数", celestialBodyDataEditor.segments);
            celestialBodyDataEditor.orbitalPlaneMaterial = EditMaterial("平面材质", celestialBodyDataEditor.orbitalPlaneMaterial);
            celestialBodyDataEditor.ADNodeMaterial = EditMaterial("升降点标记材质", celestialBodyDataEditor.ADNodeMaterial);
            celestialBodyDataEditor.LineRendererMaterial = EditMaterial("LineRenderer材质", celestialBodyDataEditor.LineRendererMaterial);
            #endregion
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion
        if (GUILayout.Button("从需要更改的星球信息中读入celestialBodyData"))
        {
            #region 读入星球轨道信息
            celestialBodyDataEditor.SemiMajorAxis = celestialBodyDataEditor.celestialBodyData.SemiMajorAxis;
            celestialBodyDataEditor.OrbitalEccentricit = celestialBodyDataEditor.celestialBodyData.OrbitalEccentricit;
            celestialBodyDataEditor.OrbitalInclination = celestialBodyDataEditor.celestialBodyData.OrbitalInclination;
            celestialBodyDataEditor.ArgumentOfPeriapsis = celestialBodyDataEditor.celestialBodyData.ArgumentOfPeriapsis;
            celestialBodyDataEditor.LongitudeOfTheAscendingNode = celestialBodyDataEditor.celestialBodyData.LongitudeOfTheAscendingNode;
            celestialBodyDataEditor.MeanAnomalyAtT0 = celestialBodyDataEditor.celestialBodyData.MeanAnomalyAtT0;
            #endregion
            #region 读入星球物理信息
            celestialBodyDataEditor.MeanRadius = celestialBodyDataEditor.celestialBodyData.MeanRadius;
            celestialBodyDataEditor.Mass = celestialBodyDataEditor.celestialBodyData.Mass;
            celestialBodyDataEditor.Obliquity = celestialBodyDataEditor.celestialBodyData.Obliquity;
            celestialBodyDataEditor.ObliquityLongitude = celestialBodyDataEditor.celestialBodyData.ObliquityLongitude;
            celestialBodyDataEditor.SiderealRotationPeriod = celestialBodyDataEditor.celestialBodyData.SiderealRotationPeriod;
            #endregion
            celestialBodyDataEditor.changeData = true;
        }
        if (GUILayout.Button("写入celestialBodyData"))
        {
            celestialBodyDataEditor.celestialBodyData.CelestialBodyName = celestialBodyDataEditor.celestialBodyData.name;
            #region 写入星球轨道信息
            celestialBodyDataEditor.celestialBodyData.OrbitingBodyName = celestialBodyDataEditor.OrbitingBodyName;
            celestialBodyDataEditor.celestialBodyData.Apoapsis = celestialBodyDataEditor.Apoapsis;
            celestialBodyDataEditor.celestialBodyData.Periapsis = celestialBodyDataEditor.Periapsis;
            celestialBodyDataEditor.celestialBodyData.SemiMajorAxis = celestialBodyDataEditor.SemiMajorAxis;
            celestialBodyDataEditor.celestialBodyData.OrbitalEccentricit = celestialBodyDataEditor.OrbitalEccentricit;
            celestialBodyDataEditor.celestialBodyData.EtimesP = celestialBodyDataEditor.EtimesP;
            celestialBodyDataEditor.celestialBodyData.OrbitalInclination = celestialBodyDataEditor.OrbitalInclination;
            celestialBodyDataEditor.celestialBodyData.ArgumentOfPeriapsis = celestialBodyDataEditor.ArgumentOfPeriapsis;
            celestialBodyDataEditor.celestialBodyData.LongitudeOfTheAscendingNode = celestialBodyDataEditor.LongitudeOfTheAscendingNode;
            celestialBodyDataEditor.celestialBodyData.PlaneNormalVector = celestialBodyDataEditor.planeVector.normalized;
            celestialBodyDataEditor.celestialBodyData.AscendingNode = celestialBodyDataEditor.AscendingNode;
            celestialBodyDataEditor.celestialBodyData.DescendingNode = celestialBodyDataEditor.DescendingNode;
            celestialBodyDataEditor.celestialBodyData.OrbitalPeriod = celestialBodyDataEditor.OrbitalPeriod;
            celestialBodyDataEditor.celestialBodyData.OrbitalAngularMomentum = celestialBodyDataEditor.OrbitalAngularMomentum;
            celestialBodyDataEditor.celestialBodyData.SpecificAngularMomentum = celestialBodyDataEditor.SpecificAngularMomentum;
            celestialBodyDataEditor.celestialBodyData.OrbitalMechanicalEnergy = celestialBodyDataEditor.OrbitalMechanicalEnergy;
            celestialBodyDataEditor.celestialBodyData.MeanAnomalyAtT0 = celestialBodyDataEditor.MeanAnomalyAtT0;
            celestialBodyDataEditor.celestialBodyData.TrueAnomalyAtT0 = celestialBodyDataEditor.TrueAnomalyAtT0;
            celestialBodyDataEditor.celestialBodyData.TrueAnomaly = celestialBodyDataEditor.TrueAnomaly;
            celestialBodyDataEditor.celestialBodyData.EccentricAnomaly = celestialBodyDataEditor.EccentricAnomaly;
            celestialBodyDataEditor.celestialBodyData.MeanAnomaly = celestialBodyDataEditor.MeanAnomaly;
            #endregion
            #region 写入星球物理信息
            celestialBodyDataEditor.celestialBodyData.MeanRadius = celestialBodyDataEditor.MeanRadius;
            celestialBodyDataEditor.celestialBodyData.Mass = celestialBodyDataEditor.Mass;
            celestialBodyDataEditor.celestialBodyData.StandardGravitationalParameter = celestialBodyDataEditor.StandardGravitationalParameter;
            celestialBodyDataEditor.celestialBodyData.Density = celestialBodyDataEditor.Density;
            celestialBodyDataEditor.celestialBodyData.Obliquity = celestialBodyDataEditor.Obliquity;
            celestialBodyDataEditor.celestialBodyData.ObliquityLongitude = celestialBodyDataEditor.ObliquityLongitude;
            celestialBodyDataEditor.celestialBodyData.RotationAxis = celestialBodyDataEditor.RotationAxis;
            celestialBodyDataEditor.celestialBodyData.SiderealRotationPeriod = celestialBodyDataEditor.SiderealRotationPeriod;
            celestialBodyDataEditor.celestialBodyData.SphereOfInfluence = celestialBodyDataEditor.SphereOfInfluence;
            #endregion
        }
    }
    private int EditInt(string Title, int data)
    {
        EditorGUILayout.LabelField(Title, GUILayout.MaxWidth(Title.Length * TitleLengthFactor));
        return EditorGUILayout.IntField(data, GUILayout.MaxWidth(DataBoxLength));
    }
    private float EditFloat(string Title, float data)
    {
        EditorGUILayout.LabelField(Title, GUILayout.MaxWidth(Title.Length * TitleLengthFactor));
        return EditorGUILayout.FloatField(data, GUILayout.MaxWidth(DataBoxLength));
    }
    private double EditDouble(string Title, double data)
    {
        EditorGUILayout.LabelField(Title, GUILayout.MaxWidth(Title.Length * TitleLengthFactor));
        return EditorGUILayout.DoubleField(data, GUILayout.MaxWidth(DataBoxLength));
    }
    private float EditFloatSlider(string Title, float data, float left, float right)
    {
        EditorGUILayout.LabelField(Title, GUILayout.MaxWidth(Title.Length * TitleLengthFactor));
        return EditorGUILayout.Slider(data, left, right);
    }
    private Material EditMaterial(string Title, Material data)
    {
        EditorGUILayout.LabelField(Title, GUILayout.MaxWidth(Title.Length * TitleLengthFactor));
        return (Material)EditorGUILayout.ObjectField(data, typeof(Material), true);
    }
}
#endif