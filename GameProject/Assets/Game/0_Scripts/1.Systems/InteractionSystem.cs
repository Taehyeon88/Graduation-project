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

    private InputAction selectGrid;
    private InputAction cancelUse;
    private InputAction changeCheatMode;
    private InteractionCase currentInteraction;

    private event Action<bool> updatedAction;
    private event Action<bool> cheatUpdatedAction;
    private void Start()
    {
        Initialze();
    }
    private void Initialze()
    {
        selectGrid = playerInput.actions["SelectGrid"];
        cancelUse = playerInput.actions["CancelUse"];
        changeCheatMode = playerInput.actions["ChangeCheatMode"];
    }

    private void Update()
    {
        switch (currentInteraction)
        {
            case InteractionCase.SetUp:
                updatedAction?.Invoke(selectGrid.WasPressedThisFrame());
                break;
            case InteractionCase.MainGame:
                updatedAction?.Invoke(selectGrid.WasPerformedThisFrame());
                break;

        }

        cheatUpdatedAction?.Invoke(changeCheatMode.WasPerformedThisFrame());

        GridSelected = selectGrid.WasPressedThisFrame();
        CancelUse = cancelUse.WasPressedThisFrame();
    }


}
