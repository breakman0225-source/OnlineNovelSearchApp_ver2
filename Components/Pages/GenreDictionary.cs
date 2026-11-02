namespace NovelsSearchApp.Components.Pages;

public class GenreMapper
{
    public static readonly Dictionary<int, string>GenreDictionary = new ()
    {
        {0, "未選択"},
        {101, "異世界[恋愛]"},
        {102, "現実世界[恋愛]"},
        {201, "ハイファンタジー[ファンタジー]"},
        {202, "ローファンタジー[ファンタジー]"},
        {301, "純文学[文芸]"},
        {302, "ヒューマンドラマ[文芸]"},
        {303, "歴史[文芸]"},
        {304, "推理[文芸]"},
        {305, "ホラー[文芸]"},
        {306, "アクション[文芸]"},
        {307, "コメディー[文芸]"},
        {401, "宇宙[SF]"},
        {403, "空想科学[SF]"},
        {404, "パニック[SF]"},
        {9901, "エッセイ[その他]"},
        {9002, "リプレイ[その他]"},
        {9999, "その他[その他]"}
    };

    public static string GetGenreValue(int genreId)
    {
        if(GenreDictionary.TryGetValue(genreId, out string? genreName))
        {
            return genreName;
        }
        else
        {
            return "不明なジャンル";
        }
    }
}