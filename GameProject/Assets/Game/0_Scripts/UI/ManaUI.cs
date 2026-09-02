using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManaUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mana_Text;
    [SerializeField] private Slider mana_Slider;

    [Header("Mana Tween Animantion")]
    [SerializeField] private float refill_duration = 0.1f;
    [SerializeField] private Ease refill_ease = Ease.Linear;

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<RefillManaGA>(RefillManaPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<SpendManaGA>(SpendManaPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<RefillManaGA>(RefillManaPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<SpendManaGA>(SpendManaPostReaction, ReactionTiming.POST);
    }

    private void RefillManaPostReaction(RefillManaGA refillManaGA)
    {
        PlayTween().OnComplete(() =>
        {
            mana_Text.SetText(ManaSystem.Instance.CurrentMana.ToString());
        });

    }
    private void SpendManaPostReaction(SpendManaGA spendManaGA)
    {
        PlayTween().OnComplete(() =>
        {
            mana_Text.SetText(ManaSystem.Instance.CurrentMana.ToString());
        });
    }

    private Tween PlayTween()
    {
         return DOTween.To(() =>
            mana_Slider.value,
            v => mana_Slider.value = v,
            ManaSystem.Instance.CurrentMana / (float)ManaSystem.Instance.MaxMana,
            refill_duration
        ).SetEase(refill_ease);
    }
}
