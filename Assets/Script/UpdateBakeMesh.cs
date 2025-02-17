using UnityEngine;

public class UpdateBakeMesh : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private MeshCollider meshCollider;

    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    void Update()
    {
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        if (skinnedMeshRenderer != null && meshCollider != null)
        {
            Mesh weaponColliderMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(skinnedMeshRenderer.sharedMesh);
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = weaponColliderMesh;
        }
    }
}
