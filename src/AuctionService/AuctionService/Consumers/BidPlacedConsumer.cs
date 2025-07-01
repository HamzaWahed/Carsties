using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer(AppDbContext dbContext) : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming bid placed");

        var auction = await dbContext.Auctions.FindAsync(context.Message.AuctionId);
        
        if (auction == null) return;

        if (!auction.CurrentHighBid.HasValue || (context.Message.Status.Contains("Accepted") &&
                                                 auction.CurrentHighBid.Value < context.Message.Amount))
        {
            auction.CurrentHighBid = context.Message.Amount;
            await dbContext.SaveChangesAsync();
        }
    }
}