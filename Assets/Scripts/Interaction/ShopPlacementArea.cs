
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ShopPlacementArea : MonoBehaviour
{
    private BoxCollider areaCollider;

    private void Awake()
    {
        areaCollider = GetComponent<BoxCollider>();
    }

    public bool ContainsPoint(Vector3 worldPoint)
    {
        Vector3 localPoint =
            transform.InverseTransformPoint(worldPoint);

        Vector3 center = areaCollider.center;
        Vector3 halfSize = areaCollider.size * 0.5f;

        return localPoint.x >= center.x - halfSize.x &&
               localPoint.x <= center.x + halfSize.x &&
               localPoint.z >= center.z - halfSize.z &&
               localPoint.z <= center.z + halfSize.z;
    }
}
