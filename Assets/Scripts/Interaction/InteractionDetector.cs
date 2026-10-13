
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform holdPoint;

    [Header("Placement Preview")]
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

    private InteractableObject currentTarget;
    private InteractableObject heldObject;
    private InputSystem_Actions inputActions;

    private GameObject previewObject;
    private MeshRenderer previewRenderer;
    private Material previewMaterial;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Dispose();

        if (previewObject != null)
            Destroy(previewObject);

        if (previewMaterial != null)
            Destroy(previewMaterial);
    }

    private void Update()
    {
        DetectTarget();

        if (inputActions.Player.Interact.WasPressedThisFrame())
        {
            if (heldObject != null)
            {
                TryPlaceHeldObject();
            }
            else if (currentTarget != null)
            {
                currentTarget.Interact(holdPoint);

                if (currentTarget.IsHeld)
                {
                    heldObject = currentTarget;
                    CreatePreview();
                }
            }
        }

        if (heldObject != null &&
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            heldObject.RotateHeld(15f);
        }

        UpdatePlacementPreview();
    }

    private void DetectTarget()
    {
        InteractableObject detectedTarget = null;

        if (Physics.Raycast(
            transform.position,
            transform.forward,
            out RaycastHit hit,
            interactionDistance))
        {
            detectedTarget =
                hit.collider.GetComponentInParent<InteractableObject>();
        }

        currentTarget = detectedTarget;
    }


    private bool TryGetPlacementHit(out RaycastHit hit)
    {
        if (Physics.Raycast(
            transform.position,
            transform.forward,
            out hit,
            interactionDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            return hit.normal.y >= 0.9f;
        }

        return false;
    }





    private void TryPlaceHeldObject()
    {
        if (!TryGetPlacementHit(out RaycastHit hit))
        {
            Debug.Log("Không tìm thấy bề mặt đặt phù hợp.");
            return;
        }

        ShelfPlacementZone shelfZone =
            hit.collider.GetComponentInParent<ShelfPlacementZone>();

        if (shelfZone == null)
        {
            Debug.Log("Chỉ có thể đặt sản phẩm lên kệ.");
            return;
        }

        ShelfSlot slot =
            shelfZone.FindNearestEmptySlot(hit.point);

        if (slot == null)
        {
            Debug.Log("Kệ không còn vị trí trống.");
            return;
        }

        if (!heldObject.CanPlaceAt(
            slot.transform.position,
            out _,
            out _))
        {
            Debug.Log("Vị trí slot không hợp lệ.");
            return;
        }

        if (shelfZone.TryPlaceInSlot(heldObject, slot))
        {
            heldObject = null;
            DestroyPreview();
        }
    }




    private void CreatePreview()
    {
        DestroyPreview();

        MeshFilter sourceMesh =
            heldObject.GetComponentInChildren<MeshFilter>();

        MeshRenderer sourceRenderer =
            heldObject.GetComponentInChildren<MeshRenderer>();

        if (sourceMesh == null || sourceRenderer == null)
        {
            Debug.LogWarning(
                "Không tìm thấy MeshFilter hoặc MeshRenderer.",
                heldObject);
            return;
        }

        previewObject = new GameObject("PlacementPreview");

        MeshFilter previewMesh =
            previewObject.AddComponent<MeshFilter>();
        previewMesh.sharedMesh = sourceMesh.sharedMesh;

        previewRenderer =
            previewObject.AddComponent<MeshRenderer>();

        previewMaterial = new Material(sourceRenderer.sharedMaterial);
        previewRenderer.sharedMaterial = previewMaterial;

        previewObject.transform.localScale =
            heldObject.transform.lossyScale;

        SetPreviewColor(invalidColor);
        previewObject.SetActive(false);
    }


    private void UpdatePlacementPreview()
    {
        if (heldObject == null || previewObject == null)
            return;

        if (!TryGetPlacementHit(out RaycastHit hit))
        {
            previewObject.SetActive(false);
            return;
        }

        ShelfPlacementZone shelfZone =
            hit.collider.GetComponentInParent<ShelfPlacementZone>();

        if (shelfZone == null)
        {
            previewObject.SetActive(false);
            return;
        }

        ShelfSlot slot =
            shelfZone.FindNearestEmptySlot(hit.point);

        if (slot == null)
        {
            previewObject.SetActive(false);
            return;
        }

        bool isValid = heldObject.CanPlaceAt(
            slot.transform.position,
            out Vector3 position,
            out Quaternion rotation);

        previewObject.SetActive(true);
        previewObject.transform.SetPositionAndRotation(
            position, rotation);

        SetPreviewColor(isValid ? validColor : invalidColor);
    }


    private void SetPreviewColor(Color color)
    {
        if (previewMaterial == null)
            return;

        if (previewMaterial.HasProperty("_BaseColor"))
            previewMaterial.SetColor("_BaseColor", color);

        if (previewMaterial.HasProperty("_Color"))
            previewMaterial.SetColor("_Color", color);
    }

    private void DestroyPreview()
    {
        if (previewObject != null)
            Destroy(previewObject);

        if (previewMaterial != null)
            Destroy(previewMaterial);

        previewObject = null;
        previewRenderer = null;
        previewMaterial = null;
    }

    private void OnGUI()
    {
        string prompt = null;

        if (heldObject != null)
        {
            prompt = "E: Đặt | R: Xoay";
        }
        else if (currentTarget != null)
        {
            prompt = "E: Nhặt";
        }

        if (prompt == null)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 20;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        GUI.Label(
            new Rect(
                Screen.width / 2f - 200f,
                Screen.height - 100f,
                400f,
                40f),
            prompt,
            style);
    }
}
