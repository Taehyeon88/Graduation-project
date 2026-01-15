using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "SODatabase", menuName = "ScriptableObject/Database")]
public class ScriptableObjectDatabase : ScriptableObject
{
    string path = "Assets/ScriptableObjects";

    Dictionary<string, List<ScriptableObject>> sosBySoType = new Dictionary<string, List<ScriptableObject>>();

    [SerializeField] List<VisualList> visuals = new List<VisualList>();

    public ScriptableObject FindSODataByName(string type, string name)
    {
        var target = sosBySoType[type];
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

#if UNITY_EDITOR
    public void Intialize(Type type)
    {
        if (!Directory.Exists(path))
        {
            Debug.LogWarning($"{path}해당 경로의 폴더가 존재하지 않습니다.");
            return;
        }

        string soTypeName = type.Name;

        if (!sosBySoType.ContainsKey(soTypeName))
        {
            sosBySoType.Add(soTypeName, new List<ScriptableObject>());
        }

        sosBySoType[soTypeName].Clear();
        string[] guids = AssetDatabase.FindAssets($"t:{soTypeName}", new[] { path });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (so != null)
            {
                sosBySoType[soTypeName].Add(so);
            }
        }

        if (sosBySoType[soTypeName].Count == 0)
            sosBySoType.Remove(soTypeName);

        visuals.Clear();
        foreach (var key in sosBySoType.Keys)
            visuals.Add(new VisualList(sosBySoType[key]));

        if (SOKeyType.types.Length != sosBySoType.Keys.Count)
            CreateSOKeyTypeCS(sosBySoType.Keys.ToArray());
    }
    public void IntializeSODatabase()
    {
        if (!Directory.Exists(path))
        {
            Debug.LogWarning($"{path}해당 경로의 폴더가 존재하지 않습니다.");
            return;
        }
        sosBySoType.Clear();

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] {path});

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (so != null)
            {
                //Debug.Log($"SO이름:{so.GetType().Name}");

                string soTypeName = so.GetType().Name;

                if (!sosBySoType.ContainsKey(soTypeName))
                {
                    sosBySoType.Add(soTypeName, new List<ScriptableObject>() {so});
                }
                else
                {
                    sosBySoType[soTypeName].Add(so);
                }
            }
        }

        visuals.Clear();
        foreach (var key in sosBySoType.Keys)
            visuals.Add(new VisualList(sosBySoType[key]));

        if (SOKeyType.types.Length != sosBySoType.Keys.Count)
            CreateSOKeyTypeCS(sosBySoType.Keys.ToArray());
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

            string keystring = "{" + string.Format("{0}", string.Join(",", sosBySoType.Keys.ToArray())) + "}";
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

[System.Serializable]
public class VisualList
{
    [SerializeField] List<ScriptableObject> list = new List<ScriptableObject>();

    public VisualList(List<ScriptableObject> list)
    {
        this.list = list;
    }
}