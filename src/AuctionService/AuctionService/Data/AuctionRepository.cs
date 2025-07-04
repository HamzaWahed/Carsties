using AuctionService.Models;
using AuctionService.Models.Dtos;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionRepository(AppDbContext context, IMapper mapper) : IAuctionRepository
{
    public async Task<List<AuctionDto>> GetAuctionsAsync(string date)
    {
        var query = context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.AuctionEnd.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        // ReSharper disable once InvalidXmlDocComment
        /**
         * The ProjectTo function is used to optimize mapping when using any package that exposes IQueryable. Here,
         * we use EF Core, which does have an IQueryable method (i.e., AsQueryable). We can then call ProjectTo on it,
         * to map it to our desired object.
         *
         * Why?
         * Suppose you have an a parent class with a nested class inside, and you want to map it to a flattened DTO class.
         * The Dto class has a single field that needs to be mapped from the nested class. If you use Map<T>, then
         * EF Core queries the entire table for both the parent class and nested class. This is clearly a waste, as we
         * simply need a single field from the nested class. ProjectTo uses a SELECT clause to select the single field
         * that is needed for the mapping, optimizing the performance for querying.
         */
        return await query.ProjectTo<AuctionDto>(mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<AuctionDto> GetAuctionByIdAsync(Guid id)
    {
        return await context.Auctions
            .ProjectTo<AuctionDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Auction> GetAuctionEntityById(Guid id)
    {
        return await context.Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void AddAuction(Auction auction)
    {
        context.Auctions.Add(auction);
    }

    public void RemoveAuction(Auction auction)
    {
        context.Auctions.Remove(auction);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}