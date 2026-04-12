using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatantViewStatusUI : MonoBehaviour
{
    [Header("공용 상태 정보창")]
    [SerializeField] private StatusEffectUI statusEffectPrefab;

    [Header("영웅 상태 정보창")]
    [SerializeField] private PerkUI perkUIPrefab;
    [SerializeField] private RectTransform hero_equipUI;
    [SerializeField] private GridLayoutGroup perkUI;
    [SerializeField] private RectTransform hero_statusEffectUI;

    [Header("몬스터 상태 정보창")]
    [SerializeField] private EnemyUI enemyUIPrefab;
    [SerializeField] private RectTransform enemyInfosUI;
    [SerializeField] private RectTransform enemysUI;
    [SerializeField] private Image enemyImage;
    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] private Slider enemyHPSlider;
    [SerializeField] private TMP_Text enemyHPText;
    [SerializeField] private RectTransform enemy_statusEffectUI;
    [SerializeField] private RectTransform enemyStatusEffectUIPool;
    [SerializeField] private Image nextActionImage;
    [SerializeField] private TMP_Text nextActionText;

    //공용
    private Dictionary<int, Dictionary<StatusEffectType, StatusEffectUI>> statusEffectUIDic = new();
    
    //영웅
    private readonly List<PerkUI> perkUIs = new();
    private bool perkUIActive = true;
    private int perkCount => perkUI.transform.childCount;

    //몬스터
    private IReadOnlyList<EnemyView> enemies => EnemySystem.Instance.Enemise;
    private int enemyCount => EnemySystem.Instance.Enemise.Count;
    private List<EnemyUI> enemyUIs = new();
    private EnemyView currentSelectedEnemy;
    private bool isEnemyInfosUIActive = true;

    void Update()
    {
        //적 인터렉션 -> 상태 UI 대상 변경
        if (InteractionSystem.GridSelected && InteractionSystem._InteractionStep == InteractionStep.None)
        {
            Vector3 pos = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1f);
            Vector2Int isoPosition = new((int)pos.x, (int)pos.y);
            Token token = TokenSystem.Instance.GetTokenByPosition(isoPosition);
            if (token is EnemyView enemyView)
            {
                SetEnemyUIInfos(enemyView);
            }
        }

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
        }

        //몬스터 상태 UI 정렬
        if (enemyCount > 0)
        {
            //파괴된 적의 UI 제거 (자동 제거)
            for (int i = 0; i < enemyUIs.Count; i++)
            {
                if (enemyUIs[i] == null) continue;
                if (enemyUIs[i].enemyView == null)
                {
                    Destroy(enemyUIs[i].gameObject);
                    enemyUIs[i] = null;
                }
            }
            enemyUIs.RemoveAll(e => e == null);

            //현재 몬스터들에 맞게 UI생성 (자동 생성)
            if (enemyCount != enemyUIs.Count)
            {
                List<EnemyView> temp = new(enemies);
                foreach (var enemyUI in enemyUIs)
                {
                    if (temp.Contains(enemyUI.enemyView))
                        temp.Remove(enemyUI.enemyView);
                }
                foreach (var enemy in temp)
                {
                    EnemyUI enemyUI = Instantiate(enemyUIPrefab, enemysUI);
                    enemyUI.Set(enemy, this);
                    enemyUIs.Add(enemyUI);
                }
            }
        }

        //현재 보고 있는 적 정보 실시간 업데이트 
        //예: 체력, 다음 할 공격, 상태효과
        if (currentSelectedEnemy != null)
        {
            //체력 업데이트
            enemyHPSlider.maxValue = currentSelectedEnemy.MaxHealth; 
            enemyHPSlider.value = currentSelectedEnemy.CurrentHealth;
            enemyHPText.SetText("{0}/{1}", currentSelectedEnemy.CurrentHealth, currentSelectedEnemy.MaxHealth);

            //다음 할 공격 업데이트
            if (currentSelectedEnemy.NextAction != null)
            {
                nextActionImage.sprite = currentSelectedEnemy.NextAction.Icon;
                nextActionText.text = currentSelectedEnemy.NextAction.Description;
            }
        }
        else
        {
            ////현재 설정된 대상 없을 때, 자동 설정
            ////예: 초기 혹은 선택 대상이 사망시 사용
            //var enemies = this.enemies;
            //if (enemies.Count > 0 && enemies[0] != null)
            //    SetEnemyUIInfos(enemies[0]);
        }
    }

    //Hero

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

    //Enemy

    /// <summary>
    /// 현재 보고 있는 몬스터(대상) 결정(설정) 함수
    /// </summary>
    /// <param name="enemyView"></param>
    public void SetEnemyUIInfos(EnemyView enemyView)
    {
        enemyImage.sprite = enemyView.EnemySprite;
        enemyNameText.text = enemyView.EnemyName;
        enemyHPSlider.maxValue = enemyView.MaxHealth;
        enemyHPSlider.value = enemyView.CurrentHealth;
        enemyHPText.SetText("{0}/{1}", enemyView.CurrentHealth, enemyView.MaxHealth);

        //상태 효과 업데이트

        //기존 적 상태 효과UI Pool로 되돌리기
        if (currentSelectedEnemy != null)
        {
            int preTokenId = currentSelectedEnemy.gameObject.GetInstanceID();
            if (statusEffectUIDic.ContainsKey(preTokenId))
            {
                var statusEffectUIs = statusEffectUIDic[preTokenId];
                foreach (var UI in statusEffectUIs.Values)
                {
                    UI.transform.SetParent(enemyStatusEffectUIPool);
                }
            }
        }
        //새로운 적 상태 효과UI 적용
        int tokenId = enemyView.gameObject.GetInstanceID();
        if (statusEffectUIDic.ContainsKey(tokenId))
        {
            var statusEffectUIs = statusEffectUIDic[tokenId];
            foreach (var UI in statusEffectUIs.Values)
            {
                UI.transform.SetParent(enemy_statusEffectUI);
            }
        }

        //선택된 적 비주얼 갱신
        UISystem.Instance.SetEnemyVisualSelected(true, enemyView);

        currentSelectedEnemy = enemyView;
    }

    //버튼 이벤트 함수
    public void UpdateEnemyInfosUIActive()
    {
        isEnemyInfosUIActive = !isEnemyInfosUIActive;
        enemyInfosUI.gameObject.SetActive(isEnemyInfosUIActive);
    }

    //Combatant

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
                else if (combatantView is EnemyView enemyView)
                {
                    if (currentSelectedEnemy == enemyView)
                    {
                        StatusEffectUI statusEffectUI = Instantiate(statusEffectPrefab, enemy_statusEffectUI.transform);
                        statusEffectUIs.Add(statusEffectType, statusEffectUI);
                    }
                    else
                    {
                        StatusEffectUI statusEffectUI = Instantiate(statusEffectPrefab, enemyStatusEffectUIPool.transform);
                        statusEffectUIs.Add(statusEffectType, statusEffectUI);
                    }
                }
            }
            statusEffectUIs[statusEffectType].Set(sprite, stackCount);
        }
    }
}
