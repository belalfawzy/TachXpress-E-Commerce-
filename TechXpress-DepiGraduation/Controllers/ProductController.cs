using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TechXpress_DepiGraduation.Data.Services;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Controllers;

public class ProductController: Controller
{
    private readonly IProductService productService;
    public ProductController(IProductService _productService)
    {
        productService = _productService;

    }
    
    public async Task<IActionResult> Index()
    {
        // var p = new Product()
        // {
        //     Id = 1,
        //     Name = "Laptop",
        //     Description = "ff fff fff fff fff ff ff ff ff ff ff fff fff ffff ff ffff ffffffff ffff fff f  fff",
        //     Price = 120000,
        //     Image = "kk",
        //     color = { "red", "black", "silver" },
        //     
        // };
        // await productService.CreateAsync(p);
        //
        var products = await productService.GetAllAsync();
    
        return View(products);
    }
    
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([Bind("Name,Description,Price,Image,color,CategoryId")]Product p)
    {
        ModelState.Remove("Categories");
        if (ModelState.IsValid)
        {
            await productService.CreateAsync(p);
            return RedirectToAction(nameof(Index));
        }

        return View(p);
    }

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
    public async Task<IActionResult> Edit(int id)
    {
        var p= await productService.GetItemByIdAsync(id);
        if (p == null) return NotFound();
        return View(p);
    }

    [HttpPost]
    public async Task<IActionResult> Edit([Bind("Name,Description,Price,Image,color,CategoryId")] Product p)
    {

        ModelState.Remove("Categories");

        if (ModelState.IsValid)
        {
            await productService.EditAsync(p);
            return RedirectToAction(nameof(Index));
        }

        return View(p);
    }


    



}