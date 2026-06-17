using BusBoys.Assets.Scripts.Core.Utilities;
using BusBoys.Assets.Scripts.Vehicles.Bus;
using UnityEngine;

public class TrainingGrid : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] Vector2 gridSize = new Vector2(5, 5);
    [SerializeField] Vector2 spacing = new Vector2(3, 5);
    [SerializeField] MeshDisabler meshDisabler;
    [SerializeField] ThirdPersonCamera thirdPersonCamera;
    private void Awake()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 position = new Vector3(x * spacing.x, 0, y * spacing.y) + transform.position;
                GameObject obj = Instantiate(prefab, position, Quaternion.identity, transform);
                if (meshDisabler != null && (x > 0 || y > 0))
                {
                    meshDisabler.DisableMeshRenderersInObject(obj);
                }else
                {
                    BusController bus = obj.GetComponentInChildren<BusController>();
                    if (bus == null) return;
                    thirdPersonCamera.SetTarget(bus.transform);
                }
            }
        }
    }
}
