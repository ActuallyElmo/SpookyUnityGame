using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Item))]
public class PickupItem : MonoBehaviour
{
    private Item itemData;

    void Awake()
    {
        itemData = GetComponent<Item>(); // ia componenta Item de pe cub
    }

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        Camera cam = Camera.main;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                bool added = InventoryManager.instance.AddItem(itemData);
                if (added)
                    Destroy(gameObject); // dispare de pe jos
            }
        }
    }
}
