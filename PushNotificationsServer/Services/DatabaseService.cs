using Microsoft.Extensions.Logging;
using PushNotificationsServer.Models;
using Supabase;

namespace PushNotificationsServer.Services
{
    public interface IDatabaseService 
    {
        public Task<User?> RegisterUserAsync(RegisterUserRequest request);
    }
    public class DatabaseService : IDatabaseService
    {
        private readonly Client _supabaseClient;
        private readonly ILogger<DatabaseService> _logger;
        public DatabaseService(Client supabaseClient, ILogger<DatabaseService> logger)
        {
            _supabaseClient = supabaseClient;
            _logger = logger;
        }
        public async Task<User?> RegisterUserAsync (RegisterUserRequest request)
        {
            try
            {
                // Check if user arleady exists
                // Verificar si el usuario ya existe

                var existingUsers = await _supabaseClient
                    .From<User>()
                    .Where(u => u.FcmToken == request.FcmToken)
                    .Get();
                
                var existingUser = existingUsers.Models.FirstOrDefault();

                if (existingUser != null)
                {
                    _logger.LogInformation($"User with FCM token {request.FcmToken} already exists.");
                    return existingUser;
                }
                else
                {
                    var newUser = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        FcmToken = request.FcmToken
                    };

                    var insertResult = await _supabaseClient
                        .From<User>()
                        .Insert(newUser);

                    var createdUser = insertResult.Models.FirstOrDefault();
                    _logger.LogInformation($"Created new user with FCM token {request.FcmToken}.");
                    return createdUser;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user.");
                return null;
            }
        }
    }
}
