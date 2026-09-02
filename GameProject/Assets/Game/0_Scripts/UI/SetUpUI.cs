using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class SetUpUI : MonoBehaviour
{
    [SerializeField] private RectTransform selectedTrans;     //선택 패널
    [SerializeField] private RectTransform setupUIPanel;      //패널 데이터
    [SerializeField] private List<RectTransform> cellTrans;   //각 셀 데이터
    private Vector2[] anchorPositions = new Vector2[3];
    private Button[] buttons = new Button[3];

    public HeroData SelectedData {  get; private set; }
    private int selectedIndex = -1;
    private bool isAnimating = false;
    public void SetUp(HeroData[] heroDatas)
    {
        gameObject.SetActive(true);        //UI 활성화

        for (int i = 0; i < cellTrans.Count; i++)
        {
            //버튼 할당
            buttons[i] = cellTrans[i].GetComponent<Button>();

            //버튼 바인딩
            int index = i;
            buttons[index].onClick.AddListener(() =>
            {
                if (isAnimating) return;  //연출 중, 버튼 클릭 불가

                SelectedData = heroDatas[index];
                this.selectedIndex = index;

                //선택됨UI 연출 실행
                selectedTrans.anchoredPosition = buttons[index].GetComponent<RectTransform>().anchoredPosition;
            });

            //데이터 이미지 할당
            Image image = buttons[i].transform.GetChild(0).GetComponent<Image>();
            if (image != null)
                image.sprite = heroDatas[i].simbolIcon;
        }

        //Slot 초기화
        for (int i = 0; i < anchorPositions.Length; i++)
            anchorPositions[i] = cellTrans[i].anchoredPosition;

        buttons[0].onClick.Invoke();     //초기 선택된 영웅 설정
    }

    public void RemoveSlot()
    {
        //buttons[selectedIndex].onClick.RemoveAllListeners();
        SelectedData = null;

        RectTransform target = buttons[selectedIndex].GetComponent<RectTransform>();
        Sequence sequ = DOTween.Sequence();

        isAnimating = true;

        //target 셀 삭제 연출
        sequ.Append(
            target.DOScale(0f, 0.2f)
            );

        if (cellTrans.Count > 1)
        {
            //이동 시킬 셀 찾기, 이동 목표 위치 찾기
            int index = cellTrans.IndexOf(target);
            for (int i = index + 1; i < cellTrans.Count; i++)
            {
                if (cellTrans[i] == null) continue;

                //셀 이동 연출 실행
                sequ.Append(
                    cellTrans[i].DOAnchorPos(anchorPositions[i - 1],
                    cellTrans.Count - index == 3 ? 0.18f : 0.3f)
                    .SetEase(Ease.OutCubic)
                );
            }

            sequ.Join(
                selectedTrans.DOAnchorPos(anchorPositions[0], 0.31f));
        }
        else
        {
            //선택된 삭제
            sequ.Join(
                selectedTrans.DOScale(0f, 0.25f)
                );
        }

        sequ.OnComplete(() =>
        {
            isAnimating = false;
            cellTrans.Remove(target);
            target.gameObject.SetActive(false);
            Destroy(target.gameObject);

            if (cellTrans.Count <= 0)
            {
                EndSetUp();     //셋업 종료
            }
            else
            {
                foreach (var button in buttons)
                {
                    if (button != null && button.gameObject.activeSelf)
                    {
                        button.onClick.Invoke();      //선택됨 대상 자동 설정
                        break;
                    }
                }
            }
        });
    }

    public void EndSetUp()
    {
        //이동 위치 연산
        Vector2 pos = setupUIPanel.anchoredPosition;
        Vector2 targetPos = new Vector2(pos.x - setupUIPanel.rect.width, pos.y);

        //UI패널이 들어가는 연출
        setupUIPanel.DOAnchorPos(targetPos, 0.5f)
                   .OnComplete(() => setupUIPanel.gameObject.SetActive(false));   //인터렉션 비활성화
    }
}
