using System.Collections;
using UnityEngine;

public class TutorialSystem : Singleton<TutorialSystem>
{
    [SerializeField] private TutorialData[] tutorialDatas;
    [SerializeField] private TutorialUI tutorialUI;

    public bool IsTutorialing { get; private set; } = false;

    public void StartTutorial()
    {
        IsTutorialing = true;
        StartCoroutine(C_StartTutorial());
    }

    public IEnumerator C_StartTutorial()
    {
        tutorialUI.SetBackPanelActive(true);
        tutorialUI.SetBlockUIActive(true);
        tutorialUI.SetBlockUI2Active(false);
        tutorialUI.SetBlockUI3Active(false);
        foreach (var data in tutorialDatas)
            if(data.point != null)
                data.point.SetActive(false);

        for (int i = 0; i < tutorialDatas.Length; i++)
        {
            TutorialData data = tutorialDatas[i];
            if (data == null) continue;

            if (data.point != null)
                data.point.SetActive(true);                                            //뚤리는 포인트 활성화
            yield return tutorialUI.ApplyContents(data.Title, data.Description);      //텍스트 연출 대기

            if (i == 0)
            {
                yield return new WaitUntil(() => HeroSystem.Instance.HeroView != null); //영웅 배치됨 대기
            }
            else if (i == 6)
            {
                yield return new WaitUntil(() => InteractionSystem.CancelUse);        //클릭 인터렉션 대기
                tutorialUI.SetBackPanelActive(false);
                tutorialUI.SetBlockUIActive(false);
                tutorialUI.SetBlockUI3Active(true);
                tutorialUI.SetTextEmpty();

                yield return new WaitUntil(() =>
                {
                    int dis = TokenSystem.Instance.GetDistance(
                        EnemySystem.Instance.Enemise[0], 
                        HeroSystem.Instance.HeroPosition
                        );
                    return dis == 1;
                });

                tutorialUI.SetBackPanelActive(true);
                tutorialUI.SetBlockUIActive(true);
            }
            else if (i == 7)
            {
                yield return new WaitUntil(() => InteractionSystem.CancelUse);        //클릭 인터렉션 대기
                tutorialUI.SetBackPanelActive(false);
                tutorialUI.SetBlockUIActive(false);
                tutorialUI.SetTextEmpty();

                yield return new WaitUntil(() => CardSystem.Instance.handCA == 0);

                tutorialUI.SetBackPanelActive(true);
                tutorialUI.SetBlockUIActive(true);
                tutorialUI.SetBlockUI3Active(false);
            }
            else if (i == 9)
            {
                tutorialUI.SetBlockUIActive(false);
                tutorialUI.SetBlockUI2Active(true);

                yield return new WaitUntil(() => ActionSystem.Instance.IsPerforming);

                PauseSystem.Instance.SetPause(true); //일시정지 처리

                tutorialUI.SetBlockUIActive(true);
                tutorialUI.SetBlockUI2Active(false);
            }
            else if ( i == 10)
            {
                yield return new WaitUntil(() => InteractionSystem.CancelUse);
                PauseSystem.Instance.SetPause(false); //일시정지 비활성화 처리
            }
            else
            {
                yield return new WaitUntil(() => InteractionSystem.CancelUse);        //클릭 인터렉션 대기
            }

            if (data.point != null)
                data.point.SetActive(false);                                           //뚤리는 포인트 비활성화
        }

        tutorialUI.SetBackPanelActive(false);
        tutorialUI.SetBlockUIActive(false);
        tutorialUI.SetTextEmpty();

        Debug.Log("튜토리얼 종료");
    }
}

[System.Serializable]
public class TutorialData
{
    public string Title;
    public string Description;
    public GameObject point;
}