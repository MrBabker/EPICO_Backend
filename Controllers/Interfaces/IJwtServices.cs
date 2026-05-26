using epico_backend.models.players;

namespace epico_backend.Controllers.Interfaces
{
    public interface IJwtServices
    {
        public string CreateToken(PlayerModel player);
    }
}
