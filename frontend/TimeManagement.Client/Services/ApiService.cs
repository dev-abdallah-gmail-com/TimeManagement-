using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace TimeManagement.Client.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ApiService> _logger;
        // Use relative tasks API path so HttpClient.BaseAddress is used
        private const string ExternalTasksBase = "api/tasks";
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ApiService(IHttpClientFactory httpFactory, ILogger<ApiService> logger)
        {
            _http = httpFactory.CreateClient("ApiClient");
            _logger = logger;

            // Ensure we request JSON responses
            try
            {
                if (!_http.DefaultRequestHeaders.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/json")))
                {
                    _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
            }
            catch
            {
                // ignore header setup errors
            }
        }

        private void LogRequest(string path)
        {
            try
            {
                var baseAddr = _http.BaseAddress?.ToString() ?? "<no-base-address>";
                var full = path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? path : new Uri(new Uri(baseAddr), path).ToString();
                var hasAuth = _http.DefaultRequestHeaders.Authorization != null;
                _logger.LogDebug("HTTP Request -> {Url} HasAuth={HasAuth}", full, hasAuth);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to compute request URL for logging");
            }
        }

        private async Task<T?> HandleResponse<T>(HttpResponseMessage res)
        {
            var content = string.Empty;
            try
            {
                content = await res.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read response content");
            }

            if (res.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    // No content
                    return default;
                }

                // Detect HTML responses (common when server returns error page) to provide a clearer message
                var trimmed = content.TrimStart();
                if (trimmed.StartsWith("<"))
                {
                    _logger.LogError("Server returned HTML content instead of JSON. Raw content: {Content}", content);
                    throw new HttpRequestException($"Server returned non-JSON response (HTML). This often indicates an error page, CORS issue, or wrong endpoint. Raw content: {content}");
                }

                try
                {
                    var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize response. Raw content: {Content}", content);
                    throw new HttpRequestException($"Failed to parse server response. Ensure the endpoint returns JSON matching the expected model. Raw content: {content}");
                }
            }
            else
            {
                var message = !string.IsNullOrWhiteSpace(content) ? content : $"Request failed: {(int)res.StatusCode} {res.ReasonPhrase}";
                _logger.LogWarning("API request failed: {Message}", message);
                throw new HttpRequestException(message);
            }
        }

        // Auth
        public async Task<AuthResponse?> RegisterAsync(RegisterModel model)
        {
            var res = await _http.PostAsJsonAsync("api/auth/register", model);
            return await HandleResponse<AuthResponse>(res);
        }
        public async Task<AuthResponse?> RegisterAdminAsync(RegisterModel model)
        {
            var res = await _http.PostAsJsonAsync("api/auth/register-admin", model);
            return await HandleResponse<AuthResponse>(res);
        }
        public async Task<AuthResponse?> LoginAsync(LoginModel model)
        {
            var res = await _http.PostAsJsonAsync("api/auth/login", model);
            return await HandleResponse<AuthResponse>(res);
        }

        // Tasks - use relative tasks API path (HttpClient.BaseAddress must point to API server)
        public async Task<TaskResponseDto?> CreateTaskAsync(CreateTaskModel model)
        {
            LogRequest(ExternalTasksBase);
            var res = await _http.PostAsJsonAsync(ExternalTasksBase, model);
            return await HandleResponse<TaskResponseDto>(res);
        }

        public async Task<TaskResponseDto?> GetTaskAsync(int id)
        {
            var path = $"{ExternalTasksBase}/{id}";
            LogRequest(path);
            var res = await _http.GetAsync(path);
            return await HandleResponse<TaskResponseDto>(res);
        }

        public async Task<List<TaskResponseDto>> GetAllTasksAsync()
        {
            var path = $"{ExternalTasksBase}/all";
            LogRequest(path);
            var res = await _http.GetAsync(path);
            return await HandleResponse<List<TaskResponseDto>>(res) ?? new List<TaskResponseDto>();
        }

        public async Task<List<TaskResponseDto>> GetMyTasksAsync()
        {
            var path = $"{ExternalTasksBase}/my-tasks";
            LogRequest(path);
            var res = await _http.GetAsync(path);
            return await HandleResponse<List<TaskResponseDto>>(res) ?? new List<TaskResponseDto>();
        }

        public async Task<List<TaskResponseDto>> GetAssignedTasksAsync()
        {
            var path = $"{ExternalTasksBase}/assigned-to-me";
            LogRequest(path);
            var res = await _http.GetAsync(path);
            return await HandleResponse<List<TaskResponseDto>>(res) ?? new List<TaskResponseDto>();
        }

        public async Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskModel model)
        {
            var path = $"{ExternalTasksBase}/{id}";
            LogRequest(path);
            var res = await _http.PutAsJsonAsync(path, model);
            return await HandleResponse<TaskResponseDto>(res);
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var path = $"{ExternalTasksBase}/{id}";
            LogRequest(path);
            var res = await _http.DeleteAsync(path);
            if (res.IsSuccessStatusCode) return true;
            var content = await res.Content.ReadAsStringAsync();
            _logger.LogWarning("Delete task failed: {Content}", content);
            return false;
        }

        public async Task<TaskResponseDto?> AssignTaskAsync(int id, AssignTaskModel model)
        {
            var path = $"{ExternalTasksBase}/{id}/assign";
            LogRequest(path);
            var res = await _http.PostAsJsonAsync(path, model);
            return await HandleResponse<TaskResponseDto>(res);
        }

        public async Task<TaskResponseDto?> AcceptRejectTaskAsync(int id, AcceptRejectModel model)
        {
            var path = $"{ExternalTasksBase}/{id}/accept-reject";
            LogRequest(path);
            var res = await _http.PostAsJsonAsync(path, model);
            return await HandleResponse<TaskResponseDto>(res);
        }

        public async Task<TaskResponseDto?> UpdateTaskStatusAsync(int id, TaskStatus status)
        {
            var path = $"{ExternalTasksBase}/{id}/status";
            LogRequest(path);
            var res = await _http.PatchAsync(path, JsonContent.Create((int)status));
            return await HandleResponse<TaskResponseDto>(res);
        }

        public async Task<TaskResponseDto?> CompleteTaskAsync(int id, CompleteTaskModel model)
        {
            var path = $"{ExternalTasksBase}/{id}/complete";
            LogRequest(path);
            var res = await _http.PostAsJsonAsync(path, model);
            return await HandleResponse<TaskResponseDto>(res);
        }

        public async Task<TaskResponseDto?> ApproveRejectTaskAsync(int id, ApproveRejectModel model)
        {
            var path = $"{ExternalTasksBase}/{id}/approve-reject";
            LogRequest(path);
            var res = await _http.PostAsJsonAsync(path, model);
            return await HandleResponse<TaskResponseDto>(res);
        }

        public async Task<List<TaskHistoryDto>> GetTaskHistoryAsync(int id)
        {
            var path = $"{ExternalTasksBase}/{id}/history";
            LogRequest(path);
            var res = await _http.GetAsync(path);
            return await HandleResponse<List<TaskHistoryDto>>(res) ?? new List<TaskHistoryDto>();
        }

        // Tags
        public async Task<List<TagDto>> GetAllTagsAsync()
        {
            var res = await _http.GetAsync("api/tags");
            return await HandleResponse<List<TagDto>>(res) ?? new List<TagDto>();
        }

        public async Task<TagDto?> GetTagAsync(int id)
        {
            var res = await _http.GetAsync($"api/tags/{id}");
            return await HandleResponse<TagDto>(res);
        }

        public async Task<TagDto?> CreateTagAsync(CreateTagModel model)
        {
            var res = await _http.PostAsJsonAsync("api/tags", model);
            return await HandleResponse<TagDto>(res);
        }

        public async Task<TagDto?> UpdateTagAsync(int id, CreateTagModel model)
        {
            var res = await _http.PutAsJsonAsync($"api/tags/{id}", model);
            return await HandleResponse<TagDto>(res);
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var res = await _http.DeleteAsync($"api/tags/{id}");
            if (res.IsSuccessStatusCode) return true;
            var content = await res.Content.ReadAsStringAsync();
            _logger.LogWarning("Delete tag failed: {Content}", content);
            return false;
        }

        // Admin - user management
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var res = await _http.GetAsync("api/admin/users");
            return await HandleResponse<List<UserDto>>(res) ?? new List<UserDto>();
        }

        public async Task<UserDto?> GetUserAsync(string userId)
        {
            var res = await _http.GetAsync($"api/admin/users/{userId}");
            return await HandleResponse<UserDto>(res);
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var res = await _http.DeleteAsync($"api/admin/users/{userId}");
            if (res.IsSuccessStatusCode) return true;
            var content = await res.Content.ReadAsStringAsync();
            _logger.LogWarning("Delete user failed: {Content}", content);
            return false;
        }

        // API root
        public async Task<string?> GetApiRootAsync()
        {
            var res = await _http.GetAsync("/");
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadAsStringAsync();
            }
            return null;
        }
    }
}
