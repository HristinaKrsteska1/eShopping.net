using eShopping.NET.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eShopping.NET.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //Init cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            //Check if cart is empty
            if(cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty";
                return View();

            }

            //Calculate total and save to ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }
            ViewBag.GrandTotal = total;
            
            //Return view
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //Init CartVM
            CartVM model = new CartVM();
            //Init quanitity
            int quanity = 0;

            //Init price
            decimal price = 0;

            //Check for cart session
            if(Session["cart"] != null)
            {
                //Get total quanity and price
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                     quanity += item.Quantity;
                      price += item.Price;
                }
            }
            else
            {
                //Or set quanity and price to 0
                model.Quantity = 0;
                model.Price = 0;
            }
            //Return partial view with model
            return PartialView(model);
        }
    }
}