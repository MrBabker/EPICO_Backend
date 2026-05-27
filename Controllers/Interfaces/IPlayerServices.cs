using epico_backend.DTOs;
using epico_backend.models.players;
using Microsoft.AspNetCore.Mvc;

namespace epico_backend.Controllers.Interfaces
{
    public interface IPlayerServices
    {
        public Task<List<GetPlayerDTO>> GetAllPlayers();
        public Task<List<GetPlayerDTO>> Get10Players();
        public Task<GetPlayerDTO?> GetCurrentUser(int payloadId);
        public Task<PlayerModel> CreatePlayer(CreatePlayerDTOO DTO);
        public  Task<bool> CheckName(string name);
        public Task<bool> CheckEmail(string email);
        public Task<PlayerModel?> Login(LoginDTO DTO);
        public Task<bool> UpdatePassword(UpdatePasswordDTO DTO);
        public Task<PlayerModel?> AddPoints(int playerId, int amount);


    }
}
