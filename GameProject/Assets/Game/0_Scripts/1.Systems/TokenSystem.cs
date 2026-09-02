using DG.Tweening;
using IsoTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TokenSystem : Singleton<TokenSystem> //몬스터 및 영웅 세팅 | 몬스터, 건물 추가 및 삭제 (게임 중) | 토큰 이동, 등
{
    [Header("TokenGrid")]
    [field: SerializeField] private TokenGrid grid;
    [field : SerializeField] public IsoWorld IsoWorld { get; private set; }

    [Header("TokenSubSystems")]
    [field: SerializeField] public TokenSetup Setup { get; private set; }
    [field: SerializeField] public TokenServiceAPI API { get; private set; }
    [field: SerializeField] public TokenMainAPI Main { get; private set; }

    //외부 시스템 접근 데이터들
    public Token SelectedToken { get; private set; }
    public List<HeroView> HeroViews { get; set; } = new();   //하위 시스템에게만 제한적으로 사용
    public List<EnemyView> EnemyViews { get; private set; } = new();
    public int gridWidth => grid.width; public int gridHeight => grid.height;
}
