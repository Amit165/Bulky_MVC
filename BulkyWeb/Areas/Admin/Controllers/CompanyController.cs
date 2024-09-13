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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        public CompanyController(IUnitOfWork db)
        {
            _UnitOfWork = db;            
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _UnitOfWork.Company.GetAll().ToList();  
            return View(objCompanyList);
        }

        public IActionResult Upsert(int? id) //Modify Create for Upsert
        {           

           if(id == null || id == 0)
            {
                //Create
                return View(new Company());
            }
            else
            {
                //Update
                Company companyObj = _UnitOfWork.Company.Get(u => u.Id == id);
                return View(companyObj);
            }

        }

        [HttpPost]
        public IActionResult Upsert(Company CompanyObj)
        {
            
            if (ModelState.IsValid)
            {  

                if (CompanyObj.Id == 0)
                {
                    _UnitOfWork.Company.Add(CompanyObj);
                    _UnitOfWork.Save();
                    TempData["success"] = "Company Created successfully";
                }
                else 
                {
                    _UnitOfWork.Company.Update(CompanyObj);
                    _UnitOfWork.Save();
                    TempData["success"] = "Company Updated successfully";
                }                
                
                return RedirectToAction("Index");
            }
            else
            {
              
                return View(CompanyObj);
            }
            
        }


        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Company? companyFromdb = _UnitOfWork.Company.Get(u => u.Id == id);
            //Company? CompanyFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id == id);
            //Company? CompanyFromDb2 = _db.Categories.Where(u=>u.Id == id).FirstOrDefault();

            if (companyFromdb == null)
            {
                return NotFound();
            }
            return View(companyFromdb);
        }

        [HttpPost]
        public IActionResult Edit(Company obj)
        {

            if (ModelState.IsValid)
            {
                _UnitOfWork.Company.Update(obj);
                _UnitOfWork.Save();
                TempData["success"] = "Company Edited successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

  

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll() 
        {
            List<Company> objCompanyList = _UnitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }
        
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = _UnitOfWork.Company.Get(u => u.Id == id);
            if(companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
           

            _UnitOfWork.Company.Remove(companyToBeDeleted);
            _UnitOfWork.Save();
            

            return Json(new { success = true, message = "Delete successful !" });
        }
        #endregion
    }
}
