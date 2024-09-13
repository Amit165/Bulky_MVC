using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using BulkyBook.Models.ViewModels;
using System.Configuration.Internal;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment)
        {
            _UnitOfWork = db;
            _webHostEnvironment = webHostEnvironment; //For File upload
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _UnitOfWork.Product.GetAll(includeProperties : "Category").ToList();   //to populate the navigation property using .include.         
            return View(objProductList);
        }

        public IActionResult Upsert(int? id) //Modify Create for Upsert
        {
            //Projections in EF core
            //IEnumerable<SelectListItem> CategoryList = _UnitOfWork.Category
            //    .GetAll().ToList().Select(u => new SelectListItem
            //    {
            //        Text = u.Name,
            //        Value = u.Id.ToString()
            //    })

            //ViewBag.CategoryList = CategoryList;
            // ViewData["CategoryList"] = CategoryList;

            ProductVM productVM = new()
            {
                CategoryList = _UnitOfWork.Category.GetAll().ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()                           
            };
            if(id == null || id == 0)
            {
                //Create
                return View(productVM);
            }
            else
            {
                //Update
                productVM.Product = _UnitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }

        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            
            if (ModelState.IsValid)
            {   //Code to upload File
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        //delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                    }

                    using (var fileStream= new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if (productVM.Product.Id == 0)
                {
                    _UnitOfWork.Product.Add(productVM.Product);
                }
                else 
                {
                    _UnitOfWork.Product.Update(productVM.Product);
                }                
                _UnitOfWork.Save();
                TempData["success"] = "Product Created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _UnitOfWork.Category.GetAll().ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
            
        }


        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? productFromdb = _UnitOfWork.Product.Get(u => u.Id == id);
            //Product? ProductFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id == id);
            //Product? ProductFromDb2 = _db.Categories.Where(u=>u.Id == id).FirstOrDefault();

            if (productFromdb == null)
            {
                return NotFound();
            }
            return View(productFromdb);
        }

        [HttpPost]
        public IActionResult Edit(Product obj)
        {

            if (ModelState.IsValid)
            {
                _UnitOfWork.Product.Update(obj);
                _UnitOfWork.Save();
                TempData["success"] = "Product Edited successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product? productFromdb = _UnitOfWork.Product.Get(u => u.Id == id);
        //    //Product? ProductFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id == id);
        //    //Product? ProductFromDb2 = _db.Categories.Where(u=>u.Id == id).FirstOrDefault();

        //    if (productFromdb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(productFromdb);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int? id)
        //{
        //    Product? obj = _UnitOfWork.Product.Get(u => u.Id == id);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }
        //    _UnitOfWork.Product.Remove(obj);
        //    _UnitOfWork.Save();
        //    TempData["success"] = "Product Deleted successfully";
        //    return RedirectToAction("Index");
        //}

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll() 
        {
            List<Product> objProductList = _UnitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }
        
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _UnitOfWork.Product.Get(u => u.Id == id);
            if(productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, 
                productToBeDeleted.ImageUrl.TrimStart('\\'));

            _UnitOfWork.Product.Remove(productToBeDeleted);
            _UnitOfWork.Save();

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            return Json(new { success = true, message = "Delete successful !" });
        }
        #endregion
    }
}
