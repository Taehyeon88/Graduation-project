using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class APUI : MonoBehaviour
{
    [SerializeField] private TMP_Text ap_Text;
    [SerializeField] private Slider ap_Slider;

    [Header("Mana Tween Animantion")]
    [SerializeField] private float refill_duration = 0.1f;
    [SerializeField] private Ease refill_ease = Ease.Linear;


    //Publics
    public void RefillAPUI(int ap)
    {
        PlayTween().OnComplete(() =>
        {
            ap_Text.SetText(ap.ToString());
        });
    }
    public void SpendAPUI(int ap)
    {
        PlayTween().OnComplete(() =>
        {
            ap_Text.SetText(ap.ToString());
        });
    }

    //Privates

    private Tween PlayTween()
    {
         return DOTween.To(() =>
            ap_Slider.value,
            v => ap_Slider.value = v,
            APSystem.Instance.CurrentAP / (float)APSystem.Instance.MaxAP,
            refill_duration
        ).SetEase(refill_ease);
    }
}
