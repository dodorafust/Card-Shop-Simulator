
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
        // Nếu tầng đã có hàng, chỉ chấp nhận cùng loại sản phẩm.
        foreach (ShelfSlot slot in slots)
        {
            if (slot == null || !slot.IsOccupied)
                continue;

            InteractableObject existingItem = slot.Occupant;

            if (existingItem == null)
                continue;

            if (existingItem.Product != product)
            {
                Debug.Log(
                    "Tầng kệ đang chứa loại sản phẩm khác.",
                    this);

                return false;
            }
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


    public bool TryCollectOneMatchingProduct(BoxContents box)
    {
        if (box == null || !box.IsOpen)
            return false;

        ProductDefinition boxProduct = box.Product;

        if (boxProduct == null || slots == null)
            return false;

        // Duyệt từ cuối mảng về đầu mảng để lấy hàng
        // theo thứ tự ngược với lúc fill.
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            ShelfSlot slot = slots[i];

            if (slot == null || !slot.IsOccupied)
                continue;

            InteractableObject item = slot.Occupant;

            if (item == null || item.IsHeld)
                continue;

            // Chỉ thu hồi sản phẩm cùng loại với thùng.
            if (item.Product != boxProduct)
                continue;

            // Chỉ chuyển hàng vào thùng khi có thể thu hồi.
            if (!box.TryAdd(1))
                return false;

            InteractableObject removedItem = slot.TakeOccupant();

            if (removedItem != item)
            {
                // Hoàn tác nếu slot không còn hợp lệ.
                box.TryTake(1);

                Debug.LogError(
                    "Không thể thu hồi sản phẩm: trạng thái slot đã thay đổi.",
                    slot);

                return false;
            }

            Destroy(removedItem.gameObject);

            Debug.Log(
                $"Đã thu hồi {boxProduct.DisplayName}. " +
                $"Số lượng trong thùng: {box.Quantity}",
                box);

            return true;
        }

        return false;
    }
}
