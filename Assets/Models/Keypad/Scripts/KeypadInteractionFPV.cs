using UnityEngine;

namespace NavKeypad
{
    public class KeypadInteractionFPV : MonoBehaviour
    {
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private LayerMask keypadLayer;

        private Camera cam;
        private PlayerInputHandler input;

        private void Awake()
        {
            cam = Camera.main;
            input = GetComponent<PlayerInputHandler>();
        }

        private void Update()
        {
            // Ray from center of screen
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green);

            if (!input.InteractTriggered) return;

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, keypadLayer))
            {
                if (hit.collider.TryGetComponent(out KeypadButton keypadButton))
                {
                    keypadButton.PressButton();
                }
            }

            // Consume input so it doesn't spam
            input.ResetPickupTrigger();
        }
    }
}
