namespace AuthService.API.Data
{
    public class UserEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; }
        public string Email { get; set; }   // unique
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "Patient";
    }
}
