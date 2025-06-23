using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<IResult> SearchItems([FromQuery] SearchParams searchParams)
        {
            var query = DB.PagedSearch<Item, Item>()
                .PageNumber(searchParams.PageNumber)
                .PageSize(searchParams.PageSize);

            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm);
            }

            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make)),
                "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
            };

            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) 
                                                 && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.Now)
            };

            if (!string.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(x => x.Seller == searchParams.Seller);
            }
            
            if (!string.IsNullOrEmpty(searchParams.Winner))
            {
                query.Match(x => x.Winner == searchParams.Winner);
            }
            
            var res = await query.ExecuteAsync();

            return TypedResults.Ok(new
            {
                results = res.Results,
                pageCount = res.PageCount,
                totalCount = res.TotalCount
            });
        }
    }
}