using AuctionService.Data;
using AuctionService.Models;
using AuctionService.Models.Dtos;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;

[Route("api/auctions")]
[ApiController]
public class AuctionsController(IAuctionRepository context, IMapper mapper, IPublishEndpoint publishEndpoint)
    : ControllerBase
{
    /**
     * This method does not explicitly state that the return type is Ok(AuctionDto). You should annotate
     * the return type to be successful return type in the body that the client expects (in this case, AuctionDto).
     * ASP.NET Core implicitly creates a Task<OkObjectResult> in this case.
     *
     * Why?
     * Task<ActionResult<List<AuctionDto>>> tells clients (e.g. OpenAPI spec or frontend app) that the response body
     * contains a List<AuctionDto> type. ASP.NET Core implicitly converts it into a OkObjectResult type, which is simply
     * the data (i.e., List<AuctionDto>) and the 200 status code. This is then converted to an appropriate response type
     * before it leaves the pipeline.
     * For example:
     * HTTP/1.1 200 OK
        Content-Type: application/json; charset=utf-8
        Content-Length: 1234

        [
          {
            "id": "some-guid-1",
            "auctionEnd": "2025-07-10T20:00:00Z",
            "itemName": "Vintage Painting"
          },
          {
            "id": "some-guid-2",
            "auctionEnd": "2025-07-11T18:30:00Z",
            "itemName": "Antique Chair"
          }
        ]
     */
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> Get(string? date)
    {
        return Ok(await context.GetAuctionsAsync(date));
    }

    /**
     * Use IActionResult when the return type of the function can be different types. This is more flexible, and is
     * typically used with [ProducesResponseType] annotation. Use Action<T> to return a concrete type, like the GET
     * method above.
     *
     * example:
     * If you annotate the return type of the function below with Task<OkObjectResult>, it conflicts with the return
     * type when the object is not found, as that has a return type of Task<NotFoundObjectResult>. Therefore,
     * IActionResult is the best choice due to the method having different return types.
     */
    [HttpGet("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AuctionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id)
    {
        var auctionDto = await context.GetAuctionByIdAsync(id);

        if (auctionDto == null)
        {
            return NotFound($"Auction with id {id} does not exist.");
        }

        return Ok(auctionDto);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuctionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post([FromBody] CreateAuctionDto createAuctionDto)
    {
        var auction = mapper.Map<Auction>(createAuctionDto);
        auction.Seller = User.Identity.Name;

        context.AddAuction(auction);

        var auctionDto = mapper.Map<AuctionDto>(auction);

        await publishEndpoint.Publish(mapper.Map<AuctionCreated>(auctionDto));

        var result = await context.SaveChangesAsync();

        if (!result)
        {
            return BadRequest("Could not save changes to the database.");
        }

        /*
         * CreatedAtAction is used to return a 201 response with the third argument as the response body (i.e., auctionDto).
         * The response will include a "Location" field, where this new auction object can be retrieved from.
         * The first parameter is the name of the target action method. The second parameter denotes the route value,
         * so in the case below, this evaluates to the route /api/auctions/{id}.
         */
        return CreatedAtAction(
            nameof(Get), // the nameof method is used to output the name of the method (i.e. GET) as a string (i.e. "GET")
            new { id = auction.Id },
            auctionDto
        );
    }

    [Authorize]
    [HttpPut("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateAuctionDto updateAuctionDto)
    {
        var auction = await context.GetAuctionEntityById(id);

        if (auction == null)
        {
            return NotFound($"Auction with id {id} does not exist.");
        }

        if (auction.Seller != User.Identity.Name)
        {
            return Forbid();
        }

        mapper.Map(updateAuctionDto, auction);

        var updatedAuctionMessage = mapper.Map<AuctionUpdated>(updateAuctionDto);
        updatedAuctionMessage.Id = id.ToString();
        await publishEndpoint.Publish(updatedAuctionMessage);

        var result = await context.SaveChangesAsync();

        return !result ? BadRequest("Update failed or no changes were provided.") : Ok();
    }

    [Authorize]
    [HttpDelete("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var auction = await context.GetAuctionEntityById(id);

        if (auction == null)
        {
            return NotFound($"Auction with id {id} does not exist.");
        }

        if (auction.Seller != User.Identity.Name)
        {
            return Forbid();
        }

        context.RemoveAuction(auction);
        await publishEndpoint.Publish(new AuctionDeleted()
        {
            Id = id.ToString()
        });

        var result = await context.SaveChangesAsync();
        return !result ? BadRequest("Could not save changes to the database") : Ok();
    }
}