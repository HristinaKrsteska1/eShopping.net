using eShopping.NET.Models.Data;
using eShopping.NET.Models.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eShopping.NET.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/Login");
        }

        // GET: Account/Login 
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

        //GET:/account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

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
            return Redirect("~/account/Login");
        }


    }
}