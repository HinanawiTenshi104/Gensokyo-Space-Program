using System.Collections.Generic;
using UnityEngine;

public class CelestialBodyManager : MonoBehaviour
{
    #region ����
    GameObject sun;
    GameObject[] planets;
    List<GameObject> celestialBodies;//sun+planets
    CelestialBodyDataSO sunData;
    List<CelestialBodyDataSO> planetcbdatas = new List<CelestialBodyDataSO>();
    List<CelestialBodyDataSO> celestialBodiescbdatas;
    List<GameObject> surfaces;
    List<Vector3> LocalRotationAxes = new List<Vector3>();
    #endregion
    void Start()
    {
        #region ��ȡ���������
        sun = GameObject.FindGameObjectWithTag("Sun");
        planets = GameObject.FindGameObjectsWithTag("CelestialBody");
        celestialBodies = new List<GameObject> { sun };
        celestialBodies.AddRange(planets);

        sunData = sun.GetComponent<CelestialBodyDataHolder>().celestialBodyData;
        planetcbdatas = OrbitalMechanicsFunctions.GetCelestialBodyDatas(planets);
        celestialBodiescbdatas = new List<CelestialBodyDataSO> { sunData };
        celestialBodiescbdatas.AddRange(planetcbdatas);
        #endregion
        #region ����λ�ó�ʼ��
        sunData.Velocity = Vector3.zero;
        sunData.Position = Vector3.zero;
        foreach (var cbdata in planetcbdatas)
        {
            cbdata.MeanAnomaly = cbdata.MeanAnomalyAtT0;
        }
        #endregion
        CelestialBodyResizer();
        #region ������ת
        surfaces = new List<GameObject>();
        for (int i = 0; i < celestialBodies.Count; i++)
        {
            double SiderealRotationPeriod = celestialBodiescbdatas[i].SiderealRotationPeriod;
            Vector3 RotationAxis = celestialBodiescbdatas[i].RotationAxis;

            GameObject surface = celestialBodies[i].transform.Find("Appearance").Find("Surface").gameObject;
            if (surface == null)
            {
                Debug.Log("δ���ҵ�" + celestialBodies[i].name +"��surface������");
            }

            LocalRotationAxes.Add(Quaternion.FromToRotation(surface.transform.forward, celestialBodies[i].transform.forward) * RotationAxis);  //��Ϊtransform.Rotate��Axis������local�ģ�����Ҫ������������ת��ĳ���Ե�����ͼ����ת��
            surface.transform.rotation = surface.transform.rotation * Quaternion.FromToRotation(Vector3.forward, RotationAxis); //����ת��ĵ�����ͼ��[�Ӿ��ϵ�Z��]����ת�����

            surfaces.Add(surface);
        }

        ////DEBUG ��
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
        ////#DEBUG ��
        #endregion
    }
    void Update()
    {
        UpdateMeanAnomalys();
        UpdatePositions();
        MoveCelestialBodys();
        RotateCelestialBodys();
    }
    public void CelestialBodyResizer()
    {
        for (int i = 0; i < celestialBodies.Count; i++)
        {
            celestialBodies[i].transform.localScale = (float)(celestialBodiescbdatas[i].MeanRadius * Constants.PlanetRadiusShrinkFactor) * Vector3.one;
        }
    }
    public void UpdateMeanAnomalys()
    {
        for (int i = 0; i < planets.Length; i++)
        {
            //Debug.Log("planet " + planets[i].name + "'s MeanAnomaly updating!");
            OrbitalMechanicsFunctions.UpdateMeanAnomaly(planetcbdatas[i], Time.time);
        }
    }
    public void UpdatePositions()
    {
        for (int i = 0; i < planets.Length; i++)
        {
            //Debug.Log("planet " + planets[i].name + "'s Position updating!");
            planetcbdatas[i].Position = OrbitalMechanicsFunctions.CalculatePlanetWorldPosition(planets[i]);
        }
    }
    public void MoveCelestialBodys()
    {
        for (int i = 0; i < planets.Length; i++)
        {
            //Debug.Log("Moving planet " + planets[i].name + "!");
            planets[i].transform.position = planetcbdatas[i].Position;
        }
    }
    public void RotateCelestialBodys()
    {
        for (int i = 0; i < celestialBodies.Count; i++)
        {
            double SiderealRotationPeriod = celestialBodiescbdatas[i].SiderealRotationPeriod;
            surfaces[i].transform.Rotate(LocalRotationAxes[i], (float)(-360f / (SiderealRotationPeriod * Constants.TShrinkFactor) * Time.deltaTime));
            //Debug.Log(celestialBodies[i].name + " Rotated!\nRotated by " + (float)(-360f / (SiderealRotationPeriod * Constants.TShrinkFactor) * Time.deltaTime) + "degree!");
        }
    }
}