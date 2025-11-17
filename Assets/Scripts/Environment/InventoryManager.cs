using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("UI Slots")]
    public Image[] slotImages; // 3 sloturi UI

    [Header("Items")]
    public Item[] slots = new Item[3]; // 3 sloturi fixe
    public GameObject[] itemPrefabs;   // Prefab-urile obiectelor pentru drop

    [Header("Slot Selection")]
    public int selectedSlotIndex = 0;
    public Vector2 defaultSlotSize = new Vector2(90, 90);
    public Vector2 selectedSlotSize = new Vector2(95, 95);

    void Awake()
    {
        instance = this;
    }

    // Adaugă item în primul slot liber
    public bool AddItem(Item newItem)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = newItem;
                UpdateUI();
                Debug.Log($"Item adaugat: {newItem.itemName} in slot {i}");
                return true;
            }
        }
        Debug.Log("Inventar plin!");
        return false;
    }

    void Update()
    {
        HandleSlotSelection();

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            DropSelectedItem();
        }
    }

    void HandleSlotSelection()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            selectedSlotIndex = 0;
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            selectedSlotIndex = 1;
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            selectedSlotIndex = 2;

        UpdateSlotSizes();
    }

    void UpdateSlotSizes()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            RectTransform rt = slotImages[i].GetComponent<RectTransform>();
            rt.sizeDelta = (i == selectedSlotIndex) ? selectedSlotSize : defaultSlotSize;
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slots[i] != null)
            {
                slotImages[i].sprite = slots[i].icon;
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].color = new Color(1, 1, 1, 0.2f);
            }
        }

        UpdateSlotSizes();
    }

    public void DropSelectedItem()
    {
        Item itemToDrop = slots[selectedSlotIndex];
        if (itemToDrop != null)
        {
            // Creează obiectul în fața playerului
            Vector3 dropPos = PlayerSingleton.instance.transform.position + PlayerSingleton.instance.transform.forward * 2f;
            Instantiate(itemPrefabs[itemToDrop.id], dropPos, Quaternion.identity);

            // Scoate itemul din slot
            slots[selectedSlotIndex] = null;
            UpdateUI();
            Debug.Log($"Dropped {itemToDrop.itemName} from slot {selectedSlotIndex}");
        }
        else
        {
            Debug.Log("Slot gol!");
        }
    }
}
