using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;
using TableForge.Editor.UI;
using UnityEngine.UIElements;

public enum SOBatchType
{
    Add, Remove, NonSelected
}

public class ScriptableObjectBatchTool : EditorWindow
{
    private SOBatchType _batchType = SOBatchType.NonSelected;

    private int nameSpaceIndex = 0;     //선택된 nameSpace 인덱스
    private int typeIndex = 0;          //선택된 SO타입 인덱스
    private string selectedSOType;      //선택된 SO타입
    private List<ScriptableObject> selectedSOs = new List<ScriptableObject>(); //선택된 SO

    private bool showLeftPanel = false; //삭제용 패널 사용여부
    private Vector2 leftScroll;
    private Vector2 rightScroll;
    private int selectedIndex = 0;      //삭제 선택 SO

    int addCount = 0;

    [MenuItem("Tools/ScriptableObjectBatchTool")]
    public static void ShowEditor()
    {
        GetWindow<ScriptableObjectBatchTool>("SO_BatchTool");

        Debug.Log(string.Join(",", TypeRegistry.TypeNames));
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        if(showLeftPanel)
            DrawLeftPanel();

        DrawDivider(1);

        DrawRightPanel();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

        var soGuids = AssetDatabase.FindAssets($"t:{selectedSOType}");
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
                if (selectedSOs.Contains(allTypes[i])) continue;

                selectedIndex = i;
                selectedSOs.Add(allTypes[i]);
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

        nameSpaceIndex = EditorGUILayout.Popup("NameSpace", nameSpaceIndex, TypeRegistry.Namespaces.ToArray());
        var soTypeNames = TypeRegistry.TypesByNamespaceAndName[TypeRegistry.Namespaces[nameSpaceIndex]].Keys.ToList();
        typeIndex = EditorGUILayout.Popup("SOType", typeIndex, soTypeNames.ToArray());
        selectedSOType = soTypeNames[typeIndex];

        EditorGUILayout.Space();
        _batchType = (SOBatchType)EditorGUILayout.EnumPopup("BatchType", _batchType);


        if (_batchType == SOBatchType.Remove)
        {
            showLeftPanel = true;

            EditorGUILayout.Space(5);
            DrawSelectedChips();
        }
        else if (_batchType == SOBatchType.Add)
        {
            showLeftPanel = false;

            EditorGUILayout.Space();
            addCount = EditorGUILayout.IntField("Count", addCount);
            if (GUILayout.Button("Confirm"))
            {
                Debug.Log($"{addCount}개수만큼 추가되었습니다.");
            }

        }
        else showLeftPanel = false;

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

    const float CHIP_WIDTH = 140f;
    const float CHIP_HEIGHT = 26f;
    const float CHIP_MARGIN = 6f;

    const float CONT_HEIGHT = 200f;
    float c_y_Max = CONT_HEIGHT;

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

        foreach (var so in selectedSOs.ToList())
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
            c_y_Max = Mathf.Max(CONT_HEIGHT, Mathf.Abs(y - container.y) + CHIP_HEIGHT + CHIP_MARGIN);
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
            selectedSOs.Remove(so);
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
            Debug.Log("삭제 성공");
            selectedSOs.Clear();
        }
    }
}

