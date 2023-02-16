using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

    int NameColumnWidth = 100;
    int Vector3ColumnWidth = 300;

    bool showPlayerCoordinateSystem;
    bool showCelestialBodyCoordinateSystem;
    bool showSunCoordinateSystem;

    Vector2 scrollPos;
    #endregion
    void OnGUI()
    {
        #region Debugger设置
        EditorGUILayout.LabelField("名称列宽");
        NameColumnWidth = EditorGUILayout.IntField(NameColumnWidth);
        EditorGUILayout.LabelField("Vector3列宽");
        Vector3ColumnWidth = EditorGUILayout.IntField(Vector3ColumnWidth);

        showPlayerCoordinateSystem = EditorGUILayout.Toggle("显示玩家坐标系", showPlayerCoordinateSystem);
        showSunCoordinateSystem = EditorGUILayout.Toggle("显示太阳信息", showSunCoordinateSystem);
        showCelestialBodyCoordinateSystem = EditorGUILayout.Toggle("显示星球坐标系", showCelestialBodyCoordinateSystem);
        #endregion
        DrawHeaders();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if (showPlayerCoordinateSystem)
            DrawPlayerCoordinate();

        if (showSunCoordinateSystem)
            DrawSunCoordinate();

        if (showCelestialBodyCoordinateSystem)
            DrawCelestialBodysCoordinate();

        EditorGUILayout.EndScrollView();

        Repaint();
    }

    void DrawHeaders()
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("名称", EditorStyles.boldLabel, GUILayout.Width(NameColumnWidth));
        EditorGUILayout.LabelField("原点", GUILayout.Width(Vector3ColumnWidth));
        EditorGUILayout.LabelField("位置", GUILayout.Width(Vector3ColumnWidth));
        EditorGUILayout.LabelField("速度", GUILayout.Width(Vector3ColumnWidth));

        EditorGUILayout.EndHorizontal();
    }
    void DrawPlayerCoordinate()
    {
        EditorGUILayout.BeginHorizontal();

        //名称，原点，位置和速度
        EditorGUILayout.LabelField("Player", GUILayout.Width(NameColumnWidth));
        EditorGUILayout.LabelField(Vector3.zero.ToString(), GUILayout.Width(Vector3ColumnWidth));
        EditorGUILayout.LabelField($" ({player.transform.position})", GUILayout.Width(Vector3ColumnWidth));
        EditorGUILayout.LabelField($" ({player.GetComponent<Rigidbody>().velocity})", GUILayout.Width(Vector3ColumnWidth));

        EditorGUILayout.EndHorizontal();
    }

    void DrawSunCoordinate()
    {
        EditorGUILayout.BeginHorizontal();

        //名称，原点，位置和速度
        EditorGUILayout.LabelField("Sun", GUILayout.Width(NameColumnWidth));
        EditorGUILayout.LabelField(sun.transform.position.ToString(), GUILayout.Width(Vector3ColumnWidth));
        EditorGUILayout.LabelField($" ({sun.transform.position})", GUILayout.Width(Vector3ColumnWidth));
        EditorGUILayout.LabelField($" ({sun.GetComponent<CelestialBodyDataHolder>().celestialBodyData.Velocity})", GUILayout.Width(Vector3ColumnWidth));

        EditorGUILayout.EndHorizontal();
    }

    void DrawCelestialBodysCoordinate()
    {
        foreach (var cbdata in cbdatas)
        {
            //名称，原点，位置和速度
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(cbdata.CelestialBodyName, GUILayout.Width(NameColumnWidth));
            EditorGUILayout.LabelField(sun.transform.position.ToString(), GUILayout.Width(Vector3ColumnWidth));
            EditorGUILayout.LabelField($" ({cbdata.Position})", GUILayout.Width(Vector3ColumnWidth));
            EditorGUILayout.LabelField($" ({cbdata.Velocity})", GUILayout.Width(Vector3ColumnWidth));

            EditorGUILayout.EndHorizontal();
        }
    }
}