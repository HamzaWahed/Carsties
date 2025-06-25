using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer(IMapper mapper) : IConsumer<AuctionUpdated>
{
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("--> Consuming auction updated: " + context.Message.Id);

        var item = await DB.Find<Item>().OneAsync(context.Message.Id);

        mapper.Map(context.Message, item);

        await item.SaveAsync();
    }
}