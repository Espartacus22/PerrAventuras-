using UnityEngine;

public static class ItemDatabase
{
    public static ItemData GetItem(int id)
        => Resources.Load<ItemData>($"Items/Item_{id:D3}");
}
