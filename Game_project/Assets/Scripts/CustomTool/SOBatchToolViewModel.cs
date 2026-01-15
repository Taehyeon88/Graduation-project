#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TableForge.Editor.UI;
using UnityEditor;
using System.IO;

public enum SOBatchType
{
    NonSelected, Add, Remove, Initialize
}

public class SOBatchToolViewModel
{
    public int nameSpaceIndex { get; set; }
    public int typeIndex { get; set; }
    public SOBatchType batchType { get; set; }
    public int addCount { get; set; }

    public string[] nameSpaces => TypeRegistry.Namespaces.ToArray();
    public string[] soTypesNameByNameSpace => TypeRegistry.TypesByNamespaceAndName[TypeRegistry.Namespaces[nameSpaceIndex]].Keys.ToArray();
    public string selectedSOTypeName => soTypesNameByNameSpace[typeIndex];
    public bool showLeftPanel => batchType == SOBatchType.Remove;
    public IReadOnlyList<ScriptableObject> SelectedSOs => selectedSOs;

    private Type selectedSOType => TypeRegistry.TypesByNamespaceAndName[TypeRegistry.Namespaces[nameSpaceIndex]][selectedSOTypeName];

    private List<ScriptableObject> selectedSOs = new List<ScriptableObject>(); //선택된 SO
    private string outPutFolder = "Assets/ScriptableObjects";

    private ScriptableObjectDatabase soDatabase;

    public SOBatchToolViewModel()
    {
        var so = Resources.Load<ScriptableObjectDatabase>("SODatabase");
        if (so != null)
        {
            Debug.Log("SODB로드 성공!!");
            soDatabase = so;
        }
    }

    public void AddSelectedSO(ScriptableObject so)
    {
        selectedSOs.Add(so);
    }
    public void CancelSelectedObject(ScriptableObject so)
    {
        selectedSOs.Remove(so);
    }

    public void RemoveScriptableObjects()
    {
        foreach (var so in selectedSOs)
        {
            if (so == null) continue;

            string assetPath = AssetDatabase.GetAssetPath(so);
            if (string.IsNullOrEmpty(assetPath)) continue;

            AssetDatabase.DeleteAsset(assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        soDatabase.Intialize(selectedSOType);

        Debug.Log("삭제 성공");
        selectedSOs.Clear();
    }
    public void AddScriptableObjects()
    {
        if (!Directory.Exists(outPutFolder))
        {
            Directory.CreateDirectory(outPutFolder);
        }
        if(!PlayerPrefs.HasKey("si")) 
            PlayerPrefs.SetInt("si",0);

        Debug.Log($"{addCount}개수만큼 추가되었습니다.");

        for (int i = 0; i < addCount; i++)
        {
            int si = PlayerPrefs.GetInt("si");
            ScriptableObject so = ScriptableObject.CreateInstance(selectedSOType);

            string assetPath = $"{outPutFolder}/{selectedSOTypeName.Substring(0, 3)}_{si}.asset";
            AssetDatabase.CreateAsset(so, assetPath);

            so.name = $"{selectedSOTypeName.Substring(0, 3)}_{si}";
            EditorUtility.SetDirty(so);
            PlayerPrefs.SetInt("si", si + 1 == int.MaxValue ? 0 : si + 1);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        soDatabase.Intialize(selectedSOType);
    }
    public void InitializeSODatabase()
    {
        soDatabase.IntializeSODatabase();
        Debug.Log($"모든 SO에 대한 데이터베이스 초기화 성공.");
    }
}
#endif