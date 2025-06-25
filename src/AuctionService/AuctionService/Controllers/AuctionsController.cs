using AuctionService.Models;
using AuctionService.Models.Dtos;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[Route("api/auctions")]
[ApiController]
public class AuctionsController(AppDbContext db, IMapper mapper, IPublishEndpoint publishEndpoint) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> Get(string? date)
    {
        var query = db.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.AuctionEnd.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDto>(mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id:Guid}")]
    public async Task<IResult> Get(Guid id)
    {
        var auction = await db.Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return Results.NotFound($"Auction with id {id} does not exist.");
        }

        var auctionDto = mapper.Map<AuctionDto>(auction);
        return TypedResults.Ok(auctionDto);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateAuctionDto createAuctionDto)
    {
        var auction = mapper.Map<Auction>(createAuctionDto);
        //TODO: add the current user as seller
        auction.Seller = "test";

        db.Auctions.Add(auction);
        var auctionDto = mapper.Map<AuctionDto>(auction);
        await publishEndpoint.Publish(mapper.Map<AuctionCreated>(auctionDto));
        var result = await db.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest("Could not save changes to the database.");
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = auction.Id },
            auctionDto
        );
    }

    [HttpPut("{id:Guid}")]
    public async Task<IResult> Put(Guid id, [FromBody] UpdateAuctionDto updateAuctionDto)
    {
        var auction = await db.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return Results.NotFound($"Auction with id {id} does not exist.");
        }

        mapper.Map(updateAuctionDto, auction);
        var result = await db.SaveChangesAsync() > 0;

        return !result ? Results.BadRequest("Update failed or no changes were provided.") : Results.Ok();
    }

    [HttpDelete("{id:Guid}")]
    public async Task<IResult> Delete(Guid id)
    {
        var auction = await db.Auctions.FindAsync(id);
        if (auction == null)
        {
            return Results.NotFound($"Auction with id {id} does not exist.");
        }

        db.Auctions.Remove(auction);
        var result = await db.SaveChangesAsync() > 0;
        return !result ? Results.BadRequest("Could not save changes to the database") : Results.Ok();
    }
}