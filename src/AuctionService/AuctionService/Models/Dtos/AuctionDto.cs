namespace AuctionService.Models.Dtos;

public class AuctionDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime AuctionEnd { get; set; } = DateTime.UtcNow;
    public required string Seller { get; set; }
    public required string Winner { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string Color { get; set; }
    public int Mileage { get; set; }
    public required string ImageUrl { get; set; }
    public required string Status { get; set; }
    public int ReservePrice { get; set; } = 0;
    public int? SoldAmount { get; set; }
    public int? CurrentHighBid { get; set; }
}