using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SkillHighlightUI : MonoBehaviour
{
    [SerializeField] private RectTransform Highlight_Rect;
    [SerializeField] private Color defualt_Color;
    [SerializeField] private float color_Trans_Duration = 8f;

    private Image highlight_Image;
    private Sequence colored_Squ;
    private void Start()
    {
        colored_Squ = DOTween.Sequence();

        float hue = 0f;

        highlight_Image = Highlight_Rect.GetComponent<Image>();
        colored_Squ.Append(
                    DOTween.To(
                        () => hue,
                        value =>
                        {
                            hue = value;
                            highlight_Image.color = Color.HSVToRGB(hue, 0.7f, 1f);
                        },
                        1f,
                        color_Trans_Duration
                    )
                );
        colored_Squ.SetLoops(-1, LoopType.Yoyo).Pause();
    }

    public void Show(Vector2 position)
    {
        highlight_Image.color = defualt_Color;
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
        colored_Squ.Play();
    }
    public void UpdateSeletedPosition(Vector2 position)
    {
        Highlight_Rect.anchoredPosition = position;
    }

    public void HideSeleted()
    {
        colored_Squ.Pause();
        Highlight_Rect.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        colored_Squ?.Kill();
    }
}
