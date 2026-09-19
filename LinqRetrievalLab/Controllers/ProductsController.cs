using LinqRetrievalLab.Data;
using LinqRetrievalLab.DTOs;
using LinqRetrievalLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinqRetrievalLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly CatalogContext _context;

    public ProductsController(CatalogContext context)
    {
        _context = context;
    }

    // ============================================
    // GET PRODUCT WITH CATEGORY
    // GET: api/products/1
    // ============================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // GET PRODUCTS BY CATEGORY
    // GET: api/products/category/1
    // ============================================

    [HttpGet("category/{categoryId:int}")]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        try
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == categoryId);

            if (!categoryExists)
            {
                return NotFound("Category not found.");
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // SEARCH PRODUCTS BY NAME
    // GET: api/products/search?name=lap
    // ============================================

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string? name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Search string is required.");
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(name))
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // PRODUCTS BY PRICE RANGE
    // GET:
    // api/products/price-range?lower=1000&upper=30000
    // ============================================

    [HttpGet("price-range")]
    public async Task<IActionResult> GetProductsByPriceRange(
        [FromQuery] decimal lower,
        [FromQuery] decimal upper)
    {
        try
        {
            if (lower < 0 || upper < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            if (lower > upper)
            {
                return BadRequest(
                    "Lower price cannot be greater than upper price.");
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p =>
                    p.Price >= lower &&
                    p.Price <= upper)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // TOTAL STOCK
    // GET: api/products/statistics/stock
    // ============================================

    [HttpGet("statistics/stock")]
    public async Task<IActionResult> GetTotalStock()
    {
        try
        {
            var totalStock = await _context.Products
                .Select(p => (int?)p.Stock)
                .SumAsync() ?? 0;

            return Ok(new
            {
                TotalStock = totalStock
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // AVERAGE PRICE
    // GET: api/products/statistics/average-price
    // ============================================

    [HttpGet("statistics/average-price")]
    public async Task<IActionResult> GetAveragePrice()
    {
        try
        {
            var averagePrice = await _context.Products
                .Select(p => (decimal?)p.Price)
                .AverageAsync() ?? 0;

            return Ok(new
            {
                AveragePrice = averagePrice
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // TOTAL NUMBER OF PRODUCTS
    // GET: api/products/statistics/count
    // ============================================

    [HttpGet("statistics/count")]
    public async Task<IActionResult> GetProductCount()
    {
        try
        {
            var count = await _context.Products.CountAsync();

            return Ok(new
            {
                TotalProducts = count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // ADD PRODUCT
    // POST: api/products
    // ============================================

    [HttpPost]
    public async Task<IActionResult> AddProduct(ProductDto dto)
    {
        try
        {
            if (dto == null)
            {
                return BadRequest("Product data is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Product name is required.");
            }

            if (dto.Price < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            if (dto.Stock < 0)
            {
                return BadRequest("Stock cannot be negative.");
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // EDIT PRODUCT
    // PUT: api/products/1
    // ============================================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> EditProduct(
        int id,
        ProductDto dto)
    {
        try
        {
            if (dto == null)
            {
                return BadRequest("Product data is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Product name is required.");
            }

            if (dto.Price < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            if (dto.Stock < 0)
            {
                return BadRequest("Stock cannot be negative.");
            }

            var product = await _context.Products
                .FindAsync(id);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();

            return Ok(product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }

    // ============================================
    // DELETE PRODUCT
    // DELETE: api/products/1
    // ============================================

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var product = await _context.Products
                .FindAsync(id);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Server error: {ex.Message}");
        }
    }
}