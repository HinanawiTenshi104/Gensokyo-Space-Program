using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "newCelestialBodyData", menuName = "Data/Celestial Body Data", order = 0)]
public class CelestialBodyDataSO : ScriptableObject
{
    [ReadOnly, Header("��������")] public string CelestialBodyName;
    #region һ����Ϣ
    [Header("\nһ����Ϣ")]
    [ReadOnly, Header("��������λ��")] public Vector3 Position;
    [ReadOnly, Header("���������ٶ�")] public Vector3 Velocity;
    #endregion
    #region �������
    [Header("\n����������")]
    [ReadOnly, Header("������������")] public string OrbitingBodyName = null;
    [ReadOnly, Header("Զ����")] public double Apoapsis;   //��λ��meter
    [ReadOnly, Header("������")] public double Periapsis;  //��λ��meter
    [ReadOnly, Header("�볤��")] public double SemiMajorAxis;  //��λ��meter
    [ReadOnly, Header("���������")] public float OrbitalEccentricit;
    [ReadOnly, Header("�����ʡ���׼��")] public double EtimesP;    //��λ��meter
    [ReadOnly, Header("������")] public float OrbitalInclination; //��λ��degree
    [ReadOnly, Header("���ĵ����")] public float ArgumentOfPeriapsis;   //��λ��degree
    [ReadOnly, Header("�����㾭��")] public float LongitudeOfTheAscendingNode;   //��λ��degree
    [ReadOnly, Header("���������")] public Vector3 PlaneNormalVector;
    [ReadOnly, Header("������λ��")] public Vector3 AscendingNode;
    [ReadOnly, Header("������λ��")] public Vector3 DescendingNode;
    [ReadOnly, Header("�������")] public double OrbitalPeriod; //��λ��second
    [ReadOnly, Header("����Ƕ���")] public double OrbitalAngularMomentum;   //��λ��kg*m^2*s^-1
    [ReadOnly, Header("�ȽǶ���")] public double SpecificAngularMomentum;   //��λ��m^2*s^-1
    [ReadOnly, Header("�����е��")] public double OrbitalMechanicalEnergy;  //��λ��kg*m^2*s^-2
    [ReadOnly, Header("��0ʱ�̵Ĺ����ƽ�����")] public float MeanAnomalyAtT0;    //��λ��degree
    [ReadOnly, Header("��0ʱ�̵Ĺ����������")] public float TrueAnomalyAtT0;    //��λ��degree
    [ReadOnly, Header("������")] public float TrueAnomaly;    //��λ��rad
    [ReadOnly, Header("ƫ�����")] public float EccentricAnomaly;   //��λ��rad
    [ReadOnly, Header("ƽ�����")] public float MeanAnomaly;    //��λ��rad
    #endregion
    #region ��������
    [Header("\n������������")]
    [ReadOnly, Header("ƽ���뾶")] public double MeanRadius;    //��λ��meter
    [ReadOnly, Header("����")] public double Mass;    //��λ��kg
    [ReadOnly, Header("��׼��������")] public double StandardGravitationalParameter;  //��λ��m^3*s^-2
    [ReadOnly, Header("�ܶ�")] public double Density; //��λ��kg*m^-3
    [ReadOnly, Header("��ת�����")] public float Obliquity; //��λ��degree
    [ReadOnly, Header("��ת���ʼ����")] public float ObliquityLongitude;  //��λ��degree
    [ReadOnly, Header("��ת��")] public Vector3 RotationAxis;
    [ReadOnly, Header("��ת����")] public double SiderealRotationPeriod;    //��λ��second
    [ReadOnly, Header("�������þ���")] public double SphereOfInfluence;   //��λ��meter
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