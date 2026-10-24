using UnityEngine;
using UnityEngine.EventSystems;

//버튼을 드래그해서 대상 위에 놓으면 플레이어 스킬 사용
public class UseSkillUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private bool isDragging;

    public void OnBeginDrag(PointerEventData eventData)
    {
        //다른 선택 모드 중이거나 사용 조건이 안 되면 드래그 무시
        Interactions interactions = Interactions.Instance;
        isDragging = !interactions.IsHeroMoveMode
            && !interactions.IsCardTargetMode
            && !interactions.IsPileViewOpen
            && PlayerSkillSystem.Instance.CanUse();
    }

    public void OnDrag(PointerEventData eventData) { }   //IDragHandler 필수 구현 (이동 연출 없음)

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        Token target = GetTokenUnderMouse();
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
}
