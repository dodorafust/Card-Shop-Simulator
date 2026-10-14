
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
    private Transform originalParent;
    private ShelfSlot currentShelfSlot;
    private bool isHeld;
    private float furnitureRotationY;

    [SerializeField]
    private PlacementType placementType = PlacementType.ShelfProduct;

    public PlacementType Type => placementType;


    public bool IsHeld => isHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
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

        boxCollider.enabled = false;

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

        boxCollider.enabled = true;
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
