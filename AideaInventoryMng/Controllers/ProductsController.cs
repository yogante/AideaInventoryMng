using AideaInventoryMng.Data;
using AideaInventoryMng.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

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
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();

            return Ok(products);
        }


        // 商品登録
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            // 登録日時・更新日時を設定
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            // 商品を登録対象に追加
            _context.Products.Add(product);

            // データベースに保存
            await _context.SaveChangesAsync();

            // 登録した商品を返す（HTTP 201）
            return CreatedAtAction(
                nameof(GetProducts),
                new { id = product.Id },
                product
            );
        }

        // 商品詳細取得
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            // IDを指定して商品を検索
            var product = await _context.Products.FindAsync(id);

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
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            // IDを指定して既存の商品を検索
            var existingProduct = await _context.Products.FindAsync(id);

            //商品が存在しない場合は404を返す
            if (existingProduct== null)
            {
                return NotFound();
            }

            //商品情報を更新
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.StockQty = product.StockQty;

            //更新日時を設定
            existingProduct.UpdatedAt = DateTime.UtcNow;

            //データベースに保存
            await _context.SaveChangesAsync();

            //更新成功(HTTP 204)
            return NoContent();

        }

        // 商品詳細取得
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
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

            // 削除成功(HTTP 204)
            return NoContent();

        }
    }
}
