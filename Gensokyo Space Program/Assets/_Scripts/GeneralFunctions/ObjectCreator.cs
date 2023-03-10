using UnityEngine;

//参考:https://www.youtube.com/watch?v=Q3JFtUEUM2M
namespace _Scripts.GeneralFunctions
{
    public static class ObjectCreator
    {
        //创建一个样子是平面的GameObject，初始朝向为XY平面(也就是normal vector=vector.forward)
        public static GameObject CreatePlane(string name, float width, float height, Material mat)
        {
            GameObject go = new GameObject(name);
            MeshFilter mf = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
            MeshRenderer mr = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

            Mesh m = new Mesh();
            m.vertices = new []
            {
                new Vector3(-width/2.0f, -height/2.0f, 0),
                new Vector3(width/2.0f, -height/2.0f, 0),
                new Vector3(width/2.0f, height/2.0f, 0),
                new Vector3(-width/2.0f, height/2.0f, 0),
            };

            m.uv = new []
            {
                new Vector2(0,0),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0)
            };
            m.triangles = new [] { 0, 1, 2, 0, 2, 3 };
            
            if (mf != null) mf.mesh= m;
            if (mr != null) mr.material = mat;

            m.RecalculateBounds();
            m.RecalculateNormals();

            return go;
        }

        public static GameObject CreateSphere(string name, float radius, GameObject parent, Material mat = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.localScale = radius * Vector3.one;
            if (mat != null)
            {
                go.GetComponent<MeshRenderer>().material = mat;
            }
            go.transform.parent = parent.transform;
            return go;
        }
    }
}
