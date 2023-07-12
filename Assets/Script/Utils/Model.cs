using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

namespace Qkyo.Utils
{
    public static class Model
    {
        public static void Unitize(ref List<Vector3>vertex, Vector3 center, Vector3 max, Vector3 min)
        {
            Vector3 distance = max - min;
            float scale = 2 / Mathf.Max( distance.x, Mathf.Max( distance.y, distance.z ) );

            for (int i = 0; i < vertex.Count; i++)
            { 
                vertex[i] -= center;
                vertex[i] *= scale;
            }
        }

        public static Vector3 Unitize(Vector3 vertex, Vector3 center, Vector3 max, Vector3 min)
        {
            Vector3 distance = max - min;
            float scale = 2 / Mathf.Max(distance.x, Mathf.Max(distance.y, distance.z));

            vertex -= center;
            vertex *= scale;

            return vertex;
        }

        public static List<Vector3> Unitize(List<Vector3> vertex)
        {
            List<Vector3> m_list = new List<Vector3>(vertex);

            Vector3 max, min, center;
            max = vertex[0];
            min = vertex[0];

            // Find the max and min
            for (int i = 1; i < vertex.Count; i++)
            {
                max.x = Mathf.Max( vertex[i].x, max.x );
                max.y = Mathf.Max( vertex[i].y, max.y );
                max.z = Mathf.Max( vertex[i].z, max.z );

                min.x = Mathf.Min(vertex[i].x, min.x);
                min.y = Mathf.Min(vertex[i].y, min.y);
                min.z = Mathf.Min(vertex[i].z, min.z);
            }

            Debug.Log(max + "," + min);
            center = (max + min) / 2.0f;

            Vector3 distance = max - min;
            float scale = 2 / Mathf.Max(distance.x, Mathf.Max(distance.y, distance.z));

            for (int i = 0; i < m_list.Count; i++)
            {
                m_list[i] -= center;
                m_list[i] *= scale;
            }

            return m_list;
        }
    }
}
