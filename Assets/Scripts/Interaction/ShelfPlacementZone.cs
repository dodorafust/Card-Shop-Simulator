
using UnityEngine;

public class ShelfPlacementZone : MonoBehaviour
{
    [SerializeField] private ShelfSlot[] slots;

    public bool HasEmptySlot
    {
        get
        {
            foreach (ShelfSlot slot in slots)
            {
                if (slot != null && !slot.IsOccupied)
                    return true;
            }

            return false;
        }
    }

    public ShelfSlot FindNearestEmptySlot(Vector3 hitPoint)
    {
        ShelfSlot nearestSlot = null;
        float nearestDistance = float.MaxValue;

        foreach (ShelfSlot slot in slots)
        {
            if (slot == null || slot.IsOccupied)
                continue;

            Vector2 slotXZ = new Vector2(
                slot.transform.position.x,
                slot.transform.position.z);

            Vector2 hitXZ = new Vector2(
                hitPoint.x,
                hitPoint.z);

            float distance = Vector2.Distance(slotXZ, hitXZ);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestSlot = slot;
            }
        }

        return nearestSlot;
    }

    public bool TryPlaceInSlot(
        InteractableObject item,
        ShelfSlot slot)
    {
        if (item == null ||
            slot == null ||
            slot.IsOccupied)
        {
            return false;
        }

        return slot.TryPlace(item);
    }
}
