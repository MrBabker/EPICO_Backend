using epico_backend.Controllers.Interfaces;
using epico_backend.data;
using epico_backend.DTOs;
using epico_backend.models.players;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Numerics;
using System.Text.Json;

namespace epico_backend.Controllers.Services
{
    public class PlayerService : IPlayerServices
    {
        private readonly ILogger<PlayerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _appDbContext;
        private readonly RedisService _redis;
        public PlayerService(ILogger<PlayerService> logger, IConfiguration configuration, AppDbContext appDbContext, RedisService redis)
        {
            _logger = logger;
            _configuration = configuration;
            _appDbContext = appDbContext;
            _redis = redis;
        }

        public async Task<List<GetPlayerDTO>> GetAllPlayers()
        {
            string key = "all_players";

            var cached = await _redis.GetAsync(key);

            if (cached.HasValue)
            {
                return JsonSerializer.Deserialize<List<GetPlayerDTO>>(
       cached.ToString()!
   ) ?? new List<GetPlayerDTO>();
            }

            var players = await _appDbContext.players
                .Select(x => new GetPlayerDTO
                {
                    Id = x.Id,
                    Email  = x.Email,
                    UserName = x.UserName,
                    Level = x.Level,
                    Name = x.Name,
                    Points = x.Points,
                })
                .ToListAsync();

            await _redis.SetAsync(
                key,
                JsonSerializer.Serialize(players),
                TimeSpan.FromSeconds(10)
            );

            return players;
        }
        public async Task<List<GetPlayerDTO>> Get10Players()
        {
            string key = "top10_players";

            var cached = await _redis.GetAsync(key);

            if (cached.HasValue)
            {
                Console.WriteLine("Cached");
                return JsonSerializer.Deserialize<List<GetPlayerDTO>>(cached.ToString())
                       ?? new List<GetPlayerDTO>();
            }
         
            var players = await _appDbContext.players
                .OrderByDescending(p => p.Points)
                .Take(10).Select(x => new GetPlayerDTO
                {
                    Id = x.Id,
                    Email = x.Email,
                    UserName = x.UserName,
                    Level = x.Level,
                    Name = x.Name,
                    Points = x.Points
                })
                .ToListAsync();

            await _redis.SetAsync(
                key,
                JsonSerializer.Serialize(players),
                TimeSpan.FromSeconds(10)
            );

            return players;
        }
        public async Task<GetPlayerDTO?> GetCurrentUser(int payloadId)
        {
           
            var player = await _appDbContext.players.Select(x => new GetPlayerDTO
            {
                Id = x.Id,
                Email = x.Email,
                UserName = x.UserName,
                Level = x.Level,
                Name = x.Name,
                Points = x.Points
            })
                .FirstOrDefaultAsync(x => x.Id == payloadId);

            if (player == null)
            {
                return null;
            }

            return player;
        }
        public async Task<bool> CheckName(string name)
        {
            string userName = name.ToLowerInvariant();

            return await _appDbContext.players
                .AnyAsync(p => p.UserName == userName);
        }
        public async Task<bool> CheckEmail(string email)
        {
            string userEmail = email.ToLowerInvariant();

            return await _appDbContext.players
                .AnyAsync(p => p.Email == userEmail);
        }
        public async Task<PlayerModel> CreatePlayer(CreatePlayerDTOO DTO)
        {
            if (DTO.Name == null || DTO.Email == null || DTO.Password == null) return null;

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(DTO.Password,10);

            PlayerModel player = new PlayerModel
            {
                Name = DTO.Name,
                UserName = DTO.Name.ToLowerInvariant(),
                Email = DTO.Email.ToLowerInvariant(),
                Password = hashedPassword,
                Points = 0,
                Level = 0
            };

            await _appDbContext.players.AddAsync(player);
            await _appDbContext.SaveChangesAsync();

            return player;
        }

        public async Task<PlayerModel?> Login(LoginDTO DTO)
        {
            string value = DTO.NameOrEmail.ToLowerInvariant();

            var player = await _appDbContext.players
                .FirstOrDefaultAsync(p =>
                    p.Email == value ||
                    p.UserName == value);

            if (player == null)
                return null;

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(
                DTO.Password,
                player.Password);

            if (!isPasswordCorrect)
                return null;

            return player;
        }


        public async Task<bool> UpdatePassword(UpdatePasswordDTO DTO)
        {
            if (DTO.ResetPasswordToken.Trim().Length <= 0) return false;

            var player = await _appDbContext.players
                .FirstOrDefaultAsync(p =>
                    p.ResetPasswordToken == DTO.ResetPasswordToken);

            if (player == null)
                return false;

            string hashedPassword = BCrypt.Net.BCrypt
                .HashPassword(DTO.NewPassword);

            player.Password = hashedPassword;

            player.ResetPasswordToken = null;

            await _appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<PlayerModel?> AddPoints(int playerId, int amount)
        {
            var player = await _appDbContext.players
                .FirstOrDefaultAsync(p => p.Id == playerId);

            if (player == null)
                return null;

            player.Points += amount;

            await _appDbContext.SaveChangesAsync();

            return player;
        }
    }
}
