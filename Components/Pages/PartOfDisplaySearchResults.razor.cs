//Those line is used for using directive.
using System.Runtime.CompilerServices;
using System.Security.Principal;
using Microsoft.JSInterop;
using NovelsSearchApp.Components.Pages.UserInput;
using NovelsSearchApp.Models;


namespace NovelsSearchApp.Components.Pages;

public partial class PartOfDisplaySearchResults
{
    private int TheNumberOfAllPages { get; set; } = default!;
    private List<int> allPageNumber = new();
    private List<int> displayPages = new();
    private IEnumerable<NovelModel>? NovelsInChildForDisplaying {
        get
        {
            if(LocalNovelStock == null || !LocalNovelStock.Any()) return null;

            int lim = int.Parse(NovelParametersForPageCount.LimSelect);
            if (!string.IsNullOrWhiteSpace(NovelParametersForPageCount.LowPriorityCriteria))
            {
                return LocalNovelStock.Skip(lim * (NovelParametersForPageCount.CurrentPageNumber - 1)).Take(lim);
            }
            else
            {
                return LocalNovelStock;
            }
        }
     }
    private List<NovelModel>? LocalNovelStock { get; set; }
    private IEnumerable<NovelModel>? _lastNovelsInChild;//This variable is only used to check change, so this variabl has `IEnumerable` type.
    private int _lastAllPages;
    private bool _isFetchingMore = false;
    //private IEnumerable<NovelModel>? _lastDisplayingNovels;// What is this member for?(09/27)
   
    protected override void OnParametersSet()//This method is used to match the number of novels in the first Page with `LimSelect`.
    {
        if(NovelsInChild == null || !NovelsInChild.Any())
        {
            return;
        }

        int lim = int.Parse(NovelParametersForPageCount.LimSelect);

        if (!string.IsNullOrWhiteSpace(NovelParametersForPageCount.LowPriorityCriteria))
        {
            if(_lastNovelsInChild != NovelsInChild)
            {
                if (!_isFetchingMore)
                {   //If section means This API connection is first search.
                    LocalNovelStock = NovelsInChild.ToList();
                    //NovelsInChildForDisplaying = LocalNovelStock.Take(lim);(09/27)
                }
                else
                {   //else section means This API connection is pagination.
                    LocalNovelStock!.AddRange(NovelsInChild);
                }
            }
        } 
        else
        {
            LocalNovelStock = NovelsInChild.ToList();
        }

        _lastNovelsInChild = NovelsInChild;
        base.OnParametersSet();//This `base.onParametersSet` is not neccesary.
    }

    private int CalculateAllPages()
    {
       
        int intlim = int.Parse(NovelParametersForPageCount.LimSelect);
        int totalCount = 0;

        if (!string.IsNullOrWhiteSpace(NovelParametersForPageCount.LowPriorityCriteria))
        {
            totalCount = LocalNovelStock!.Count();
        }
        else
        {
            totalCount = GetAllcountForPageNumber;
        }

        TheNumberOfAllPages = (int)Math.Ceiling((double)totalCount / intlim);
        
        return TheNumberOfAllPages;
    }

    //This is formula to adjust the number of pages. Display only the pages within five pages of the current page.
    private List<int> MakePageList()
    {
        //This is handle to make List for Page.
        int allPages = CalculateAllPages();
        int lim = int.Parse(NovelParametersForPageCount.LimSelect);
        
        int pageLimit = 2000 / lim;

        //This if statement is used in the second time search because the results of the first search remains in `allPageNumber` List.
        if(allPageNumber.Any())
        {
            allPageNumber.Clear();
        }
        
        for(int i=1; i<=allPages&i<=pageLimit; i++)
        {
            allPageNumber.Add(i);
        }
        
        return allPageNumber;
    }
    
    //This process sets the upper and lower limits for the pages to be displayed.
    private List<int> MakeDisplayPages()
    {
        if (displayPages.Any())
        {
            displayPages.Clear();
        }
        
        foreach(var displayPage in MakePageList())
        {
            if(displayPage <= NovelParametersForPageCount.CurrentPageNumber + 5 &&
            displayPage >= NovelParametersForPageCount.CurrentPageNumber - 5)
            {
                displayPages.Add(displayPage);
            }
        }
        
        return displayPages;
    }

    private async Task HandlePagenationAsync(int pageNumber)
    {
        int lim = int.Parse(NovelParametersForPageCount.LimSelect);

        if(!string.IsNullOrWhiteSpace(NovelParametersForPageCount.LowPriorityCriteria))
        {
            if(pageNumber > CalculateAllPages() && OnPagenationParam.HasDelegate)
            {
                var _lastLocalNovelStock = LocalNovelStock!.Count;

                _lastAllPages = CalculateAllPages();
                _isFetchingMore = true;

                await OnPagenationParam.InvokeAsync();

                _isFetchingMore = false;

                if(CalculateAllPages() > _lastAllPages)
                {
                    if(_lastLocalNovelStock >= lim * NovelParametersForPageCount.CurrentPageNumber)
                    {
                        NovelParametersForPageCount.CurrentPageNumber = pageNumber;//This code means displaying page watched by user automatically forward to newxt page.
                    }
                }
            }
            else
            {
                NovelParametersForPageCount.CurrentPageNumber = pageNumber;//This code means displaying page watched by user automatically forward to newxt page. 
            }

            /*NovelsInChildForDisplaying = LocalNovelStock!
            .Skip(lim * (NovelParametersForPageCount.CurrentPageNumber - 1))
            .Take(lim)
            .ToList(); |09/27|*/              
            
        }
        else//This is AND Search pagination.
        {
            NovelParametersForPageCount.CurrentPageNumber = pageNumber;

            if (OnPagenationParam.HasDelegate)
            {
                //NovelParametersForPageCount.TheNumberOfDisplayingTimes += 1;//This code is incorrect because there is increment handle in parent component.
                await OnPagenationParam.InvokeAsync();
                //`await` is needed because delegated method `DisplayResults` has the function to get API, with network communication.
            }
        }

        // Reset the scroll position to the top of the search results after pagination.
        await JSRuntime.InvokeVoidAsync("eval", "document.getElementById('novel-list-top').scrollIntoView()");
    }

    private string GetNovelURL(string ncode)
    {
        var baseUriForNcode = "https://ncode.syosetu.com/";
        var novelURL = baseUriForNcode + $"{ncode.ToLower()}/";

        return novelURL;
    }

}