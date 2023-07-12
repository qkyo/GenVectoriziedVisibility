using UnityEngine;

//[RequireComponent(typeof(MeshRenderer))]
//[RequireComponent(typeof(MeshFilter))]
//[RequireComponent(typeof(SkinnedMeshRenderer))]

public class qObject : MonoBehaviour
{
    private void OnEnable()
    {
        qObjectManager.RegisterObject(this);
    }
    private void OnDisable()
    {
        qObjectManager.UnregisterObject(this);
    }
}