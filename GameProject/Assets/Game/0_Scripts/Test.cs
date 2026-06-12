using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] private MapThemeData themeData;  //맵 테마 데이터

    [Header("시각화용")]
    [SerializeField] private GameObject prefab;

    private int[,] simpleGrid;
    private int[,] mainGrid;

    private int current_object_cnt;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            GenerateMap();
        }
    }

    //1.테마 데이터 로드
    //2.청크 설치 가능 개수 받기(Random)
    //3.모든 청크 랜덤 설치 위치 지정(Random)
    //4.남은 오브젝트들 가중치로 what(오브젝트) where(어디)에 설치 할지(Random 지정)
    //5.완료
    private void GenerateMap()
    {
        //simpleGrid 초기화
        int width = TokenSystem.Instance.gridWidth;
        int height = TokenSystem.Instance.gridHeight;

        simpleGrid = new int[width, height];
        mainGrid = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                simpleGrid[x, y] = 0;
                mainGrid[x, y] = 0;
            }
        }

        //청크 랜덤 설치
        int chunkCount = UnityEngine.Random.Range(themeData.MinChunkCount, themeData.MaxChunkCount);
        chunkCount = Mathf.Min(chunkCount, themeData.ChunkPool.Length);
        for (int i = 0; i < chunkCount; i++)
        {
            SetChunkInGrid(DataSystem.Instance.GetChunkById(themeData.ChunkPool[i]));
        }

        //오브젝트 랜덤 설치
        int objectCount = UnityEngine.Random.Range(themeData.MinObjectCount, themeData.MaxObjectCount);
        objectCount -= current_object_cnt;
        SetObjectInGrid(objectCount);


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject gameObject = Instantiate(prefab, new Vector3(x, 0, y), Quaternion.identity);

                if (simpleGrid[x, y] == 1)
                {
                    Renderer renderer = gameObject.GetComponent<Renderer>();
                    renderer.material.color = Color.red;
                }
            }
        }
    }

    private void SetChunkInGrid(ChunkData chunkData)
    {
        //1.맵 안에 청크 설치가능 위치 찾기 (토큰 존재 여부 상관X)
        //2.토큰 존재 여부를 고려해 설치 가능 위치 추리기
        //3.설치 가능한 위치중에 랜덤으로 선정
        //4.grid에 설치된 위치 표시 및 특정 위치에서 특정 오브젝트가 설치 됨을 표시

        int width = simpleGrid.GetLength(0);   //그리드 가로 블럭 수
        int height = simpleGrid.GetLength(1);  //그리드 세로 블럭 수

        (Vector2Int, ChunkData)[] enable_Poses = new (Vector2Int, ChunkData)[width * height];
        int possible_cnt = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x + chunkData.Width > width) continue;
                if (y + chunkData.Height > height) break;

                int blockId = 0;
                bool unable = false;
                for (int cy = 0; cy < chunkData.Height; cy++)
                {
                    for (int cx = 0; cx < chunkData.Width; cx++)
                    {
                        Vector2Int pos = new Vector2Int(x + cx, y + cy);

                        if (chunkData.Objects.Length <= blockId) break;
                        if (chunkData.Objects[blockId] > 0 && simpleGrid[pos.x, pos.y] == 1) unable = true;

                        if (unable) break;
                        blockId++;
                    }
                    if (unable) break;
                }

                if (!unable)
                {
                    enable_Poses[possible_cnt] = (new Vector2Int(x, y), chunkData); //청크 정보 저장
                    possible_cnt++;
                }
            }
        }

        //랜덤 위치 생성
        if (possible_cnt == 0) return;

        int r_value = UnityEngine.Random.Range(0, possible_cnt);

        Vector2Int r_Pos = enable_Poses[r_value].Item1;
        ChunkData r_Data = enable_Poses[r_value].Item2;

        //청크가 들어갈 각각의 그리드 블럭에 정보 저장
        int rx = 0; int ry = 0;
        foreach (var objId in r_Data.Objects)
        {
            Vector2Int pos = r_Pos + new Vector2Int(rx, ry);

            if (objId > 0)
            {
                simpleGrid[pos.x, pos.y] = 1;
                mainGrid[pos.x, pos.y] = objId;
                current_object_cnt++;            //오브젝트 개수 카운트
            }

            rx++;
            if (rx >= chunkData.Width)
            {
                rx = 0;
                ry++;
            }
        }
    }

    private void SetObjectInGrid(int remainCount)
    {
        //남은 오브젝트 개수만큼 가중치 랜덤 함수를 통해서 특정 오브젝트를 선정해서 뽑음.

    }
}
