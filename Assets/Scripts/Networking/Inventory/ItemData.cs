using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public Sprite icon;

    public int maxStack = 99;
    public bool isConsumable;
    public float healAmount;
    public bool isEquipable;

    // Prefab que se dropea en el mundo desde el inventario tras descartarlo, para que luego cualquiera pueda recogerlo 
    public GameObject pickupPrefab;
}
