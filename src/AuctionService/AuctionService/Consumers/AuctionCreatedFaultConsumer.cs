using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Consumers;

public class AuctionCreatedFaultConsumer(AppDbContext db) : IConsumer<Fault<AuctionCreated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        Console.WriteLine("--> Consuming faulty creation");

        var exception = context.Message.Exceptions.First();

        if (exception.ExceptionType == "System.ArgumentException")
        {
            var auction = await db.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == context.Message.Message.Id);

            db.Remove(auction);
            await db.SaveChangesAsync();
        }
    }
}