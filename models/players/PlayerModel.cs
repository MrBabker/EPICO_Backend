namespace epico_backend.models.players
{
    public class PlayerModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? ResetPasswordToken { get; set; }
        public int Points { get; set; }
        public int Level { get; set; }
    }
}
