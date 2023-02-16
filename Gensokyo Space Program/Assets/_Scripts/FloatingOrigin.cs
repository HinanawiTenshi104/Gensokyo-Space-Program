using System.Collections.Generic;
using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    [SerializeField] bool activeFloatingOrigin = true;
    [SerializeField] float maxDistance = 200;   //unity length unit
    [SerializeField] float maxVelocity = 200;

    GameObject originObject;    //Ҫһֱ�Ƶ�ԭ�������
    List<GameObject> originsNeededToMove;    //Ҫ���ƶ�������ϵ������ԭ������
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
                    if (i == 0)//ΪҪһֱ�Ƶ�ԭ�������
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
                //Debug.Log("�����ƶ�ԭ�㣡");
            }
        }
    }
}
