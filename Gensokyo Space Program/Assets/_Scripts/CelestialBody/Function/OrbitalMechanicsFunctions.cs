using System;
using System.Collections.Generic;
using UnityEngine;

public static class OrbitalMechanicsFunctions
{
    public static void UpdateMeanAnomaly(CelestialBodyDataSO cbdata, float timesinceT0)
    {
        float MeanAnomalyAtT0 = cbdata.MeanAnomalyAtT0;
        double OrbitalPeriod = cbdata.OrbitalPeriod;

        cbdata.MeanAnomaly = (float)(MeanAnomalyAtT0 + 2f * Mathf.PI * timesinceT0 / (OrbitalPeriod * Constants.OrbitSizeShrinkFactor * 100)) % (2f * Mathf.PI);
    }
    public static Vector3 CalculatePlanetPosition(float OrbitalEccentricit, double EtimesP, float ArgumentOfPeriapsis, float MeanAnomaly, Vector3 targetPlaneNormalVector)
    {
        float TrueAnomaly = MAnomalyToTAnomaly(OrbitalEccentricit, MeanAnomaly);
        double Radius = EtimesP * Constants.OrbitSizeShrinkFactor / (1 - OrbitalEccentricit * Math.Cos(TrueAnomaly - Math.PI / 2.0f + ArgumentOfPeriapsis * Mathf.Deg2Rad));
        Vector3 point = new Vector3((float)(Radius * Math.Cos(TrueAnomaly)), (float)(Radius * Math.Sin(TrueAnomaly)), 0);
        return Quaternion.FromToRotation(Vector3.forward, targetPlaneNormalVector) * point;
    }
    public static Vector3 CalculatePlanetPosition(CelestialBodyDataSO celestialBodyData)
    {
        return CalculatePlanetPosition(celestialBodyData.OrbitalEccentricit, celestialBodyData.EtimesP, celestialBodyData.ArgumentOfPeriapsis, celestialBodyData.MeanAnomaly, celestialBodyData.PlaneNormalVector);
    }
    public static Vector3 CalculatePlanetWorldPosition(GameObject planet)
    {
        CelestialBodyDataHolder planetcbdataholder = planet.GetComponent<CelestialBodyDataHolder>();
        CelestialBodyDataSO planetcbdata = planetcbdataholder.celestialBodyData;
        Vector3 planetPos;
        if (planet.tag != "CelestialBody")//没有轨道信息的物体
        {
            return planetcbdata.Position;
        }
        else
        {
            planetPos = CalculatePlanetPosition(planetcbdata);
        }

        GameObject OrbitingObject = planetcbdataholder.OrbitingObject;
        while (OrbitingObject != null)//直到没有中心天体为止一直叠加中心天体的坐标(一直转换坐标系)
        {
            if (OrbitingObject.tag == "CelestialBody")
            {
                CelestialBodyDataSO centralcbdata = OrbitingObject.GetComponent<CelestialBodyDataHolder>().celestialBodyData;
                planetPos += CalculatePlanetPosition(centralcbdata);

                OrbitingObject = OrbitingObject.GetComponent<CelestialBodyDataHolder>().OrbitingObject;
            }
            else
            {
                planetPos += OrbitingObject.transform.position;
                break;
            }
        }

        return planetPos;
    }
    public static Vector3 CalculatePointOnTheEllipse(float OrbitalEccentricit, double EtimesP, float ArgumentOfPeriapsis, float TrueAnomaly)
    {
        double Radius = EtimesP * Constants.OrbitSizeShrinkFactor / (1 - OrbitalEccentricit * Math.Cos(TrueAnomaly - Math.PI / 2.0f + ArgumentOfPeriapsis * Mathf.Deg2Rad));
        return new Vector3((float)(Radius * Math.Cos(TrueAnomaly)), (float)(Radius * Math.Sin(TrueAnomaly)), 0);
    }
    public static Vector3 TranslatePoints(Vector3 point, Vector3 targetPlaneNormalVector)//把椭圆轨道平面坐标系上的点转换到赤道坐标系的点
    {
        return Quaternion.FromToRotation(Vector3.forward, targetPlaneNormalVector) * point;
    }
    public static void SetupOrbitVisuaizer(LineRenderer lineRenderer, int segments, float OrbitalEccentricit, double EtimesP, float ArgumentOfPeriapsis, Vector3 planeVector)
    {
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)(2 * Math.PI * i / segments);
            points[i] = TranslatePoints(CalculatePointOnTheEllipse(OrbitalEccentricit, EtimesP, ArgumentOfPeriapsis, angle), planeVector.normalized);
        }
        points[segments] = points[0];

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    public static List<CelestialBodyDataSO> GetCelestialBodyDatas(GameObject[] planets)
    {
        List<CelestialBodyDataSO> cbdatas = new List<CelestialBodyDataSO>();
        for (int i = 0; i < planets.Length; i++)
        {
            CelestialBodyDataSO temp = planets[i].GetComponent<CelestialBodyDataHolder>().celestialBodyData;
            if (temp == null)
            {
                Debug.Log("GetCelestialBodyDatas函数找不到cbdata!");
                break;
            }
            cbdatas.Add(temp);
        }
        return cbdatas;
    }

    //开普勒方程数值解法，参考：http://murison.alpheratz.net/dynamics/twobody/KeplerIterations_summary.pdf
    //开普勒方程的三阶近似
    private static float KeplerStart3(float OrbitalEccentricit, float MeanAnomaly)
    {
        float t33, t34, t35;
        t33 = Mathf.Cos(OrbitalEccentricit);
        t34 = Mathf.Pow(OrbitalEccentricit, 2);
        t35 = t34 * OrbitalEccentricit;
        return MeanAnomaly + (-0.5f * t35 + OrbitalEccentricit + (t34 + 1.5f * t33 * t35) * t33) * Mathf.Sin(MeanAnomaly);
    }
    private static float eps3(float OrbitalEccentricit, float MeanAnomaly, float x)
    {
        float t1, t2, t3, t4, t5, t6;
        t1 = Mathf.Cos(x);
        t2 = OrbitalEccentricit * t1 - 1;
        t3 = Mathf.Sin(x);
        t4 = OrbitalEccentricit * t3;
        t5 = -x + t4 + MeanAnomaly;
        t6 = t5 / (0.5f * t4 * t5 / t2 + t2);
        return t5 / ((0.5f * t3 - 1 / 6 * t1 * t6) * OrbitalEccentricit * t6 + t2);
    }
    //把平近点角转换成偏近点角
    public static float MAnomalyToEAnomaly(float OrbitalEccentricit, float MeanAnomaly, double tolerance = 1e-5)
    {
        float EccentricAnomaly = 0, EccentricAnomaly0, MeanAnomalyNormal;
        double dEccentricAnomaly;
        int count = 0;
        MeanAnomalyNormal = MeanAnomaly % (2 * Mathf.PI);
        EccentricAnomaly0 = KeplerStart3(OrbitalEccentricit, MeanAnomalyNormal);
        dEccentricAnomaly = tolerance + 1;

        while (dEccentricAnomaly > tolerance)
        {
            count++;
            EccentricAnomaly = EccentricAnomaly0 - eps3(OrbitalEccentricit, MeanAnomalyNormal, EccentricAnomaly0);
            dEccentricAnomaly = Math.Abs(EccentricAnomaly - EccentricAnomaly0);
            EccentricAnomaly0 = EccentricAnomaly;
            if (count >= 100)
            {
                Debug.Log("Astounding! KeplerSolve failed to converge!");
                Debug.Log("输入进来的平近点角为：" + MeanAnomaly);
                Debug.Log("输入进来的标准化平近点角为：" + MeanAnomalyNormal);
                break;
            }
        }

        return EccentricAnomaly;
    }
    //把偏近点角转换成真近点角
    public static float EAnomalyToTAnomaly(float OrbitalEccentricit, float EccentricAnomaly)
    {
        float temp = Mathf.Cos(EccentricAnomaly);
        if (EccentricAnomaly%(2*Mathf.PI)>Mathf.PI) 
            return 2 * Mathf.PI - Mathf.Acos((temp - OrbitalEccentricit) / (1 - OrbitalEccentricit * temp));
        else 
            return Mathf.Acos((temp - OrbitalEccentricit) / (1 - OrbitalEccentricit * temp));
    }
    //把真近点角转换成偏近点角
    public static float TAnomalyToEAnomaly(float OrbitalEccentricit, float TrueAnomaly)
    {
        float temp = Mathf.Cos(TrueAnomaly);
        if (TrueAnomaly % (2 * Mathf.PI) > Mathf.PI)
            return 2 * Mathf.PI - Mathf.Acos((OrbitalEccentricit + temp) / (1 + OrbitalEccentricit * temp));
        else
            return Mathf.Acos((OrbitalEccentricit + temp) / (1 + OrbitalEccentricit * temp));
    }
    //把偏近点角转换成平近点角
    public static float EAnomalyToMAnomaly(float OrbitalEccentricit, float EccentricAnomaly)
    {
        return (EccentricAnomaly - OrbitalEccentricit * Mathf.Sin(EccentricAnomaly)) % (2 * Mathf.PI);
    }
    //把平近点角转换成真近点角
    public static float MAnomalyToTAnomaly(float OrbitalEccentricit, float MeanAnomaly, double tolerance = 1e-5)
    {
        return EAnomalyToTAnomaly(OrbitalEccentricit, MAnomalyToEAnomaly(OrbitalEccentricit, MeanAnomaly, tolerance));
    }
    //把真近点角转换成平近点角
    public static float TAnomalyToMAnomaly(float OrbitalEccentricit, float TrueAnomaly)
    {
        return EAnomalyToMAnomaly(OrbitalEccentricit, TAnomalyToEAnomaly(OrbitalEccentricit, TrueAnomaly));
    }
}