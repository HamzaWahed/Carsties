using MongoDB.Entities;

namespace SearchService.Models;

// For MongoDB.Entities, you must derive from the Entity class to turn the class into a MongoDB entity with a generated id.
// Therefore, you don't need to add an external id property. 
public class Item : Entity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime AuctionEnd { get; set; }
    public string Seller { get; set; }
    public string Winner { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    public int Mileage { get; set; }
    public string ImageUrl { get; set; }
    public string Status { get; set; }
    public int ReservePrice { get; set; }
    public int? SoldAmount { get; set; }
    public int? CurrentHighBid { get; set; }
}