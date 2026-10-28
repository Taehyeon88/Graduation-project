using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//모든 UGUI 버튼 클릭 시 버튼음 재생 (GAMEMASTER 하위에서 씬을 넘나듦)
public class ButtonClickSound : Singleton<ButtonClickSound>
{
    private const int ClickSoundId = 3005;   //버튼 클릭음

    private readonly List<RaycastResult> raycastResults = new();
    private Button pressedButton;   //누른 시점에 포인터 아래 있던 버튼

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
    }

    private void Update()
    {
        if (Instance != this) return;

        if (Input.GetMouseButtonDown(0)) pressedButton = FindButtonUnderPointer();

        if (Input.GetMouseButtonUp(0))
        {
            //Button의 onClick과 같은 조건: 누른 버튼 위에서 뗐을 때
            if (pressedButton != null && pressedButton.interactable && pressedButton == FindButtonUnderPointer())
            {
                SoundSystem.Instance.PlaySound(ClickSoundId);
            }
            pressedButton = null;
        }
    }

    private Button FindButtonUnderPointer()
    {
        if (EventSystem.current == null) return null;

        PointerEventData pointer = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        EventSystem.current.RaycastAll(pointer, raycastResults);
        if (raycastResults.Count == 0) return null;

        return raycastResults[0].gameObject.GetComponentInParent<Button>();   //최상단만 확인 — 다른 UI가 가리면 클릭되지 않음
    }
}
