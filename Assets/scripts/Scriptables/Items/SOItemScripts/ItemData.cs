
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Information")]
    public bool isKey;

    [ShowIf("@isKey")]
    public KeyId keyId;

    [HideIf("@isKey")]
    public ItemId itemId;
    
    [HideIf("@isKey")]
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
        itemName = keyId.name;
        
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