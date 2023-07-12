using UnityEngine;
using System.IO;
using UnityEngine.Experimental.Rendering;
using System.Collections.Generic;

namespace Qkyo.Utils
{
    public static class Gizmos
    {
        public static GameObject CreateInstance(string component) 
        {
            var instance = new GameObject(component);

            if (component == "LineRenderer")
                instance.AddComponent<LineRenderer>();

            return instance;
        }

        public static void DrawLines(List<Vector3> points, LineRenderer renderer)
        {
            renderer.SetPositions(points.ToArray());
        }
    }
}
