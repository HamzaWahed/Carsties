using Contracts;
using MassTransit;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    public Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        throw new NotImplementedException();
    }
}