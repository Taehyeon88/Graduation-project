using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    //스킬, 아이템, 상태 효과에 대한 설명을 담는다
    //[SerializeField] private RectTransform[]
    [SerializeField] private TMP_Text title_Text;
    [SerializeField] private TMP_Text description_Text;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private int characterWrapLimit;
    [SerializeField] private float spacing;
    [SerializeField] private RectTransform rectTransform;

    private bool isTrackingMode = false;

    private void Update()
    {
        Vector2 position = Input.mousePosition;

        if (isTrackingMode)
        {
            transform.position = position;
        }
    }

    public void SetText(string description, string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            title_Text.gameObject.SetActive(false);
        }
        else
        {
            title_Text.gameObject.SetActive(true);
            title_Text.SetText(title);
        }

        description_Text.SetText(description);

        int titleLength = title_Text.text.Length;
        int descriptionLength = description_Text.text.Length;

        layoutElement.enabled =
            (titleLength > characterWrapLimit || descriptionLength > characterWrapLimit)
            ? true : false;
    }

    public void SetMode(bool isTracking, Vector2 position)
    {
        float pivotX = position.x >= Screen.width / 2 ? 1 : 0;
        float pivotY = position.y >= Screen.height / 2 ? 1 : 0;

        rectTransform.pivot = new Vector2(pivotX, pivotY);

        Vector2 spacingVector = new Vector2(
            pivotX > 0.0f ? - spacing : spacing,
            pivotY > 0.0f ? - spacing : spacing
            );
        transform.position = position + spacingVector;

        isTrackingMode = isTracking;
    }
}
