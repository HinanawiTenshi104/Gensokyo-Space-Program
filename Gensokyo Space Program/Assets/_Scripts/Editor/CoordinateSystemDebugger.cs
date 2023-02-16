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
    #region ����
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
        #region Debugger����
        EditorGUILayout.LabelField("�����п�");
        NameColumnWidth = EditorGUILayout.IntField(NameColumnWidth);
        EditorGUILayout.LabelField("Vector3�п�");
        Vector3ColumnWidth = EditorGUILayout.IntField(Vector3ColumnWidth);

        showPlayerCoordinateSystem = EditorGUILayout.Toggle("��ʾ�������ϵ", showPlayerCoordinateSystem);
        showSunCoordinateSystem = EditorGUILayout.Toggle("��ʾ̫����Ϣ", showSunCoordinateSystem);
        showCelestialBodyCoordinateSystem = EditorGUILayout.Toggle("��ʾ��������ϵ", showCelestialBodyCoordinateSystem);
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

        GUILayout.Label("����", EditorStyles.boldLabel, GUILayout.Width(NameColumnWidth));
        EditorGUILayout.LabelField("ԭ��", GUILayout.Width(Vector3ColumnWidth));
        EditorGUILayout.LabelField("λ��", GUILayout.Width(Vector3ColumnWidth));
        EditorGUILayout.LabelField("�ٶ�", GUILayout.Width(Vector3ColumnWidth));

        EditorGUILayout.EndHorizontal();
    }
    void DrawPlayerCoordinate()
    {
        EditorGUILayout.BeginHorizontal();

        //���ƣ�ԭ�㣬λ�ú��ٶ�
        EditorGUILayout.LabelField("Player", GUILayout.Width(NameColumnWidth));
        EditorGUILayout.LabelField(Vector3.zero.ToString(), GUILayout.Width(Vector3ColumnWidth));
        EditorGUILayout.LabelField($" ({player.transform.position})", GUILayout.Width(Vector3ColumnWidth));
        EditorGUILayout.LabelField($" ({player.GetComponent<Rigidbody>().velocity})", GUILayout.Width(Vector3ColumnWidth));

        EditorGUILayout.EndHorizontal();
    }

    void DrawSunCoordinate()
    {
        EditorGUILayout.BeginHorizontal();

        //���ƣ�ԭ�㣬λ�ú��ٶ�
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
            //���ƣ�ԭ�㣬λ�ú��ٶ�
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(cbdata.CelestialBodyName, GUILayout.Width(NameColumnWidth));
            EditorGUILayout.LabelField(sun.transform.position.ToString(), GUILayout.Width(Vector3ColumnWidth));
            EditorGUILayout.LabelField($" ({cbdata.Position})", GUILayout.Width(Vector3ColumnWidth));
            EditorGUILayout.LabelField($" ({cbdata.Velocity})", GUILayout.Width(Vector3ColumnWidth));

            EditorGUILayout.EndHorizontal();
        }
    }
}