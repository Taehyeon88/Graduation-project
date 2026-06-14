using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomMapCreator : Singleton<RandomMapCreator>
{
    private MapThemeData themeData;      //맵 테마 데이터
    private Vector2Int[] heroStartPoses; //플레이어 시작 위치

    private int[,] simpleGrid;
    private int[,] mainGrid;

    private int current_object_cnt;

    //1.테마 데이터 로드
    //2.청크 설치 가능 개수 받기(Random)
    //3.모든 청크 랜덤 설치 위치 지정(Random)
    //4.남은 오브젝트들 가중치로 what(오브젝트) where(어디)에 설치 할지(Random 지정)
    //5.완료
    public void CreateMap(MapThemeData themeData, Vector2Int[] heroStartPoses)
    {
        this.themeData = themeData;
        this.heroStartPoses = heroStartPoses;

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
        current_object_cnt = 0;

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

        //오브젝트 생성
        TokenSystem.Instance.SetUpObjects(mainGrid);
    }

    private void SetChunkInGrid(ChunkData chunkData)
    {
        //1.맵 안에 청크 설치가능 위치 찾기 (토큰 존재 여부 상관X)
        //2.토큰 존재 여부를 고려해 설치 가능 위치 추리기
        //3.설치 가능한 위치중에 랜덤으로 선정
        //4.grid에 설치된 위치 표시 및 특정 위치에서 특정 오브젝트가 설치 됨을 표시

        int width = simpleGrid.GetLength(0);   //그리드 가로 블럭 수
        int height = simpleGrid.GetLength(1);  //그리드 세로 블럭 수

        (Vector2Int pos, ChunkData data)[] enable_Poses = new (Vector2Int, ChunkData)[width * height];
        int possible_cnt = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x + chunkData.Width >= width) continue;
                if (y + chunkData.Height >= height) break;
                if (x == 0 || y == 0) continue;

                int blockId = 0;
                bool unable = false;
                for (int cy = 0; cy < chunkData.Height; cy++)
                {
                    for (int cx = 0; cx < chunkData.Width; cx++)
                    {
                        Vector2Int pos = new Vector2Int(x + cx, y + cy);

                        if (chunkData.Objects.Length <= blockId) break;
                        if (heroStartPoses.Contains(pos)) break;
                        if (chunkData.Objects[blockId] > 0 && simpleGrid[pos.x, pos.y] == 1) unable = true;

                        if (unable) break;
                        blockId++;
                    }
                    if (unable) break;
                }

                if (!unable)
                {
                    //Debug.Log($"설치 가능 위치: {new Vector2Int(x, y)}");
                    enable_Poses[possible_cnt] = (new Vector2Int(x, y), chunkData); //청크 정보 저장
                    possible_cnt++;
                }
            }
        }

        //랜덤 위치 생성
        if (possible_cnt == 0) return;

        int r_value = UnityEngine.Random.Range(0, possible_cnt);

        Vector2Int r_Pos = enable_Poses[r_value].pos;
        ChunkData r_Data = enable_Poses[r_value].data;

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

        //Debug.Log($"총 설치 가능 위치 수: {possible_cnt}, 선택된 위치 : {r_Pos}");
    }

    private void SetObjectInGrid(int remainCount)
    {
        //남은 오브젝트 개수만큼 가중치 랜덤 함수를 통해서 특정 오브젝트를 선정해서 뽑음.

        //1.모든 오브젝트의 가중치를 가져와서 리스트화.
        //2.해당 가중치 리스트로 n회 랜덤 뽑기 실행.
        //3.각 n번 뽑은 오브젝트를 main, simple에 갱신 처리.

        List<(int id, int weight)> object_list = new(themeData.ObstacleSpawnInfos.Length * 2);
        foreach (var objInfo in themeData.ObstacleSpawnInfos)
        {
            int cnt = objInfo.maxSpawnLimit;
            for (int i = 0; i < cnt; i++)
                object_list.Add((objInfo.ObstacleId, objInfo.spawnWeight));
        }

        List<Vector2Int> emptyPoses = new List<Vector2Int>(simpleGrid.Length);
        for (int y = 0; y < simpleGrid.GetLength(1); y++)
        {
            for (int x = 0; x < simpleGrid.GetLength(0); x++)
            {
                if (simpleGrid[x, y] == 0)
                    emptyPoses.Add(new(x, y));
            }
        }

        while (remainCount > 0 && object_list.Count > 0 && emptyPoses.Count > 0)
        {
            int object_index = GetWeightRandomIndex(object_list);

            //해당 인덱스의 오브젝트를 어디에서 설치 할지 랜덤 탐색(단, 닫힌 구간에 설치 X 처리)
            int grid_index = UnityEngine.Random.Range(0, emptyPoses.Count);
            Vector2Int pos = emptyPoses[grid_index];

            bool isClose = false;
            var map = (int[,])simpleGrid.Clone();
            map[pos.x, pos.y] = 1;

            foreach (var dir in UtilityBFS.Dirs)
            {
                Vector2Int target_pos = pos + dir;

                if (!TokenSystem.Instance.IsBound(target_pos)) continue;
                if (simpleGrid[target_pos.x, target_pos.y] == 1) continue;

                bool isExist = UtilityBFS.IsPathExist(map, target_pos, heroStartPoses[0]);
                if (!isExist)
                {
                    isClose = true;
                }
            }

            if (isClose)
            {
                emptyPoses.RemoveAt(grid_index);
            }
            else
            {
                //해당 오브젝트 배치(simple, main)
                //남은 개수, 오브젝트 리스트, 남은 빈 공간 갱신

                simpleGrid[pos.x, pos.y] = 1;
                mainGrid[pos.x, pos.y] = object_list[object_index].id;

                remainCount--;
                object_list.RemoveAt(object_index);
                emptyPoses.RemoveAt(grid_index);
            }
        }
    }

    //가중치 기반 랜덤 뽑기 함수(가중치 리스트를 통해서 리스트의 인덱스를 반환하는 함수)
    private int GetWeightRandomIndex(List<(int id, int weight)> list)
    {
        List<float> weights = new(list.Count);

        float total = 0f;

        foreach (var element in list)
        {
            weights.Add(element.weight);

            total += element.weight;
        }

        float rand = UnityEngine.Random.value * total;
        float current = 0f;
        for (int i = 0; i < list.Count; i++)
        {
            current += weights[i];

            if (rand <= current)
                return i;
        }

        return list.Count - 1;
    }
}
