using Microsoft.EntityFrameworkCore;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.Cart
{
    public class ShoppingCart
    {
        private readonly AppDbContext _context;
        public string ShoppingCartId { get; set; }
        public ShoppingCart( AppDbContext context)
        {
            _context = context;  
        }
        public static ShoppingCart GetShoppingCart(IServiceProvider services)
        {
            var session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            var context = services.GetService<AppDbContext>();

            string cartId = session.GetString("CartId") ?? Guid.NewGuid().ToString();
            session.SetString("CartId", cartId);

            return new ShoppingCart(context) { ShoppingCartId = cartId };
        }
        public List<ShoppingCartItem> GetShoppingCartItems()
        {
            return _context.ShoppingCartItems.Where(x=>x.ShoppingCartId==ShoppingCartId).Include(x=> x.Product).ToList();
        }

        public decimal GetTotal()
        {
            return _context.ShoppingCartItems.Where(x => x.ShoppingCartId == ShoppingCartId).Select(x => x.Product.Price * x.Quantity).Sum();


        }
        public int GetShoppingCartItemsCount()
        {
            return _context.ShoppingCartItems
                .Where(x => x.ShoppingCartId == ShoppingCartId)
                .Sum(x => x.Quantity);
        }


        public async Task AddItemToCart(Product product)
        {
            var shoppingCartItem =await _context.ShoppingCartItems.FirstOrDefaultAsync(x => x.ShoppingCartId == ShoppingCartId && x.Product.Id == product.Id);

            if(shoppingCartItem == null)
            {
                shoppingCartItem = new ShoppingCartItem()
                {
                    ShoppingCartId = ShoppingCartId,
                    Product = product,
                    Quantity = 1
                };
                await _context.ShoppingCartItems.AddAsync(shoppingCartItem);
             
            }
            else
            {
                shoppingCartItem.Quantity++;
                
            }
            await _context.SaveChangesAsync();

        }

        public async Task RemoveItemFromCart( Product product)
        {
            var shoppingCartItem = await _context.ShoppingCartItems.FirstOrDefaultAsync(x => x.ShoppingCartId == ShoppingCartId && x.Product.Id == product.Id);

            if(shoppingCartItem!= null)
            {
                if (shoppingCartItem.Quantity > 1)
                    shoppingCartItem.Quantity--;
                else
                    _context.ShoppingCartItems.Remove(shoppingCartItem);
            }
            await _context.SaveChangesAsync();

        }

        public async Task DeleteItem(int Id)
        {
            
            var item= await _context.ShoppingCartItems.FirstOrDefaultAsync(x=>x.Id==Id);
            
            if (item != null)
            {
                _context.ShoppingCartItems.Remove(item);
            }
            await _context.SaveChangesAsync();
        }

        public async Task Makecartempty()
        {
            var items = _context.ShoppingCartItems
                .Where(s => s.ShoppingCartId == ShoppingCartId);
            _context.ShoppingCartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
        public int GetItemQuantity(int productId)
        {
            var item = _context.ShoppingCartItems.FirstOrDefault(x => x.Product.Id == productId);
            return item?.Quantity ?? 0;
        }

        public Dictionary<int, int> GetCartQuantities()
        {
            return _context.ShoppingCartItems
                .ToDictionary(x => x.Product.Id, x => x.Quantity);
        }


    }
}
