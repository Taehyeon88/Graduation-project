using System.Collections.Generic;

public class StageProgress   //스테이지 하나의 진행 기록 (GameSystem이 스테이지별로 보관, 씬 전환 후에도 유지)
{
    public const int StarCount = 3;   //★1 승리 / ★2 용병대장 체력 / ★3 특수 조건 — 기획 고정 틀

    public bool IsCleared { get; private set; }             //승리(★1) 또는 비전투 구조물 통과 시 true → 다음 스테이지 해금
    public bool IsInsigniaGiven { get; private set; }       //휘장은 첫 클리어 1회만
    public IReadOnlyList<Hero> LastSelection => lastSelection;   //지난 영웅 카드 선택 (팝업 미리 채움)

    private readonly bool[] stars = new bool[StarCount];    //조건별 달성 기록 (한 번 달성하면 유지)
    private readonly List<Hero> lastSelection = new();

    public int StarTotal
    {
        get
        {
            int total = 0;
            foreach (bool star in stars)
                if (star) total++;
            return total;
        }
    }

    public bool HasStar(int index) => stars[index];

    public void MarkCleared() => IsCleared = true;

    //이번 판 달성 결과를 기록하고 새로 달성한 별 개수를 돌려준다
    public int RecordStars(bool[] achieved)
    {
        int gained = 0;
        for (int i = 0; i < StarCount; i++)
        {
            if (!achieved[i] || stars[i]) continue;
            stars[i] = true;
            gained++;
        }
        return gained;
    }

    //첫 지급이면 true (이후 false)
    public bool TryGiveInsignia()
    {
        if (IsInsigniaGiven) return false;
        IsInsigniaGiven = true;
        return true;
    }

    public void SetLastSelection(IEnumerable<Hero> selection)
    {
        lastSelection.Clear();
        lastSelection.AddRange(selection);
    }
}
