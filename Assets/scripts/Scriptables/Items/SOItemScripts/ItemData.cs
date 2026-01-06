using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Information")]
    public KeyIds itemID;
    public string itemName;
    [TextArea]
    public string itemDescription;
    public Sprite itemImage;

    [Header("Classification")]
    public ItemType itemType;
    public ItemRarity itemRarity;

    [Header("Stack")]
    public int maxStack = 1;
    public bool isStackable;

    private void OnValidate()
    {
        if (itemType is ItemType.Equippable or ItemType.Collectable)
        {
            isStackable = false;
            maxStack = 1;
        }
        else
        {
            isStackable = true;
            if (maxStack < 1)
            {
                maxStack = 1;
            }
        }
    }
}