
using UnityEngine;

public enum PlacementType
{
    ShelfProduct,
    Furniture
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]

public class InteractableObject : MonoBehaviour
{
    private Rigidbody rb;
    private BoxCollider boxCollider;
    private Collider[] boxColliders;
    private bool[] boxColliderWasEnabled;
    private Transform originalParent;
    private ShelfSlot currentShelfSlot;
    private bool isHeld;
    private float furnitureRotationY;

    [SerializeField]
    private PlacementType placementType = PlacementType.ShelfProduct;

    [SerializeField]
    private ProductDefinition productDefinition;

    public PlacementType Type => placementType;
    public ProductDefinition Product => productDefinition;

    public bool IsHeld => isHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        if (productDefinition != null)
        {
            Debug.Log(
                $"Sản phẩm: {productDefinition.DisplayName} | " +
                $"ID: {productDefinition.ProductId} | " +
                $"Giá cơ bản: {productDefinition.BasePrice}",
                this);
        }
    }

    public virtual void Interact(Transform holdPoint)
    {
        if (isHeld || holdPoint == null)
            return;

        if (currentShelfSlot != null)
        {
            currentShelfSlot.Release(this);
            currentShelfSlot = null;
        }
        originalParent = transform.parent;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (GetComponent<BoxContents>() != null)
        {
            // Lưu trạng thái collider trước khi tắt.
            boxColliders = GetComponentsInChildren<Collider>(true);
            boxColliderWasEnabled = new bool[boxColliders.Length];

            for (int i = 0; i < boxColliders.Length; i++)
            {
                boxColliderWasEnabled[i] = boxColliders[i].enabled;
                boxColliders[i].enabled = false;
            }
        }
        else
        {
            // Giữ nguyên hành vi hiện tại của các vật thể khác.
            boxCollider.enabled = false;
        }

        // Lưu góc xoay thế giới trước khi gắn vào HoldPoint.
        if (placementType == PlacementType.Furniture)
        {
            furnitureRotationY = transform.eulerAngles.y;
        }

        if (placementType == PlacementType.Furniture)
        {
            furnitureRotationY = transform.eulerAngles.y;
        }

        transform.SetParent(holdPoint, false);
        transform.localPosition = Vector3.zero;

        if (placementType == PlacementType.Furniture)
        {
            transform.rotation = GetFurnitureRotation();
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }

        isHeld = true;
        // Nội thất được ẩn khi cầm, sản phẩm nhỏ vẫn hiển thị.
        if (placementType == PlacementType.Furniture)
        {
            foreach (Renderer objectRenderer in GetComponentsInChildren<Renderer>())
            {
                objectRenderer.enabled = false;
            }
        }
        Debug.Log("Đã nhặt: " + gameObject.name);
    }

    public bool CanPlaceAt(
        Vector3 hitPoint,
        out Vector3 placePosition,
        out Quaternion placeRotation)
    {
        placePosition = hitPoint;
        placeRotation = Quaternion.identity;

        if (!isHeld)
            return false;

        placeRotation =
            Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        Vector3 scale = transform.lossyScale;
        scale = new Vector3(
            Mathf.Abs(scale.x),
            Mathf.Abs(scale.y),
            Mathf.Abs(scale.z));

        Vector3 halfExtents =
            Vector3.Scale(boxCollider.size, scale) * 0.5f;

        placePosition = hitPoint
            + Vector3.up * (halfExtents.y + 0.02f);

        Vector3 boxCenter = placePosition
            + placeRotation * Vector3.Scale(
                boxCollider.center, scale);

        Collider[] overlaps = Physics.OverlapBox(
            boxCenter,
            halfExtents - Vector3.one * 0.01f,
            placeRotation,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        foreach (Collider other in overlaps)
        {
            if (other.transform == transform ||
                other.transform.IsChildOf(transform))
                continue;

            return false;
        }

        return true;
    }

    public bool TryPlaceAt(Vector3 hitPoint)
    {
        if (!CanPlaceAt(
            hitPoint,
            out Vector3 placePosition,
            out Quaternion placeRotation))
        {
            Debug.Log("Vị trí đặt không hợp lệ.");
            return false;
        }

        transform.SetParent(originalParent, true);
        transform.SetPositionAndRotation(
            placePosition, placeRotation);

        if (GetComponent<BoxContents>() != null &&
            boxColliders != null &&
            boxColliderWasEnabled != null)
        {
            for (int i = 0; i < boxColliders.Length; i++)
            {
                if (boxColliders[i] != null)
                {
                    boxColliders[i].enabled = boxColliderWasEnabled[i];
                }
            }

            boxColliders = null;
            boxColliderWasEnabled = null;
        }
        else
        {
            boxCollider.enabled = true;
        }
        rb.isKinematic = false;
        isHeld = false;
        // Hiện lại mô hình sau khi đặt nội thất xuống.
        if (placementType == PlacementType.Furniture)
        {
            foreach (Renderer objectRenderer in GetComponentsInChildren<Renderer>())
            {
                objectRenderer.enabled = true;
            }
        }

        Debug.Log("Đã đặt: " + gameObject.name);
        return true;
    }

    public bool PlaceDirectlyAtSlot(Transform slotTransform)
    {
        if (slotTransform == null || isHeld)
            return false;

        if (currentShelfSlot != null)
        {
            currentShelfSlot.Release(this);
            currentShelfSlot = null;
        }

        // Đặt sản phẩm thành con của đối tượng chứa slot.
        transform.SetParent(slotTransform.parent, true);
        transform.SetPositionAndRotation(
            slotTransform.position,
            slotTransform.rotation);

        boxCollider.enabled = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;

        return true;
    }

    public void RotateHeld(float angle)
    {
        if (!isHeld)
            return;

        if (placementType == PlacementType.Furniture)
        {
            furnitureRotationY =
                (furnitureRotationY + angle) % 360f;

            transform.rotation = Quaternion.Euler(
                0f, furnitureRotationY, 0f);
        }
        else
        {
            transform.Rotate(0f, angle, 0f, Space.Self);
        }
    }

    public Quaternion GetFurnitureRotation()
    {
        return Quaternion.Euler(
            0f, furnitureRotationY, 0f);
    }

    public void SetShelfSlot(ShelfSlot slot)
    {
        currentShelfSlot = slot;
    }
}
