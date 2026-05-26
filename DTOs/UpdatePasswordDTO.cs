namespace epico_backend.DTOs
{
    public class UpdatePasswordDTO
    {
        public string ResetPasswordToken { get; set; }
        public string NewPassword { get; set; }
    }
}
