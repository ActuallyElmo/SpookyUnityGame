using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemName; // Numele obiectului
    public Sprite icon;     // Imaginea care va apărea în inventar
    public int id; // indexul prefab-ului în InventoryManager.itemPrefabs
}
