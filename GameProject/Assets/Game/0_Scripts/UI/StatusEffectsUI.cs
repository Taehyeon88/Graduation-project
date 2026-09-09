using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusEffectsUI : MonoBehaviour
{
    [SerializeField] private StatusEffectUI statusEffectPrefab;
    [SerializeField] private Transform shieldSEUI;
    [SerializeField] private TMP_Text shieldSEText;

    private Dictionary<StatusEffectType, StatusEffectUI> statusEffectUIs = new();
    private Tween add_Shield_Tween;
    private Sequence remove_Shield_Squ;
    private bool shield_StatusEffect = false;  //방어 존재 여부 체크

    public void UpdateStatusEffect(StatusEffectType statusEffectType, int stackCount, Sprite sprite = null)
    {
        //방어SE 예외처리
        if (statusEffectType == StatusEffectType.ARMOR)
        {
            if (stackCount <= 0)
            {
                if (shield_StatusEffect)
                {
                    shield_StatusEffect = false;
                    UpdateShieldSE(false);
                }
            }
            else
            {
                shieldSEText.SetText(stackCount.ToString());

                if (!shield_StatusEffect)
                {
                    UpdateShieldSE(true);
                    shield_StatusEffect = true;
                }
                else
                {
                    SoundSystem.Instance.PlaySound(2001);
                }
            }
            return;
        }


        if (stackCount == 0)
        {
            if (statusEffectUIs.ContainsKey(statusEffectType))
            {
                StatusEffectUI statusEffectUI = statusEffectUIs[statusEffectType];
                statusEffectUIs.Remove(statusEffectType);
                Destroy(statusEffectUI.gameObject);
            }
        }
        else
        {
            if (!statusEffectUIs.ContainsKey(statusEffectType))
            {
                StatusEffectUI statusEffectUI = Instantiate(statusEffectPrefab, transform);
                statusEffectUIs.Add(statusEffectType, statusEffectUI);
            }
            statusEffectUIs[statusEffectType].Set(sprite, stackCount);
        }
    }

    private void UpdateShieldSE(bool add)
    {
        if (add_Shield_Tween == null && remove_Shield_Squ == null)
        {
            add_Shield_Tween = shieldSEUI.DOScale(Vector3.one, 0.2f)
                                        .SetEase(Ease.InOutSine)
                                        .SetAutoKill(false)
                                        .Pause();

            remove_Shield_Squ = DOTween.Sequence();

            Tween tween1 = shieldSEUI.DOShakePosition(
                    0.2f,
                    new Vector3(0.2f, 0f, 0f),
                    10,
                    0f
                );
            Tween tween2 = shieldSEUI.DOScale(Vector3.zero, 0.1f);

            remove_Shield_Squ
                .Append(tween1)
                .Append(tween2)
                .SetAutoKill(false)
                .Pause();
        }

        if (add_Shield_Tween.IsPlaying() 
            || remove_Shield_Squ.IsPlaying()) return;

        if (add)   //방어 생성
        {
            SoundSystem.Instance.PlaySound(2001);
            add_Shield_Tween.Restart();
        }
        else       //방어 삭제
        {
            //방어 제거 사운드
            remove_Shield_Squ.Restart();
        }
    }

    private void OnDisable()
    {
        add_Shield_Tween?.Kill();
        remove_Shield_Squ?.Kill();
    }
}
