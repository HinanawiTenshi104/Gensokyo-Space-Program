using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class StarMap : MonoBehaviour
{
    Transform icon;
    GameObject[] icons;
    ScenesTracker ScenesTracker;
    [SerializeField] Camera starMapCamera;
    [SerializeField] GameObject focusObject;
    [SerializeField] Vector3 focusPoint;
    [SerializeField] float distanceToTheFocusPoint;
    Camera[] cameras;
    float margin = 100;//理论值是icon的半径(当星球质心刚好在屏幕边缘的时候，icon还有一半在屏幕内)

    private bool InStarMap_old;
    private Vector3 previousMousePosition;
    private int tapTimes;
    private float resetTimer=0.3f;
    private float selectFocusDistance = 30f;//30像素

    private void Start()
    {
        icon = transform.Find("Icons");
        if (icon == null)
        {
            Debug.Log("Starmap未能找到Icons物体！");
            return;
        }
        icons = new GameObject[icon.childCount];
        for (int i = 0; i < icon.childCount; i++)
        {
            icons[i] = icon.GetChild(i).gameObject;
        }

        GameObject temp = GameObject.Find("ScenesTracker");
        if (temp == null)
        {
            Debug.Log("StarMap找不到ScenesTracker!");
        }
        else
        {
            ScenesTracker = temp.GetComponent<ScenesTracker>();
        }

        ResizeIcons(0.2f);
        foreach (GameObject _icon in icons)
        {
            _icon.SetActive(false);
        }

        focusObject = GameObject.FindGameObjectWithTag("Sun");
        distanceToTheFocusPoint = (float)(focusObject.GetComponent<CelestialBodyDataHolder>().celestialBodyData.MeanRadius * Constants.PlanetRadiusShrinkFactor * 10);
    }
    private void Update()
    {
        if (ScenesTracker.InStarMap)
        {
            if (ScenesTracker.InStarMap != InStarMap_old)
            {
                InStarMap_old = ScenesTracker.InStarMap;
                starMapCamera.enabled = false;
                cameras = Camera.allCameras;
                SwitchCameraToStarMapCamera();

                distanceToTheFocusPoint = 3000;
            }//进入星图初始设置
            UpdateIcons();
            #region 相机位置设置
            #region 左键
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                StartCoroutine("ResetTapTimes");
                tapTimes++;
            }
            if (tapTimes >= 2)

            {
                tapTimes = 0;
                #region 双击鼠标左键事件
                List<GameObject> possibleFocus = new List<GameObject>();
                List<float> possibleFocusDistance = new List<float>();
                foreach (GameObject icon in icons)//遍历可能的焦点物体
                {
                    float distance = Vector3.Distance(icon.transform.position, Input.mousePosition);
                    if (distance <= selectFocusDistance)
                    {
                        possibleFocus.Add(icon);
                        possibleFocusDistance.Add(distance);
                    }
                }
                if (possibleFocus.Count != 0)//如果有候选的焦点物体
                {
                    float minDistance = possibleFocusDistance[0];
                    int minDistanceIndex = 0;
                    for (int i = 1; i < possibleFocus.Count; i++)//就选择离鼠标最近的焦点物体
                    {
                        if (possibleFocusDistance[i] < minDistance)
                        {
                            minDistance = possibleFocusDistance[i];
                            minDistanceIndex = i;
                        }
                    }

                    GameObject objectOfTheIcon = GetObjectOfTheIcon(possibleFocus[minDistanceIndex]);
                    if (objectOfTheIcon == null)
                    {
                        Debug.Log("找不到" + possibleFocus[minDistanceIndex].name + "对应的游戏物体！");
                        return;
                    }
                    focusObject = objectOfTheIcon;
                    if (objectOfTheIcon.tag == "CelestialBody")
                    {
                        distanceToTheFocusPoint = (float)(focusObject.GetComponent<CelestialBodyDataHolder>().celestialBodyData.MeanRadius * Constants.PlanetRadiusShrinkFactor * 10);
                    }
                    else
                    {
                        distanceToTheFocusPoint = 10;
                    }
                }
                #endregion
            }
            #endregion
            #region 右键
            if (Input.GetMouseButton(1))//按住右键
            {
                Vector3 direction = previousMousePosition - starMapCamera.ScreenToViewportPoint(Input.mousePosition);

                starMapCamera.transform.Rotate(new Vector3(1, 0, 0), direction.y * 180);
                starMapCamera.transform.Rotate(new Vector3(0, 1, 0), -direction.x * 180);
            }
            previousMousePosition = starMapCamera.ScreenToViewportPoint(Input.mousePosition);

            #endregion
            #region 滚轮
            float deltaDistanceToTheFocusPoint = 0.1f * distanceToTheFocusPoint;
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                distanceToTheFocusPoint += deltaDistanceToTheFocusPoint;
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                distanceToTheFocusPoint -= deltaDistanceToTheFocusPoint;
            }
            #endregion
            if (focusObject != null)
            {
                focusPoint = focusObject.transform.position;
            }
            starMapCamera.transform.position = focusPoint - distanceToTheFocusPoint * starMapCamera.transform.forward;
            #endregion
        }
        else
        {
            if (ScenesTracker.InStarMap != InStarMap_old)
            {
                InStarMap_old = ScenesTracker.InStarMap;
                foreach (GameObject _icon in icons)
                {
                    _icon.SetActive(false);
                }
                SetCamerasBack();
            }//离开星图设置
        }
    }

    public void ResizeIcons(float size = 1)
    {
        icon.localScale = size * Vector3.one;
    }
    public GameObject GetObjectOfTheIcon(GameObject icon)
    {
        string nameOfTheObject = icon.name.Replace(" Icon", "");
        GameObject objectOfTheIcon;
        if (nameOfTheObject == "Ship")
        {
            objectOfTheIcon = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            objectOfTheIcon = GameObject.Find(nameOfTheObject);
        }
        if (objectOfTheIcon == null)
        {
            Debug.Log("找不到" + icon.name + "对应的游戏物体！");
            return null;
        }

        return objectOfTheIcon;
    }
    public void UpdateIcons()
    {
        for (int i = 0; i < icons.Length; i++)
        {
            GameObject objectOfTheIcon = GetObjectOfTheIcon(icons[i]);
            Vector3 position;

            if (objectOfTheIcon.tag == "CelestialBody")
            {
                position = OrbitalMechanicsFunctions.CalculatePlanetWorldPosition(objectOfTheIcon);
            }
            else
            {
                position = objectOfTheIcon.transform.position;
            }
            PlaceIcon(icons[i], position);
        }
    }
    public void PlaceIcon(GameObject Icon, Vector3 PositionInWorld)
    {
        Vector3 screenPoint = starMapCamera.WorldToScreenPoint(PositionInWorld);
        if ((screenPoint.x > -margin) && (screenPoint.x < starMapCamera.pixelWidth + margin) &&
            (screenPoint.y > -margin) && (screenPoint.y < starMapCamera.pixelHeight + margin) &&
            (screenPoint.z > -margin))
        {
            //在屏幕上
            Icon.SetActive(true);
            Icon.transform.position = new Vector3(screenPoint.x, screenPoint.y, 0);
        }
        else
        {
            Icon.SetActive(false);
        }
    }
    public void SwitchCameraToStarMapCamera()
    {
        foreach (Camera camera in cameras)
        {
            camera.enabled = false;
        }
        starMapCamera.enabled = true;
    }
    public void SetCamerasBack()
    {
        foreach (Camera camera in cameras)
        {
            camera.enabled = true;
        }
        starMapCamera.enabled = false;
    }

    private IEnumerator ResetTapTimes()
    {
        yield return new WaitForSeconds(resetTimer);
        tapTimes = 0;
    }
}