using AutoMapper;

namespace BasketApi
{
    public class AutoMapperConfig
    {
        public static IMapper InitializeMapper()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<Domain.Basket, Contracts.BasketModel>()
                        .ForMember(x => x.Items, opt => opt.ResolveUsing(src => src.GetItems()));

                cfg.CreateMap<Domain.BasketItem, Contracts.BasketItemModel>();
            });

            var mapper = config.CreateMapper();
            return mapper;
        }
    }
}
