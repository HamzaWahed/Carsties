using AuctionService.Models;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    [Fact]
    public void HasReservedPrice_ReservePriceGtZero_True()
    {
        var auction = new Auction
        {
            Id = Guid.Empty,
            ReservePrice = 10,
        };

        var result = auction.HasReservedPrice();

        Assert.True(result);
    }

    [Fact]
    public void HasReservedPrice_ReservedPriceIsZero_False()
    {
        var auction = new Auction
        {
            Id = Guid.Empty,
        };

        var result = auction.HasReservedPrice();

        Assert.False(result);
    }
}