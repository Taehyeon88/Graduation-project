using System.Collections.Generic;

public class SpawnWaveGA : GameAction
{
    public List<EnemyView> SpawnedEnemies { get; set; } = new();   //이번 웨이브로 생성된 몬스터 (Performer가 채움)
}
