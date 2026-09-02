using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactions : Singleton<Interactions>
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private RectTransform select_Grid_UI_Range;

    //제약 변수s
    public bool IsSetUpHero = false;
    public bool IsSkillTargetMode = false;
    public bool IsSkillHovering = false;
    public bool IsHeroMoveMode = false;

    //InputSystem 변수s
    public bool GridSelected { get; private set; } = false;
    public bool CancelUse { get; private set; } = false;

    private static event Action SelectGridEvent;
    private static event Action<int> PlaySkillEvent;
    private InputAction m_SelectGrid;
    private InputAction m_CancelUse;

    private float select_grid_UI_Min_X;
    private float select_grid_UI_Max_X;
    private float select_grid_UI_Min_Y;
    private float select_grid_UI_Max_Y;

    private void Start()
    {
        m_SelectGrid = playerInput.actions["SelectGrid"];
        m_CancelUse = playerInput.actions["CancelUse"];

        float width = select_Grid_UI_Range.rect.width;
        float height = select_Grid_UI_Range.rect.height;
        Vector2 pos = select_Grid_UI_Range.position;

        select_grid_UI_Min_X = pos.x - width / 2;
        select_grid_UI_Max_X = pos.x + width / 2;
        select_grid_UI_Min_Y = pos.y - height / 2;
        select_grid_UI_Max_Y = pos.y + height / 2;
    }
    private void Update()
    {
        GridSelected = m_SelectGrid.WasPressedThisFrame() 
                && IsInRange(Input.mousePosition);
        CancelUse = m_CancelUse.WasPressedThisFrame();
    }

    void OnSelectGrid()
    {
        if(IsInRange(Input.mousePosition))
            SelectGridEvent?.Invoke();
    }

    void OnPlaySkill1() => PlaySkillEvent?.Invoke(1);
    void OnPlaySkill2() => PlaySkillEvent?.Invoke(2);
    void OnPlaySkill3() => PlaySkillEvent?.Invoke(3);
    void OnPlaySkill4() => PlaySkillEvent?.Invoke(4);
    void OnPlaySkill5() => PlaySkillEvent?.Invoke(5);

    public static void SetSelectGridEvent(Action action, bool isAdd)
    {
        if(isAdd) SelectGridEvent += action;
        else SelectGridEvent -= action;
    }
    public static void SetPlaySkillEvent(Action<int> action, bool isAdd)
    {
        if (isAdd) PlaySkillEvent += action;
        else PlaySkillEvent -= action;
    }


    //제약 변수 사용 가능 체크s
    public bool CanPlaySkillTargetMode()
    {
        if(IsSkillTargetMode)
            return false;

        return true;
    }

    public bool CanSkillHovering()
    {
        if (IsSkillTargetMode || IsSkillHovering)
            return false;

        return true;
    }
    public bool CanCancelSkilHovering()
    {
        if (IsSkillTargetMode)
            return false;

        return true;
    }

    //Privates
    private bool IsInRange(Vector2 pos)
    {
        return pos.x >= select_grid_UI_Min_X &&
               pos.x <= select_grid_UI_Max_X &&
               pos.y >= select_grid_UI_Min_Y &&
               pos.y <= select_grid_UI_Max_Y;
    }
}
