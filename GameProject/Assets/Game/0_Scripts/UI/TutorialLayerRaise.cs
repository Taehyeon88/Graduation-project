using UnityEngine;

//튜토리얼 중에만 이 Canvas(설정 아이콘)를 튜토리얼 층 위로 올림. 평소에는 부모 Canvas 순서를 따라 시작 화면 등 다른 화면 뒤에 머문다
[RequireComponent(typeof(Canvas))]
public class TutorialLayerRaise : MonoBehaviour
{
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    private void LateUpdate()
    {
        bool raise = TutorialSystem.Instance != null && TutorialSystem.Instance.IsActive;
        if (canvas.overrideSorting != raise)
            canvas.overrideSorting = raise;
    }
}
