
using UnityEngine;

public class ShelfRestockZone : MonoBehaviour
{
    [Header("Slots ordered from left to right")]
    [SerializeField] private ShelfSlot[] slots;

    public bool TryRestockOne(BoxContents box)
    {
        if (box == null || !box.IsOpen)
            return false;

        if (box.Quantity <= 0)
        {
            Debug.Log("Thùng hàng đã hết sản phẩm.", box);
            return false;
        }

        ProductDefinition product = box.Product;

        if (product == null || product.WorldPrefab == null)
        {
            Debug.LogError(
                "Thùng hàng chưa được gán Product hoặc WorldPrefab.",
                box);
            return false;
        }

        ShelfSlot targetSlot = FindNextEmptySlot();

        if (targetSlot == null)
        {
            Debug.Log("Kệ không còn vị trí trống.", this);
            return false;
        }

        GameObject spawnedObject = Instantiate(
            product.WorldPrefab,
            targetSlot.transform.position,
            targetSlot.transform.rotation);

        InteractableObject item =
            spawnedObject.GetComponent<InteractableObject>();

        if (item == null)
        {
            Destroy(spawnedObject);
            Debug.LogError(
                "Prefab sản phẩm thiếu InteractableObject.",
                spawnedObject);
            return false;
        }

        if (!targetSlot.TryPlaceSpawned(item))
        {
            Destroy(spawnedObject);
            Debug.LogWarning(
                "Không thể đặt sản phẩm vào slot.",
                targetSlot);
            return false;
        }

        // Chỉ trừ hàng sau khi slot đã nhận sản phẩm.
        if (!box.TryTake(1))
        {
            targetSlot.Release(item);
            Destroy(spawnedObject);

            Debug.LogError(
                "Không thể trừ hàng trong thùng; đã hoàn tác.",
                box);
            return false;
        }

        Debug.Log(
            $"Đã bổ sung {product.DisplayName}. " +
            $"Số lượng còn lại: {box.Quantity}",
            this);

        return true;
    }

    private ShelfSlot FindNextEmptySlot()
    {
        if (slots == null)
            return null;

        foreach (ShelfSlot slot in slots)
        {
            if (slot != null && !slot.IsOccupied)
                return slot;
        }

        return null;
    }
}
