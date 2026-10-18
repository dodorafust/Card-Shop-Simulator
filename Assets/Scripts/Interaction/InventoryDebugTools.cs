
using UnityEngine;

public class InventoryDebugTools : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ProductDefinition product;
    [SerializeField, Min(1)] private int quantity = 5;

    [ContextMenu("Add Test Stock")]
    private void AddTestStock()
    {
        if (!ValidateReferences())
            return;

        bool success = inventory.TryAdd(product, quantity);

        Debug.Log(
            success
                ? $"Đã nhập {quantity} x {product.DisplayName}. " +
                  $"Tồn kho: {inventory.GetQuantity(product)}"
                : "Nhập hàng thất bại.",
            this);
    }

    [ContextMenu("Remove Test Stock")]
    private void RemoveTestStock()
    {
        if (!ValidateReferences())
            return;

        bool success = inventory.TryRemove(product, quantity);

        Debug.Log(
            success
                ? $"Đã lấy {quantity} x {product.DisplayName}. " +
                  $"Tồn kho: {inventory.GetQuantity(product)}"
                : $"Không thể lấy {quantity}. " +
                  $"Tồn kho hiện tại: {inventory.GetQuantity(product)}",
            this);
    }

    [ContextMenu("Log Current Stock")]
    private void LogCurrentStock()
    {
        if (!ValidateReferences())
            return;

        Debug.Log(
            $"{product.DisplayName}: " +
            $"{inventory.GetQuantity(product)} sản phẩm trong kho.",
            this);
    }

    private bool ValidateReferences()
    {
        if (inventory != null && product != null)
            return true;

        Debug.LogError(
            "Hãy gán Inventory và Product trong Inspector.",
            this);

        return false;
    }
}
