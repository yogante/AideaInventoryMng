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
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQty = p.StockQty,
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
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQty = request.StockQty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

           // 商品を登録対象に追加
            _context.Products.Add(product);

            // データベースに保存
            await _context.SaveChangesAsync();

            // EntityからResponse DTOに変換
            var response = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQty = product.StockQty,
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
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQty = p.StockQty,
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

        // 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest request)
        {
            // IDを指定して既存の商品を検索
            var existingProduct = await _context.Products.FindAsync(id);

            //商品が存在しない場合は404を返す
            if (existingProduct== null)
            {
                return NotFound();
            }


            //DTOからEntityへ値を反映
            existingProduct.Name = request.Name;
            existingProduct.Description = request.Description;
            existingProduct.Price = request.Price;
            existingProduct.StockQty = request.StockQty;

            //更新日時を設定
            existingProduct.UpdatedAt = DateTime.UtcNow;

            //データベースに保存
            await _context.SaveChangesAsync();

            //更新成功(HTTP 204)
            return NoContent();

        }

        // 商品詳細取得
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // IDを指定して商品を検索
            var product = await _context.Products.FindAsync(id);

            // 商品が存在しない場合は404を返す
            if (product == null)
            {
                return NotFound();
            }

            // 削除対象に設定
            _context.Products.Remove(product);
            
            // データベースに保存
            await _context.SaveChangesAsync();

            // 削除成功(HTTP 204)
            return NoContent();

        }


    }
}
