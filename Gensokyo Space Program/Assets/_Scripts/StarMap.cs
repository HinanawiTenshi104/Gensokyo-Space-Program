using System.Collections;
using System.Collections.Generic;
using _Scripts.CelestialBody;
using _Scripts.CelestialBody.Function;
using _Scripts.Managers;
using UnityEngine;

namespace GensokyoSpaceProgram._Scripts
{
    public class StarMap : MonoBehaviour
    {
        Transform _icon;
        GameObject[] _icons;
        ScenesTracker _scenesTracker;
        [SerializeField] Camera starMapCamera;
        [SerializeField] GameObject focusObject;
        [SerializeField] Vector3 focusPoint;
        [SerializeField] float distanceToTheFocusPoint;
        Camera[] _cameras;
        readonly float _margin = 100;//理论值是icon的半径(当星球质心刚好在屏幕边缘的时候，icon还有一半在屏幕内)

        private bool _inStarMapOld;
        private Vector3 _previousMousePosition;
        private int _tapTimes;
        private readonly float _resetTimer=0.3f;
        private readonly float _selectFocusDistance = 30f;//30像素

        private void Start()
        {
            _icon = transform.Find("Icons");
            if (_icon == null)
            {
                Debug.Log("Starmap未能找到Icons物体！");
                return;
            }
            _icons = new GameObject[_icon.childCount];
            for (int i = 0; i < _icon.childCount; i++)
            {
                _icons[i] = _icon.GetChild(i).gameObject;
            }

            GameObject temp = GameObject.Find("ScenesTracker");
            if (temp == null)
            {
                Debug.Log("StarMap找不到ScenesTracker!");
            }
            else
            {
                _scenesTracker = temp.GetComponent<ScenesTracker>();
            }

            ResizeIcons(0.2f);
            foreach (GameObject icon in _icons)
            {
                icon.SetActive(false);
            }

            focusObject = GameObject.FindGameObjectWithTag("Sun");
            distanceToTheFocusPoint = (float)(focusObject.GetComponent<CelestialBodyDataHolder>().celestialBodyData.MeanRadius * Constants.PlanetRadiusShrinkFactor * 10);
        }
        private void Update()
        {
            if (_scenesTracker.inStarMap)
            {
                if (_scenesTracker.inStarMap != _inStarMapOld)
                {
                    _inStarMapOld = _scenesTracker.inStarMap;
                    starMapCamera.enabled = false;
                    _cameras = Camera.allCameras;
                    SwitchCameraToStarMapCamera();

                    distanceToTheFocusPoint = 3000;
                }//进入星图初始设置
                UpdateIcons();
                #region 相机位置设置
                #region 左键
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    StartCoroutine(nameof(ResetTapTimes));
                    _tapTimes++;
                }
                if (_tapTimes >= 2)

                {
                    _tapTimes = 0;
                    #region 双击鼠标左键事件
                    List<GameObject> possibleFocus = new List<GameObject>();
                    List<float> possibleFocusDistance = new List<float>();
                    foreach (GameObject icon in _icons)//遍历可能的焦点物体
                    {
                        float distance = Vector3.Distance(icon.transform.position, Input.mousePosition);
                        if (distance <= _selectFocusDistance)
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
                        if (objectOfTheIcon.CompareTag("CelestialBody"))
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
                    Vector3 direction = _previousMousePosition - starMapCamera.ScreenToViewportPoint(Input.mousePosition);

                    starMapCamera.transform.Rotate(new Vector3(1, 0, 0), direction.y * 180);
                    starMapCamera.transform.Rotate(new Vector3(0, 1, 0), -direction.x * 180);
                }
                _previousMousePosition = starMapCamera.ScreenToViewportPoint(Input.mousePosition);

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

                Transform temp = starMapCamera.transform;
                temp.position = focusPoint - distanceToTheFocusPoint * temp.forward;

                #endregion
            }
            else
            {
                if (_scenesTracker.inStarMap != _inStarMapOld)
                {
                    _inStarMapOld = _scenesTracker.inStarMap;
                    foreach (GameObject icon in _icons)
                    {
                        icon.SetActive(false);
                    }
                    SetCamerasBack();
                }//离开星图设置
            }
        }

        public void ResizeIcons(float size = 1)
        {
            _icon.localScale = size * Vector3.one;
        }
        public GameObject GetObjectOfTheIcon(GameObject icon)
        {
            string nameOfTheObject = icon.name.Replace(" Icon", "");
            GameObject objectOfTheIcon = (nameOfTheObject == "Ship")
                ? GameObject.FindGameObjectWithTag("Player")
                : GameObject.Find(nameOfTheObject);
            if (objectOfTheIcon == null)
            {
                Debug.Log("找不到" + icon.name + "对应的游戏物体！");
                return null;
            }

            return objectOfTheIcon;
        }
        public void UpdateIcons()
        {
            foreach (GameObject icon in _icons)
            {
                GameObject objectOfTheIcon = GetObjectOfTheIcon(icon);
                Vector3 position = (objectOfTheIcon.CompareTag("CelestialBody"))
                    ? OrbitalMechanicsFunctions.CalculatePlanetWorldPosition(objectOfTheIcon)
                    : objectOfTheIcon.transform.position;

                PlaceIcon(icon, position);
            }
        }
        public void PlaceIcon(GameObject icon, Vector3 positionInWorld)
        {
            Vector3 screenPoint = starMapCamera.WorldToScreenPoint(positionInWorld);
            if ((screenPoint.x > -_margin) && (screenPoint.x < starMapCamera.pixelWidth + _margin) &&
                (screenPoint.y > -_margin) && (screenPoint.y < starMapCamera.pixelHeight + _margin) &&
                (screenPoint.z > -_margin))
            {
                //在屏幕上
                icon.SetActive(true);
                icon.transform.position = new Vector3(screenPoint.x, screenPoint.y, 0);
            }
            else
            {
                icon.SetActive(false);
            }
        }
        public void SwitchCameraToStarMapCamera()
        {
            foreach (Camera cam in _cameras)
            {
                cam.enabled = false;
            }
            starMapCamera.enabled = true;
        }
        public void SetCamerasBack()
        {
            foreach (Camera cam in _cameras)
            {
                cam.enabled = true;
            }
            starMapCamera.enabled = false;
        }

        private IEnumerator ResetTapTimes()
        {
            yield return new WaitForSeconds(_resetTimer);
            _tapTimes = 0;
        }
    }
}