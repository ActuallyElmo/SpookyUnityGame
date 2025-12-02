using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public Transform holdParent; 
    private bool isHeld = false;
    private PlayerInputHandler playerInput; 

    void Update()
{
    if (!isHeld || playerInput == null) return;

    if (playerInput.DropTriggered)
    {
        Drop();
        playerInput.ResetDropTrigger();
    }
}



    private void OnTriggerStay(Collider other)
    {
        if (!isHeld)
        {
            PlayerInputHandler input = other.GetComponent<PlayerInputHandler>();
            if (input != null && input.PickupTriggered)
            {
                Pickup(input);
                input.ResetPickupTrigger();
            }
        }
    }

    void Pickup(PlayerInputHandler input)
    {
        isHeld = true;
        playerInput = input;

        transform.SetParent(holdParent);
        transform.localPosition = new Vector3(0.2f, -0.3f, 0.5f); 
        transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    void Drop()
    {
        isHeld = false;
        transform.SetParent(null);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        transform.position = holdParent.position + holdParent.forward * 1f;
        transform.rotation = Quaternion.identity;

        playerInput = null; 
    }
}
