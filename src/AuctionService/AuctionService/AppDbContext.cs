using AuctionService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionService;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Auction> Auctions { get; set; }
}