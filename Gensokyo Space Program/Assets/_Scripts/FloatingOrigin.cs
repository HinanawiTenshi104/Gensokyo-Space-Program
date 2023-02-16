using System.Collections.Generic;
using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    [SerializeField] bool activeFloatingOrigin = true;
    [SerializeField] float maxDistance = 200;   //unity length unit
    [SerializeField] float maxVelocity = 200;

    GameObject originObject;    //要一直移到原点的物体
    List<GameObject> originsNeededToMove;    //要被移动的坐标系的坐标原点物体
    List<CoordinateSystemOrigin> coordinateSystemOrigins;

    void Start()
    {
        originObject = gameObject;
        originsNeededToMove = new List<GameObject>();
        coordinateSystemOrigins = new List<CoordinateSystemOrigin>();

        originsNeededToMove.Add(originObject);
        originsNeededToMove.AddRange(GameObject.FindGameObjectsWithTag("Origin"));

        for (int i = 0; i < originsNeededToMove.Count; i++)
        {
            if (i == 0)
            {
                coordinateSystemOrigins.Add(null);
            }
            else
            {
                coordinateSystemOrigins.Add(originsNeededToMove[i].GetComponent<CoordinateSystemOrigin>());
            }
        }
    }
    void LateUpdate()
    {
        if (activeFloatingOrigin)
        {
            if ((originObject.transform.position.magnitude > maxDistance) || (originObject.GetComponent<Rigidbody>().velocity.magnitude > maxVelocity))
            {
                Vector3 deltaPosition = -originObject.transform.position;
                Vector3 deltaVelocity = -originObject.GetComponent<Rigidbody>().velocity;

                for (int i = 0; i < originsNeededToMove.Count; i++)
                {
                    if (i == 0)//为要一直移到原点的物体
                    {
                        originsNeededToMove[i].transform.position += deltaPosition;
                        originsNeededToMove[i].GetComponent<Rigidbody>().velocity += deltaVelocity;
                    }
                    else
                    {
                        coordinateSystemOrigins[i].Position += deltaPosition;
                        coordinateSystemOrigins[i].Velocity += deltaVelocity;
                    }
                }
                //Debug.Log("重新移动原点！");
            }
        }
    }
}
