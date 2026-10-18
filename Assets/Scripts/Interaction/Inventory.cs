
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryEntry
{
    [SerializeField]
    private ProductDefinition product;

    [SerializeField, Min(0)]
    private int quantity;

    public ProductDefinition Product => product;
    public int Quantity => quantity;

    public InventoryEntry(ProductDefinition product, int quantity)
    {
        this.product = product;
        this.quantity = quantity;
    }

    public void Add(int amount)
    {
        quantity += amount;
    }

    public bool TryRemove(int amount)
    {
        if (amount <= 0 || amount > quantity)
            return false;

        quantity -= amount;
        return true;
    }
}

public class Inventory : MonoBehaviour
{
    [SerializeField]
    private List<InventoryEntry> entries = new List<InventoryEntry>();

    public int GetQuantity(ProductDefinition product)
    {
        InventoryEntry entry = FindEntry(product);
        return entry != null ? entry.Quantity : 0;
    }

    public bool TryAdd(ProductDefinition product, int amount)
    {
        if (product == null || amount <= 0)
            return false;

        InventoryEntry entry = FindEntry(product);

        if (entry != null)
        {
            entry.Add(amount);
        }
        else
        {
            entries.Add(new InventoryEntry(product, amount));
        }

        return true;
    }

    public bool TryRemove(ProductDefinition product, int amount)
    {
        if (product == null || amount <= 0)
            return false;

        InventoryEntry entry = FindEntry(product);

        if (entry == null || !entry.TryRemove(amount))
            return false;

        if (entry.Quantity == 0)
            entries.Remove(entry);

        return true;
    }

    private InventoryEntry FindEntry(ProductDefinition product)
    {
        if (product == null)
            return null;

        foreach (InventoryEntry entry in entries)
        {
            if (entry != null && entry.Product == product)
                return entry;
        }

        return null;
    }
}
