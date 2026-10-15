
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform furnitureFloor;

    [Header("Placement Preview")]
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

    [Header("Furniture Placement")]
    [SerializeField] private LayerMask floorLayer;

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

    private bool TryGetFurnitureFloorHit(out RaycastHit hit)
    {
        if (Physics.Raycast(
            transform.position,
            transform.forward,
            out hit,
            interactionDistance,
            floorLayer,
            QueryTriggerInteraction.Ignore))
        {
            return hit.normal.y >= 0.9f;
        }

        return false;
    }


    private bool TryGetFurniturePreviewPoint(
        out Vector3 previewPoint,
        out bool isValidFloorHit)
    {
        previewPoint = Vector3.zero;
        isValidFloorHit = false;

        // Ưu tiên raycast sàn như logic hiện tại.
        if (TryGetFurnitureFloorHit(out RaycastHit floorHit))
        {
            previewPoint = floorHit.point;
            isValidFloorHit = true;
            return true;
        }

        // Không chạm sàn: chỉ tìm vị trí dự phòng để hiển thị preview.
        if (furnitureFloor == null)
            return false;

        Ray ray = new Ray(transform.position, transform.forward);

        // Nếu tia nhìn chạm kệ hoặc vật cản,
        // chiếu điểm va chạm xuống độ cao của sàn.
        if (Physics.Raycast(
            ray,
            out RaycastHit obstacleHit,
            interactionDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            previewPoint = new Vector3(
                obstacleHit.point.x,
                furnitureFloor.position.y,
                obstacleHit.point.z);

            return true;
        }

        // Nếu không chạm vật thể nào, tìm giao điểm với mặt phẳng sàn.
        float floorY = furnitureFloor.position.y;
        float directionY = ray.direction.y;

        if (Mathf.Abs(directionY) < 0.001f)
        {
            // Tia gần song song với sàn: dùng điểm cuối tầm nhìn,
            // chiếu xuống mặt phẳng sàn để vẫn hiển thị preview.
            Vector3 endPoint = ray.GetPoint(interactionDistance);

            previewPoint = new Vector3(
                endPoint.x,
                floorY,
                endPoint.z);

            return true;
        }

        float distanceToFloor =
            (floorY - ray.origin.y) / directionY;

        if (distanceToFloor >= 0f &&
            distanceToFloor <= interactionDistance)
        {
            previewPoint = ray.GetPoint(distanceToFloor);
            return true;
        }

        // Không có giao điểm trong tầm nhìn: vẫn tạo điểm dự phòng
        // trên sàn, nhưng điểm này không được xem là hợp lệ để đặt.
        Vector3 fallbackPoint = ray.GetPoint(interactionDistance);

        previewPoint = new Vector3(
            fallbackPoint.x,
            floorY,
            fallbackPoint.z);

        return true;
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

        if (!TryGetFurnitureFloorHit(out RaycastHit hit))
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



        bool isFurniture =
            heldObject != null &&
            heldObject.Type == PlacementType.Furniture;

        bool hasValidHit;
        RaycastHit hit;
        Vector3 furniturePreviewPoint = Vector3.zero;
        bool isValidFurnitureFloorHit = false;

        if (isFurniture)
        {
            hasValidHit = TryGetFurniturePreviewPoint(
                out furniturePreviewPoint,
                out isValidFurnitureFloorHit);

            // Dùng một RaycastHit giả để tái sử dụng phần xử lý bên dưới.
            hit = default;

            if (hasValidHit)
            {
                hit.point = furniturePreviewPoint;
                hit.normal = Vector3.up;
            }
        }
        else
        {
            hasValidHit = TryGetPlacementHit(out hit);
        }

        if (!hasValidHit)
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

            isValid =
                isValid &&
                insideShop &&
                isValidFurnitureFloorHit;

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
