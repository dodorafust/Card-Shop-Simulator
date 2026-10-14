
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

    [SerializeField]
    private ShopPlacementArea shopPlacementArea;
    private InteractableObject currentTarget;
    private InteractableObject heldObject;
    private InputSystem_Actions inputActions;
    private float furnitureRotationY;

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

                    if (heldObject.Type == PlacementType.Furniture)
                    {

                    }

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
        if (heldObject == null)
            return;

        if (heldObject.Type == PlacementType.Furniture)
        {
            TryPlaceFurniture();
            return;
        }

        // Giữ nguyên quy tắc đặt hàng hóa lên kệ.
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

    private void TryPlaceFurniture()
    {
        if (shopPlacementArea == null)
        {
            Debug.LogWarning(
                "Chưa gán ShopPlacementArea trong Inspector.");
            return;
        }

        if (!TryGetPlacementHit(out RaycastHit hit))
        {
            Debug.Log("Không tìm thấy mặt sàn phù hợp.");
            return;
        }

        // Tính vị trí và góc xoay dự kiến của nội thất.
        Quaternion furnitureRotation =
            heldObject.GetFurnitureRotation();

        if (!heldObject.CanPlaceAt(
            hit.point,
            out Vector3 position,
            out _))
        {
            Debug.Log("Vị trí đặt nội thất bị chặn.");
            return;
        }

        // Tính nửa kích thước vùng chiếm chỗ.
        BoxCollider furnitureCollider =
            heldObject.GetComponent<BoxCollider>();

        if (furnitureCollider == null)
        {
            Debug.LogWarning("Nội thất không có BoxCollider.");
            return;
        }

        Vector3 halfExtents = Vector3.Scale(
            furnitureCollider.size,
            heldObject.transform.lossyScale) * 0.5f;

        // Kiểm tra toàn bộ vùng chiếm chỗ nằm trong cửa hàng.
        if (!shopPlacementArea.ContainsFootprint(
            position,
            halfExtents,
            furnitureRotation))
        {
            Debug.Log("Không thể đặt: nội thất vượt ranh giới cửa hàng.");
            return;
        }


        if (heldObject.TryPlaceAt(hit.point))
        {
            heldObject.transform.SetPositionAndRotation(
                position,
                heldObject.GetFurnitureRotation());

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

        // Nhánh đặt nội thất tự do trong cửa hàng.


        if (heldObject.Type == PlacementType.Furniture)
        {
            if (shopPlacementArea == null)
            {
                previewObject.SetActive(false);
                return;
            }

            bool isValid = heldObject.CanPlaceAt(
                hit.point,
                out Vector3 furniturePosition,
                out _);

            Quaternion furnitureRotation =
                heldObject.GetFurnitureRotation();

            // Kiểm tra toàn bộ vùng chiếm chỗ của nội thất.
            Vector3 halfExtents =
                Vector3.Scale(
                    heldObject.GetComponent<BoxCollider>().size,
                    heldObject.transform.lossyScale) * 0.5f;

            bool insideShop = shopPlacementArea.ContainsFootprint(
                furniturePosition,
                halfExtents,
                furnitureRotation);

            isValid = isValid && insideShop;

            previewObject.SetActive(true);
            previewObject.transform.SetPositionAndRotation(
                furniturePosition,
                furnitureRotation);

            SetPreviewColor(isValid ? validColor : invalidColor);
            return;
        }



        // Nhánh hàng hóa: giữ nguyên cơ chế preview theo slot kệ.
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

        bool isProductValid = heldObject.CanPlaceAt(
            slot.transform.position,
            out Vector3 productPosition,
            out Quaternion productRotation);

        previewObject.SetActive(true);
        previewObject.transform.SetPositionAndRotation(
            productPosition,
            productRotation);

        SetPreviewColor(isProductValid ? validColor : invalidColor);
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
