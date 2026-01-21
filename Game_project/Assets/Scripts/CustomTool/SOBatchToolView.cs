#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SOBatchToolView : EditorWindow
{
    private SOBatchToolViewModel _viewModel;

    private Vector2 leftScroll;
    private Vector2 rightScroll;
    private int selectedIndex;

    const float CHIP_WIDTH = 140f;
    const float CHIP_HEIGHT = 26f;
    const float CHIP_MARGIN = 6f;

    const float CONT_HEIGHT = 200f;
    float c_y_Max = CONT_HEIGHT;

    [MenuItem("Tools/ScriptableObjectBatchTool")]
    public static void ShowEditor()
    {
        GetWindow<SOBatchToolView>("SO_BatchTool");
    }

    private void OnEnable()
    {
         _viewModel = new SOBatchToolViewModel();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        if(_viewModel.showLeftPanel)
            DrawLeftPanel();

        DrawDivider(1);

        DrawRightPanel();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

        var soGuids = AssetDatabase.FindAssets($"t:{_viewModel.selectedSOTypeName}");
        var allTypeNames = new List<string>();
        var allTypes = new List<ScriptableObject>();
        foreach (var guid in soGuids)
        {
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));
            allTypeNames.Add(so.name);
            allTypes.Add(so);
        }

        EditorGUILayout.LabelField("TargetList : ", EditorStyles.whiteBoldLabel);
        EditorGUILayout.Space();

        for(int i = 0; i < allTypeNames.Count; i++)
        {
            GUIStyle style = (i == selectedIndex) ?
                           EditorStyles.boldLabel : EditorStyles.label;

            if (GUILayout.Button(allTypeNames[i], style))
            {
                if (_viewModel.SelectedSOs.Contains(allTypes[i])) continue;

                selectedIndex = i;
                _viewModel.AddSelectedSO(allTypes[i]);
            }
        }


        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical();
        rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

        GUILayout.Label("ScriptableObjectBatchTool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        _viewModel.nameSpaceIndex = EditorGUILayout.Popup("NameSpace", _viewModel.nameSpaceIndex, _viewModel.nameSpaces);
        _viewModel.typeIndex = EditorGUILayout.Popup("SOType", _viewModel.typeIndex, _viewModel.soTypesNameByNameSpace);

        EditorGUILayout.Space();
        _viewModel.batchType = (SOBatchType)EditorGUILayout.EnumPopup("BatchType", _viewModel.batchType);


        if (_viewModel.batchType == SOBatchType.Remove)
        {
            EditorGUILayout.Space(5);
            DrawSelectedChips();
        }
        else if (_viewModel.batchType == SOBatchType.Add)
        {
            EditorGUILayout.Space();
            _viewModel.addCount = EditorGUILayout.IntField("Count", _viewModel.addCount);
            if (GUILayout.Button("Confirm"))
            {
                _viewModel.AddScriptableObjects();
            }

        }
        else if (_viewModel.batchType == SOBatchType.Initialize)
        {
            if (GUILayout.Button("Confirm"))
            {
                _viewModel.InitializeSODatabase();
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawDivider(float thinkness = 1f)
    {
        Rect rect = EditorGUILayout.GetControlRect(
                GUILayout.Width(thinkness),
                GUILayout.ExpandHeight(true)
                );

        EditorGUI.DrawRect(rect, new Color(0.25f, 0.25f, 0.25f));
    }

    private void DrawSelectedChips()
    {
        Rect container = GUILayoutUtility.GetRect(
            0,
            c_y_Max,
            GUILayout.ExpandWidth(true)
        );

        GUI.Box(container, GUIContent.none);

        DrawChipFlow(container);
        DrawConfirmButton(container.xMax, container.yMax);
    }

    private void DrawChipFlow(Rect container)
    {
        float x = container.x + CHIP_MARGIN;
        float y = container.y + CHIP_MARGIN;

        float maxX = container.xMax - CHIP_MARGIN;

        foreach (var so in _viewModel.SelectedSOs.ToList())
        {
            if (x + CHIP_WIDTH > maxX)
            {
                x = container.x + CHIP_MARGIN;
                y += CHIP_HEIGHT + CHIP_MARGIN;
            }

            Rect chipRect = new Rect(
                x,
                y,
                CHIP_WIDTH,
                CHIP_HEIGHT
            );

            DrawChipRect(chipRect, so);

            x += CHIP_WIDTH + CHIP_MARGIN;
        }

        if (Event.current.type == EventType.Repaint)
        {
            float current = c_y_Max;
            c_y_Max = Mathf.Max(CONT_HEIGHT, Mathf.Abs(y - container.y) + CHIP_HEIGHT + CHIP_MARGIN);

            if (current != c_y_Max) Repaint();
        }
    }

    private void DrawChipRect(Rect rect, ScriptableObject so)
    {
        EditorGUI.DrawRect(rect, new Color(0.22f, 0.22f, 0.22f));

        Handles.color = new Color(0.1f, 0.1f, 0.1f);
        Handles.DrawAAPolyLine(
            1f,
            new Vector3(rect.xMin, rect.yMin),
            new Vector3(rect.xMax, rect.yMin),
            new Vector3(rect.xMax, rect.yMax),
            new Vector3(rect.xMin, rect.yMax),
            new Vector3(rect.xMin, rect.yMin)
        );

        Rect labelRect = new Rect(
            rect.x + 8,
            rect.y + 4,
            rect.width - 28,
            rect.height - 8
        );
        GUI.Label(labelRect, so.name);

        Rect closeRect = new Rect(
            rect.xMax - 22,
            rect.y + 4,
            18,
            rect.height - 8
        );

        if (GUI.Button(closeRect, "X"))
        {
            _viewModel.CancelSelectedObject(so);
            GUI.FocusControl(null);
        }
    }

    private void DrawConfirmButton(float x, float y)
    {
        float width = 80f;
        float height = 27f;
        float margin = 10;

        Rect rect = new Rect(
            x - width - margin,
            y + margin,
            width, height
        );

        if (GUI.Button(rect, "Confirm"))
        {
            _viewModel.RemoveScriptableObjects();
        }
    }
}
#endif
