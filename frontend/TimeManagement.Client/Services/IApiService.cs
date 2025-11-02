// Interface for frontend API service that wraps backend endpoints
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimeManagement.Client.Services
{
    public interface IApiService
    {
        // Auth
        Task<AuthResponse?> RegisterAsync(RegisterModel model);
        Task<AuthResponse?> RegisterAdminAsync(RegisterModel model);
        Task<AuthResponse?> LoginAsync(LoginModel model);

        // Tasks
        Task<TaskResponseDto?> CreateTaskAsync(CreateTaskModel model);
        Task<TaskResponseDto?> GetTaskAsync(int id);
        Task<List<TaskResponseDto>> GetAllTasksAsync();
        Task<List<TaskResponseDto>> GetMyTasksAsync();
        Task<List<TaskResponseDto>> GetAssignedTasksAsync();
        Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskModel model);
        Task<bool> DeleteTaskAsync(int id);
        Task<TaskResponseDto?> AssignTaskAsync(int id, AssignTaskModel model);
        Task<TaskResponseDto?> AcceptRejectTaskAsync(int id, AcceptRejectModel model);
        Task<TaskResponseDto?> UpdateTaskStatusAsync(int id, TaskStatus status);
        Task<TaskResponseDto?> CompleteTaskAsync(int id, CompleteTaskModel model);
        Task<TaskResponseDto?> ApproveRejectTaskAsync(int id, ApproveRejectModel model);
        Task<List<TaskHistoryDto>> GetTaskHistoryAsync(int id);

        // Tags
        Task<List<TagDto>> GetAllTagsAsync();
        Task<TagDto?> GetTagAsync(int id);
        Task<TagDto?> CreateTagAsync(CreateTagModel model);
        Task<TagDto?> UpdateTagAsync(int id, CreateTagModel model);
        Task<bool> DeleteTagAsync(int id);

        // Admin - user management
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserAsync(string userId);
        Task<bool> DeleteUserAsync(string userId);

        // API root
        Task<string?> GetApiRootAsync();
    }
}
