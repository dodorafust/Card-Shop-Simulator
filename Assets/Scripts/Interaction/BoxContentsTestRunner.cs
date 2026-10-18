
using UnityEngine;

public class BoxContentsTestRunner : MonoBehaviour
{
    [SerializeField] private BoxContents box;

    [ContextMenu("Run Box Contents Tests")]
    private void RunTests()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Hãy nhấn Play trước khi chạy kiểm thử.", this);
            return;
        }

        if (box == null)
        {
            Debug.LogError("Chưa gán BoxContents.", this);
            return;
        }

        int initial = box.Quantity;

        if (initial < 3)
        {
            Debug.LogError(
                "Thùng cần có ít nhất 3 sản phẩm để kiểm thử.", this);
            return;
        }

        bool passed = true;

        passed &= Check(
            box.TryTake(3),
            "Lấy 3 sản phẩm thành công");

        passed &= Check(
            box.Quantity == initial - 3,
            "Số lượng giảm đúng 3");

        int quantityBeforeInvalidTake = box.Quantity;

        passed &= Check(
            !box.TryTake(quantityBeforeInvalidTake + 1),
            "Từ chối lấy vượt số lượng còn lại");

        passed &= Check(
            box.Quantity == quantityBeforeInvalidTake,
            "Số lượng không đổi sau thao tác lấy thất bại");

        passed &= Check(
            box.TryAdd(2),
            "Thêm lại 2 sản phẩm thành công");

        passed &= Check(
            box.Quantity == initial - 1,
            "Số lượng sau cùng bằng số ban đầu trừ 1");

        Debug.Log(
            passed
                ? "BOX CONTENTS TEST: TẤT CẢ ĐỀU PASS"
                : "BOX CONTENTS TEST: CÓ TEST FAIL",
            this);
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
