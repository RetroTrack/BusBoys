using UnityEngine;

namespace BusBoys.Assets.Scripts.Core.Utilities
{
    public class MeshDisabler : MonoBehaviour
    {
        [ContextMenu("Disable Mesh Renderers")]
        //Gets the mesh renderer in all children and disables it. Used for training in unity.
        //This is runs from the object that contains this script
        public void DisableMeshRenderers()
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = false;
            }
        }

        [ContextMenu("Enable Mesh Renderers")]
        //Gets the mesh renderer in all children and enables it
        //This is runs from the object that contains this script
        public void EnableMeshRenderers()
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = true;
            }
        }

        //Gets the mesh renderer in all children and disables it. Used for training in unity.
        //This is runs for the given game object.
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
