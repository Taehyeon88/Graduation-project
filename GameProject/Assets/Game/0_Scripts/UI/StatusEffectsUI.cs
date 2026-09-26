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
    private Tween addShieldTween;
    private Sequence removeShieldSqu;
    private bool shieldStatusEffect = false;  //방어 존재 여부 체크

    public void UpdateStatusEffect(StatusEffectType statusEffectType, int stackCount, Sprite sprite = null)
    {
        //방어SE 예외처리
        if (statusEffectType == StatusEffectType.ARMOR)
        {
            if (stackCount <= 0)
            {
                if (shieldStatusEffect)
                {
                    shieldStatusEffect = false;
                    UpdateShieldSE(false);
                }
            }
            else
            {
                shieldSEText.SetText(stackCount.ToString());

                if (!shieldStatusEffect)
                {
                    UpdateShieldSE(true);
                    shieldStatusEffect = true;
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
            SoundSystem.Instance.PlaySound(2002);   //상태효과 획득 사운드

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
        if (addShieldTween == null && removeShieldSqu == null)
        {
            addShieldTween = shieldSEUI.DOScale(Vector3.one, 0.2f)
                                        .SetEase(Ease.InOutSine)
                                        .SetAutoKill(false)
                                        .Pause();

            removeShieldSqu = DOTween.Sequence();

            Tween tween1 = shieldSEUI.DOShakePosition(
                    0.2f,
                    new Vector3(0.2f, 0f, 0f),
                    10,
                    0f
                );
            Tween tween2 = shieldSEUI.DOScale(Vector3.zero, 0.1f);

            removeShieldSqu
                .Append(tween1)
                .Append(tween2)
                .SetAutoKill(false)
                .Pause();
        }

        if (addShieldTween.IsPlaying()
            || removeShieldSqu.IsPlaying()) return;

        if (add)   //방어 생성
        {
            SoundSystem.Instance.PlaySound(2001);
            addShieldTween.Restart();
        }
        else       //방어 삭제
        {
            //방어 제거 사운드
            removeShieldSqu.Restart();
        }
    }

    private void OnDisable()
    {
        addShieldTween?.Kill();
        removeShieldSqu?.Kill();
    }
}
