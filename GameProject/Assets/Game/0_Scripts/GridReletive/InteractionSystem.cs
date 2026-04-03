using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum InteractionCase
{
    None, 
    SetUp,      //전투 시작전, 셋업 단계
    MainGame,   //메인 전투 단계
}
public class InteractionSystem : Singleton<InteractionSystem>
{
    public static bool GridSelected { get; private set; } = false;
    public static bool CancelReadyUseCard { get; private set; } = false;

    [SerializeField] private PlayerInput playerInput;

    private InputAction m_SelectGrid;
    private InputAction m_CancelReadyUseCard;
    private InputAction m_ChangeCheatMode;
    private InteractionCase currentInteraction;

    private event Action<bool> updatedAction;

    private event Action<bool> cheatUpdatedAction;
    private void Start()
    {
        Initialze();
    }
    private void Initialze()
    {
        m_SelectGrid = playerInput.actions["SelectGrid"];
        m_CancelReadyUseCard = playerInput.actions["CancelReadyUseCard"];
        m_ChangeCheatMode = playerInput.actions["ChangeCheatMode"];
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

        cheatUpdatedAction?.Invoke(m_ChangeCheatMode.WasPerformedThisFrame());

        GridSelected = m_SelectGrid.WasPressedThisFrame();
        CancelReadyUseCard = m_CancelReadyUseCard.WasPressedThisFrame();
    }

    void OnStartCheat() => CheatSystem.Instance?.StartCheat();

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

    public void SetCheatInteraction(Action<bool> action)
    {
        cheatUpdatedAction = action;
    }
    public void EndInteraction()
    {
        updatedAction = null;
        currentInteraction = InteractionCase.None;
    }
}
