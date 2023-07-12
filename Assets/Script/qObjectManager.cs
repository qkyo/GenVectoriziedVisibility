using GLTF.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using Qkyo;
using Unity.VisualScripting;

public class qObjectManager
{
    public List<Vector3> vPos = new List<Vector3>();
    public List<Vector3> vNormals = new List<Vector3>();
    public List<Vector3> tNormals = new List<Vector3>();
    public List<Vector4> edges = new List<Vector4>();
    public List<int> vtIdx = new List<int>();
    public List<int> tIdx = new List<int>();
    public List<int> eIdx = new List<int>();
    public List<int> vIdx = new List<int>();

    public Vector3 boundMax;
    public Vector3 boundMin;
    public Vector3 objCenter;

    private static bool meshObjectsNeedRebuilding = false;
    private static List<qObject> objects = new List<qObject>();

    public Matrix4x4 localToWorld;
    Mesh mesh; 
    Dictionary<string, string> vtPair = new Dictionary<string, string>();

    public qObjectManager()
    {
        FindAllObjects();
    }

    public void Initialize()
    {
        FindAllObjects();
    }

    public static void RegisterObject(qObject obj)
    {
        objects.Add(obj);
        meshObjectsNeedRebuilding = true;
    }
    public static void UnregisterObject(qObject obj)
    {
        objects.Remove(obj);
        meshObjectsNeedRebuilding = true;
    }

    void FindAllObjects()
    {
        foreach (qObject obj in objects)
        {
            if (obj.GetComponent<MeshFilter>() != null)
                mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            else
                mesh = obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;

            localToWorld = obj.transform.localToWorldMatrix;
            int firstVertex = vPos.Count;
            int firstTriangle = tIdx.Count;
            var tempT = Enumerable.Range(0, mesh.triangles.Count() / 3).ToArray();
            boundMax = new Vector3(-mesh.bounds.max.x, mesh.bounds.max.y, mesh.bounds.max.z);
            boundMin = new Vector3(-mesh.bounds.min.x, mesh.bounds.min.y, mesh.bounds.min.z);
            objCenter = mesh.bounds.center;

            Vector3 distance = boundMax - boundMin;
            float scale = 2 / Mathf.Max(distance.x, Mathf.Max(distance.y, distance.z));

            tIdx.AddRange(tempT.Select(index => index + firstTriangle));
            vPos.AddRange(mesh.vertices.Select(vec =>Qkyo.Utils.Model.Unitize(vec, objCenter, boundMax, boundMin)));
            for (int i = 0; i < vPos.Count; i++)
                vPos[i] = new Vector3(-vPos[i].x, vPos[i].y, vPos[i].z);
            
            vIdx = Enumerable.Range(0, vPos.Count).ToList();
            vtIdx.AddRange(mesh.triangles.Select(index => index + firstVertex));
            vNormals.AddRange(mesh.normals);
            for (int i = 0; i < vNormals.Count; i++)
                vNormals[i] = new Vector3(-vNormals[i].x, vNormals[i].y, vNormals[i].z);

        }

        // Triangle normal
        //for (int i = 0; i < tIdx.Count; i++)
        //{
        //    tNormals.Add(Vector3.Cross(vPos[vtIdx[i * 3 + 1]] - vPos[vtIdx[i * 3 + 0]],
        //                               vPos[vtIdx[i * 3 + 2]] - vPos[vtIdx[i * 3 + 1]]).normalized);
        //}

        // edges = GetEdge(vtIdx.ToArray());

        //int j = 0;
        //foreach (KeyValuePair<string, string> item in vtPair)
        //{
        //    int[] arr1 = item.Key.Select(c => int.Parse(c.ToString())).ToArray();
        //    int[] arr2 = item.Value.Select(c => int.Parse(c.ToString())).ToArray();

        //    if (arr2.Length == 1)
        //        edges.Add(new Vector4((int)arr1[0], (int)arr1[1], (int)arr2[0], -1));
        //    else
        //        edges.Add(new Vector4((int)arr1[0], (int)arr1[1], (int)arr2[0], (int)arr2[1]));

        //    eIdx.Add(j);
        //    j++;
        //}

        // Qkyo.Model.ObjFormatAnalyzer.CalEdge(tIdx.Count, vPos.Count, vtIdx, ref eIdx, ref edges);

        //Qkyo.Utils.Data.SaveVector4ArrayToFile(edges.ToArray(), "lxyEdgesArr");
        //Qkyo.Utils.Data.SaveIntegerArrayToFile(eIdx.ToArray(), "lxyEIdxArr");

        //Debug.Log("vPos.size : " + vPos.Count);
        //Debug.Log("vNormals.size : " + vNormals.Count);
        //Debug.Log("tNormals.size : " + tNormals.Count);
        //Debug.Log("edges.size : " + edges.Count);
        //Debug.Log("vtIdx.size : " + vtIdx.Count);
        //Debug.Log("tIdx.size : " + tIdx.Count);
        //Debug.Log("eIdx.size : " + eIdx.Count);
        //Debug.Log("vIdx.size : " + vIdx.Count);

        //foreach (var v in eIdx)
        //    Debug.Log(v);

        //foreach (var v in vPos)
        //    Debug.Log(v);

        //Debug.Log("====1===");

        //foreach (var v in vtIdx)
        //    Debug.Log(v);

        //Debug.Log("====2===");

        //foreach (var v in vNormals)
        //    Debug.Log(v);

        //Debug.Log("====3===");

        //foreach (var v in tIdx)
        //    Debug.Log(v);

        //Debug.Log("====4===");

        //foreach (var v in tNormals)
        //    Debug.Log(v);

        //Debug.Log("====5===");

        //foreach (var v in vtIdx_ep)
        //    Debug.Log(v);

        //Debug.Log("====6===");

        //foreach (var v in etIdx)
        //    Debug.Log(v);


        //Debug.Log("====7===");
        //foreach (var v in edges)
        //    Debug.Log(v);


        //Debug.Log("====8===");
        //foreach (var v in vIdx)
        //    Debug.Log(v);
    }


    List<Vector4> GetEdge(int[] temp)
    {
        Dictionary<string, string> test = new Dictionary<string, string>();
        Dictionary<string, string> ans = new Dictionary<string, string>();
        int j = 1;
        for (int i = 0; i < temp.Length; i++)
        {
            if (i == j * 3 - 1)
            {
                //Debug.Log(temp[i] + " " + temp[i - 2] + " " + (j - 1));
                if (temp[i - 2] < temp[i])
                {
                    string key = temp[i - 2].ToString() + "," + temp[i].ToString();
                    string value = (j - 1).ToString();
                    if (test.ContainsKey(key))
                    {
                        test[key] = test[key] + "," + value;
                    }
                    else
                    {
                        test.Add(key, value);

                    }
                }
                else
                {

                    string key = temp[i].ToString() + "," + temp[i - 2].ToString();
                    string value = (j - 1).ToString();
                    if (test.ContainsKey(key))
                    {
                        test[key] = test[key] + "," + value;
                    }
                    else
                    {
                        test.Add(key, value);

                    }
                }

                j++;
            }
            else
            {
                //Debug.Log(temp[i] + " " + temp[i + 1] + " " + (j-1));
                if (temp[i + 1] < temp[i])
                {

                    string key = temp[i + 1].ToString() + "," + temp[i].ToString();
                    string value = (j - 1).ToString();
                    if (test.ContainsKey(key))
                    {
                        test[key] = test[key] + "," + value;
                    }
                    else
                    {
                        test.Add(key, value);

                    }

                }
                else
                {
                    string key = temp[i].ToString() + "," + temp[i + 1].ToString();
                    string value = (j - 1).ToString();
                    if (test.ContainsKey(key))
                    {
                        test[key] = test[key] + "," + value;
                    }
                    else
                    {
                        test.Add(key, value);

                    }
                }
            }

        }


        //foreach (KeyValuePair<string, string> item in test)
        //{
        //    Debug.Log(item.Key + "\t" + item.Value);
        //}


        j = 1;
        for (int i = 0; i < temp.Length; i++)
        {
            if (i == j * 3 - 1)
            {
                //Debug.Log(temp[i] + " " + temp[i - 2] + " " + (j - 1));
                if (temp[i - 2] < temp[i])
                {
                    string key = temp[i - 2].ToString() + "," + temp[i].ToString();
                    string value = null;
                    if (test.ContainsKey(key))
                    {
                        value = test[key];

                        string key2 = temp[i].ToString() + "," + temp[i - 2].ToString();
                        if (!ans.ContainsKey(key) && !ans.ContainsKey(key2))
                        {
                            ans.Add(temp[i].ToString() + "," + temp[i - 2].ToString(), value);
                        }
                    }


                }
                else
                {

                    string key = temp[i].ToString() + "," + temp[i - 2].ToString();
                    string value = null;
                    if (test.ContainsKey(key))
                    {
                        value = test[key];

                        string key2 = temp[i - 2].ToString() + "," + temp[i].ToString();


                        if (!ans.ContainsKey(key) && !ans.ContainsKey(key2))
                        {
                            ans.Add(temp[i].ToString() + "," + temp[i - 2].ToString(), value);

                        }
                    }


                }

                j++;
            }
            else
            {
                //Debug.Log(temp[i] + " " + temp[i + 1] + " " + (j-1));
                if (temp[i + 1] < temp[i])
                {

                    string key = temp[i + 1].ToString() + "," + temp[i].ToString();
                    string value = null;
                    if (test.ContainsKey(key))
                    {
                        value = test[key];

                        string key2 = temp[i].ToString() + "," + temp[i + 1].ToString();

                        if (!ans.ContainsKey(key) && !ans.ContainsKey(key2))
                        {
                            ans.Add(temp[i].ToString() + "," + temp[i + 1].ToString(), value);
                        }
                    }



                }
                else
                {
                    string key = temp[i].ToString() + "," + temp[i + 1].ToString();
                    string value = null;
                    if (test.ContainsKey(key))
                    {
                        value = test[key];

                        string key2 = temp[i + 1].ToString() + "," + temp[i].ToString();

                        if (!ans.ContainsKey(key) && !ans.ContainsKey(key2))
                        {
                            ans.Add(temp[i].ToString() + "," + temp[i + 1].ToString(), value);
                        }
                    }


                }
            }

        }
        List<Vector4> vectorList = new List<Vector4>();

        foreach (KeyValuePair<string, string> item in ans)
        {
            string tempStr;
            Vector4 tempVec = new Vector4();

            if (item.Value.Contains(","))
            {

                tempStr = item.Key + "," + item.Value;

            }

            else
            {
                tempStr = item.Key + "," + item.Value + ",-1";
            }

            string[] components = tempStr.Split(',');
            int[] intComponents = Array.ConvertAll(components, int.Parse);
            tempVec = new Vector4(intComponents[0], intComponents[1], intComponents[2], intComponents[3]);

            //listAns.Add(intComponents);
            vectorList.Add(tempVec);

        }

        return vectorList;
    }

}