//This area is used for using directive.
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Components;
using NovelsSearchApp.Models;

namespace NovelsSearchApp.Components.Pages.UserInput;



public class SearchConditionDict
{
    /*public Dictionary<string, string?> keywordOfSearch = new Dictionary<string, string?>
        {
            {"out", "json"},
            {"of", "t-n-w-s-g-k-ga-gp-nu"}
            //This "of" parameter specify output items. in this case, 
            // "title, wirter, abstract, genre, keyword, global_point, novelupdated_at, and ncode which is " is oututed.
        };*/
    
    public static Dictionary<string, string?> AddToDictionary(SearchConditionModel parameters, int? overrideSt)
    {
       //var dictionaryForAddingKeyword = new SearchConditionDict();
       var keywordOfSearch = new Dictionary<string, string?>
       {
            {"out", "json"},
            {"of", "t-n-w-s-g-k-ga-gp-nu"}
            //This "of" parameter specify output items. in this case, 
            // "title, wirter, abstract, genre, keyword, global_point, novelupdated_at, and ncode which is " is oututed.
       };

        //This is the handle which specifies output of novels. `word` is specifies element of novels to be generated. `notword` is specifies element os novels to be not generated.
        if(!string.IsNullOrWhiteSpace(parameters.HighPriorityCriteria))
        {
            keywordOfSearch.Add("word", parameters.HighPriorityCriteria);   
        }

        if(!string.IsNullOrWhiteSpace(parameters.Exceptword))
        {
            keywordOfSearch.Add("notword", parameters.Exceptword);
        }

        if(!string.IsNullOrWhiteSpace(parameters.OrderSelect))
        {
            keywordOfSearch.Add("order", parameters.OrderSelect);
        }

        if(!string.IsNullOrWhiteSpace(parameters.LimSelect))
        {
            if (!string.IsNullOrWhiteSpace(parameters.LowPriorityCriteria))
            {
                keywordOfSearch.Add("lim", 500.ToString());
            }
            else//This magic number "500" is teh limit of the number of novels that can be obtained from narou API.
            {
                keywordOfSearch.Add("lim", parameters.LimSelect);   
            }
        }

        if(!string.IsNullOrWhiteSpace(parameters.LowPriorityCriteria))
        {
            keywordOfSearch.Add("st", overrideSt.ToString());
        }
        else
        {
            if(parameters.CurrentPageNumber != 1)
            {
                var st = (parameters.CurrentPageNumber - 1) * int.Parse(parameters.LimSelect) + 1;
                keywordOfSearch.Add("st", st.ToString());
            }
            
        }
        /*if(overrideSt != 0)
        {
            var st = overrideSt * 500 + 1;
            keywordOfSearch.Add("st", st.ToString());
        }
        else if(!string.IsNullOrWhiteSpace(parameters.LowPriorityCriteria))
        {
            if(parameters.CurrentPageNumber == 1)
            {
                var st = 1;
                keywordOfSearch.Add("st", st.ToString());
            }
            else
            {
                var st = parameters.TheNumberOfDisplayingTimes * 500 + 1;
                keywordOfSearch.Add("st", st.ToString());
            }
        }
        else
        {
            if(parameters.CurrentPageNumber != 1)
            {
                var stForPagenate = (parameters.CurrentPageNumber - 1) * int.Parse(parameters.LimSelect) + 1;
                keywordOfSearch.Add("st", stForPagenate.ToString());
            }
        }*/

        return keywordOfSearch;
    }
}