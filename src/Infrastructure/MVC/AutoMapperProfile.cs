using AutoMapper;
using Models = IdentityServer4.Models;
using Entities = IdentityServer4.EntityFramework.Entities;

namespace safnet.Identity.Api.Infrastructure.MVC
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Models.Client, Entities.Client>();
            CreateMap<Entities.Client, Models.Client>();
        }
    }
}
