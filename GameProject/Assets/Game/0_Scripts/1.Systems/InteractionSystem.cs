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

public enum InteractionStep
{
    None,
    CardInteracting,
}

public class InteractionSystem : Singleton<InteractionSystem>
{
    public static bool GridSelected { get; private set; } = false;
    public static bool CancelUse { get; private set; } = false;
    public static InteractionStep _InteractionStep { get; set; } = InteractionStep.None;

    [SerializeField] private PlayerInput playerInput;

    private InputAction m_SelectGrid;
    private InputAction m_CancelUse;
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
        m_CancelUse = playerInput.actions["CancelUse"];
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
        CancelUse = m_CancelUse.WasPressedThisFrame();
    }


}
