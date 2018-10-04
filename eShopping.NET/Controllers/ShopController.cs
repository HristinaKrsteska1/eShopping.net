using eShopping.NET.Models.Data;
using eShopping.NET.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eShopping.NET.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            //Declare list of caregoryVM
            List<CategoriesVM> listOfCategories;
            //Init the list
            using(dbConnection db= new dbConnection())
            {
                listOfCategories = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoriesVM(x)).ToList();
            }
            //Return partial view with the list
            return PartialView(listOfCategories);
        }

        //GET: shop/category/name
        public ActionResult Category(string name)
        {
            //Declare a list of ProductVM
            List<ProductVM> listOfProducts;

            using(dbConnection db = new dbConnection())
            {
                //Get category id
                CategoryDTO categoryDto = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDto.Id;
                //Init the list
                listOfProducts = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                //Get category name
                var procuctCategory = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = procuctCategory.CategoryName;
            }
            //Return view with list
            return View(listOfProducts);
        }

        //GET:shop/product-details/name
        [HttpGet]
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //Declare productVM and productDTO
            ProductVM model;
            ProductDTO dto;
            //Init product id
            int id = 0;

            using(dbConnection db = new dbConnection())
            {
                //Check if product exists
                if(!db.Products.Any(x=> x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                //Init productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //Get inserted ID
                id = dto.Id;

                //Init model
                model = new ProductVM(dto);               
            }

            //Get gallery images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));
            //Return view with model
            return View("ProductDetails", model);
        }
    }
}