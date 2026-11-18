using System.Collections.Generic;
using UnityEngine;

//튜토리얼이 지목·공개·잠금하는 UI/월드 오브젝트에 붙이는 ID 표식
public class TutorialTarget : MonoBehaviour
{
    private static readonly Dictionary<string, List<TutorialTarget>> targets = new();   //ID마다 목록 (씬 전환 때 중복 GAMEMASTER가 같은 ID를 잠깐 등록했다 사라져도 원본이 남도록)

    [SerializeField] private string id;
    [SerializeField] private bool hideUntilRevealed = true;   //튜토리얼 중 공개되기 전까지 숨김

    private CanvasGroup canvasGroup;           //UGUI 대상을 숨길 때 사용
    private float savedAlpha;
    private bool savedInteractable;
    private bool savedBlocksRaycasts;
    private readonly List<Renderer> hiddenRenderers = new();   //월드 대상을 숨길 때 끈 렌더러
    private bool isHidden;

    public string Id => id;

    private void OnEnable() => Register();

    private void OnDisable() => Unregister();

    //런타임에 생성한 UI(영웅 목록 버튼 등)에 ID를 부여할 때 사용
    public void Configure(string targetId, bool hide)
    {
        Unregister();
        id = targetId;
        hideUntilRevealed = hide;
        if (isActiveAndEnabled) Register();
    }

    private void Register()
    {
        if (string.IsNullOrEmpty(id)) return;

        if (!targets.TryGetValue(id, out var list))
        {
            list = new List<TutorialTarget>();
            targets.Add(id, list);
        }
        if (!list.Contains(this)) list.Add(this);
        ApplyVisibility();
    }

    private void Unregister()
    {
        if (!string.IsNullOrEmpty(id) && targets.TryGetValue(id, out var list))
            list.Remove(this);
    }

    //가장 나중에 등록된 대상을 반환
    public static bool TryGet(string id, out TutorialTarget target)
    {
        target = null;
        if (!targets.TryGetValue(id, out var list)) return false;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == null) continue;
            target = list[i];
            return true;
        }
        return false;
    }

    //공개 목록이 바뀌었을 때 전체 갱신
    public static void RefreshAll()
    {
        foreach (var list in targets.Values)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null) list.RemoveAt(i);
                else list[i].ApplyVisibility();
            }
        }
    }

    private void ApplyVisibility()
    {
        bool visible = !hideUntilRevealed || TutorialSystem.IsRevealed(id);
        if (visible == !isHidden) return;

        if (visible) Show();
        else Hide();
    }

    //비활성화하면 소유 스크립트의 구독이 풀리므로 활성 상태는 유지한 채 보이지만 않게 함
    private void Hide()
    {
        isHidden = true;
        if (transform is RectTransform)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            savedAlpha = canvasGroup.alpha;
            savedInteractable = canvasGroup.interactable;
            savedBlocksRaycasts = canvasGroup.blocksRaycasts;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            return;
        }

        hiddenRenderers.Clear();
        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            if (!renderer.enabled) continue;
            renderer.enabled = false;
            hiddenRenderers.Add(renderer);
        }
    }

    private void Show()
    {
        isHidden = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = savedAlpha;
            canvasGroup.interactable = savedInteractable;
            canvasGroup.blocksRaycasts = savedBlocksRaycasts;
            canvasGroup = null;
            return;
        }

        foreach (var renderer in hiddenRenderers)
        {
            if (renderer != null) renderer.enabled = true;
        }
        hiddenRenderers.Clear();
    }
}
