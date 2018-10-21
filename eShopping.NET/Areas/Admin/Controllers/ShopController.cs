using eShopping.NET.Areas.Admin.Models.ViewModels.Shop;
using eShopping.NET.Models.Data;
using eShopping.NET.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace eShopping.NET.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            //Declare a list of models
            List<CategoriesVM> categoriesVMlist;

            using (dbConnection db = new dbConnection())
            {
                //Init the list
                categoriesVMlist = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoriesVM(x))
                    .ToList();

            }

            //Return view with list
            return View(categoriesVMlist);
        }

        //POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string categoryName)
        {
            //Declare id
            string id;

            using (dbConnection db = new dbConnection())
            {
                //Chech categoryName is unique
                if (db.Categories.Any(x => x.Name == categoryName))
                {
                    return "titletaken";
                }
                //Init dto
                CategoryDTO dto = new CategoryDTO();

                //Add to dto
                dto.Name = categoryName;
                dto.Slug = categoryName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //Save dto
                db.Categories.Add(dto);
                db.SaveChanges();
                //Get the id
                id = dto.Id.ToString();
            }

            //Return id
            return id;
        }
        //Get: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (dbConnection db = new dbConnection())
            {
                //Get the category
                CategoryDTO dto = db.Categories.Find(id);
                //Remove the category
                if (dto == null)
                {
                    return Content("Page does not exisrs");
                }

                db.Categories.Remove(dto);
                //Save
                db.SaveChanges();
            }

            //Redirect
            return RedirectToAction("Categories");
        }

        //GOST: Admin/Shop/RenameCategory/id
        [HttpPost]
        public string RenameCategory(string newCategoryName, int id)
        {
            using (dbConnection db = new dbConnection())
            {
                //Check if category name is unique
                if (db.Categories.Any(x => x.Name == newCategoryName))
                {
                    return "titletaken";
                }

                //Get the DTO
                CategoryDTO dto = db.Categories.Find(id);

                //Edit DTO
                dto.Name = newCategoryName;
                dto.Slug = newCategoryName.Replace(" ", "-").ToLower();

                //Save
                db.SaveChanges();
            }
            //Return

            return "Ok";
        }

        //GET:Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Init model
            ProductVM model = new ProductVM();

            using (dbConnection db = new dbConnection())
            {
                //Add select list of categories to model
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            //Return view with model
            return View(model);

        }
        //POST:Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Check model state
            if (!ModelState.IsValid)
            {
                using (dbConnection db = new dbConnection())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            };

            using (dbConnection db = new dbConnection())
            {
                //Make sure product name is unique
                if (db.Categories.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "The product name already exists");
                    return View(model);
                };

            }
            //Declare product id
            int id;
            using (dbConnection db = new dbConnection())
            {
                //Init and save productDTO
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;
                
                CategoryDTO categoryDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = categoryDTO.Name;
                
                db.Products.Add(product);
                db.SaveChanges();

                //Get inserted id
                id = product.Id;              
            }

            //Set TempData message
            TempData["successMessage"] = "You have added a product!";
            #region Upload Image
            //Create necessary directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString()+ "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString()+ "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
            {
                Directory.CreateDirectory(pathString1);
            }

            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            if (!Directory.Exists(pathString2))
            {
                Directory.CreateDirectory(pathString2);
            }

            if (!Directory.Exists(pathString3))
            {
                Directory.CreateDirectory(pathString3);
            }

            if (!Directory.Exists(pathString4))
            {
                Directory.CreateDirectory(pathString4);
            }

            if (!Directory.Exists(pathString5))
            {
                Directory.CreateDirectory(pathString5);
            }

            //Check if a file was uploaded

            if(file != null && file.ContentLength > 0)
            {
                //Get file extension
                string exists = file.ContentType.ToLower();
                //Verify extension
                if (exists !="image/jpg"
                     &&exists != "image/jpeg"
                     && exists != "image/pjpeg"
                     && exists != "image/gif"
                     && exists != "image/x-png"
                     && exists != "image/png")
                {                 
                    using (dbConnection db = new dbConnection())
                    {
                         model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                         ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                         return View(model);                     
                    }
                }
                //Init image name
                string imageName = file.FileName;
                
                //Save image name to dto
                using(dbConnection db= new dbConnection())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //Set original and thumb image path
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //Save original
                file.SaveAs(path);

                //Create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            #endregion
            //Redirect
            return RedirectToAction("AddProduct");
        }

        //GET:Admin/shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //Declare a list of productVM
            List<ProductVM> listOfProducts;
            //Set page number
            var pageNumber = page ?? 1;
            using(dbConnection db = new dbConnection())
            {
                //Init the list
                listOfProducts = db.Products
                    .ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();
                //Populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Set selected category
                ViewBag.SelectedCat = catId.ToString();                
            }
            //Set pagination
            var onePageOfProducts = listOfProducts.ToPagedList(pageNumber, 5);
            ViewBag.OnePageOfProducts = onePageOfProducts;


            //Return view with list
            return View(listOfProducts);
        }

        //GET:Admin/Shop/EditProduct/id
        public ActionResult EditProduct(int id)
        {
            //Declare productVM
            ProductVM model;

            using(dbConnection db = new dbConnection())
            {
                //Get the product
                ProductDTO dto = db.Products.Find(id);
                //Make sure the product exits
                if(dto == null)
                {
                    return Content("Producr does not exits");
                }

                //Init model
                model = new ProductVM(dto);

                //Make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }
            //Return View with model
            return View(model);
        }

        //POST:Admin/Shop/EditProduct/id
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Get product id
            int id = model.Id;
            using(dbConnection db= new dbConnection())
            {
                //Populate category select list and gallery images
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                   .Select(fn => Path.GetFileName(fn));

            //Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Make sure product name is unique
            using(dbConnection db = new dbConnection())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            //Update product
            using (dbConnection db = new dbConnection())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price; 
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;

                db.SaveChanges();          
            }
            TempData["successMessage"] = "You have edited the product!";
            #region Image Upload
            //Check for file upload
            if (file != null && file.ContentLength > 0)
            {
                //Verify extension                
                string exists = file.ContentType.ToLower();
                //Verify extension
                if (exists != "image/jpg"
                     && exists != "image/jpeg"
                     && exists != "image/pjpeg"
                     && exists != "image/gif"
                     && exists != "image/x-png"
                     && exists != "image/png")
                {
                    using (dbConnection db = new dbConnection())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }

                //Set upload directory paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
               
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
                //Delete files from directories
                DirectoryInfo dir1 = new DirectoryInfo(pathString1);
                DirectoryInfo dir2 = new DirectoryInfo(pathString2);

               
                foreach (FileInfo file2 in dir1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (FileInfo file3 in dir2.GetFiles())
                {
                    file3.Delete();
                }

                //Save image name
                string imageName = file.FileName;
                using(dbConnection db = new dbConnection())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //Save original and thumb images
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

               
                file.SaveAs(path);


                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion
            return RedirectToAction("EditProduct");
        }

        //GET:Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            //Delete product from DB
            using(dbConnection db = new dbConnection())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }
            //Delete product folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
            {
                Directory.Delete(pathString, true);
            }
            //Redirect
            return RedirectToAction("Products");
        }

        //POST: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //loop through files
            foreach (string fileName in Request.Files)
            {
                //init the file
                HttpPostedFileBase file = Request.Files[fileName];

                //Check if it's not null
                if(file != null && file.ContentLength > 0)
                {
                    //Set directory paths
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathStirng1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //Set image paths
                    var path1 = string.Format("{0}\\{1}", pathStirng1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);
                    //Save original and thumb

                    file.SaveAs(path1);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }
            }
        }

        // POST: Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        //GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            //Init list of OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using(dbConnection db = new dbConnection())
            {
                //Init list of OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                //Loop through lits of Orders
                foreach (var order in orders)
                {
                    //Init product dictoinary
                    Dictionary<string, int> productsAndQuantity = new Dictionary<string, int>();

                    //Declare total
                    decimal total = 0m;

                    //Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Get username
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.Username;

                    //Loop through list of OrderDetailsDTO
                    foreach (var item in orderDetailsList)
                    {
                        //Get product
                        ProductDTO product = db.Products.Where(x => x.Id == item.ProductId).FirstOrDefault();

                        //Get product price
                        decimal price = product.Price;

                        //Get product name
                        string productName = product.Name;

                        //Add to product dictionary
                        productsAndQuantity.Add(productName, item.Quantity);

                        //Get total
                        total = item.Quantity * price;
                    }

                    //Add to OrdersForAdmin list
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        Username = username,
                        Total=total,
                        ProductsAndQuantity=productsAndQuantity,
                        DateCreated =order.DateCreated
                    });
                }
            }
            //Return view
            return View(ordersForAdmin);
        }
    }
}