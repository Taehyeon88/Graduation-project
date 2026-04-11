using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class SplineCreator : Singleton<SplineCreator>
{
    //[SerializeField] private Transform splinePoolTrans;                 //슬프라인 프리팹 풀 Transform

    //private Dictionary<int, List<SplineContainer>> splineTransById = new();         //Dic - Key: EnemyView-InstanceId | Value: 스프라인 프리팹
    //private List<SplineContainer> splines;                               //스프라인 풀의 프리팹들 미리 받아 놓기 (초기화 단계에)
    //private const int splineZValue = -5;

    //private void Start()
    //{
    //    Initialized();
    //}

    //private void Initialized()
    //{
    //    var temp = splinePoolTrans.GetComponentsInChildren<SplineContainer>(true);
    //    splines = new(temp);
    //    foreach(var spline in splines)
    //        spline.gameObject.SetActive(false);
    //}
    ////함수
    ////스프라인 생성(등록) 함수
    //public void CreateSpline(int tokenId, List<Vector2Int> path, Vector2Int startPos)
    //{
    //    if (splines == null)
    //        Initialized();

    //    //남는 spline 찾아서 받기 + 활성화
    //    SplineContainer splineContainer = splines[0];
    //    splineContainer.gameObject.SetActive(true);
    //    splines.Remove(splineContainer);

    //    //spline 등록 하기
    //    if (!splineTransById.ContainsKey(tokenId))
    //    {
    //        splineTransById.Add(tokenId, new());
    //    }
    //    splineTransById[tokenId].Add(splineContainer);

    //    //spline의 SplineContainer값 조정
    //    Spline spline = splineContainer.Spline;
    //    Debug.Log($"Spline 존재여부: {spline != null}");
    //    for (int i = 0; i < path.Count; i++)
    //    {
    //        //시작 위치의 Knock 추가 (i == 0 일 때)
    //        if (i == 0)
    //        {
    //            BezierKnot startknot = new(Utility.GridToWorldPoint(startPos, splineZValue));
    //            spline.Add(startknot);
    //            spline.SetTangentMode(TangentMode.Linear);
    //        }
    //        //각각 Knock 생성 및 값 할당(위치, 탄젠트, 탄제트 모드)

    //        BezierKnot knot = new();
    //        knot.Position = Utility.GridToWorldPoint(path[i], splineZValue);
    //        spline.SetTangentMode(TangentMode.Linear);
    //        spline.Insert(i, knot);
    //    }

    //    //spline의 SplineInstantiate값 조정
    //    var splineInstantiate = splineContainer.GetComponent<SplineInstantiate>();
    //    if (splineInstantiate != null)
    //    {
    //        splineInstantiate.enabled = true;
    //        splineInstantiate.InstantiateMethod = SplineInstantiate.Method.InstanceCount;
    //        splineInstantiate.MaxSpacing = path.Count;
    //        splineInstantiate.MinSpacing = path.Count;
    //    }
    //}

    ////스프라인 생성(등록) 해제 함수
    //public void RemoveSpline(int tokenId)
    //{
    //    if (splines == null)
    //        Initialized();

    //    //spline 등록 해제 하기
    //    if (splineTransById.ContainsKey(tokenId))
    //    {
    //        foreach (var splineContainer in splineTransById[tokenId])
    //        {
    //            if(splineContainer == null) continue;

    //            splines.Add(splineContainer);

    //            Spline spline = splineContainer.Spline;
    //            spline.Clear();
    //            splineContainer.gameObject.SetActive(false);
    //        }
    //        splineTransById.Remove(tokenId);
    //    }
    //}
}
