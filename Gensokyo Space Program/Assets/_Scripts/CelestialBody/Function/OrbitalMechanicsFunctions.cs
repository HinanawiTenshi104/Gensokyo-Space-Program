using System;
using System.Collections.Generic;
using _Scripts.CelestialBody.ScriptableObjects;
using UnityEngine;

namespace _Scripts.CelestialBody.Function
{
    public static class OrbitalMechanicsFunctions
    {
        public static void UpdateMeanAnomaly(CelestialBodyDataSO cbdata, float timesinceT0)
        {
            float meanAnomalyAtT0 = cbdata.MeanAnomalyAtT0;
            double orbitalPeriod = cbdata.OrbitalPeriod;

            cbdata.MeanAnomaly = (float)(meanAnomalyAtT0 + 2f * Mathf.PI * timesinceT0 / (orbitalPeriod * Constants.OrbitSizeShrinkFactor * 100)) % (2f * Mathf.PI);
        }
        public static Vector3 CalculatePlanetPosition(float orbitalEccentricity, double etimesP, float argumentOfPeriapsis, float meanAnomaly, Vector3 targetPlaneNormalVector)
        {
            float trueAnomaly = MAnomalyToTAnomaly(orbitalEccentricity, meanAnomaly);
            double radius = etimesP * Constants.OrbitSizeShrinkFactor / (1 - orbitalEccentricity * Math.Cos(trueAnomaly - Math.PI / 2.0f + argumentOfPeriapsis * Mathf.Deg2Rad));
            Vector3 point = new Vector3((float)(radius * Math.Cos(trueAnomaly)), (float)(radius * Math.Sin(trueAnomaly)), 0);
            return Quaternion.FromToRotation(Vector3.forward, targetPlaneNormalVector) * point;
        }
        public static Vector3 CalculatePlanetPosition(CelestialBodyDataSO celestialBodyData)
        {
            return CalculatePlanetPosition(celestialBodyData.OrbitalEccentricity, celestialBodyData.EtimesP, celestialBodyData.ArgumentOfPeriapsis, celestialBodyData.MeanAnomaly, celestialBodyData.PlaneNormalVector);
        }
        public static Vector3 CalculatePlanetWorldPosition(GameObject planet)
        {
            CelestialBodyDataHolder planetcbdataholder = planet.GetComponent<CelestialBodyDataHolder>();
            CelestialBodyDataSO planetcbdata = planetcbdataholder.celestialBodyData;
            Vector3 planetPos;
            if (!planet.CompareTag("CelestialBody"))//没有轨道信息的物体
            {
                return planetcbdata.Position;
            }
            else
            {
                planetPos = CalculatePlanetPosition(planetcbdata);
            }

            GameObject orbitingObject = planetcbdataholder.orbitingObject;
            while (orbitingObject != null)//直到没有中心天体为止一直叠加中心天体的坐标(一直转换坐标系)
            {
                if (orbitingObject.CompareTag("CelestialBody"))
                {
                    CelestialBodyDataSO centralcbdata = orbitingObject.GetComponent<CelestialBodyDataHolder>().celestialBodyData;
                    planetPos += CalculatePlanetPosition(centralcbdata);

                    orbitingObject = orbitingObject.GetComponent<CelestialBodyDataHolder>().orbitingObject;
                }
                else
                {
                    planetPos += orbitingObject.transform.position;
                    break;
                }
            }

            return planetPos;
        }
        public static Vector3 CalculatePointOnTheEllipse(float orbitalEccentricity, double etimesP, float argumentOfPeriapsis, float trueAnomaly)
        {
            double radius = etimesP * Constants.OrbitSizeShrinkFactor / (1 - orbitalEccentricity * Math.Cos(trueAnomaly - Math.PI / 2.0f + argumentOfPeriapsis * Mathf.Deg2Rad));
            return new Vector3((float)(radius * Math.Cos(trueAnomaly)), (float)(radius * Math.Sin(trueAnomaly)), 0);
        }
        public static Vector3 TranslatePoints(Vector3 point, Vector3 targetPlaneNormalVector)//把椭圆轨道平面坐标系上的点转换到赤道坐标系的点
        {
            return Quaternion.FromToRotation(Vector3.forward, targetPlaneNormalVector) * point;
        }
        public static void SetupOrbitVisuaizer(LineRenderer lineRenderer, int segments, float orbitalEccentricity, double etimesP, float argumentOfPeriapsis, Vector3 planeVector)
        {
            Vector3[] points = new Vector3[segments + 1];
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)(2 * Math.PI * i / segments);
                points[i] = TranslatePoints(CalculatePointOnTheEllipse(orbitalEccentricity, etimesP, argumentOfPeriapsis, angle), planeVector.normalized);
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
        private static float KeplerStart3(float orbitalEccentricity, float meanAnomaly)
        {
            float t33, t34, t35;
            t33 = Mathf.Cos(orbitalEccentricity);
            t34 = Mathf.Pow(orbitalEccentricity, 2);
            t35 = t34 * orbitalEccentricity;
            return meanAnomaly + (-0.5f * t35 + orbitalEccentricity + (t34 + 1.5f * t33 * t35) * t33) * Mathf.Sin(meanAnomaly);
        }
        private static float Eps3(float orbitalEccentricity, float meanAnomaly, float x)
        {
            float t1, t2, t3, t4, t5, t6;
            t1 = Mathf.Cos(x);
            t2 = orbitalEccentricity * t1 - 1;
            t3 = Mathf.Sin(x);
            t4 = orbitalEccentricity * t3;
            t5 = -x + t4 + meanAnomaly;
            t6 = t5 / (0.5f * t4 * t5 / t2 + t2);
            return t5 / ((0.5f * t3 - 1f / 6f * t1 * t6) * orbitalEccentricity * t6 + t2);
        }
        //把平近点角转换成偏近点角
        public static float MAnomalyToEAnomaly(float orbitalEccentricity, float meanAnomaly, double tolerance = 1e-5)
        {
            float eccentricAnomaly = 0, eccentricAnomaly0, meanAnomalyNormal;
            double dEccentricAnomaly;
            int count = 0;
            meanAnomalyNormal = meanAnomaly % (2 * Mathf.PI);
            eccentricAnomaly0 = KeplerStart3(orbitalEccentricity, meanAnomalyNormal);
            dEccentricAnomaly = tolerance + 1;

            while (dEccentricAnomaly > tolerance)
            {
                count++;
                eccentricAnomaly = eccentricAnomaly0 - Eps3(orbitalEccentricity, meanAnomalyNormal, eccentricAnomaly0);
                dEccentricAnomaly = Math.Abs(eccentricAnomaly - eccentricAnomaly0);
                eccentricAnomaly0 = eccentricAnomaly;
                if (count >= 100)
                {
                    Debug.Log("Astounding! KeplerSolve failed to converge!");
                    Debug.Log("输入进来的平近点角为：" + meanAnomaly);
                    Debug.Log("输入进来的标准化平近点角为：" + meanAnomalyNormal);
                    break;
                }
            }

            return eccentricAnomaly;
        }
        //把偏近点角转换成真近点角
        public static float EAnomalyToTAnomaly(float orbitalEccentricity, float eccentricAnomaly)
        {
            float temp = Mathf.Cos(eccentricAnomaly);
            if (eccentricAnomaly%(2*Mathf.PI)>Mathf.PI) 
                return 2 * Mathf.PI - Mathf.Acos((temp - orbitalEccentricity) / (1 - orbitalEccentricity * temp));
            else 
                return Mathf.Acos((temp - orbitalEccentricity) / (1 - orbitalEccentricity * temp));
        }
        //把真近点角转换成偏近点角
        public static float TAnomalyToEAnomaly(float orbitalEccentricity, float trueAnomaly)
        {
            float temp = Mathf.Cos(trueAnomaly);
            if (trueAnomaly % (2 * Mathf.PI) > Mathf.PI)
                return 2 * Mathf.PI - Mathf.Acos((orbitalEccentricity + temp) / (1 + orbitalEccentricity * temp));
            else
                return Mathf.Acos((orbitalEccentricity + temp) / (1 + orbitalEccentricity * temp));
        }
        //把偏近点角转换成平近点角
        public static float EAnomalyToMAnomaly(float orbitalEccentricity, float eccentricAnomaly)
        {
            return (eccentricAnomaly - orbitalEccentricity * Mathf.Sin(eccentricAnomaly)) % (2 * Mathf.PI);
        }
        //把平近点角转换成真近点角
        public static float MAnomalyToTAnomaly(float orbitalEccentricity, float meanAnomaly, double tolerance = 1e-5)
        {
            return EAnomalyToTAnomaly(orbitalEccentricity, MAnomalyToEAnomaly(orbitalEccentricity, meanAnomaly, tolerance));
        }
        //把真近点角转换成平近点角
        public static float TAnomalyToMAnomaly(float orbitalEccentricity, float trueAnomaly)
        {
            return EAnomalyToMAnomaly(orbitalEccentricity, TAnomalyToEAnomaly(orbitalEccentricity, trueAnomaly));
        }
    }
}