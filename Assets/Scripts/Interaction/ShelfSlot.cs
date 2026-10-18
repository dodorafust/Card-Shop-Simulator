
using UnityEngine;

public class ShelfSlot : MonoBehaviour
{
    public InteractableObject Occupant { get; private set; }

    public bool IsOccupied => Occupant != null;

    public bool TryPlace(InteractableObject item)
    {
        if (item == null || IsOccupied)
            return false;

        if (!item.TryPlaceAt(transform.position))
            return false;

        Occupant = item;
        item.SetShelfSlot(this);

        return true;
    }

    public bool TryPlaceSpawned(InteractableObject item)
    {
        if (item == null || IsOccupied)
            return false;

        if (!item.PlaceDirectlyAtSlot(transform))
            return false;

        Occupant = item;
        item.SetShelfSlot(this);

        return true;
    }


    public void Release(InteractableObject item)
    {
        if (Occupant == item)
            Occupant = null;
    }
}
