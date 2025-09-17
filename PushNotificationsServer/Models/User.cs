using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PushNotificationsServer.Models
{
    [Table("user")]
    public class User : BaseModel
    {
        [PrimaryKey("uid")]
        public string Id { get; set; } = string.Empty;
        
        [Column("fcm_token")]
        public string FcmToken { get; set; } = string.Empty;
    }

    public class RegisterUserRequest
    {
        public required string Id { get; set; }
        public required string FcmToken { get; set; }
    }

     public class RegisterUserResponse
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
}
