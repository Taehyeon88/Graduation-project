using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DiceSystem : Singleton<DiceSystem>
{
    [SerializeField] private Transform diceView;
    private Transform diceObject;

    private Dice dice;
    private bool startRolling;
    private Sequence rollingSquence;
    public void SetDice(Dice dice)
    {
        this.dice = dice;
        diceObject = DiceCreator.Instance.CreateDice(dice.data, diceView.position, diceView);

        SetTweens();
    }

    private void SetTweens()
    {
        Vector3 startPos = new Vector3(0, 0.8f, -2);
        float jumpDistance = 2.3f;

        rollingSquence = DOTween.Sequence();

        rollingSquence.Append(diceObject.DOMove(startPos, 1f))
           .Append(diceObject.DOJump(startPos + Vector3.up * jumpDistance, 1f, 1, 0.6f))
           .Join(diceObject.DORotate(new Vector3(360f, 360f, 360f), 0.5f, RotateMode.LocalAxisAdd)
           .SetLoops(-1, LoopType.Incremental)
           .SetEase(Ease.Linear));

        rollingSquence.Pause();
    }

    void OnEnable()
    {
        ActionSystem.AttachPerformer<RollDiceGA>(RollDicePerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<RollDiceGA>();
    }

    private IEnumerator RollDicePerformer(RollDiceGA rollDiceGA)
    {
        yield return null;
        startRolling = true;
        InteractionSystem.Instance.SetInteraction(InteractionCase.RollDice, RollDice);
        yield return new WaitUntil(() => !startRolling);
        InteractionSystem.Instance.EndInteraction();

        //주사위 랜덤 난수 생성
        int randomValue = UnityEngine.Random.Range(0, dice.FaceCount);
        int result = dice.NumberPerFace[randomValue];

        //주사위 애니메이션 연출
        Vector3 startPos = diceObject.transform.position;
        float rotateTime = 1.5f;

        rollingSquence.Restart();
        yield return new WaitForSeconds(rotateTime);

        float timeScale = 1f;
        while (timeScale > 0)
        {
            timeScale = Mathf.Clamp01(timeScale - (Time.deltaTime / 3f));
            rollingSquence.timeScale = timeScale;

            if (timeScale < 0.2)
            {
                Quaternion targetRot = dice.rotationPerFace[randomValue];
                rollingSquence.Pause();
                rollingSquence.timeScale = 1f;

                Sequence squ = DOTween.Sequence();

                squ.Append(diceObject.DORotateQuaternion(targetRot, 0.8f))
                   .Append(diceObject.DOPunchScale(Vector3.one / 2f, 0.8f))
                   .AppendInterval(1.5f)
                   .OnComplete(() => diceObject.localPosition = Vector3.zero);
                break;
            }

            yield return null;
        }

        //플레이어 이동 스태미너에 값 갱신
        AddSPDGA addSPDGA = new(result);
        ActionSystem.Instance.AddReaction(addSPDGA);
    }
    private void RollDice(bool isSelect)
    {
        if (isSelect)
        {
            startRolling = false;
        }
    }
}
