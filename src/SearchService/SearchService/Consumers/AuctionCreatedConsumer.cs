using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer(IMapper mapper) : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("--> Consuming auction created: " + context.Message.Id);

        var item = mapper.Map<Item>(context.Message);

        if (item.Make == "Foo") throw new ArgumentException("Car with name Foo is not allowed.");

        await DB.SaveAsync(item);
    }
}