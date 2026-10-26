using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

//스테이지 선택 흐름(CurrentStageLevel 제약)을 무시하고 원하는 구조물을 바로 실행하는 디버그 창
//플레이 중: 현재 런 상태를 유지한 채 이동 / 에디터 모드: StageScene에서 Play를 시작한 뒤 새 런 상태로 이동
[InitializeOnLoad]
public class StageLauncherWindow : EditorWindow
{
    private static readonly int[] LaunchableIds = { 1, 2, 7 };   //실행 가능한 구조물 ID (일반 전투, 주점, 정예 전투)

    private const string BattleSceneName = "GameDemoScene";
    private const string StageScenePath = "Assets/Game/2_Scenes/StageScene.unity";
    private const string PendingStructureKey = "StageLauncher.PendingStructure";   //Play 진입 후 실행할 구조물 에셋 경로
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
            string mode = playing ? "플레이 중: 현재 런 상태(덱·체력·층)를 유지한 채 이동합니다." : "에디터 모드: StageScene에서 Play를 시작해 새 런 상태로 이동합니다.";
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
            }
        }
    }

    private static void Launch(StructureData data)
    {
        if (Application.isPlaying)
            LaunchInPlay(data);
        else
            LaunchFromEditor(data);
    }

    //플레이 중: StageSelectSystem.Update와 같은 호출 (구조물 선택 → 씬 이동)
    private static void LaunchInPlay(StructureData data)
    {
        GameSystem game = GameSystem.Instance;

        //이전 전투의 클리어/오버 상태가 남으면 새 전투가 바로 멈추므로 되돌림 (private set이라 리플렉션)
        SetResultFlag(game, nameof(GameSystem.IsGameClear));
        SetResultFlag(game, nameof(GameSystem.IsGameOver));

        game.SelectStructure(data);
        SceneManager.LoadScene(data.SceneName);
    }

    private static void SetResultFlag(GameSystem game, string propertyName)
    {
        typeof(GameSystem).GetProperty(propertyName).SetValue(game, false);
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
