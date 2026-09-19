using LinqRetrievalLab.Data;
using LinqRetrievalLab.DTOs;
using LinqRetrievalLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinqRetrievalLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CatalogContext _context;

    public CategoriesController(CatalogContext context)
    {
        _context = context;
    }

    // ============================================
    // GET CATEGORY
    // GET: api/categories/1
    // ============================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        try
        {
            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // ADD CATEGORY
    // POST: api/categories
    // ============================================

    [HttpPost]
    public async Task<IActionResult> AddCategory(CategoryDto dto)
    {
        try
        {
            if (dto == null)
            {
                return BadRequest("Category data is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var category = new Category
            {
                Name = dto.Name
            };

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.Id },
                category);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // EDIT CATEGORY
    // PUT: api/categories/1
    // ============================================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> EditCategory(
        int id,
        CategoryDto dto)
    {
        try
        {
            if (dto == null)
            {
                return BadRequest("Category data is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var category = await _context.Categories
                .FindAsync(id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            category.Name = dto.Name;

            await _context.SaveChangesAsync();

            return Ok(category);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // DELETE CATEGORY
    // DELETE: api/categories/1
    // ============================================

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            if (category.Products.Any())
            {
                return BadRequest(
                    "Cannot delete a category that still has products.");
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }
}