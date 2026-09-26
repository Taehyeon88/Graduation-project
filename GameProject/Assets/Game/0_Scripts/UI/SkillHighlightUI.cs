using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SkillHighlightUI : MonoBehaviour
{
    [SerializeField] private RectTransform Highlight_Rect;
    [SerializeField] private Color defualt_Color;
    [SerializeField] private float color_Trans_Duration = 8f;

    private Image highlightImage;
    private Sequence coloredSqu;
    private void Start()
    {
        coloredSqu = DOTween.Sequence();

        float hue = 0f;

        highlightImage = Highlight_Rect.GetComponent<Image>();
        coloredSqu.Append(
                    DOTween.To(
                        () => hue,
                        value =>
                        {
                            hue = value;
                            highlightImage.color = Color.HSVToRGB(hue, 0.7f, 1f);
                        },
                        1f,
                        color_Trans_Duration
                    )
                );
        coloredSqu.SetLoops(-1, LoopType.Yoyo).Pause();
    }

    public void Show(Vector2 position)
    {
        highlightImage.color = defualt_Color;
        Highlight_Rect.gameObject.SetActive(true);
        Highlight_Rect.anchoredPosition = position;
    }

    public void Hide()
    {
        Highlight_Rect.gameObject.SetActive(false);
    }

    public void ShowSeleted(Vector2 position)
    {
        Highlight_Rect.gameObject.SetActive(true);
        Highlight_Rect.anchoredPosition = position;
        coloredSqu.Play();
    }
    public void UpdateSeletedPosition(Vector2 position)
    {
        Highlight_Rect.anchoredPosition = position;
    }

    public void HideSeleted()
    {
        coloredSqu.Pause();
        Highlight_Rect.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        coloredSqu?.Kill();
    }
}
