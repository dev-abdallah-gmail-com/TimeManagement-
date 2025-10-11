using TimeManagement.Api.DTOs;

namespace TimeManagement.Api.Services;

public interface ITagService
{
    Task<List<TagDto>> GetAllTagsAsync();
    Task<TagDto?> GetTagByIdAsync(int tagId);
    Task<TagDto> CreateTagAsync(CreateTagDto tagDto);
    Task<TagDto?> UpdateTagAsync(int tagId, CreateTagDto tagDto);
    Task<bool> DeleteTagAsync(int tagId);
}
