using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

//스테이지 선택 흐름(해금 제약)을 무시하고 원하는 구조물을 바로 실행하는 디버그 창
//플레이 중: 현재 런 상태를 유지한 채 이동 / 에디터 모드: StageScene에서 Play를 시작한 뒤 새 런 상태로 이동
[InitializeOnLoad]
public class StageLauncherWindow : EditorWindow
{
    private static readonly int[] LaunchableIds = { 1, 2, 7, 100 };   //실행 가능한 구조물 ID (일반 전투, 주점, 정예 전투, 튜토리얼 전투)

    private const string BattleSceneName = "GameDemoScene";
    private const string StageScenePath = "Assets/Game/2_Scenes/StageScene.unity";
    private const int TutorialStructureId = 100;   //튜토리얼 전투 구조물 ID
    private const string PendingStructureKey = "StageLauncher.PendingStructure";   //Play 진입 후 실행할 구조물 에셋 경로
    private const string PendingGrowthTutorialKey = "StageLauncher.PendingGrowthTutorial";   //Play 진입 후 '튜토리얼 클리어 후 시작'으로 실행할지
    private const string RestoreStartSceneKey = "StageLauncher.RestoreStartScene";
    private const string PrevStartScenePathKey = "StageLauncher.PrevStartScenePath";

    private readonly List<StructureData> structures = new();

    static StageLauncherWindow()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("Tools/Debug/Stage Launcher")]
    private static void Open()
    {
        GetWindow<StageLauncherWindow>("Stage Launcher");
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void Refresh()
    {
        structures.Clear();
        foreach (string guid in AssetDatabase.FindAssets("t:StructureData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<StructureData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data != null && System.Array.IndexOf(LaunchableIds, data.ID) >= 0) structures.Add(data);
        }
        structures.Sort((a, b) => a.ID.CompareTo(b.ID));
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh")) Refresh();

        bool playing = Application.isPlaying;
        bool canUse = true;

        if (playing && GameSystem.Instance == null)
        {
            EditorGUILayout.HelpBox("GameSystem이 없습니다.", MessageType.Info);
            canUse = false;
        }
        else if (playing && ActionSystem.Instance != null && ActionSystem.Instance.IsPerforming)
        {
            EditorGUILayout.HelpBox("액션 수행 중입니다.", MessageType.Info);
            canUse = false;
        }
        else
        {
            string mode = playing ? "플레이 중: 현재 진행 상태(보유 덱·클리어 기록)를 유지한 채 이동합니다." : "에디터 모드: StageScene에서 Play를 시작해 새 진행 상태로 이동합니다.";
            EditorGUILayout.HelpBox(mode, MessageType.None);
        }

        foreach (StructureData data in structures)
        {
            bool missingStage = data.SceneName == BattleSceneName && data.StageData == null;
            if (missingStage)
                EditorGUILayout.HelpBox($"{data.name}: StageData가 연결되어 있지 않아 전투를 시작할 수 없습니다.", MessageType.Warning);

            using (new EditorGUI.DisabledScope(!canUse || missingStage))
            {
                if (GUILayout.Button($"{data.name} ({data.SceneName})"))
                    Launch(data);

                //튜토리얼 전투는 성장 파트(결과 이후)만 바로 테스트하는 시작 옵션도 제공
                if (data.ID == TutorialStructureId && GUILayout.Button("   └ 튜토리얼 클리어 후 스테이지 선택에서 시작 (진행 상태 초기화)"))
                    LaunchGrowthTutorial(data);
            }
        }
    }

    private static void Launch(StructureData data)
    {
        SessionState.EraseBool(PendingGrowthTutorialKey);   //이전에 취소된 '튜토리얼 클리어 후 시작' 요청이 남아 있지 않게
        if (Application.isPlaying)
            LaunchInPlay(data);
        else
            LaunchFromEditor(data);
    }

    //튜토리얼 전투를 클리어한 상태(별 3 · 휘장 1, 스테이지 2 해금)로 만들고 스테이지 선택 씬에서 튜토리얼 성장 파트를 시작
    private static void LaunchGrowthTutorial(StructureData data)
    {
        SessionState.SetBool(PendingGrowthTutorialKey, true);
        if (Application.isPlaying)
            LaunchGrowthTutorialInPlay(data);
        else
            LaunchFromEditor(data);
    }

    private static void LaunchGrowthTutorialInPlay(StructureData data)
    {
        SessionState.EraseBool(PendingGrowthTutorialKey);

        GameSystem game = GameSystem.Instance;
        game.InitializeGameData();   //새 진행 상태 (튜토리얼 상태도 함께 초기화됨)
        game.EnterStage(data, 1, new List<Hero>());

        //승리 처리와 같은 기록: 클리어, 별 3개, 휘장 (GameSystem.RecordClear는 private이라 리플렉션으로 호출)
        System.Reflection.MethodInfo recordClear = typeof(GameSystem).GetMethod("RecordClear", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        recordClear.Invoke(game, new object[] { new[] { true, true, true } });

        if (!TutorialSystem.DebugResumeAfterBattle())
            Debug.LogWarning("StageLauncher: TutorialSystem을 찾지 못했거나 SceneEntered 단계가 없습니다.");

        SceneManager.LoadScene("StageScene");   //진입 시 StageSelectSystem.Start가 튜토리얼 단계를 시작
    }

    //플레이 중: StageSelectSystem.Update와 같은 호출 (구조물 진입 → 씬 이동). 해금 제약은 무시, 영웅 카드는 자동 선택
    //EnterStage가 클리어/오버 상태도 되돌린다. 스테이지 번호는 현재 값을 유지(보상 테이블 기준)
    private static void LaunchInPlay(StructureData data)
    {
        GameSystem game = GameSystem.Instance;
        game.EnterStage(data, game.CurrentStageLevel, game.GetAutoSelection(data));
        SceneManager.LoadScene(data.SceneName);
    }

    //에디터 모드: GAMEMASTER가 있는 StageScene에서 Play를 시작하고, 진입 후 구조물을 실행
    private static void LaunchFromEditor(StructureData data)
    {
        var startScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StageScenePath);
        if (startScene == null)
        {
            Debug.LogError($"StageLauncher: {StageScenePath}을 찾을 수 없습니다.");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        SceneAsset prevStartScene = EditorSceneManager.playModeStartScene;
        SessionState.SetString(PrevStartScenePathKey, prevStartScene != null ? AssetDatabase.GetAssetPath(prevStartScene) : string.Empty);
        SessionState.SetBool(RestoreStartSceneKey, true);
        SessionState.SetString(PendingStructureKey, AssetDatabase.GetAssetPath(data));

        EditorSceneManager.playModeStartScene = startScene;
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            RestoreStartScene();   //Play 시작 씬 지정은 이미 쓰였으므로 바로 원복
            if (!string.IsNullOrEmpty(SessionState.GetString(PendingStructureKey, string.Empty)))
                EditorApplication.update += WaitAndLaunch;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.update -= WaitAndLaunch;
            SessionState.EraseString(PendingStructureKey);
            SessionState.EraseBool(PendingGrowthTutorialKey);
            RestoreStartScene();
        }
    }

    //GameSystem이 생길 때까지 기다린 뒤 실행
    private static void WaitAndLaunch()
    {
        if (GameSystem.Instance == null) return;

        EditorApplication.update -= WaitAndLaunch;

        string path = SessionState.GetString(PendingStructureKey, string.Empty);
        SessionState.EraseString(PendingStructureKey);

        var data = AssetDatabase.LoadAssetAtPath<StructureData>(path);
        if (data == null)
        {
            Debug.LogError($"StageLauncher: 구조물 에셋을 찾을 수 없습니다. ({path})");
            return;
        }

        if (SessionState.GetBool(PendingGrowthTutorialKey, false))
            LaunchGrowthTutorialInPlay(data);
        else
            LaunchInPlay(data);
    }

    private static void RestoreStartScene()
    {
        if (!SessionState.GetBool(RestoreStartSceneKey, false)) return;

        string prevPath = SessionState.GetString(PrevStartScenePathKey, string.Empty);
        EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(prevPath) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(prevPath);

        SessionState.EraseBool(RestoreStartSceneKey);
        SessionState.EraseString(PrevStartScenePathKey);
    }
}
