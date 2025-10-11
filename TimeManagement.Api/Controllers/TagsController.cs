using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManagement.Api.DTOs;
using TimeManagement.Api.Services;

namespace TimeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TagDto>>> GetAllTags()
    {
        var tags = await _tagService.GetAllTagsAsync();
        return Ok(tags);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(int id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);

        if (tag == null)
        {
            return NotFound(new { message = "Tag not found." });
        }

        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagDto tagDto)
    {
        var result = await _tagService.CreateTagAsync(tagDto);
        return CreatedAtAction(nameof(GetTag), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TagDto>> UpdateTag(int id, [FromBody] CreateTagDto tagDto)
    {
        var result = await _tagService.UpdateTagAsync(id, tagDto);

        if (result == null)
        {
            return NotFound(new { message = "Tag not found." });
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(int id)
    {
        var result = await _tagService.DeleteTagAsync(id);

        if (!result)
        {
            return NotFound(new { message = "Tag not found." });
        }

        return NoContent();
    }
}
