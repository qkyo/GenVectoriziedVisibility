using GLTF.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class GenVectorizedVisibility : MonoBehaviour
{
    #region native plugin import

    [DllImport("cuda_tempvis_gen", EntryPoint = "gen_temporal_visY")]
    private unsafe static extern void gen_temporal_visY(Vector3* _ls_v, Vector3* _ls_vn,
                                                        Vector3* _vPos, Vector3* _vNor,
                                                        int* _vtIdx, int* _tIdx,
                                                        int* _nIdx, int* _vIdx,
                                                        Vector3* _tNor, int* _eIdx, Vector4* _EdgeQ_v,
                                                        int _nv, int _nf,
                                                        int _ne, int _dnv,
                                                        byte[] des);

    [DllImport("cuda_tempvis_gen", EntryPoint = "export_edgeIfno")]
    private unsafe static extern void export_edgeIfno(int _ne, Vector4* _EdgeQ_v);

    [DllImport("cuda_tempvis_gen", EntryPoint = "cal_edge_info")]
    private unsafe static extern void cal_edge_info(int _nv, int _nf,
                                                    int* _vtIdx,
                                                    int* _ne,
                                                    int* des_eIdx,
                                                    Vector4* _EdgeQ_v);

    [DllImport("cuda_tempvis_gen", EntryPoint = "export_model_info")]
    private unsafe static extern void export_model_info(Vector3* _ls_v, Vector3* _ls_vn,
                                                        Vector3* _vPos, Vector3* _vNor,
                                                        int* _vtIdx, int* _tIdx,
                                                        int* _nIdx, int* _vIdx,
                                                        Vector3* _tNor, int* _eIdx, Vector4* _EdgeQ_v,
                                                        int _nv, int _nf,
                                                        int _ne, int _dnv);


    [DllImport("cuda_tempvis_gen", EntryPoint = "gen_temporal_visZ")]
    private unsafe static extern void gen_temporal_visZ(Vector3* _ls_v, Vector3* _ls_vn,
                                                        Vector3* _vPos, Vector3* _vNor,
                                                        int* _vtIdx, int* _tIdx,
                                                        int* _nIdx, int* _vIdx,
                                                        Vector3* _tNor,
                                                        int _nv, int _nf,
                                                        int _dnv,
                                                        byte[] des);

    [DllImport("cuda_tempvis_gen", EntryPoint = "gen_temporal_vis_Final")]
    private unsafe static extern void gen_temporal_vis_Final(Vector3* _ls_v, Vector3* _ls_vn,
                                                            Vector3* _vPos, Vector3* _vNor,
                                                            int* _vtIdx, int* _tIdx,
                                                            int* _nIdx, int* _vIdx,
                                                            Vector3* _tNor,
                                                            int _nv, int _nf,
                                                            int _dnv);


    [DllImport("cuda_tempvis_gen", EntryPoint = "gen_temporal_vis_single_vertex")]
    private unsafe static extern void gen_temporal_vis_single_vertex(Vector3* _ls_v, Vector3* _ls_vn,
                                                                    Vector3* _vPos, Vector3* _vNor,
                                                                    int* _vtIdx, int* _tIdx,
                                                                    int* _nIdx, int* _vIdx,
                                                                    Vector3* _tNor,
                                                                    int _nv, int _nf, int _dnv,
                                                                    int _vidx,
                                                                    Vector3* _des_visibility,
                                                                    Vector3* _des_broken_vis,
                                                                    Vector3* _des_order_vis,
                                                                    int* vis_size);

    [DllImport("cuda_tempvis_gen", EntryPoint = "gen_temporal_vis_single_vertex_ab")]
    private unsafe static extern void gen_temporal_vis_single_vertex_ab(Vector3* _ls_v, Vector3* _ls_vn,
                                                                        Vector3* _vPos, Vector3* _vNor,
                                                                        int* _vtIdx, int* _tIdx,
                                                                        int* _nIdx, int* _vIdx,
                                                                        Vector3* _tNor,
                                                                        int _nv, int _nf, int _dnv,
                                                                        int _vidx,
                                                                        Vector3* _des_visibility,
                                                                        Vector3* _des_broken_vis,
                                                                        Vector3* _des_order_vis,
                                                                        int* _des_broken_vis_gs,
                                                                        int* _des_order_vis_gs,
                                                                        int* vis_size);


    // _original_v: vertex pos array, num of vertex is 484
    // _vtIdx:      the array of the vertex indices corresponding to each triangle
    // _nv:         original_v.size()
    // _nf:         number of triangles, 3*_vtIdx.size()
    // des_v:       the resulting vertex array which excludes the redundant vertice
    // n_des_v:     n_des_v[0] = des_v.size()

    [DllImport("cuda_tempvis_gen", EntryPoint = "weld_vertex")]
    private unsafe static extern void weld_vertex(Vector3* _original_v,
                                                  int* _vtIdx,
                                                  int _nv, int _nf,
                                                  Vector3* des_v, int* des_vidx,
                                                  int* n_des_v);
    #endregion

    [Header("Debug")]
    [Tooltip("If the extra txt are written to the local during the run time.")]
    public bool isDebugging = false;
    [Tooltip("If the camera will transform to the shading point view.")]
    public bool isDepthCamera = false;
    [Tooltip("If the edge infos are generated in unity.")]
    public bool isGenEdge = false;

    [Header("Visibility Mode")]
    [Tooltip("If we generate the visibility using one vertex per time.")]
    public bool genVisUsingOneVertex = true;
    [Tooltip("If the visibilities of the model and the plane are drawn as a group.")]
    public bool isSeparatedDrawn = true;
    [Range(0, 453)]
    public int shownVIdx = 0;

    [Header("Cameras")]
    public GameObject depthCamera;
    public Camera MainCamera;
    public Camera GizmosCamera;

    [Header("GUI")]
    public LineRenderer LineRenderer;
    public LineRenderer anotherLineRenderer;
    public LineRenderer andAnotherLineRenderer;
    public LineRenderer BrokenLineRenderer;
    public GameObject vertexRenderer;
    public TextMeshProUGUI VertexIdxUI;

    #region variable declaration
    // public RawImage outputImgComponent;

    // Data struct reference: https://resolute-beet-613.notion.site/Unity-Mesh-Export-Data-Format-Reference-6e67afe61d564cc69c2e0f5b70db3e9e?pvs=4
    //                                        Debug data in cpp concole
    //                                      size  -  Supposed to be              -       Mine be       -     work done      
    // _ls_v:  model454 model vpos          (454  -  import from model_454                 ×                  √      )
    // _ls_vn: model454 model vnor          (454  -  import from model_454                 ×                  √      )
    // _vPos:  model411 vertex position     (411  -  import from model_411                                     √      )
    // _vn:    model454 vertex normal       (454  -  import from model_454, identify with _ls_vn               √      )
    // _vtIdx  vertex idx of each triangle  (2328 -  generated using model_411                                 √      )
    // _tIdx:  triangle idx                 (776  -  generated using model_411                                 √      )
    // _nIdx:  normal index of vnor         (776  -  generated using model_411                                 √      )
    // _vIdx:  model411 vertex index        (411  -  generated from model_411                                  √      )
    // _tNor:  model411 face normal ?       (776  -  import vn from model_411                                  √      )
    // _nv:    model411 vertex number
    // _nf:    face number
    // _dnv:   num of ls_v (vpos)
    // _ne:    [no need] edge num

    List<Vector3> ls_vPos = new List<Vector3>();
    List<Vector3> ls_vn = new List<Vector3>();
    List<Vector3> vPos = new List<Vector3>();
    List<Vector3> vNormals = new List<Vector3>();
    List<Vector3> tNormals = new List<Vector3>();
    List<Vector4> edges = new List<Vector4>();
    List<int> vtIdx = new List<int>();
    List<int> tIdx = new List<int>();
    List<int> eIdx = new List<int>();
    List<int> vIdx = new List<int>();
    List<int> nIdx = new List<int>();

    List<int> vtIdx454 = new List<int>();
    List<int> tIdx454 = new List<int>();
    List<int> vIdx454 = new List<int>();
    List<int> eIdx454 = new List<int>();
    List<Vector4> edges454 = new List<Vector4>();

    List<Vector3> jennyVPos = new List<Vector3>();
    List<int> jennyVtIdx = new List<int>();
    List<int> jennyVSize = new List<int>();
    List<int> jennyEIdx = new List<int>();
    List<Vector4> jennyEdges = new List<Vector4>();

    List<Vector3> desDisorderVis = new List<Vector3>();
    List<Vector3> desBrokenVis = new List<Vector3>();
    List<Vector3> desOrderVis = new List<Vector3>();
    List<int> desBrokenVisGs = new List<int>();
    List<int> desOrderVisGs = new List<int>();
    List<int> vSize = new List<int>();

    qObjectManager m_qObjectManager;
    Matrix4x4 objlocalToWorld;

    byte[] returnData;
    int lastShownVIdx;

    const int width = 32;
    const int height = 2355;
    const int bytePerChannel = 16;
    #endregion

    void Start()
    {
        // Set camera and default state
        lastShownVIdx = shownVIdx;
        SetupCamera();

        // Get model454 info
        m_qObjectManager = new qObjectManager();
        ls_vPos = m_qObjectManager.vPos;
        ls_vn = m_qObjectManager.vNormals;
        vNormals = m_qObjectManager.vNormals;
        vtIdx454 = m_qObjectManager.vtIdx;
        tIdx454 = m_qObjectManager.tIdx;
        vIdx454 = m_qObjectManager.vIdx;
        eIdx454 = m_qObjectManager.eIdx;
        edges454 = m_qObjectManager.edges;
        objlocalToWorld = m_qObjectManager.localToWorld;

        // Fill others model data
        GetWeldVertex();
        vIdx = Enumerable.Range(0, jennyVPos.Count).ToList();
        tIdx.AddRange(Enumerable.Range(0, jennyVtIdx.Count / 3).ToArray());
        for (int i = 0; i < tIdx.Count; i++)
        {
            nIdx.Add(i);
            nIdx.Add(i);
            nIdx.Add(i);

            // Triangle normal
            tNormals.Add(Vector3.Cross(jennyVPos[jennyVtIdx[i * 3 + 1]] - jennyVPos[jennyVtIdx[i * 3 + 0]],
                                        jennyVPos[jennyVtIdx[i * 3 + 2]] - jennyVPos[jennyVtIdx[i * 3 + 1]]).normalized);
        }

        if (isDebugging)
        { 
            Qkyo.Utils.IO.SaveVector3ArrayToFile(ls_vPos.ToArray(), "q_ls_vPos");
            Qkyo.Utils.IO.SaveVector3ArrayToFile(ls_vn.ToArray(), "q_ls_vn");
            Qkyo.Utils.IO.SaveVector3ArrayToFile(tNormals.ToArray(), "q_tNormals");
            Qkyo.Utils.IO.SaveIntegerArrayToFile(jennyVtIdx.ToArray(), "q_VtIdx");
            Qkyo.Utils.IO.SaveVector3ArrayToFile(jennyVPos.ToArray(), "q_vPos");
            Qkyo.Utils.IO.SaveIntegerArrayToFile(tIdx.ToArray(), "q_tIdx");
            Qkyo.Utils.IO.SaveIntegerArrayToFile(nIdx.ToArray(), "q_nIdx");
            Qkyo.Utils.IO.SaveIntegerArrayToFile(vIdx.ToArray(), "q_vIdx");
        }

        if(isGenEdge)   GetEdgeInfo();

        // Do not draw the line on the screen.
        // However, write a visibility image to the local
        if (!genVisUsingOneVertex)
            GetVectorizedVisibility();
        else
        {
            GetVectorizedVisibility(shownVIdx, isSeparatedDrawn);
            DrawGizmos(shownVIdx, isSeparatedDrawn);
        }
    }

    void Update()
    {
        if (genVisUsingOneVertex)
            if (lastShownVIdx != shownVIdx)
            {
                GetVectorizedVisibility(shownVIdx);
                DrawGizmos(shownVIdx);
                lastShownVIdx = shownVIdx;
            }
    }

    void GetWeldVertex()
    {

        Vector3[] jennyVPosArray = new Vector3[454];
        jennyVPos = new List<Vector3>(jennyVPosArray);
        int[] jennyVtIdxArray = new int[2328];
        jennyVtIdx = new List<int>(jennyVtIdxArray);
        int[] jennyVSizeArray = new int[1];
        jennyVSize = new List<int>(jennyVSizeArray);

        unsafe
        {
            int nv = 454;
            int nf = 776;

            fixed (Vector3* ls_vPosPtr = ls_vPos.ToArray())
            fixed (int* vtIdx454Ptr = vtIdx454.ToArray())
            fixed (Vector3* jennyVPosPtr = jennyVPos.ToArray())
            fixed (int* jennyVtIdxPtr = jennyVtIdx.ToArray())
            fixed (int* jennyVSizePtr = jennyVSize.ToArray())
            {
                weld_vertex(ls_vPosPtr, vtIdx454Ptr, nv, nf, jennyVPosPtr, jennyVtIdxPtr, jennyVSizePtr);
                jennyVPosArray = Qkyo.Utils.IO.ConvertPointerToArray(jennyVPosPtr, 454);
                jennyVtIdxArray = Qkyo.Utils.IO.ConvertPointerToArray(jennyVtIdxPtr, 2328);
                jennyVSizeArray = Qkyo.Utils.IO.ConvertPointerToArray(jennyVSizePtr, 1);
            }
        }

        //Qkyo.Utils.Data.SaveVector3ArrayToFile(jennyVPosArray, "vposArr");
        //Qkyo.Utils.Data.SaveIntegerArrayToFile(jennyVtIdxArray, "vtIdxArr");
        //Qkyo.Utils.Data.SaveIntegerArrayToFile(jennyVSizeArray, "vSizeArr");

        // Resize the array of jenny
        Vector3[] resized_jVPosArr = new Vector3[jennyVSizeArray[0]];
        Array.Copy(jennyVPosArray, resized_jVPosArr, jennyVSizeArray[0]);
        jennyVPos = new List<Vector3>(resized_jVPosArr);
        jennyVtIdx = new List<int>(jennyVtIdxArray);
        jennyVSize = new List<int>(jennyVSizeArray);

        if (isDebugging)
        {
            Debug.Log("========== After weld test ============");
            Debug.Log("jennyVPos Size: " + jennyVPos.Count);
            Debug.Log("jennyVtIdx Size: " + jennyVtIdx.Count);
            Debug.Log("jennyVSize Size: " + jennyVSize.Count);
            Debug.Log("========== End of weld test ============");
        }
    }

    void GetEdgeInfo()
    {
        // Usable
        int[] jennyEIdxArr = new int[2328];
        Vector4[] jennyEdgesArr = new Vector4[2328];

        Debug.Log(vIdx454.Count);
        Debug.Log(tIdx454.Count);
        Debug.Log(vtIdx454.Count);
        Debug.Log(eIdx454.Count);
        Debug.Log(eIdx454.Count);
        Debug.Log(edges454.Count);

        unsafe
        {
            int ne = eIdx.Count;
            fixed (Vector4* edgesPtr = edges454.ToArray())
            fixed (int* vtIdx454Ptr = vtIdx454.ToArray())
            fixed (int* eIdxPtr = eIdx454.ToArray())
            {
                cal_edge_info(vIdx454.Count, tIdx454.Count, vtIdx454Ptr, &ne, eIdxPtr, edgesPtr);
                jennyEIdxArr = Qkyo.Utils.IO.ConvertPointerToArray(eIdxPtr, 454);
                jennyEdgesArr = Qkyo.Utils.IO.ConvertPointerToArray(edgesPtr, 2328);
            }
        }

        Qkyo.Utils.IO.SaveVector4ArrayToFile(jennyEdgesArr, "jennyEdgesArr");
        Qkyo.Utils.IO.SaveIntegerArrayToFile(jennyEIdxArr, "jennyEIdxArr");
    }

    void GetVectorizedVisibility()
    {
        if (isDebugging)
        { 
            Debug.Log(" ========== Running GetVectorizedVisibility() ==========  ");
            Debug.Log("ls_vPos.size : " + ls_vPos.Count);
            Debug.Log("ls_vn.size : " + ls_vn.Count);
            //Debug.Log("vPos.size : " + vPos.Count);
            Debug.Log("jennyVPos.size : " + jennyVPos.Count);
            // Debug.Log("vNormals.size : " + vNormals.Count);
            Debug.Log("vtIdx.size : " + jennyVtIdx.Count);
            Debug.Log("tIdx.size : " + tIdx.Count);
            Debug.Log("nIdx.size : " + nIdx.Count);
            Debug.Log("vIdx.size : " + vIdx.Count);
            Debug.Log("tNormals.size : " + tNormals.Count);
            Debug.Log("vIdx.Count : " + vIdx.Count);
            Debug.Log("tIdx.Count : " + tIdx.Count);
            Debug.Log("ls_vPos.Count : " + ls_vPos.Count);
            Debug.Log(" ========== End of GetVectorizedVisibility() ==========  ");
        }

        unsafe
        {
            fixed (Vector3* ls_vPosPtr = ls_vPos.ToArray())
            fixed (Vector3* ls_vnPtr = ls_vn.ToArray())
            fixed (Vector3* vPosPtr = jennyVPos.ToArray())
            fixed (Vector3* tNormalsPtr = tNormals.ToArray())
            fixed (int* vtIdxPtr = jennyVtIdx.ToArray())
            fixed (int* nIdxPtr = nIdx.ToArray())
            fixed (int* tIdxPtr = tIdx.ToArray())
            fixed (int* vIdxPtr = vIdx.ToArray())
                gen_temporal_vis_Final(ls_vPosPtr, ls_vnPtr,
                                        vPosPtr, ls_vnPtr,
                                        vtIdxPtr, tIdxPtr,
                                        nIdxPtr, vIdxPtr,
                                        tNormalsPtr,
                                        vIdx.Count, tIdx.Count,
                                        ls_vPos.Count);
        }


        // returnData = new byte[width * height * 3 * bytePerChannel];

        //unsafe
        //{
        //    fixed (Vector3* ls_vPosPtr = ls_vPos.ToArray())
        //    fixed (Vector3* ls_vnPtr = ls_vn.ToArray())
        //    fixed (Vector3* vPosPtr = vPos.ToArray())
        //    fixed (Vector3* vNormalsPtr = vNormals.ToArray())
        //    fixed (Vector3* tNormalsPtr = tNormals.ToArray())
        //    fixed (int* vtIdxPtr = vtIdx.ToArray())
        //    fixed (int* tIdxPtr = tIdx.ToArray())
        //    fixed (int* vIdxPtr = vIdx.ToArray())
        //        gen_temporal_visZ(ls_vPosPtr, ls_vnPtr,
        //                          vPosPtr, vNormalsPtr,
        //                          vtIdxPtr, tIdxPtr,
        //                          vtIdxPtr, vIdxPtr,
        //                          tNormalsPtr,
        //                          vIdx.Count, tIdx.Count,
        //                          ls_vPos.Count, returnData);
        //}

        //unsafe
        //{
        //    fixed (Vector3* ls_vPosPtr = ls_vPos.ToArray())
        //    fixed (Vector3* ls_vnPtr = ls_vn.ToArray())
        //    fixed (Vector3* vPosPtr = vPos.ToArray())
        //    fixed (Vector3* vNormalsPtr = vNormals.ToArray())
        //    fixed (Vector3* tNormalsPtr = tNormals.ToArray())
        //    fixed (Vector4* edgesPtr = edges.ToArray())
        //    fixed (int* vtIdxPtr = vtIdx.ToArray())
        //    fixed (int* tIdxPtr = tIdx.ToArray())
        //    fixed (int* vIdxPtr = vIdx.ToArray())
        //    fixed (int* eIdxPtr = eIdx.ToArray())
        //        export_model_info(vPosPtr, vNormalsPtr,
        //                          vPosPtr, vNormalsPtr,
        //                          vtIdxPtr, tIdxPtr,
        //                          vIdxPtr, vIdxPtr,
        //                          tNormalsPtr, eIdxPtr, edgesPtr,
        //                          vIdx.Count, tIdx.Count,
        //                          eIdx.Count, vIdx.Count);
        //}


        //unsafe
        //{
        //    Pin array then send to C++
        //    fixed (Vector4* edgesPtr = edges.ToArray())
        //        export_edgeIfno(eIdx.Count, edgesPtr);
        //}

        //unsafe
        //{
        //    //Pin array then send to C++
        //    fixed (Vector3* vPosPtr = vPos.ToArray())
        //    fixed (Vector3* vNormalsPtr = vNormals.ToArray())
        //    fixed (Vector3* tNormalsPtr = tNormals.ToArray())
        //    fixed (Vector4* edgesPtr = edges.ToArray())
        //    fixed (int* vtIdxPtr = vtIdx.ToArray())
        //    fixed (int* tIdxPtr = tIdx.ToArray())
        //    fixed (int* vIdxPtr = vIdx.ToArray())
        //    fixed (int* eIdxPtr = eIdx.ToArray())
        //        gen_temporal_visY(vPosPtr, vNormalsPtr,
        //                            vPosPtr, vNormalsPtr,
        //                            vtIdxPtr, tIdxPtr,
        //                            vIdxPtr, vIdxPtr,
        //                            tNormalsPtr, eIdxPtr, edgesPtr,
        //                            vIdx.Count, tIdx.Count,
        //                            eIdx.Count, vIdx.Count,
        //                            returnData);
        //}


        //RenderTexture newRenderTexture = GetRenderTexture(returnData);
        //outputImgComponent.texture = newRenderTexture;
    }

    void GetVectorizedVisibility(int m_vidx, bool isSeparatedDrawn = true)
    {
        if (isDebugging)
        {
            Debug.Log(" ========== Running GetVectorizedVisibility(m_vidx) ==========  ");
            Debug.Log("ls_vPos.size : " + ls_vPos.Count);
            Debug.Log("ls_vn.size : " + ls_vn.Count);
            //Debug.Log("vPos.size : " + vPos.Count);
            Debug.Log("jennyVPos.size : " + jennyVPos.Count);
            // Debug.Log("vNormals.size : " + vNormals.Count);
            Debug.Log("vtIdx.size : " + jennyVtIdx.Count);
            Debug.Log("tIdx.size : " + tIdx.Count);
            Debug.Log("nIdx.size : " + nIdx.Count);
            Debug.Log("vIdx.size : " + vIdx.Count);
            Debug.Log("tNormals.size : " + tNormals.Count);
            Debug.Log("vIdx.Count : " + vIdx.Count);
            Debug.Log("tIdx.Count : " + tIdx.Count);
            Debug.Log("ls_vPos.Count : " + ls_vPos.Count);
            Debug.Log(" ========== End of GetVectorizedVisibility(m_vidx) ==========  ");
        }

        if (!isSeparatedDrawn)
        {
            int[] sizeArray = new int[3];
            vSize = new List<int>(sizeArray);
            Vector3[] desDisorderVisArr = new Vector3[1024];
            desDisorderVis = new List<Vector3>(desDisorderVisArr);
            Vector3[] desBrokenVisArr = new Vector3[1024];
            desBrokenVis = new List<Vector3>(desBrokenVisArr);
            Vector3[] desOrderVisArr = new Vector3[1024];
            desOrderVis = new List<Vector3>(desOrderVisArr);

            unsafe
            {
                fixed (Vector3* ls_vPosPtr = ls_vPos.ToArray())
                fixed (Vector3* ls_vnPtr = ls_vn.ToArray())
                fixed (Vector3* vPosPtr = jennyVPos.ToArray())
                fixed (Vector3* tNormalsPtr = tNormals.ToArray())
                fixed (int* vtIdxPtr = jennyVtIdx.ToArray())
                fixed (int* nIdxPtr = nIdx.ToArray())
                fixed (int* tIdxPtr = tIdx.ToArray())
                fixed (int* vIdxPtr = vIdx.ToArray())
                fixed (Vector3* desDisorderVisPtr = desDisorderVis.ToArray())
                fixed (Vector3* desBrokenVisPtr = desBrokenVis.ToArray())
                fixed (Vector3* desOrderVisPtr = desOrderVis.ToArray())
                fixed (int* sizePtr = vSize.ToArray())
                {
                    gen_temporal_vis_single_vertex(ls_vPosPtr, ls_vnPtr,
                                                   vPosPtr, ls_vnPtr,
                                                   vtIdxPtr, tIdxPtr,
                                                   nIdxPtr, vIdxPtr,
                                                   tNormalsPtr,
                                                   vIdx.Count, tIdx.Count, ls_vPos.Count,
                                                   m_vidx,
                                                   desDisorderVisPtr,
                                                   desBrokenVisPtr,
                                                   desOrderVisPtr,
                                                   sizePtr);
                    sizeArray = Qkyo.Utils.IO.ConvertPointerToArray(sizePtr, 3);
                    desDisorderVisArr = Qkyo.Utils.IO.ConvertPointerToArray(desDisorderVisPtr, 1024);
                    desBrokenVisArr = Qkyo.Utils.IO.ConvertPointerToArray(desBrokenVisPtr, 1024);
                    desOrderVisArr = Qkyo.Utils.IO.ConvertPointerToArray(desOrderVisPtr, 1024);
                }
            }

            //Qkyo.Utils.IO.SaveVector3ArrayToFile(desDisorderVisArr, "desDisorderVisArr");
            //Qkyo.Utils.IO.SaveVector3ArrayToFile(desBrokenVisArr, "desBrokenVisArr");
            //Qkyo.Utils.IO.SaveVector3ArrayToFile(desOrderVisArr, "desOrderVisArr");

            // Resize the array of jenny
            Vector3[] res_desDisorderVisArr = new Vector3[sizeArray[0]];
            Vector3[] res_desBrokenVisArr = new Vector3[sizeArray[1]];
            Vector3[] res_desOrderVisArr = new Vector3[sizeArray[2]];

            Array.Copy(desDisorderVisArr, res_desDisorderVisArr, sizeArray[0]);
            Array.Copy(desBrokenVisArr, res_desBrokenVisArr, sizeArray[1]);
            Array.Copy(desOrderVisArr, res_desOrderVisArr, sizeArray[2]);

            vSize = new List<int>(sizeArray);
            desDisorderVis = new List<Vector3>(res_desDisorderVisArr);
            desBrokenVis = new List<Vector3>(res_desBrokenVisArr);
            desOrderVis = new List<Vector3>(res_desOrderVisArr);

            if (isDebugging)
            {
                Debug.Log("========== Received Visibility with idx ============");
                Debug.Log("desDisorderVis Size: " + desDisorderVis.Count);
                Debug.Log("desBrokenVis Size: " + desBrokenVis.Count);
                Debug.Log("desOrderVis Size: " + desOrderVis.Count);
                Debug.Log("========== End of Received Visibility with idx ============");
            }
        }

        else
        {
            int[] sizeArray = new int[5];
            vSize = new List<int>(sizeArray);
            Vector3[] desDisorderVisArr = new Vector3[1024];
            desDisorderVis = new List<Vector3>(desDisorderVisArr);
            Vector3[] desBrokenVisArr = new Vector3[1024];
            desBrokenVis = new List<Vector3>(desBrokenVisArr);
            Vector3[] desOrderVisArr = new Vector3[1024];
            desOrderVis = new List<Vector3>(desOrderVisArr);

            int[] desBrokenVisGsArr = new int[24];
            desBrokenVisGs = new List<int>(desBrokenVisGsArr);
            int[] desOrderVisGsArr = new int[24];
            desOrderVisGs = new List<int>(desOrderVisGsArr);

            unsafe
            {
                fixed (Vector3* ls_vPosPtr = ls_vPos.ToArray())
                fixed (Vector3* ls_vnPtr = ls_vn.ToArray())
                fixed (Vector3* vPosPtr = jennyVPos.ToArray())
                fixed (Vector3* tNormalsPtr = tNormals.ToArray())
                fixed (int* vtIdxPtr = jennyVtIdx.ToArray())
                fixed (int* nIdxPtr = nIdx.ToArray())
                fixed (int* tIdxPtr = tIdx.ToArray())
                fixed (int* vIdxPtr = vIdx.ToArray())
                fixed (Vector3* desDisorderVisPtr = desDisorderVis.ToArray())
                fixed (Vector3* desBrokenVisPtr = desBrokenVis.ToArray())
                fixed (Vector3* desOrderVisPtr = desOrderVis.ToArray())
                fixed (int* desBrokenVisGsPtr = desBrokenVisGs.ToArray())
                fixed (int* desOrderVisGsPtr = desOrderVisGs.ToArray())
                fixed (int* sizePtr = vSize.ToArray())
                {
                    gen_temporal_vis_single_vertex_ab(ls_vPosPtr, ls_vnPtr,
                                                       vPosPtr, ls_vnPtr,
                                                       vtIdxPtr, tIdxPtr,
                                                       nIdxPtr, vIdxPtr,
                                                       tNormalsPtr,
                                                       vIdx.Count, tIdx.Count, ls_vPos.Count,
                                                       m_vidx,
                                                       desDisorderVisPtr,
                                                       desBrokenVisPtr,
                                                       desOrderVisPtr,
                                                       desBrokenVisGsPtr,
                                                       desOrderVisGsPtr,
                                                       sizePtr);
                    sizeArray = Qkyo.Utils.IO.ConvertPointerToArray(sizePtr, 5);
                    desDisorderVisArr = Qkyo.Utils.IO.ConvertPointerToArray(desDisorderVisPtr, 1024);
                    desBrokenVisArr = Qkyo.Utils.IO.ConvertPointerToArray(desBrokenVisPtr, 1024);
                    desOrderVisArr = Qkyo.Utils.IO.ConvertPointerToArray(desOrderVisPtr, 1024);
                    desBrokenVisGsArr = Qkyo.Utils.IO.ConvertPointerToArray(desBrokenVisGsPtr, 24);
                    desOrderVisGsArr = Qkyo.Utils.IO.ConvertPointerToArray(desOrderVisGsPtr, 24);
                }
            }

            // Resize the array of jenny
            Vector3[] res_desDisorderVisArr = new Vector3[sizeArray[0]];
            Vector3[] res_desBrokenVisArr = new Vector3[sizeArray[1]];
            Vector3[] res_desOrderVisArr = new Vector3[sizeArray[2]];
            int[] res_desBrokenVisGsArr = new int[sizeArray[3]];
            int[] res_desOrderVisGsArr = new int[sizeArray[4]];

            Array.Copy(desDisorderVisArr, res_desDisorderVisArr, sizeArray[0]);
            Array.Copy(desBrokenVisArr, res_desBrokenVisArr, sizeArray[1]);
            Array.Copy(desOrderVisArr, res_desOrderVisArr, sizeArray[2]);
            Array.Copy(desBrokenVisGsArr, res_desBrokenVisGsArr, sizeArray[3]);
            Array.Copy(desOrderVisGsArr, res_desOrderVisGsArr, sizeArray[4]);

            vSize = new List<int>(sizeArray);
            desDisorderVis = new List<Vector3>(res_desDisorderVisArr);
            desBrokenVis = new List<Vector3>(res_desBrokenVisArr);
            desOrderVis = new List<Vector3>(res_desOrderVisArr);
            desBrokenVisGs = new List<int>(res_desBrokenVisGsArr);
            desOrderVisGs = new List<int>(res_desOrderVisGsArr);

            if (isDebugging)
            { 
                Qkyo.Utils.IO.SaveVector3ArrayToFile(res_desDisorderVisArr, "desDisorderVisArr");
                Qkyo.Utils.IO.SaveVector3ArrayToFile(res_desBrokenVisArr, "desBrokenVisArr");
                Qkyo.Utils.IO.SaveVector3ArrayToFile(res_desOrderVisArr, "desOrderVisArr");
                Qkyo.Utils.IO.SaveIntegerArrayToFile(res_desBrokenVisGsArr, "desBrokenVisGsArr");
                Qkyo.Utils.IO.SaveIntegerArrayToFile(res_desOrderVisGsArr, "desOrderVisGsArr");

                Debug.Log("========== Received Visibility with idx ============");
                Debug.Log("desDisorderVis Size: " + desDisorderVis.Count);
                Debug.Log("desBrokenVis Size: " + desBrokenVis.Count);
                Debug.Log("desOrderVis Size: " + desOrderVis.Count);
                Debug.Log("desBrokenVisGs Size: " + desBrokenVisGs.Count);
                Debug.Log("desOrderVisGs Size: " + desOrderVisGs.Count);
                Debug.Log("========== End of Received Visibility with idx ============");
            }

        }
    }

    void DrawGizmos(int m_shownVIdx, bool isSeperated = true)
    {
        if (!isSeparatedDrawn)
        {
            VertexIdxUI.text = "Vertex Index: " + m_shownVIdx;

            // Broken visibility is not empty. draw the line
            if (vSize[1] != 0)
            {
                Debug.Log("Broken visibility appears.");
                BrokenLineRenderer.positionCount = vSize[1];
                BrokenLineRenderer.SetPositions(desBrokenVis.ToArray());
            }
            else
                BrokenLineRenderer.positionCount = 0;

            LineRenderer.positionCount = vSize[2];
            LineRenderer.SetPositions(desOrderVis.ToArray());
            vertexRenderer.transform.position = ls_vPos[m_shownVIdx];
            if (isDepthCamera)
            {
                depthCamera.transform.position = ls_vPos[m_shownVIdx];
                depthCamera.transform.LookAt(new Vector3(0, 0, 0));
            }
        }

        // Draw the visibility of model and plane separately.
        // Otherwise, the visibility line may be connected.
        else
        {
            // Broken visibility is not empty. draw the line.
            if (vSize[1] != 0)
            {
                Debug.Log("Broken visibility appears.");
                BrokenLineRenderer.positionCount = vSize[1];
                BrokenLineRenderer.SetPositions(desBrokenVis.ToArray());
            }
            else
                BrokenLineRenderer.positionCount = 0;

            // Move the shown vertex (and depth camera) to the viewing point of visibility.
            vertexRenderer.transform.position = ls_vPos[m_shownVIdx];
            if (isDepthCamera)
            {
                depthCamera.transform.position = ls_vPos[m_shownVIdx];
                depthCamera.transform.LookAt(new Vector3(0, 0, 0));
            }

            if (desOrderVisGs.Count == 1)
            {
                VertexIdxUI.text = "Vertex Index: " + m_shownVIdx;
                LineRenderer.positionCount = vSize[2];
                anotherLineRenderer.positionCount = 0;
                andAnotherLineRenderer.positionCount = 0;
                LineRenderer.SetPositions(desOrderVis.ToArray());
            }
            else if (desOrderVisGs.Count == 2)
            {
                VertexIdxUI.text = "Vertex Index: " + m_shownVIdx;

                LineRenderer.positionCount = desOrderVisGs[0];
                anotherLineRenderer.positionCount = vSize[2] - desOrderVisGs[0];
                andAnotherLineRenderer.positionCount = 0;

                Vector3[] res_desOrderVis = new Vector3[desOrderVisGs[0]];
                Vector3[] res_anotherDesOrderVis = new Vector3[desOrderVisGs[1]];

                Array.Copy(desOrderVis.ToArray(), 0, res_desOrderVis, 0, desOrderVisGs[0]);
                Array.Copy(desOrderVis.ToArray(), desOrderVisGs[0], res_anotherDesOrderVis, 0, desOrderVisGs[1]);

                LineRenderer.SetPositions(res_desOrderVis.ToArray());
                anotherLineRenderer.SetPositions(res_anotherDesOrderVis.ToArray());

            }
            else if (desOrderVisGs.Count == 3)
            {
                VertexIdxUI.text = "Vertex Index: " + m_shownVIdx;

                LineRenderer.positionCount = desOrderVisGs[0];
                anotherLineRenderer.positionCount = desOrderVisGs[1];
                andAnotherLineRenderer.positionCount = desOrderVisGs[2];

                Vector3[] res_desOrderVis = new Vector3[desOrderVisGs[0]];
                Vector3[] res_anotherDesOrderVis = new Vector3[desOrderVisGs[1]];
                Vector3[] res_andAnotherDesOrderVis = new Vector3[desOrderVisGs[2]];

                Array.Copy(desOrderVis.ToArray(), 0, res_desOrderVis, 0, desOrderVisGs[0]);
                Array.Copy(desOrderVis.ToArray(), desOrderVisGs[0], res_anotherDesOrderVis, 0, desOrderVisGs[1]);
                Array.Copy(desOrderVis.ToArray(), desOrderVisGs[0] + desOrderVisGs[1], res_andAnotherDesOrderVis, 0, desOrderVisGs[2]);

                LineRenderer.SetPositions(res_desOrderVis.ToArray());
                anotherLineRenderer.SetPositions(res_anotherDesOrderVis.ToArray());
                andAnotherLineRenderer.SetPositions(res_andAnotherDesOrderVis.ToArray());
            }

        }
    }

    void SetupCamera()
    {
        if (isDepthCamera)
        {
            GizmosCamera.gameObject.SetActive(true);
            MainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Gizmos"));      // Hide Gizmos layer
            GizmosCamera.fieldOfView = 60;
            MainCamera.fieldOfView = 60;
        }
        else
        {
            GizmosCamera.gameObject.SetActive(false);
            MainCamera.cullingMask |= 1 << LayerMask.NameToLayer("Gizmos");      // Show Gizmos layer
            GizmosCamera.fieldOfView = 60;
            MainCamera.fieldOfView = 60;
        }
    }
}