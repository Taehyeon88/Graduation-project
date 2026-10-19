using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HeroHandDebugWindow : EditorWindow
{
    private readonly List<HeroData> heroes = new();
    private Vector2 scroll;

    [MenuItem("Tools/Debug/Hero Hand")]
    private static void Open()
    {
        GetWindow<HeroHandDebugWindow>("Hero Hand");
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
        heroes.Clear();
        foreach (string guid in AssetDatabase.FindAssets("t:HeroData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<HeroData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data != null) heroes.Add(data);
        }
        heroes.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
    }

    private static string GetLabel(HeroData data)
    {
        string label = $"{data.name} ({data.APCost})";
        if (data.Traits == null || data.Traits.Count == 0) return label;

        var names = new List<string>();
        foreach (TraitData trait in data.Traits)
            if (trait != null) names.Add(trait.name);

        return names.Count > 0 ? $"{label} - {string.Join(", ", names)}" : label;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh")) Refresh();

        bool ready = Application.isPlaying
            && CardSystem.Instance != null
            && TurnSystem.Instance != null
            && ActionSystem.Instance != null;

        if (!ready)
            EditorGUILayout.HelpBox("플레이 모드에서만 사용할 수 있습니다.", MessageType.Info);
        else if (TurnSystem.Instance.CurrentTurn != TurnType.Player)
            EditorGUILayout.HelpBox("플레이어 턴에만 사용할 수 있습니다.", MessageType.Info);
        else if (ActionSystem.Instance.IsPerforming)
            EditorGUILayout.HelpBox("액션 수행 중입니다.", MessageType.Info);

        bool canUse = ready
            && TurnSystem.Instance.CurrentTurn == TurnType.Player
            && !ActionSystem.Instance.IsPerforming;

        using (new EditorGUI.DisabledScope(!canUse))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("마나", EditorStyles.boldLabel);
            if (GUILayout.Button("AP 최대치로 채우기"))
            {
                int missing = APSystem.Instance.MaxAP - APSystem.Instance.CurrentAP;
                if (missing > 0)
                    ActionSystem.Instance.Perform(new RefillAPGA(missing));
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("영웅 받기", EditorStyles.boldLabel);
            using (var view = new EditorGUILayout.ScrollViewScope(scroll))
            {
                scroll = view.scrollPosition;
                foreach (HeroData data in heroes)
                {
                    if (GUILayout.Button(GetLabel(data)))
                        CardSystem.Instance.AddHeroToHand(data);
                }
            }
        }
    }
}
