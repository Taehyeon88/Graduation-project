using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillSystem : Singleton<SkillSystem>
{
    [Header("Skill Element")]
    [SerializeField] private SkillsUI skillsUI;
    [Header("Skill Direct Element")]
    [field : SerializeField] public SkillHighlightUI HighlightUI { get; private set; }
    [SerializeField] private float upSkill_Distance = 10f;
    [SerializeField] private float upSkill_Duration = 0.3f;
    [SerializeField] private float downSkill_Duration = 0.2f;

    private Queue<PlaySkillGA> reserved_Skills = new();
    private SkillView current_Selected_Skill;
    private bool start_Switch_Skill = false;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<PlaySkillGA>(PlayCardPerformer);
        ActionSystem.AttachPerformer<RefillSkillLimitGA>(RefillSkillLimitPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PlaySkillGA>();
        ActionSystem.DetachPerformer<RefillSkillLimitGA>();
    }

    private void Update()
    {
        //예약된 스킬이 있고 액션이 종료 되면 다음 예약 실행
        if (reserved_Skills.Count <= 0 || ActionSystem.Instance.IsPerforming)
            return;

        var playCardGA = reserved_Skills.Dequeue();
        ActionSystem.Instance.Perform(playCardGA);
    }

    //Publics
    public void PlaySkillTargetMode(SkillView skillView)
    {
        if (!Interactions.Instance.IsSkillTargetMode)
        {
            current_Selected_Skill = skillView;
            StartCoroutine(SkillTargetMode(skillView));            
        }
        else //타겟 모드 중, 스킬 변경 및 취소 처리
        {
            if (skillView != current_Selected_Skill)
            {
                start_Switch_Skill = true;
                current_Selected_Skill = skillView;
            }
            else
            {
                ResetSelectMode();
                current_Selected_Skill = null;
            }
        }
    }

    public void UpdateSkillsUI(HeroView heroView)
    {
        skillsUI.UpdateSkils(heroView);
    }

    //Privates
    private IEnumerator SkillTargetMode(SkillView skillView)
    {
        Interactions.Instance.IsSkillTargetMode = true;

        //스킬 선택됨 효과 연출 시작
        RectTransform rectTrans = skillView.GetComponent<RectTransform>();
        //팝업 UI 생성
        TooltipSystem.Instance.ShowSkillTooltip(
            rectTrans, skillView.Skill.Description, skillView.Skill.Title
        );
        HighlightUI.ShowSeleted(rectTrans.anchoredPosition);                        //테두리 하이라이트
        rectTrans.DOAnchorPosY(rectTrans.anchoredPosition.y + upSkill_Distance, upSkill_Duration)  //스킬 위로 약간 올림
                 .OnUpdate(() => HighlightUI.UpdateSeletedPosition(rectTrans.anchoredPosition));

        Skill skill = skillView.Skill;
        SkillAbility ability = skill.SkillAbility;
        var targetTypes = skill.SkillAbility.TargetTypes;
        HeroView myhero = HeroSystem.Instance.CurrentHero;
        Vector2Int myPos = TokenSystem.Instance.API.GetTokenPosition(myhero);  //나중에 바꿔야 함

        List<Vector2Int> range = null;
        bool isEnemy = targetTypes.Contains(TargetType.Enemy);

        //지정 범위 VG 생성
        //선택 범위 VG 생성
        //공격 대상 VG 생성

        //아군, 적군은 지정 범위 변경X
        //본인은 지정 범위(강제) - 자기자신의 위치 + 별도의 고정된 VG 사용

        //본인은 선택 범위(강제) - 자기자신의 위치
        //나머지는 원래 그대로 따름 but, All일 경우, 한 명 대상만 해도 다 자동 지정 처리

        //해당 선택 범위 내에 TargetType에 맞는 대상이 존재 한 타일에만 표시
        //적군 일 경우, X | 아군 일 경우, O | 본인에게 경우에 따라서 달라짐


        //지정 범위 VG 업데이트
        if (targetTypes.Contains(TargetType.Self))
        {
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), myPos, "Hero_UseSelf");
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), myPos, "Positive");
        }

        if (targetTypes.Contains(TargetType.Enemy)
               || targetTypes.Contains(TargetType.Friendly))
        {
            bool penetration = ability.TargetMode is LineTM;
            range = ability.RangeMode.GetGridRanges(myPos, ability.Distance, penetration);

            foreach (var r in range)
            {
                VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), r, ability.RangeVG);
            }

            //IUseCustomRangeVG은 잠시 사용하지 않는 처리
        }

        //선택 - 그리드 미리보기
        string currenttpstring = "";
        while (true)
        {
            if (range != null && range.Count > 0)
            {
                Vector3 isoPos = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
                Vector2Int mousepos = Utility.IsoVectorToVector2Int(isoPos);

                List<Vector2Int> targets = null;
                var targetPoints = ability.TargetMode.GetTargets(range, mousepos, myPos, ability.Distance);
                if (targetPoints != null)
                {
                    string tpstring = string.Join("", targetPoints);
                    if (currenttpstring != tpstring)
                    {
                        SoundSystem.Instance.PlaySound(3001);

                        //선택 범위 VG 업데이트
                        VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), ability.TargetVG);

                        foreach (var target in targetPoints)
                            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), target, ability.TargetVG);

                        //공격 대상 VG 업데이트
                        targets = GetTargetPoses(targetPoints, isEnemy ? TargetType.Enemy : TargetType.Friendly);

                        foreach (var target in targets)
                            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), target, isEnemy ? "Negative" : "Positive");

                        currenttpstring = tpstring;
                    }

                    //그리드 선택 인터렉션 감지
                    if (Interactions.Instance.GridSelected)
                    {
                        targets = GetTargetPoses(targetPoints, isEnemy ? TargetType.Enemy : TargetType.Friendly);
                        if (targets != null && targets.Count > 0)
                        {
                            PlaySkillGA playCardGA = new(skill, targets, myhero);
                            reserved_Skills.Enqueue(playCardGA);
                            ResetSelectMode();
                            break;
                        }
                        else
                        {
                            SoundSystem.Instance.PlaySound(22);      //잘못된 타일 선택 사운드 재생
                        }
                    }
                }
            }
            else
            {
                if (Interactions.Instance.GridSelected)
                {
                    PlaySkillGA playCardGA = new(skill, null, myhero);
                    reserved_Skills.Enqueue(playCardGA);
                    ResetSelectMode();
                    break;
                }
            }

            //카드 사용 준비 취소 인터렉션 감지
            if (Interactions.Instance.CancelUse || current_Selected_Skill == null)
            {
                //Debug.Log("선택 모드 취소");
                ResetSelectMode();
                break;
            }

            if (start_Switch_Skill)
            {
                //Debug.Log("선택된 스킬 변경");
                var selected_Skill = current_Selected_Skill;
                ResetSelectMode();
                PlaySkillTargetMode(selected_Skill);
                break;
            }

            yield return null;
        }
    }

    //Privates

    //현재 위치 범위 안에 해당 타입에 마땅한 대상의 위치 정보 반환
    private List<Vector2Int> GetTargetPoses(List<Vector2Int> poses, TargetType type)
    {
        //타입에 맞는 대상 찾기
        //각 대상의 위치로 poses를 탐색

        List<Vector2Int> result = new(20);
        IReadOnlyList<Token> tokens;

        if (type == TargetType.Enemy)
        {
            tokens = EnemySystem.Instance.Enemise;
        }
        else if (type == TargetType.Friendly)
        {
            tokens = HeroSystem.Instance.HeroViews;
        }
        else return null;

        foreach (var pos in poses)
        {
            foreach (var token in tokens)
            {
                if (pos == TokenSystem.Instance.API.GetTokenPosition(token))
                {
                    result.Add(pos);
                }
            }
        }

        return result;
    }

    private void ResetSelectMode()
    {
        //선택 모드 연출 종료
        if (current_Selected_Skill != null)
        {
            RectTransform rectTrans = current_Selected_Skill.GetComponent<RectTransform>();

            TooltipSystem.Instance.HideSkillTooltip();        //팝업 UI 생성 취소
            HighlightUI.HideSeleted();                        //테두리 하이라이트
            rectTrans.anchoredPosition =                      //스킬 위치 원상 복귀
                new Vector2(rectTrans.anchoredPosition.x, rectTrans.anchoredPosition.y - upSkill_Distance);
        }

        start_Switch_Skill = false;
        Interactions.Instance.IsSkillTargetMode = false;
        current_Selected_Skill = null;
        VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());
    }

    //Performers
    private IEnumerator PlayCardPerformer(PlaySkillGA playSkillGA)
    {
        SpendManaGA spendManaGA = new();
        ActionSystem.Instance.AddReaction(spendManaGA);

        playSkillGA.Skill.ReduceLimit();

        foreach(var effect in playSkillGA.Skill.SkillAbility.Effects)
        {
            PerformEffectGA performEffectGA = new PerformEffectGA(effect, playSkillGA.TargetPoses, playSkillGA.MyView);
            ActionSystem.Instance.AddReaction(performEffectGA);
        }
        yield return null;
    }

    private IEnumerator RefillSkillLimitPerformer(RefillSkillLimitGA refillSkillLimitGA)
    {
        foreach (var hero in HeroSystem.Instance.HeroViews)
        {
            if (hero != null)
            {
                foreach (var skill in hero.Skills)
                {
                    skill?.ReFillLimit();
                }
            }
        }
        yield return null;
    }
}
