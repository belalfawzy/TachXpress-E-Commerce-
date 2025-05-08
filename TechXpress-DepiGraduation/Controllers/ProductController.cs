using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TechXpress_DepiGraduation.Data.Cart;
using TechXpress_DepiGraduation.Data.Services;
using TechXpress_DepiGraduation.Data.StaticMembers;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Controllers;

public class ProductController: Controller
{
    private readonly IProductService productService;
    private readonly ICategoryService _categoryService;
    private readonly ShoppingCart _shoppingCart;
    public ProductController(IProductService _productService,ICategoryService ser,ShoppingCart shoppingCart)
    {
        productService = _productService;
        _categoryService = ser;
        _shoppingCart = shoppingCart;
    }
    
    public async Task<IActionResult> Index(string searchQuery)
    {
        var products = await productService.GetAllAsync();
        if (!string.IsNullOrEmpty(searchQuery))
        {
            searchQuery = searchQuery.ToLower();
            products = products.Where(p => p.Name.ToLower().Contains(searchQuery)||p.Description.ToLower().Contains(searchQuery)).ToList() ;
        }
        var cartItems = _shoppingCart.GetShoppingCartItems();
        var quantities = cartItems.ToDictionary(i => i.Product.Id, i => i.Quantity);
        ViewBag.CartQuantities = quantities;
        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _categoryService.GetAllAsync();
        return View();
    }
    
    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Create([Bind("Name,Description,Price,Image,color,CategoryId")]Product p)
    {

        ModelState.Remove("Category");
        if (ModelState.IsValid)
        {

            await productService.CreateAsync(p);
            return RedirectToAction(nameof(Index));
        }
        return View(p);
    }
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await productService.GetItemByIdAsync(id);
        if (p == null) return View("Item already not found");
        await productService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await productService.GetItemByIdAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var p= await productService.GetItemByIdAsync(id);
        if (p == null) return NotFound("you are in get method");
        return View(p);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Edit([Bind("Id,Name,Description,Price,Image,color,CategoryId")]Product p)
    {
        ModelState.Remove("Category");
        if (ModelState.IsValid)
        {
            await productService.EditAsync(p);
            return RedirectToAction(nameof(Index));
        }
        else
        {
            return NotFound(p);
        }

    }
}