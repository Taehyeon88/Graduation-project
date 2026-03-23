using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class CombatantViewStatusUI : MonoBehaviour
{
    [Header("공용 상태 정보창")]
    [SerializeField] private StatusEffectUI statusEffectPrefab;

    [Header("영웅 상태 정보창")]
    [SerializeField] private PerkUI perkUIPrefab;
    [SerializeField] private RectTransform hero_equipUI;
    [SerializeField] private GridLayoutGroup perkUI;
    [SerializeField] private RectTransform hero_statusEffectUI;

    private Dictionary<int, Dictionary<StatusEffectType, StatusEffectUI>> statusEffectUIDic = new();
    private readonly List<PerkUI> perkUIs = new();

    private bool perkUIActive = true;
    private int perkCount => perkUI.transform.childCount; //PerkSystem.Instance.perks.Count;

    void Update()
    {
        //영웅 상태 UI 자동 정렬
        Token heroView = HeroSystem.Instance.HeroView;
        if (heroView != null)
        {
            //유물UI에 따른 상태효과UI 정렬
            float targetY = 0;
            if (perkUIActive)
            {
                //윗 패딩, 아래 패딩, 셀 크기, 스페이싱 크기, 제한된 컬럼 개수
                float paddingSum = perkUI.padding.top + perkUI.padding.bottom;
                float cellSizeY = perkUI.cellSize.y;
                float spacingY = perkUI.spacing.y;
                int culumnCount = perkUI.constraintCount;
                int rowCount = perkCount / culumnCount + (perkCount % culumnCount == 0 ? 0 : 1);

                //hero_equipUI위치 + //크기 + 윗 패딩 + 아랫 패딩 + 셀 크기 * 줄 개수 + 스페이싱 크기 * (줄 개수 -1)

                targetY = hero_equipUI.anchoredPosition.y
                    - hero_equipUI.sizeDelta.y
                    - paddingSum
                    - cellSizeY * rowCount
                    - spacingY * (rowCount - 1);

                //Debug.Log($"패딩합: {paddingSum}, 셀크기: {cellSizeY}, 스페이싱: {spacingY}, 행 제한개수: {culumnCount}, 렬 개수: {rowCount}");
                //Debug.Log($"장비 위치: {hero_equipUI.anchoredPosition.y}, 목표위치: {targetY}");
            }
            else
            {
                targetY = hero_equipUI.anchoredPosition.y - hero_equipUI.sizeDelta.y;
            }

            hero_statusEffectUI.anchoredPosition = new(hero_statusEffectUI.anchoredPosition.x, targetY);

            ////상태효과UI 예외처리
            //int heroId = heroView.gameObject.GetInstanceID();
            //if (statusEffectUIDic.ContainsKey(heroId))
            //{
            //    var statusEffectUIs = statusEffectUIDic[heroId];
            //    if (statusEffectUIs != null)
            //    {
            //        if (statusEffectUIs.Count == 0)
            //        {
            //            hero_statusEffectUI.GetComponent<ContentSizeFitter>().enabled = false;
            //            hero_statusEffectUI.sizeDelta = new(hero_statusEffectUI.sizeDelta.x, hero_equipUI.sizeDelta.y);
            //        }
            //        else
            //        {
            //            hero_statusEffectUI.GetComponent<ContentSizeFitter>().enabled = true;
            //        }
            //    }
            //}
        }
    }

    /// <summary>
    /// 유물UI 활성화 설정 함수(OnPerkUIButton 체인)
    /// </summary>
    public void UpdatePerkUIActive()
    {
        perkUIActive = !perkUIActive;
        perkUI.gameObject.SetActive(perkUIActive);
    }

    /// <summary>
    /// 유물UI 추가 함수
    /// </summary>
    /// <param name="perk"></param>
    public void AddPerkUI(Perk perk)
    {
        PerkUI perkUI = Instantiate(perkUIPrefab, transform);
        perkUI.SetUp(perk);
        perkUIs.Add(perkUI);
    }
    /// <summary>
    /// 유물UI 제거 함수
    /// </summary>
    /// <param name="perk"></param>
    public void RemovePerkUI(Perk perk)
    {
        PerkUI perkUI = perkUIs.Find(p => p.perk == perk);
        if (perkUI != null)
        {
            perkUIs.Remove(perkUI);
            Destroy(perkUI.gameObject);
        }
    }

    /// <summary>
    /// 영웅, 몬스터 공용 상태이상UI 업데이트 함수
    /// </summary>
    /// <param name="combatantView"></param>
    /// <param name="statusEffectType"></param>
    /// <param name="stackCount"></param>
    /// <param name="sprite"></param>
    public void UpdateStatusEffect(CombatantView combatantView, StatusEffectType statusEffectType, int stackCount, Sprite sprite = null)
    {
        int tokenId = combatantView.gameObject.GetInstanceID();
        if (!statusEffectUIDic.ContainsKey(tokenId))
        {
            Debug.Log("영웅ID추가 " +  tokenId);
            statusEffectUIDic.Add(tokenId, new());
        }
        var statusEffectUIs = statusEffectUIDic[tokenId];


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
                if (combatantView is HeroView)
                {
                    StatusEffectUI statusEffectUI = Instantiate(statusEffectPrefab, hero_statusEffectUI.transform);
                    statusEffectUIs.Add(statusEffectType, statusEffectUI);
                }
                else if (combatantView is EnemyView)
                {

                }
            }
            statusEffectUIs[statusEffectType].Set(sprite, stackCount);
        }
    }
}
