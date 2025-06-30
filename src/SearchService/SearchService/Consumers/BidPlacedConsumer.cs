using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming bid placed");

        var auction = await DB.Find<Item>().OneAsync(context.Message.Id);

        if (!auction.CurrentHighBid.HasValue || (context.Message.Status.Contains("Accepted") &&
                                                 auction.CurrentHighBid.Value < context.Message.Amount))
        {
            auction.CurrentHighBid = context.Message.Amount;
            await DB.SaveAsync(auction);
        }
    }
}