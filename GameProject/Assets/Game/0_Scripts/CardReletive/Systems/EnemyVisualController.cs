using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyVisualController : MonoBehaviour
{

    private IReadOnlyList<EnemyView> enemies => EnemySystem.Instance.Enemise;
    private List<int> hidedEnemyIds = new();
    private List<int> reversedEnemyIds = new();

    private EnemyView hoveredTarget;
    private EnemyView selectedTarget;

    private bool isHovering = false;
    private bool isSlecting = false;
    private bool isAllLooking = false;

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<StartBattleGA>(StartBattlePreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<KillEnemyGA>(KillEnemyPreReaction, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<StartBattleGA>(StartBattlePreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(KillEnemyPreReaction, ReactionTiming.PRE);
    }

    private void Update()
    {
        //마우스 호버 감지
        if (isAllLooking) return;

        Vector3 isoPosition = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
        if (isoPosition != null)
        {
            var enemy = TokenSystem.Instance.GetTokenByPosition(new((int)isoPosition.x, (int)isoPosition.y)) as EnemyView;
            bool canHovering = enemy != null ? (selectedTarget != null? selectedTarget != enemy : true) : false;

            if (canHovering)
            {
                //호버 취소 후 새로운 대상 호버
                if (!isHovering)
                {
                    isHovering = true;
                    Reverse(enemy); 
                    hoveredTarget = enemy;
                }
                else
                {
                    //이미 호버 중일 때, 호버 대상만 변경
                    if (hoveredTarget != enemy)
                    {
                        Hide(hoveredTarget);
                        Reverse(enemy);
                        hoveredTarget = enemy;
                    }
                }
            }
            else
            {
                //호버 취소
                if (!isHovering) return;

                isHovering = false;
                if (selectedTarget != null ? selectedTarget != enemy || selectedTarget != hoveredTarget : true)
                {
                    Hide(hoveredTarget);
                }
                hoveredTarget = null;
            }
        }
    }

    //전체 열기 감지 -> 모든 몬스터 관련 정보들 전부 활성화
    //전체 열기 취소 감지 -> 모든 몬스터 관련 정보들 비활성화(호버 체크) 
    public void SetAllLooking()
    {
        isAllLooking = !isAllLooking;

        if (isAllLooking)
        {
            foreach (var enemy in enemies)
                Reverse(enemy);
        }
        else
        {
            foreach (var enemy in enemies)
            {
                if ((isHovering && hoveredTarget == enemy)
                  || (isSlecting && selectedTarget == enemy))
                    continue;

                Hide(enemy);
            }
        }
    }

    //선택 감지 - 몬스터 핀상태 설정
    public void SetSelectedTarget(EnemyView target)
    {
        if (target == null || target == selectedTarget) return;

        isSlecting = true;
        if (selectedTarget != null)
        {
            if(!isAllLooking) 
                Hide(selectedTarget);
            VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "UI_SelectedEnemy");  //선택 그리드 비활성화
        }

        if (!isAllLooking)
            Reverse(target);
        selectedTarget = target;
        Vector2Int pos = TokenSystem.Instance.GetTokenPosition(target);
        VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), pos, "UI_SelectedEnemy");
    }

    //선택 종료 감지 - 몬스터 핀 상태 초기화
    public void StopSelecting()
    {
        if (!isAllLooking)
            Hide(selectedTarget);
        isSlecting = false;
        selectedTarget = null;
    }



    //privates

    private void Hide(EnemyView enemy)
    {
        if(enemy == null) return;

        int id = enemy.gameObject.GetInstanceID();

        if (hidedEnemyIds.Contains(id) || !reversedEnemyIds.Contains(id)) return;

        //체력 / 상태효과 / 다음 할 공격 UI 비활성화
        enemy.SetEnemyInfoUIActive(false);
        //해당 몬스터의 VisualGrid 가리기
        VisualGridCreator.Instance.SetHideVisualGrid(id);


        hidedEnemyIds.Add(id);
        reversedEnemyIds.Remove(id);
    }

    private void Reverse(EnemyView enemy)
    {
        if (enemy == null) return;

        int id = enemy.gameObject.GetInstanceID();

        if (reversedEnemyIds.Contains(id) || !hidedEnemyIds.Contains(id)) return;

        //체력 / 상태효과 / 다음 할 공격 UI 활성화
        enemy.SetEnemyInfoUIActive(true);
        //해당 몬스터의 VisualGrid 가리기 취소
        VisualGridCreator.Instance.SetReverseVisualGrid(id);

        reversedEnemyIds.Add(id);
        hidedEnemyIds.Remove(id);
    }

    private void Add(EnemyView enemy, bool hide)
    {
        if (enemy == null) return;

        int id = enemy.gameObject.GetInstanceID();

        if (hide)
        {
            //체력 / 상태효과 / 다음 할 공격 UI 비활성화
            enemy.SetEnemyInfoUIActive(false);
            //해당 몬스터의 VisualGrid 가리기
            VisualGridCreator.Instance.SetHideVisualGrid(id);

            hidedEnemyIds.Add(id);
        }
        else
        {
            //체력 / 상태효과 / 다음 할 공격 UI 활성화
            enemy.SetEnemyInfoUIActive(true);
            //해당 몬스터의 VisualGrid 가리기 취소
            VisualGridCreator.Instance.SetReverseVisualGrid(id);

            reversedEnemyIds.Add(id);
        }
    }

    //Subscribers

    //모든 몬스터의 VisualGrid들 및 이동Visual 및 체력/상태효과/다음 할 공격 UI 비활성화
    //-> 캐싱.
    private void StartBattlePreReaction(StartBattleGA startBattleGA)
    {
        foreach (var enemy in enemies)
        {
            Add(enemy, true);
        }
    }

    private void KillEnemyPreReaction(KillEnemyGA killEnemyGA)
    {
        int id = killEnemyGA.EnemyView.gameObject.GetInstanceID();

        if (hidedEnemyIds.Contains(id))
            hidedEnemyIds.Remove(id);
        else if (reversedEnemyIds.Contains(id))
            reversedEnemyIds.Remove(id);
    }
}
