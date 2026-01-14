using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TableForge.Editor.UI;

public enum SOBatchType
{
    NonSelected, Add, Remove
}

public class SOBatchToolViewModel
{
    public int nameSpaceIndex { get; set; }
    public int typeIndex { get; set; }
    public SOBatchType batchType { get; set; }
    public int addCount { get; set; }

    public string[] nameSpaces => TypeRegistry.Namespaces.ToArray();
    public string[] soTypesNameByNameSpace => TypeRegistry.TypesByNamespaceAndName[TypeRegistry.Namespaces[nameSpaceIndex]].Keys.ToArray();
    public string selectedSOType => soTypesNameByNameSpace[typeIndex];
    public bool showLeftPanel => batchType == SOBatchType.Remove;
    public IReadOnlyList<ScriptableObject> SelectedSOs => selectedSOs;

    private List<ScriptableObject> selectedSOs = new List<ScriptableObject>(); //선택된 SO


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
        Debug.Log("삭제 성공");
        selectedSOs.Clear();
    }
    public void AddScriptableObjects()
    {
        Debug.Log($"{addCount}개수만큼 추가되었습니다.");
    }
}
