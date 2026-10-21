
using UnityEngine;

public class BoxStockTransfer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private BoxContents boxContents;

    [Header("Transfer Settings")]
    [SerializeField, Min(1)] private int quantity = 5;

    [ContextMenu("Transfer Stock From Inventory To Box")]
    private void TransferStockFromInventoryToBox()
    {
        if (inventory == null || boxContents == null)
        {
            Debug.LogError(
                "Chưa gán Inventory hoặc BoxContents.",
                this);
            return;
        }

        ProductDefinition product = boxContents.Product;

        if (product == null)
        {
            Debug.LogError(
                "Thùng hàng chưa được gán ProductDefinition.",
                this);
            return;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning(
                "Số lượng chuyển phải lớn hơn 0.",
                this);
            return;
        }

        // Kiểm tra kho có đủ hàng trước khi chuyển.
        if (inventory.GetQuantity(product) < quantity)
        {
            Debug.LogWarning(
                "Kho không đủ hàng để chuyển vào thùng.",
                this);
            return;
        }

        // Trừ hàng trong kho trước.
        if (!inventory.TryRemove(product, quantity))
        {
            Debug.LogWarning(
                "Không thể lấy hàng khỏi kho.",
                this);
            return;
        }

        // Thêm hàng vào thùng.
        if (!boxContents.TryAdd(quantity))
        {
            // Hoàn tác nếu thùng không nhận được hàng.
            inventory.TryAdd(product, quantity);

            Debug.LogError(
                "Thêm hàng vào thùng thất bại. " +
                "Đã hoàn tác số lượng trong kho.",
                this);
            return;
        }

        Debug.Log(
            $"Chuyển thành công {quantity} x {product.DisplayName}. " +
            $"Tồn kho: {inventory.GetQuantity(product)}. " +
            $"Số lượng trong thùng: {boxContents.Quantity}",
            this);
    }
}
