using UnityEngine;

public class ScenesTracker : MonoBehaviour
{
    public bool InStarMap;


    private void Start()
    {
        InStarMap = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown("m"))
        {
            InStarMap = !InStarMap;
        }
    }
}