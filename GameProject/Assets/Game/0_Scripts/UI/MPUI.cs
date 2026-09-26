using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MPUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mp_Text;
    [SerializeField] private Slider mp_Slider;

    [Header("Mana Tween Animantion")]
    [SerializeField] private float refill_duration = 0.1f;
    [SerializeField] private Ease refill_ease = Ease.Linear;


    //Publics
    public void RefillMPUI(int mp)
    {
        PlayTween().OnComplete(() =>
        {
            mp_Text.SetText(mp.ToString());
        });
    }
    public void SpendMPUI(int mp)
    {
        PlayTween().OnComplete(() =>
        {
            mp_Text.SetText(mp.ToString());
        });
    }

    //Privates

    private Tween PlayTween()
    {
        return DOTween.To(() =>
           mp_Slider.value,
           v => mp_Slider.value = v,
           MPSystem.Instance.CurrentMP / (float)MPSystem.Instance.MaxMP,
           refill_duration
       ).SetEase(refill_ease);
    }
}
