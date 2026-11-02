//I will write the code to get API data which is in Narou developer and display it.
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using NovelsSearchApp.Models;
using System.Net.Http.Json;
using System.Linq;
using System.Diagnostics;
using NovelsSearchApp.Components.Pages.UserInput;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Immutable;

namespace NovelsSearchApp.Components.Pages;

public partial class NovelsSearch : ComponentBase
{
    [Inject]
    public IHttpClientFactory HttpClientFactory { get; set; } =default!;//what is default! mean?

    public IEnumerable<NovelModel>? novels;

    public bool IsGettingAPISuccess { get; set; }

    public bool IsSearchbuttonpushed { get; set; } = false;

    public string? CheckAPI { get; set; }

    protected SearchCriteria ChangeGetParamForAPI { get; set; } = default!;
    
    public int AllCountForPageNumber { get; set; } = default!;

    public bool IsApiError { get; set; } = false;


    public async Task DisplayResults()
    {
        IsSearchbuttonpushed = true;
        List<NovelModel> accumulatedNovels = new();
        int lim = int.Parse(ChangeGetParamForAPI.NovelParameters.LimSelect);
        int loopIndex = 0;
        if (!string.IsNullOrWhiteSpace(ChangeGetParamForAPI.NovelParameters.LowPriorityCriteria))
        {
            loopIndex = ChangeGetParamForAPI.NovelParameters.TheNumberOfDisplayingTimes;    
        }
        
        //This is handle of getting narou API.
        var httpClient = HttpClientFactory.CreateClient("NarouAPI");

        try
        {
            while(accumulatedNovels.Count < lim && loopIndex <= 3)
            {
                var currentSt = loopIndex * 500 + 1;//This `currentSt` is used for `OR Search`

                //`CheckAPI` is used for check the contents of URL which get the information of novels by query parameters.
                CheckAPI = ChangeGetParamForAPI.ChangeQueryString(currentSt);

                using HttpResponseMessage response = await httpClient.GetAsync(CheckAPI);//Handle the return of `ChaneQueryString`.

                //`EnsureSuccessStatusCode`checks whether the status code returned by API is in 200 range.
                //If the status code is not in 200 range, it immediately throws an `HttpRequestException`.
                response.EnsureSuccessStatusCode();            
                
                //`IsGetteingAPISuccess`is used for debug.
                IsGettingAPISuccess = true;

                using var responseStream = await response.Content.ReadAsStreamAsync();
                IEnumerable<NovelModel>? novelsList = await JsonSerializer.DeserializeAsync<IEnumerable<NovelModel>>(responseStream);
                if(novelsList != null && novelsList.Any())
                {
                    //Get allcount to calculate the number of all pages.
                    var allcount = novelsList.FirstOrDefault();
                    if(allcount != null)
                    {
                        AllCountForPageNumber = allcount.AllCount;//why does this code have no error?
                    }

                    var novelsSkippedAllCount = novelsList.Skip(1);

                    //This is OR Search Logic. 
                    //This is implemented by filtering the search results of HPC to extract novels that match the LPC condition using LINQ.
                    if(!string.IsNullOrWhiteSpace(ChangeGetParamForAPI.NovelParameters.LowPriorityCriteria))
                    {
                        List<string> LowPriorityCriteriaList = ChangeGetParamForAPI.NovelParameters.LowPriorityCriteria
                        .Split(new[] {' ', '　'}, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                        novelsSkippedAllCount = novelsSkippedAllCount.Where(novel =>
                            LowPriorityCriteriaList.Any(keyword => 
                            (novel.Title != null && novel.Title.Contains(keyword)) ||
                            (novel.Story != null && novel.Story.Contains(keyword)) ||
                            (novel.Keyword != null && novel.Keyword.Contains(keyword)) ||
                            (novel.Writer != null && novel.Writer.Contains(keyword))
                            )
                        );
                        
                        accumulatedNovels.AddRange(novelsSkippedAllCount);
                        //accumulatedNovels = novelsSkippedAllCount.ToList();

                        if(accumulatedNovels.Count > lim || loopIndex > 3)
                        {
                            loopIndex++;
                            //ChangeGetParamForAPI.NovelParameters.TheNumberOfDisplayingTimes++;;
                            break;
                        }
                    }
                    else
                    {
                        accumulatedNovels.AddRange(novelsSkippedAllCount);
                        loopIndex++;
                        //ChangeGetParamForAPI.NovelParameters.TheNumberOfDisplayingTimes++;
                        break;
                    }
                }
                else
                {
                    break;
                }
                //ChangeGetParamForAPI.NovelParameters.TheNumberOfDisplayingTimes++;
                loopIndex++;
            }
            //ChangeGetParamForAPI.NovelParameters.TheNumberOfDisplayingTimes = loopIndex;
            ChangeGetParamForAPI.NovelParameters.TheNumberOfDisplayingTimes = loopIndex;
            //`novels` is the List for displaying on UI. First index in `novelsList` is allcount, so this isn't necessary.
            novels = accumulatedNovels;

        }
        catch(HttpRequestException ex)
        {
            Console.WriteLine($"ネットワークエラーが発生しました。{ex.Message}");
            IsApiError = true;
            StateHasChanged();
        }
        catch(TaskCanceledException ex)
        {
            Console.WriteLine($"通信がタイムアウトしました。{ex.Message}");
            IsApiError = true;
            StateHasChanged();
        }
        catch(JsonException ex)
        {
            Console.WriteLine($"JSONのデシリアライズに失敗しました。{ex.Message}");
            IsApiError = true;
            StateHasChanged();
        }
        catch(ArgumentException ex)
        {
            Console.WriteLine($"{ex.Message}");
            IsApiError = true;
            StateHasChanged();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"予期しないエラーが発生しました。{ex.Message}");
            IsApiError = true;
            StateHasChanged();
        }

    }
}