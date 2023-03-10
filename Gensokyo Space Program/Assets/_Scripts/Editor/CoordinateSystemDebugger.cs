using System.Collections.Generic;
using _Scripts.CelestialBody;
using _Scripts.CelestialBody.Function;
using _Scripts.CelestialBody.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace GensokyoSpaceProgram._Scripts.Editor
{
    public class CoordinateSystemDebugger : EditorWindow
    {
        [MenuItem("Game/Coordinate System Debugger")]
        private static void OpenWindow()
        {
            var window = GetWindow<CoordinateSystemDebugger>();

            GameObject[] planets = GameObject.FindGameObjectsWithTag("CelestialBody");

            window.player = GameObject.FindGameObjectWithTag("Player");
            window.sun = GameObject.FindGameObjectWithTag("Sun");
            window.cbdatas = OrbitalMechanicsFunctions.GetCelestialBodyDatas(planets);
        }
        #region 变量
        public GameObject player;
        public GameObject sun;
        public List<CelestialBodyDataSO> cbdatas;

        int _nameColumnWidth = 100;
        int _vector3ColumnWidth = 300;

        bool _showPlayerCoordinateSystem;
        bool _showCelestialBodyCoordinateSystem;
        bool _showSunCoordinateSystem;

        Vector2 _scrollPos;
        #endregion
        void OnGUI()
        {
            #region Debugger设置
            EditorGUILayout.LabelField("名称列宽");
            _nameColumnWidth = EditorGUILayout.IntField(_nameColumnWidth);
            EditorGUILayout.LabelField("Vector3列宽");
            _vector3ColumnWidth = EditorGUILayout.IntField(_vector3ColumnWidth);

            _showPlayerCoordinateSystem = EditorGUILayout.Toggle("显示玩家坐标系", _showPlayerCoordinateSystem);
            _showSunCoordinateSystem = EditorGUILayout.Toggle("显示太阳信息", _showSunCoordinateSystem);
            _showCelestialBodyCoordinateSystem = EditorGUILayout.Toggle("显示星球坐标系", _showCelestialBodyCoordinateSystem);
            #endregion
            DrawHeaders();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (_showPlayerCoordinateSystem)
                DrawPlayerCoordinate();

            if (_showSunCoordinateSystem)
                DrawSunCoordinate();

            if (_showCelestialBodyCoordinateSystem)
                DrawCelestialBodysCoordinate();

            EditorGUILayout.EndScrollView();

            Repaint();
        }

        void DrawHeaders()
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("名称", EditorStyles.boldLabel, GUILayout.Width(_nameColumnWidth));
            EditorGUILayout.LabelField("原点", GUILayout.Width(_vector3ColumnWidth));
            EditorGUILayout.LabelField("位置", GUILayout.Width(_vector3ColumnWidth));
            EditorGUILayout.LabelField("速度", GUILayout.Width(_vector3ColumnWidth));

            EditorGUILayout.EndHorizontal();
        }
        void DrawPlayerCoordinate()
        {
            EditorGUILayout.BeginHorizontal();

            //名称，原点，位置和速度
            EditorGUILayout.LabelField("Player", GUILayout.Width(_nameColumnWidth));
            EditorGUILayout.LabelField(Vector3.zero.ToString(), GUILayout.Width(_vector3ColumnWidth));
            EditorGUILayout.LabelField($" ({player.transform.position})", GUILayout.Width(_vector3ColumnWidth));
            EditorGUILayout.LabelField($" ({player.GetComponent<Rigidbody>().velocity})", GUILayout.Width(_vector3ColumnWidth));

            EditorGUILayout.EndHorizontal();
        }

        void DrawSunCoordinate()
        {
            EditorGUILayout.BeginHorizontal();

            //名称，原点，位置和速度
            Vector3 sunPosition = sun.transform.position;
            EditorGUILayout.LabelField("Sun", GUILayout.Width(_nameColumnWidth));
            EditorGUILayout.LabelField(sunPosition.ToString(), GUILayout.Width(_vector3ColumnWidth));
            EditorGUILayout.LabelField($" ({sunPosition})", GUILayout.Width(_vector3ColumnWidth));
            EditorGUILayout.LabelField($" ({sun.GetComponent<CelestialBodyDataHolder>().celestialBodyData.Velocity})", GUILayout.Width(_vector3ColumnWidth));

            EditorGUILayout.EndHorizontal();
        }

        void DrawCelestialBodysCoordinate()
        {
            foreach (var cbdata in cbdatas)
            {
                //名称，原点，位置和速度
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(cbdata.CelestialBodyName, GUILayout.Width(_nameColumnWidth));
                EditorGUILayout.LabelField(sun.transform.position.ToString(), GUILayout.Width(_vector3ColumnWidth));
                EditorGUILayout.LabelField($" ({cbdata.Position})", GUILayout.Width(_vector3ColumnWidth));
                EditorGUILayout.LabelField($" ({cbdata.Velocity})", GUILayout.Width(_vector3ColumnWidth));

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}