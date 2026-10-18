
using UnityEngine;

public class BoxProductTransfer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxContents boxContents;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private ShelfSlot targetSlot;

    [ContextMenu("Test Transfer One Product To Shelf")]
    public void TransferOneProductToShelf()
    {
        if (boxContents == null || holdPoint == null || targetSlot == null)
        {
            Debug.LogError("Chưa gán đủ BoxContents, HoldPoint hoặc ShelfSlot.", this);
            return;
        }

        if (targetSlot.IsOccupied)
        {
            Debug.LogWarning("Ô kệ đã có sản phẩm.", this);
            return;
        }

        ProductDefinition product = boxContents.Product;

        if (product == null || product.WorldPrefab == null)
        {
            Debug.LogError("Chưa gán Product hoặc WorldPrefab cho thùng hàng.", this);
            return;
        }

        if (boxContents.Quantity <= 0)
        {
            Debug.LogWarning("Thùng hàng đã hết sản phẩm.", this);
            return;
        }

        GameObject spawnedObject = Instantiate(
            product.WorldPrefab,
            holdPoint.position,
            Quaternion.identity
        );

        InteractableObject interactable =
            spawnedObject.GetComponent<InteractableObject>();

        if (interactable == null)
        {
            Destroy(spawnedObject);
            Debug.LogError(
                "Prefab thiếu InteractableObject. Số lượng trong thùng không thay đổi.",
                this
            );
            return;
        }

        // Chuyển vật thể sang trạng thái được cầm để tương thích
        // với quy trình đặt sản phẩm hiện có.
        interactable.Interact(holdPoint);

        // Chỉ lấy hàng khỏi thùng khi ô kệ nhận được sản phẩm.
        if (!targetSlot.TryPlace(interactable))
        {
            Destroy(spawnedObject);
            Debug.LogWarning(
                "Không đặt được sản phẩm. Số lượng trong thùng được giữ nguyên.",
                this
            );
            return;
        }

        // Trừ số lượng sau khi đặt thành công.
        if (!boxContents.TryTake(1))
        {
            // Trường hợp bất thường: hoàn tác việc đặt sản phẩm.
            targetSlot.Release(interactable);
            Destroy(spawnedObject);

            Debug.LogError(
                "Không thể trừ số lượng trong thùng; đã hoàn tác việc đặt.",
                this
            );
            return;
        }

        Debug.Log(
            $"Đã đặt {product.DisplayName} lên kệ. " +
            $"Số lượng còn lại trong thùng: {boxContents.Quantity}",
            this
        );
    }
}
