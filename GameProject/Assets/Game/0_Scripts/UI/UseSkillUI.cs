using IsoTools;
using UnityEngine;
using UnityEngine.EventSystems;

//버튼을 드래그해서 대상 위에 놓으면 플레이어 스킬 사용
public class UseSkillUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private const string TargetHighlightName = "Skill_Target";

    [Header("DragVisual")]
    [SerializeField] private RectTransform dragImage;       //드래그 중 마우스를 따라다니는 연출 이미지
    [SerializeField] private float snapRadius;              //유효 대상 스냅 판정 반경 (화면 px)
    [SerializeField] private IsoObject coreHighlight;       //그리드 밖 적 코어용 하이라이트
    [SerializeField] private Vector3 coreHighlightIsoPosition;

    private bool isDragging;
    private Token snapTarget;

    private void OnDisable()
    {
        ResetDrag();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //다른 선택 모드 중이거나 사용 조건이 안 되면 드래그 무시
        Interactions interactions = Interactions.Instance;
        isDragging = !interactions.IsHeroMoveMode
            && !interactions.IsCardTargetMode
            && !interactions.IsPileViewOpen
            && PlayerSkillSystem.Instance.CanUse();

        if (!isDragging) return;

        dragImage.gameObject.SetActive(true);
        dragImage.position = Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        SetSnapTarget(FindSnapTarget());
        dragImage.position = snapTarget != null ? GetScreenPosition(snapTarget) : Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        Token target = snapTarget != null ? snapTarget : GetTokenUnderMouse();
        ResetDrag();

        if (target != null)
            PlayerSkillSystem.Instance.TryUse(target);
    }

    //Privates
    private Token GetTokenUnderMouse()
    {
        //그리드 밖에 있는 적 코어(우두머리)는 스프라이트 영역으로 판정
        EnemyFlagView enemyFlag = EnemySystem.Instance.EnemyFlag;
        if (enemyFlag != null)
        {
            Bounds bounds = enemyFlag.Model.bounds;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = bounds.center.z;
            if (bounds.Contains(mouseWorld)) return enemyFlag;
        }

        Vector3 isoPos = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
        Vector2Int pos = Utility.IsoVectorToVector2Int(isoPos);
        if (!TokenSystem.Instance.API.IsBound(pos)) return null;

        return TokenSystem.Instance.API.GetTokenByPosition(pos);
    }

    //마우스에서 snapRadius 안에 있는 유효 대상 중 가장 가까운 것
    private Token FindSnapTarget()
    {
        Token nearest = null;
        float nearestDistance = snapRadius;

        foreach (EnemyView enemy in TokenSystem.Instance.EnemyViews)
            CheckSnapCandidate(enemy, ref nearest, ref nearestDistance);
        CheckSnapCandidate(EnemySystem.Instance.EnemyFlag, ref nearest, ref nearestDistance);

        return nearest;
    }

    private void CheckSnapCandidate(Token candidate, ref Token nearest, ref float nearestDistance)
    {
        if (candidate == null || !candidate.gameObject.activeInHierarchy) return;
        if (!PlayerSkillSystem.Instance.SkillData.Skill.IsValidTarget(candidate)) return;

        float distance = Vector2.Distance(GetScreenPosition(candidate), Input.mousePosition);
        if (distance > nearestDistance) return;

        nearest = candidate;
        nearestDistance = distance;
    }

    private Vector3 GetScreenPosition(Token token)
    {
        return Camera.main.WorldToScreenPoint(token.Model.bounds.center);
    }

    //스냅 대상이 바뀔 때만 하이라이트 갱신
    private void SetSnapTarget(Token target)
    {
        if (snapTarget == target) return;

        ClearHighlight();
        snapTarget = target;
        if (snapTarget == null) return;

        if (snapTarget is EnemyFlagView)
        {
            coreHighlight.position = coreHighlightIsoPosition;
            coreHighlight.gameObject.SetActive(true);
        }
        else
        {
            VisualGridCreator.Instance.ChangeVisualGridPosition(
                gameObject.GetInstanceID(),
                TokenSystem.Instance.API.GetTokenPosition(snapTarget),
                TargetHighlightName);
        }
    }

    private void ClearHighlight()
    {
        if (coreHighlight != null)   //씬 종료 시 OnDisable 순서에 따라 이미 파괴됐을 수 있음
            coreHighlight.gameObject.SetActive(false);
        if (VisualGridCreator.Instance != null)
            VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), TargetHighlightName);
    }

    //드래그 종료 시 연출 전체 초기화
    private void ResetDrag()
    {
        isDragging = false;
        snapTarget = null;
        if (dragImage != null)
            dragImage.gameObject.SetActive(false);
        ClearHighlight();
    }
}
