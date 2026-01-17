#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "SODatabase", menuName = "ScriptableObject/Database")]
public class ScriptableObjectDatabase : ScriptableObject
{
    string path = "Assets/ScriptableObjects";

    [SerializeField]
    CustomDictionary soCustomDic;  //SOType이름 | 해당 타입의 SO들
    [NonSerialized]
    Dictionary<string, List<ScriptableObject>> runTimeDic = new Dictionary<string, List<ScriptableObject>>();  //런타임용

    public ScriptableObject FindSODataByName(string type, string name)
    {
        if (!runTimeDic.TryGetValue(type, out var target))
        {
            InitialRunTimeDictionary();
            target = runTimeDic[type];
        }

        if (target == null)
        {
            Debug.LogWarning($"{type}타입의 SO데이터를 찾을 수 없습니다.");
            return null;
        }

        var so = target.Find(s => s.name == name);
        if (so == null)
        {
            Debug.LogWarning($"{type}타입의 {name}이름의 SO데이터를 찾을 수 없습니다.");
            return null;
        }
        return so;
    }

    public void InitialRunTimeDictionary()
    {
        runTimeDic.Clear();
        foreach (var key in soCustomDic.Keys)
            runTimeDic.Add(key, soCustomDic[key]);
    }

    public void OnEnable()
    {
        if (soCustomDic == null)
            soCustomDic = new CustomDictionary();
    }

#if UNITY_EDITOR
    public void Intialize(Type type)
    {
        if (!Directory.Exists(path))
        {
            Debug.LogWarning($"{path}해당 경로의 폴더가 존재하지 않습니다.");
            return;
        }

        string soTypeName = type.Name;
        //Debug.Log($"초기화대상 타입: {soTypeName}");

        if (!soCustomDic.ContainsKey(soTypeName))
        {
            soCustomDic.Add(soTypeName, new List<ScriptableObject>());
        }

        soCustomDic[soTypeName].Clear();

        string[] guids = AssetDatabase.FindAssets($"t:{soTypeName}", new[] { path });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (so != null)
            {
                soCustomDic[soTypeName].Add(so);
            }
        }

        if (soCustomDic[soTypeName].Count == 0)
            soCustomDic.Remove(soTypeName);

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (SOKeyType.types.Length != soCustomDic.Keys.Count)
            CreateSOKeyTypeCS(soCustomDic.Keys.ToArray());
    }
    public void IntializeSODatabase()
    {
        if (!Directory.Exists(path))
        {
            Debug.LogWarning($"{path}해당 경로의 폴더가 존재하지 않습니다.");
            return;
        }
        soCustomDic.Clear();

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] {path});

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (so != null)
            {
                //Debug.Log($"SO이름:{so.GetType().Name}");

                string soTypeName = so.GetType().Name;

                if (!soCustomDic.ContainsKey(soTypeName))
                {
                    soCustomDic.Add(soTypeName, new List<ScriptableObject>() {so});
                }
                else
                {
                    soCustomDic[soTypeName].Add(so);
                }
            }
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (SOKeyType.types.Length != soCustomDic.Count)
            CreateSOKeyTypeCS(soCustomDic.Keys.ToArray());
    }

    private void CreateSOKeyTypeCS(string[] keys)
    {
        string fileName = "SOKeyType";

        string folderPath = Path.Combine(Application.dataPath, "Scripts/SO/Database");
        string path = $"{folderPath}/{fileName}.cs";

        if (Directory.Exists(folderPath))
        {
            Debug.Log("스크립트 생성됨");

            string content = "";
            content += "using System;" + '\n' + '\n';
            content += $"public static class {fileName}" + '\n' + "{" + '\n';

            string keystring = "{" + string.Format("{0}", string.Join(",", keys)) + "}";
            content += $"    public static string[] types = new string[]{keystring};" + '\n';
            foreach (string key in keys)
            {
                content += $"    public static string {key} = \"{key}\";" + '\n';
            }

            content += "}";

            File.WriteAllText(path, content);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else Debug.LogWarning($"{path}경로에 폴더가 존재하지 않습니다.");
    }
#endif
}