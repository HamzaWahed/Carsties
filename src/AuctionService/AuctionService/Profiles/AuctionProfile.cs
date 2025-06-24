using AuctionService.Models;
using AuctionService.Models.Dtos;
using Contracts;

namespace AuctionService.Profiles;

public class AuctionProfile: AutoMapper.Profile
{
    public AuctionProfile()
    {
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionDto>();
        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(dest => dest.Item, 
                opt => opt.MapFrom(src => src));
        CreateMap<CreateAuctionDto, Item>();
        CreateMap<UpdateAuctionDto, Auction>()
            .ForMember(dest => dest.Item, opts => opts.MapFrom(src => src ));
        CreateMap<UpdateAuctionDto, Item>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<AuctionDto, AuctionCreated>();
    }
}