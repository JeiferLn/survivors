using UnityEngine;

public class ItemInfo : MonoBehaviour
{
    [SerializeField]
    private ItemData itemData;

    private void Start()
    {
        Debug.Log(
            $"Id: {itemData.itemID}\n" +
            $"Name: {itemData.itemName}\n" +
            $"Type: {itemData.itemType}\n" +
            $"Description: {itemData.itemDescription}\n"
        );
    }
}