using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//같은 속도 유닛의 주사위 연출: 유닛 머리 위에서 숫자가 굴러가다 확정되고, 가장 큰 값(먼저 행동)을 강조
public class DiceRollUI : Singleton<DiceRollUI>
{
    private const int FaceMax = 6;

    [SerializeField] private RectTransform badgeTemplate;   //비활성 템플릿 (Image + CanvasGroup, 자식에 TMP_Text)

    [Header("Direct Element")]
    [SerializeField] private float rollDuration = 0.7f;
    [SerializeField] private float rollInterval = 0.06f;
    [SerializeField] private float holdDuration = 0.6f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float heightOffset = 40f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color winnerColor = new(1f, 0.85f, 0.3f);

    public IEnumerator Play(IReadOnlyList<(CombatantView unit, int value)> results)
    {
        int highest = results.Max(r => r.value);
        List<(RectTransform rect, TMP_Text text, Image image, CanvasGroup group, int value)> badges = new();

        foreach (var (unit, value) in results)
        {
            RectTransform rect = Instantiate(badgeTemplate, badgeTemplate.parent);
            rect.gameObject.SetActive(true);
            rect.position = GetScreenPosition(unit);
            rect.localScale = Vector3.zero;
            rect.DOScale(1f, 0.15f);

            Image image = rect.GetComponent<Image>();
            image.color = normalColor;
            badges.Add((rect, rect.GetComponentInChildren<TMP_Text>(), image, rect.GetComponent<CanvasGroup>(), value));
        }

        //숫자가 굴러가는 구간
        for (float elapsed = 0f; elapsed < rollDuration; elapsed += rollInterval)
        {
            foreach (var badge in badges)
                badge.text.SetText(Random.Range(1, FaceMax + 1).ToString());
            yield return new WaitForSeconds(rollInterval);
        }

        //값 확정, 가장 큰 값(먼저 행동)은 강조
        foreach (var badge in badges)
        {
            badge.text.SetText(badge.value.ToString());
            if (badge.value != highest) continue;

            badge.image.color = winnerColor;
            badge.rect.DOPunchScale(Vector3.one * 0.3f, 0.3f);
        }
        yield return new WaitForSeconds(holdDuration);

        foreach (var badge in badges)
            badge.group.DOFade(0f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        foreach (var badge in badges)
            Destroy(badge.rect.gameObject);
    }

    //유닛 모델 위쪽 중앙의 화면 좌표
    private Vector3 GetScreenPosition(CombatantView unit)
    {
        Bounds bounds = unit.Model.bounds;
        Vector3 top = new(bounds.center.x, bounds.max.y, bounds.center.z);
        return Camera.main.WorldToScreenPoint(top) + Vector3.up * heightOffset;
    }
}
