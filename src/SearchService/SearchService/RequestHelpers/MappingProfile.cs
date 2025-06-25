using AutoMapper;
using Contracts;
using SearchService.Models;

namespace SearchService.RequestHelpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AuctionCreated, Item>();
        CreateMap<AuctionUpdated, Item>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}