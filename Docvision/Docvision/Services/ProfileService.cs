using AutoMapper;
using Docvision.Dtos;
using Docvision.Models;
using Docvision.Repositories;

namespace Docvision.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IMapper _mapper;

        public ProfileService(IProfileRepository profileRepository, IMapper mapper)
        {
            _profileRepository = profileRepository;
            _mapper = mapper;
        }

        public async Task<ResponseModel> GetProfileAsync(string userId)
        {
            var user = await _profileRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new ResponseModel { Success = false, Message = "Utilisateur non trouvé." };
            }

            var profile = _mapper.Map<ProfileResponse>(user);
            return new ResponseModel { Success = true, Message = "Profil récupéré avec succès.", Data = profile };
        }

        public async Task<ResponseModel> EditProfileAsync(string userId, EditProfileRequest request)
        {
            var user = await _profileRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new ResponseModel { Success = false, Message = "Utilisateur non trouvé." };
            }

            // Vérifier qu'au moins un champ est modifié (nom OU numéro)
            if (request.UserName == null && request.PhoneNumber == null && request.Email == null &&
                (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword)))
            {
                return new ResponseModel { Success = false, Message = "Aucune modification fournie." };
            }

            // Mise à jour des informations de base (uniquement si fournies)
            if (request.UserName != null)
                user.UserName = request.UserName;

            if (request.Email != null)
                user.Email = request.Email;

            if (request.PhoneNumber != null)
                user.PhoneNumber = request.PhoneNumber;

            // Changement de mot de passe (si fourni)
            if (!string.IsNullOrEmpty(request.CurrentPassword) && !string.IsNullOrEmpty(request.NewPassword))
            {
                var passwordCheck = await _profileRepository.CheckPasswordAsync(user, request.CurrentPassword);
                if (!passwordCheck)
                {
                    return new ResponseModel { Success = false, Message = "Mot de passe actuel incorrect." };
                }

                var result = await _profileRepository.ResetPasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    return new ResponseModel { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };
                }
            }

            // Mise à jour de l'utilisateur dans la base de données (seulement si au moins un champ non-password est modifié)
            if (request.UserName != null || request.PhoneNumber != null || request.Email != null)
            {
                var updateResult = await _profileRepository.UpdateUserAsync(user);
                if (!updateResult.Succeeded)
                {
                    return new ResponseModel { Success = false, Message = string.Join(", ", updateResult.Errors.Select(e => e.Description)) };
                }
            }

            return new ResponseModel { Success = true, Message = "Profil mis à jour avec succès." };
        }
    }
}
