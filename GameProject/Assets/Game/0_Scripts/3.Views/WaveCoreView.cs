using DG.Tweening;
using IsoTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveCoreView : Token, IDamageable
{
    [SerializeField] private TMP_Text healthText;

    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    public void SetUp(WaveCoreData coreData)
    {
        CurrentHealth = MaxHealth = coreData.CoreHp;
        healthText.SetText(CurrentHealth.ToString());

        SetUpBaseBase(coreData);
    }

    public void Damage(int amount, DealDamageGA dealDamageGA)
    {
        SoundSystem.Instance.PlaySound(1);
        Tween hit_Tween = Utility.GetModelShakeTween(
                this,
                0.18f,
                new Vector3(0.10f, 0.03f, 0f),
                10,
                90f
            ).OnComplete(() => healthText.SetText(CurrentHealth.ToString()));

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);

        if (CurrentHealth <= 0)
        {
            //게임 종료
            Debug.Log("게임 승리");
        }
    }
}
