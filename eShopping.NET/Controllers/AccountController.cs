using eShopping.NET.Models.Data;
using eShopping.NET.Models.ViewModels.Account;
using eShopping.NET.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace eShopping.NET.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: Account/Login 
        [HttpGet]
        public ActionResult Login()
        {
            //Confirm the user is not logged in

            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("user-profile");
            }
            //Return View
            return View();
        }

        //POST:/account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //Check if model is valid
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //Check if the user is valid
            bool isValid = false;

            using(dbConnection db = new dbConnection())
            {
                if(db.Users.Any(x=>x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (! isValid)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
        }

        //GET:/account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        //POST:/account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //Check Model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }
            //Check if passwords match
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match");
                return View("CreateAccount", model);
            }

            using(dbConnection db = new dbConnection())
            {
                //Make sure username is unique
                if(db.Users.Any(x=> x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "Username already exists");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                //Create UserDTO
                UserDTO userdto = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress= model.EmailAddress,
                    Username=model.Username,
                    Password=model.Password


                };
                //Add the dto
                db.Users.Add(userdto);
                //Save
                db.SaveChanges();
                //Add to UserRolesDTO
                int id = userdto.Id;

                UserRoleDTO userRoledto = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };
                db.UserRoles.Add(userRoledto);
                db.SaveChanges();
            }

            //Create a TempData message
            TempData["successMessage"] = "You are now registered and can login";

            //Redirect
            return Redirect("~/account/login");
        }

        //POST: /Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        public ActionResult UserNavPartial()
        {
            //Get username
            string username = User.Identity.Name;
            //Declare model
            UserNavPartialVM model;

            using(dbConnection db = new dbConnection())
            {
                //Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                //Build the model
                model = new UserNavPartialVM
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            //Return partial view with model
            return PartialView(model);            
        }

        //GET:/account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {
            //Get username
            string username = User.Identity.Name;
            //Declare model
            UserProfileVM model;

            using(dbConnection db = new dbConnection())
            {
                //Get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);
                //Build model
                model = new UserProfileVM(dto);
            }
            //Return view with model
            return View("UserProfile", model);
        }


        [ActionName("user-profile")]
        [HttpPost]
        public ActionResult UserProfile(UserProfileVM model)
        {
            //Check model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            //Check if passwords match
            if (!string.IsNullOrEmpty(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match");
                    return View("UserProfile", model);
                }
            }    

            using(dbConnection db = new dbConnection())
            {
                //Get Username
                string username = User.Identity.Name;
                //Make sure username is unique
                if(db.Users.Where(x=> x.Id != model.Id).Any(x=> x.Username == model.Username))
                {
                    ModelState.AddModelError("", "Username" + model.Username + "already exists.");
                    model.Username = "";
                    return View("UserProfile", model);
                }
                //Edit dto
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    dto.Password = model.Password;
                }
                //Save changes
                db.SaveChanges();
            }
            //Set TempData
            TempData["SuccessMessage"] = "You have edited your profile";

            //Redirect
            return Redirect("~/account/user-profile");
        }

        //GET:/account/Orders
        public ActionResult Orders()
        {
            //Init list of OrderForUserVM
            List<OrderForUserVM> ordersForUser = new List<OrderForUserVM>();

            using(dbConnection db = new dbConnection())
            {
                //Get userId
                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;
                //Init list of OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();
                //Loop through the list of OrderVM
                foreach (var order in orders)
                {
                    //Init product dictionary
                    Dictionary<string, int> productsAndQuantity = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    //Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Loop through the list of OrderDetailsDTO
                    foreach (var item in orderDetailsDTO)
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

                    //Add to OrdersForUserVM list
                    ordersForUser.Add(new OrderForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total =total,
                        ProductsAndQuantity=productsAndQuantity,
                        DateCreated=order.DateCreated
                    });
                }
            }       
            //Return view
            return View(ordersForUser);

        }
    }
}