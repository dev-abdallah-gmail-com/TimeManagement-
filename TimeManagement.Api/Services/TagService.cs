using Microsoft.EntityFrameworkCore;
using TimeManagement.Api.Data;
using TimeManagement.Api.DTOs;
using TimeManagement.Api.Models;

namespace TimeManagement.Api.Services;

public class TagService : ITagService
{
    private readonly ApplicationDbContext _context;

    public TagService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagDto>> GetAllTagsAsync()
    {
        var tags = await _context.Tags.ToListAsync();
        return tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Color = t.Color
        }).ToList();
    }

    public async Task<TagDto?> GetTagByIdAsync(int tagId)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null)
            return null;

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };
    }

    public async Task<TagDto> CreateTagAsync(CreateTagDto tagDto)
    {
        var tag = new Tag
        {
            Name = tagDto.Name,
            Color = tagDto.Color
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };
    }

    public async Task<TagDto?> UpdateTagAsync(int tagId, CreateTagDto tagDto)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null)
            return null;

        tag.Name = tagDto.Name;
        tag.Color = tagDto.Color;

        await _context.SaveChangesAsync();

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };
    }

    public async Task<bool> DeleteTagAsync(int tagId)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null)
            return false;

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();

        return true;
    }
}
