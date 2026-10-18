
using UnityEngine;

public class BoxContents : MonoBehaviour
{
    [Header("Product")]
    [SerializeField] private ProductDefinition product;

    [Header("Stock")]
    [SerializeField, Min(0)] private int initialQuantity = 10;
    [SerializeField] private bool isOpen;

    public bool IsOpen => isOpen;

    public void ToggleOpen()
    {
        isOpen = !isOpen;

        Debug.Log(
            isOpen ? "Đã mở thùng hàng." : "Đã đóng thùng hàng.",
            this
        );
    }

    private int quantity;

    public ProductDefinition Product => product;
    public int Quantity => quantity;

    private void Awake()
    {
        quantity = initialQuantity;
    }
    public bool TryTake(int amount)
    {
        if (!isOpen || amount <= 0 || amount > quantity)
            return false;

        quantity -= amount;
        return true;
    }
    public bool TryAdd(int amount)
    {
        if (amount <= 0)
            return false;

        quantity += amount;
        return true;
    }
}
