//I will write the code to get API data which is in Narou developer and display it.
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using NovelsSearchApp.Models;
using NovelsSearchApp.Components.Pages.UserInput;
using System.IO;
using System.IO.Compression;

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

                int fetchLimit = 500;
                int currentSt = loopIndex * fetchLimit + 1;//This `currentSt` is used for `OR Search`

                CheckAPI = ChangeGetParamForAPI.ChangeQueryString(currentSt);

                //`FetchNovelsFromApiAsync` is a method that fetches novels from the API and unzip gzip files. This method returns a list of `NovelModel`.
                List<NovelModel>? novelsList = await FetchNovelsFromApiAsync(httpClient, CheckAPI);
                if(novelsList != null && novelsList.Any())
                {
                    //Get allcount to calculate the number of all pages.
                    var allcount = novelsList.FirstOrDefault();
                    if(allcount != null)
                    {
                        AllCountForPageNumber = allcount.AllCount;//why does this code have no error?
                    }

                    IEnumerable<NovelModel> novelsSkippedAllCount = novelsList.Skip(1);

                    //This is OR Search Logic. 
                    //This is implemented by filtering the search results of HPC to extract novels that match the LPC condition using LINQ.
                    if(!string.IsNullOrWhiteSpace(ChangeGetParamForAPI.NovelParameters.LowPriorityCriteria))
                    {
                        //`FilterNovelsByLowPriorityCriteria` is a method that filters novels based on low priority criteria.
                        novelsSkippedAllCount = FilterNovelsByLowPriorityCriteria(novelsSkippedAllCount, ChangeGetParamForAPI.NovelParameters.LowPriorityCriteria);
                        accumulatedNovels.AddRange(novelsSkippedAllCount);

                        if(accumulatedNovels.Count >= lim)
                        {
                            loopIndex++;
                            break;
                        }
                    }
                    else//This else block is AND Search logic(HPC).
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

    private async Task<List<NovelModel>> FetchNovelsFromApiAsync(HttpClient httpClient ,string checkAPI)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(checkAPI);//Handle the return of `ChaneQueryString`.

        //`EnsureSuccessStatusCode`checks whether the status code returned by API is in 200 range.
        //If the status code is not in 200 range, it immediately throws an `HttpRequestException`.
        response.EnsureSuccessStatusCode();

        //`responseStream` and `decompressedStream` are used to read the response content and decompress it using GZipStream.
        using var responseStream = await response.Content.ReadAsStreamAsync();
        using var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress);

        List<NovelModel>? novelsListAsReturn = await JsonSerializer.DeserializeAsync<List<NovelModel>>(decompressedStream);

        return novelsListAsReturn ?? new List<NovelModel>();
    }

    private List<NovelModel> FilterNovelsByLowPriorityCriteria(IEnumerable<NovelModel> novelsSkippedAllCount, string lowPriorityCriteria)
    {
        List<string> LowPriorityCriteriaList = lowPriorityCriteria
            .Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        return novelsSkippedAllCount.Where(novel =>
            LowPriorityCriteriaList.Any(keyword =>
                (novel.Title != null && novel.Title.Contains(keyword)) ||
                (novel.Story != null && novel.Story.Contains(keyword)) ||
                (novel.Keyword != null && novel.Keyword.Contains(keyword)) ||
                (novel.Writer != null && novel.Writer.Contains(keyword))
            )
        ).ToList();
    }
}