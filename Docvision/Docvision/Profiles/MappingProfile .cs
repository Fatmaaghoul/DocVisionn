using AutoMapper;
using Docvision.Dtos;
using Docvision.Models;

namespace Docvision.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapping des requêtes
            CreateMap<RegisterRequest, ApplicationUser>();
            CreateMap<LoginRequest, ApplicationUser>();
            CreateMap<EditProfileRequest, ApplicationUser>();

            // Mapping des réponses
            CreateMap<ApplicationUser, ProfileResponse>();
        }
    }
}