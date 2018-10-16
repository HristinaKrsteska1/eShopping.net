using eShopping.NET.Models.Data;
using eShopping.NET.Models.ViewModels.Account;
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
    }
}