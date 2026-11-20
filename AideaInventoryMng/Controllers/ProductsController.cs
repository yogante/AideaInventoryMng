using AideaInventoryMng.Data;
using AideaInventoryMng.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AideaInventoryMng.DTO.Products;
namespace AideaInventoryMng.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 商品一覧取得
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
        {
            var products = await _context.Products.Select(
                p => new ProductResponse
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Unit = p.Unit,
                    MinStockQuantity = p.MinStockQuantity,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }
                ).ToListAsync();

            return Ok(products);
        }


        // 商品登録
        [HttpPost]
        public async Task<ActionResult<ProductResponse>> CreateProduct(CreateProductRequest request)
        {
            var now = DateTime.UtcNow;

            var product = new Product
            {
                ProductCode = request.ProductCode,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Unit = request.Unit,
                MinStockQuantity = request.MinStockQuantity,
                CreatedAt = now,
                UpdatedAt = now
            };

           // 商品を登録対象に追加
            _context.Products.Add(product);

            // データベースに保存
            await _context.SaveChangesAsync();

            // EntityからResponse DTOに変換
            var response = new ProductResponse
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Unit = product.Unit,
                MinStockQuantity = product.MinStockQuantity,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            // 登録した商品を返す（HTTP 201）
            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                response
            );
        }

        // 商品詳細取得
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponse>> GetProduct(int id)
        {
            // IDを指定して商品を検索
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductResponse
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Unit = p.Unit,
                    MinStockQuantity = p.MinStockQuantity,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }
                ).FirstOrDefaultAsync();
                
                
            // 商品が存在しない場合は404を返す
            if (product == null)
            {
                return NotFound();
            }

            // 商品が見つかった場合は200を返す
            return Ok(product);
        }

        // 商品更新
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(long id, UpdateProductRequest request)
        {
            // IDを指定して既存の商品を検索
            var existingProduct = await _context.Products.FindAsync(id);

            //商品が存在しない場合は404を返す
            if (existingProduct== null)
            {
                return NotFound();
            }

            // DTOからEntityへ値を反映
            existingProduct.ProductCode = request.ProductCode;
            existingProduct.Name = request.Name;
            existingProduct.Description = request.Description;
            existingProduct.Price = request.Price;
            existingProduct.Unit = request.Unit;
            existingProduct.MinStockQuantity = request.MinStockQuantity;

            //更新日時を設定
            existingProduct.UpdatedAt = DateTime.UtcNow;

            // Versionを更新
            existingProduct.Version++;

            try
            {
                //データベースに保存
                await _context.SaveChangesAsync();

            }
            catch(DbUpdateConcurrencyException)
            {
                return Conflict();
            }

            //更新成功(HTTP 204)
            return NoContent();

        }

        // 商品削除（論理削除）
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // IDを指定して商品を検索
            var product = await _context.Products
                .Where(p => p.Id == id && p.DeletedAt == null)
                .FirstOrDefaultAsync();

            // 商品が存在しない場合は404を返す
            if (product == null)
            {
                return NotFound();
            }

            // 論理削除日時を設定
            var now = DateTime.UtcNow;

            product.DeletedAt = now;
            product.UpdatedAt = now;
            product.Version++;

            try
            {
                // データベースに保存
                await _context.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict();
            }

            // 削除成功(HTTP 204)
            return NoContent();

        }


    }
}
