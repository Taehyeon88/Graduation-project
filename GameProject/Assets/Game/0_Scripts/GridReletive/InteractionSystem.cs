using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InteractionCase
{
    None, 
    SetUp,      //전투 시작전, 셋업 단계
    MainGame,   //메인 전투 단계
}
public class InteractionSystem : Singleton<InteractionSystem>
{
    public static bool GridSelected { get; private set; } = false;

    [SerializeField] private PlayerInput playerInput;

    private InputAction m_SelectGrid;
    private InteractionCase currentInteraction;

    private event Action<bool> updatedAction;
    private void Start()
    {
        Initialze();
    }
    private void Initialze()
    {
        m_SelectGrid = playerInput.actions["SelectGrid"];
    }

    private void Update()
    {
        switch (currentInteraction)
        {
            case InteractionCase.SetUp:
                updatedAction?.Invoke(m_SelectGrid.WasPressedThisFrame());
                break;
            case InteractionCase.MainGame:
                updatedAction?.Invoke(m_SelectGrid.WasPerformedThisFrame());
                break;

        }

        GridSelected = m_SelectGrid.WasPressedThisFrame();
    }

    //void OnMousePosition(InputValue value)
    //{
    //    Vector2 mousePos = value.Get<Vector2>();
    //}

    void OnSelectCard()
    {
        //Debug.Log("카드 선택");
    }
    void OnSelectCardWithNumber1()
    {
        Debug.Log("카드1 선택");
    }
    void OnSelectCardWithNumber2()
    {
        Debug.Log("카드2 선택");
    }
    void OnSelectCardWithNumber3()
    {
        Debug.Log("카드3 선택");
    }
    void OnSelectCardWithNumber4()
    {
        Debug.Log("카드4 선택");
    }
    void OnSelectCardWithNumber5()
    {
        Debug.Log("카드5 선택");
    }


    public void SetInteraction(InteractionCase newCase, Action<bool> action)
    {
        updatedAction = action;
        currentInteraction = newCase;
    }
    public void EndInteraction()
    {
        updatedAction = null;
        currentInteraction = InteractionCase.None;
    }
}
