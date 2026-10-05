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
    public bool IsHeroMoveMode = false;
    public bool IsCardTargetMode = false;
    public bool IsPileViewOpen = false;

    //InputSystem 변수s
    public bool GridSelected { get; private set; } = false;
    public bool CancelUse { get; private set; } = false;

    private static event Action SelectGridEvent;
    private InputAction selectGrid;
    private InputAction cancelUse;

    private float selectGridUIMinX;
    private float selectGridUIMaxX;
    private float selectGridUIMinY;
    private float selectGridUIMaxY;

    private void Start()
    {
        selectGrid = playerInput.actions["SelectGrid"];
        cancelUse = playerInput.actions["CancelUse"];

        float width = select_Grid_UI_Range.rect.width;
        float height = select_Grid_UI_Range.rect.height;
        Vector2 pos = select_Grid_UI_Range.position;

        selectGridUIMinX = pos.x - width / 2;
        selectGridUIMaxX = pos.x + width / 2;
        selectGridUIMinY = pos.y - height / 2;
        selectGridUIMaxY = pos.y + height / 2;
    }
    private void Update()
    {
        GridSelected = selectGrid.WasPressedThisFrame()
                && !IsPileViewOpen
                && IsInRange(Input.mousePosition);
        CancelUse = cancelUse.WasPressedThisFrame();
    }

    void OnSelectGrid()
    {
        if(!IsPileViewOpen && IsInRange(Input.mousePosition))
            SelectGridEvent?.Invoke();
    }

    public static void SetSelectGridEvent(Action action, bool isAdd)
    {
        if(isAdd) SelectGridEvent += action;
        else SelectGridEvent -= action;
    }

    //Privates
    private bool IsInRange(Vector2 pos)
    {
        return pos.x >= selectGridUIMinX &&
               pos.x <= selectGridUIMaxX &&
               pos.y >= selectGridUIMinY &&
               pos.y <= selectGridUIMaxY;
    }
}
