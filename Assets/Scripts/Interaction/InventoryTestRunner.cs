
using UnityEngine;

public class InventoryTestRunner : MonoBehaviour
{
    [SerializeField] private ProductDefinition testProduct;

    [ContextMenu("Run Inventory Tests")]
    private void RunInventoryTests()
    {
        if (testProduct == null)
        {
            Debug.LogError(
                "Chưa gán Test Product trong Inspector.", this);
            return;
        }

        GameObject testObject = new GameObject("TemporaryInventoryTest");
        Inventory testInventory = testObject.AddComponent<Inventory>();

        bool passed = true;

        passed &= Check(
            testInventory.TryAdd(testProduct, 5),
            "Nhập 5 sản phẩm");

        passed &= Check(
            testInventory.GetQuantity(testProduct) == 5,
            "Số lượng sau khi nhập là 5");

        passed &= Check(
            testInventory.TryRemove(testProduct, 2),
            "Lấy 2 sản phẩm");

        passed &= Check(
            testInventory.GetQuantity(testProduct) == 3,
            "Số lượng sau khi lấy là 3");

        passed &= Check(
            !testInventory.TryRemove(testProduct, 10),
            "Từ chối lấy vượt tồn kho");

        passed &= Check(
            testInventory.GetQuantity(testProduct) == 3,
            "Số lượng không đổi sau thao tác thất bại");

        passed &= Check(
            !testInventory.TryAdd(testProduct, 0),
            "Từ chối nhập số lượng bằng 0");

        passed &= Check(
            !testInventory.TryRemove(testProduct, 0),
            "Từ chối lấy số lượng bằng 0");

        Debug.Log(
            passed
                ? "INVENTORY TEST: TẤT CẢ ĐỀU PASS"
                : "INVENTORY TEST: CÓ TEST FAIL",
            this);

        if (Application.isPlaying)
            Destroy(testObject);
        else
            DestroyImmediate(testObject);
    }

    private bool Check(bool condition, string testName)
    {
        if (condition)
        {
            Debug.Log("[PASS] " + testName, this);
            return true;
        }

        Debug.LogError("[FAIL] " + testName, this);
        return false;
    }
}
