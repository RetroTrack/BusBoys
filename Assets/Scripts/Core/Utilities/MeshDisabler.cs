using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Utilities
{
    public class MeshDisabler : MonoBehaviour
    {
        [ContextMenu("Disable Mesh Renderers")]
        public void DisableMeshRenderers()
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = false;
            }
        }

        [ContextMenu("Enable Mesh Renderers")]
        public void EnableMeshRenderers()
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = true;
            }
        }


        public void DisableMeshRenderersInObject(GameObject obj)
        {
            if (obj == null) return; 
            MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = false;
            }
        }
    }
}
