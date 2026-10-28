using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class RichTextFormatter
{
    //키워드 -> 스프라이트 이름 (속도는 이동력 아이콘 공용)
    private static readonly Dictionary<string, string> statIcons = new()
    {
        { "공격력", "공격력" },
        { "체력", "체력" },
        { "이동력", "이동력" },
        { "속도", "이동력" },
    };

    //키워드 -> (스프라이트 이름, 글자색 hex) (아이콘 뒤에 키워드 텍스트를 아이콘 색으로 남김)
    private static readonly Dictionary<string, (string icon, string color)> labeledIcons = new()
    {
        { "영웅 대장 체력", ("영웅 대장 체력", "2393FD") },
        { "영웅 대장 공격력", ("영웅 대장 공격력", "2293FD") },
        { "마나", ("마나", "3B73C9") },
        { "AP", ("마나", "3B73C9") },
        { "골드", ("골드", "FECD2E") },
    };

    //description 안의 키워드를 <sprite> 태그로 치환
    public static string Format(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var replacements = new Dictionary<string, string>();

        foreach (var pair in statIcons)
            replacements[pair.Key] = $"<sprite name=\"{pair.Value}\">";

        foreach (var pair in labeledIcons)
            replacements[pair.Key] = $"<sprite name=\"{pair.Value.icon}\"><color=#{pair.Value.color}>{pair.Key}</color>";

        if (StatusEffectSystem.Instance != null)
        {
            foreach (var data in StatusEffectSystem.Instance.AllData)
            {
                string hex = ColorUtility.ToHtmlStringRGB(data.Color);
                replacements[data.name] =
                    $"<sprite name=\"{data.name}\" tint=1><color=#{hex}>{data.name}</color>";
            }
        }

        //긴 키워드 우선 매칭, 이미 태그(<...>)인 구간은 그대로 둠
        var keys = new List<string>(replacements.Keys);
        keys.Sort((a, b) => b.Length.CompareTo(a.Length));
        string pattern = "<[^>]*>|(" + string.Join("|", keys.ConvertAll(ToKeywordPattern)) + ")";

        return Regex.Replace(text, pattern, match =>
            match.Groups[1].Success ? replacements[match.Value] : match.Value);
    }

    //영문 키워드는 다른 영단어 안에서 매칭되지 않도록 단어 경계 적용 (예: AP는 MAP에 매칭 X)
    private static string ToKeywordPattern(string keyword)
    {
        string escaped = Regex.Escape(keyword);
        return Regex.IsMatch(keyword, "^[A-Za-z]+$") ? $"(?<![A-Za-z]){escaped}(?![A-Za-z])" : escaped;
    }
}
