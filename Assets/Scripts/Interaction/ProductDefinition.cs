
using UnityEngine;

public enum ProductCategory
{
    Standard,
    CardPack,
    BlindBox,
    BlindBag
}

[CreateAssetMenu(
    fileName = "NewProduct",
    menuName = "Game/Products/Product Definition")]
public class ProductDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string productId;
    [SerializeField] private string displayName;

    [Header("Product Information")]
    [SerializeField] private ProductCategory category;
    [SerializeField, Min(0)] private int basePrice;

    [Header("World Representation")]
    [SerializeField] private GameObject worldPrefab;

    public string ProductId => productId;
    public string DisplayName => displayName;
    public ProductCategory Category => category;
    public int BasePrice => basePrice;
    public GameObject WorldPrefab => worldPrefab;
}
