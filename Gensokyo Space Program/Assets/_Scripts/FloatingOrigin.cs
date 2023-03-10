using System.Collections.Generic;
using UnityEngine;

namespace GensokyoSpaceProgram._Scripts
{
    public class FloatingOrigin : MonoBehaviour
    {
        [SerializeField] bool activeFloatingOrigin = true;
        [SerializeField] float maxDistance = 200;   //unity length unit
        [SerializeField] float maxVelocity = 200;

        GameObject _originObject;    //要一直移到原点的物体
        List<GameObject> _originsNeededToMove;    //要被移动的坐标系的坐标原点物体
        List<CoordinateSystemOrigin> _coordinateSystemOrigins;

        void Start()
        {
            _originObject = gameObject;
            _originsNeededToMove = new List<GameObject>();
            _coordinateSystemOrigins = new List<CoordinateSystemOrigin>();

            _originsNeededToMove.Add(_originObject);
            _originsNeededToMove.AddRange(GameObject.FindGameObjectsWithTag("Origin"));

            for (int i = 0; i < _originsNeededToMove.Count; i++)
            {
                _coordinateSystemOrigins.Add((i == 0)
                    ? null
                    : _originsNeededToMove[i].GetComponent<CoordinateSystemOrigin>());
            }
        }
        void LateUpdate()
        {
            if (activeFloatingOrigin)
            {
                if ((_originObject.transform.position.magnitude > maxDistance) || (_originObject.GetComponent<Rigidbody>().velocity.magnitude > maxVelocity))
                {
                    Vector3 deltaPosition = -_originObject.transform.position;
                    Vector3 deltaVelocity = -_originObject.GetComponent<Rigidbody>().velocity;

                    for (int i = 0; i < _originsNeededToMove.Count; i++)
                    {
                        if (i == 0)//为要一直移到原点的物体
                        {
                            _originsNeededToMove[i].transform.position += deltaPosition;
                            _originsNeededToMove[i].GetComponent<Rigidbody>().velocity += deltaVelocity;
                        }
                        else
                        {
                            _coordinateSystemOrigins[i].position += deltaPosition;
                            _coordinateSystemOrigins[i].velocity += deltaVelocity;
                        }
                    }
                    //Debug.Log("重新移动原点！");
                }
            }
        }
    }
}
