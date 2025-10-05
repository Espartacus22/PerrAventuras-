using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Item
{
    public string name;
    public int quantity;

    public Item(string name, int quantity = 1)
    {
        this.name = name;
        this.quantity = quantity;
    }

    public virtual void Use()
    {
        Debug.Log("Used ítem: " + name);
    }
}

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    public int maxSlots = 10;

    public void AddItem(Item item)
    {
        if (items.Count < maxSlots)
        {
            items.Add(item);
            Debug.Log("added element: " + item.name);
        }
        else
        {
            Debug.Log("Inventory is full. Cannot be added:" + item.name);
        }
    }

    public void RemoveItem(Item item)
    {
        if (items.Remove(item))
        {
            Debug.Log("Ítem deleted: " + item.name);
        }
        else
        {
            Debug.Log("Ítem not found: " + item.name);
        }
    }

    public void UseItem(Item item)
    {
        if (items.Contains(item))
        {
            item.Use();
        }
        else
        {
            Debug.Log("Ítem not found: " + item.name);
        }
    }
}