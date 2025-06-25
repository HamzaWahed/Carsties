using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer() : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("--> Consuming auction deleted: " + context.Message.Id);

        var item = await DB.Find<Item>().OneAsync(context.Message.Id);

        await item.DeleteAsync();
    }
}