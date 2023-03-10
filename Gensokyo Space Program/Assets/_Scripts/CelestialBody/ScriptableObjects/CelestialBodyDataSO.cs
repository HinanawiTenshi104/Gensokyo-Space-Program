using UnityEditor;
using UnityEngine;

namespace _Scripts.CelestialBody.ScriptableObjects
{
    [CreateAssetMenu(fileName = "newCelestialBodyData", menuName = "Data/Celestial Body Data", order = 0)]
    public class CelestialBodyDataSO : ScriptableObject
    {
        [ReadOnly, Header("天体名称")] public string CelestialBodyName;
        #region 一般信息
        [Header("\n一般信息")]
        [ReadOnly, Header("天体质心位置")] public Vector3 Position;
        [ReadOnly, Header("天体质心速度")] public Vector3 Velocity;
        #endregion
        #region 轨道特性
        [Header("\n天体轨道特性")]
        [ReadOnly, Header("中心天体名字")] public string OrbitingBodyName = null;
        [ReadOnly, Header("远拱点")] public double Apoapsis;   //单位：meter
        [ReadOnly, Header("近拱点")] public double Periapsis;  //单位：meter
        [ReadOnly, Header("半长轴")] public double SemiMajorAxis;  //单位：meter
        [ReadOnly, Header("轨道离心率")] public float OrbitalEccentricity;
        [ReadOnly, Header("离心率×焦准距")] public double EtimesP;    //单位：meter
        [ReadOnly, Header("轨道倾角")] public float OrbitalInclination; //单位：degree
        [ReadOnly, Header("近心点辐角")] public float ArgumentOfPeriapsis;   //单位：degree
        [ReadOnly, Header("升交点经度")] public float LongitudeOfTheAscendingNode;   //单位：degree
        [ReadOnly, Header("轨道法向量")] public Vector3 PlaneNormalVector;
        [ReadOnly, Header("升交点位置")] public Vector3 AscendingNode;
        [ReadOnly, Header("降交点位置")] public Vector3 DescendingNode;
        [ReadOnly, Header("轨道周期")] public double OrbitalPeriod; //单位：second
        [ReadOnly, Header("轨道角动量")] public double OrbitalAngularMomentum;   //单位：kg*m^2*s^-1
        [ReadOnly, Header("比角动量")] public double SpecificAngularMomentum;   //单位：m^2*s^-1
        [ReadOnly, Header("轨道机械能")] public double OrbitalMechanicalEnergy;  //单位：kg*m^2*s^-2
        [ReadOnly, Header("在0时刻的轨道的平近点角")] public float MeanAnomalyAtT0;    //单位：degree
        [ReadOnly, Header("在0时刻的轨道的真近点角")] public float TrueAnomalyAtT0;    //单位：degree
        [ReadOnly, Header("真近点角")] public float TrueAnomaly;    //单位：rad
        [ReadOnly, Header("偏近点角")] public float EccentricAnomaly;   //单位：rad
        [ReadOnly, Header("平近点角")] public float MeanAnomaly;    //单位：rad
        #endregion
        #region 物理特性
        [Header("\n天体物理特性")]
        [ReadOnly, Header("平均半径")] public double MeanRadius;    //单位：meter
        [ReadOnly, Header("质量")] public double Mass;    //单位：kg
        [ReadOnly, Header("标准重力参数")] public double StandardGravitationalParameter;  //单位：m^3*s^-2
        [ReadOnly, Header("密度")] public double Density; //单位：kg*m^-3
        [ReadOnly, Header("自转轴倾角")] public float Obliquity; //单位：degree
        [ReadOnly, Header("自转轴初始经度")] public float ObliquityLongitude;  //单位：degree
        [ReadOnly, Header("自转轴")] public Vector3 RotationAxis;
        [ReadOnly, Header("自转周期")] public double SiderealRotationPeriod;    //单位：second
        [ReadOnly, Header("引力作用距离")] public double SphereOfInfluence;   //单位：meter
        #endregion
    }

    public class ReadOnlyAttribute : PropertyAttribute { }
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}