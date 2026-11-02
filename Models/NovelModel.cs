using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace NovelsSearchApp.Models;

public class NovelModel
//I will add Properties which deal with summury and Tags and so on to this class.
{
    //title, wirter, abstract(story), genre, keyword, global_point, novelupdated_at
    [JsonPropertyName("allcount")]
    public int AllCount { get; set; }

    [Display(Name = "タイトル")]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [Display(Name ="作者")]
    [JsonPropertyName("writer")]
    public string? Writer { get; set; }

    [Display(Name = "あらすじ")]
    [JsonPropertyName("story")]
    public string? Story { get; set;}

    [Display(Name = "ジャンル")]
    [JsonPropertyName("genre")]
    public int Genre { get; set; }

    [Display(Name = "キーワード(タグ)")]
    [JsonPropertyName("keyword")]
    public string? Keyword { get; set; }

    [Display(Name = "話数")]
    [JsonPropertyName("general_all_no")]
    public int General_all_no { get; set; }

    [Display(Name = "総合評価ポイント")]
    [JsonPropertyName("global_point")]
    public int Global_point { get; set; }

    [Display(Name = "最終更新日")]
    [JsonPropertyName("novelupdated_at")]
    public string? Novelupdated_at { get; set; }

    //Begin Property used to develop website. The below property is not displayed on browser
    [Display(Name = "ncode")]
    [JsonPropertyName("ncode")]
    public string Ncode { get; set; } = default!;
    //End Property used to develop website.
}