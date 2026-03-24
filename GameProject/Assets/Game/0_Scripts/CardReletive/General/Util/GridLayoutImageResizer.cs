using UnityEngine;
using UnityEngine.UI;

public class GridLayoutImageResizer : MonoBehaviour
{
    private GridLayoutGroup gridLayoutGroup;
    private RectTransform rectTransform;

    private int curRowCount;
    void Start()
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
    void Update()
    {
        if (gridLayoutGroup != null && rectTransform != null)
        {
            int count = transform.childCount;
            int culumnCount = gridLayoutGroup.constraintCount;
            int rowCount = count / culumnCount + (count % culumnCount == 0 ? 0 : 1);

            if (curRowCount != rowCount)
            {
                float paddingSum = gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom;
                float cellSizeY = gridLayoutGroup.cellSize.y;
                float spacingY = gridLayoutGroup.spacing.y;

                //윗 패딩 + 아랫 패딩 + 셀 크기 * 줄 개수 + 스페이싱 크기 * (줄 개수 -1)
                float height = paddingSum + cellSizeY * rowCount + spacingY * (rowCount - 1);
                rectTransform.sizeDelta = new(rectTransform.sizeDelta.x, height);

                curRowCount = rowCount;
            }
        }
    }
}
