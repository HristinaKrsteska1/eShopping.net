using eShopping.NET.Models.Data;
using eShopping.NET.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
                      price += item.Quantity * item.Price;
                }
                model.Quantity = quanity;
                model.Price = price;
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

        public ActionResult AddToCartPartial(int id)
        {
            //Init CartVM list
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            //Init CartVM
            CartVM model = new CartVM();
            using(dbConnection db  = new dbConnection())
            {
                //Get the product
                ProductDTO product = db.Products.Find(id);

                //Check if the product is already in cart
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //If not, add new product
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName=product.Name,
                        Quantity=1,
                        Price=product.Price,
                        Image=product.ImageName
                    });
                }
                else
                {
                    //If it is, increment
                    productInCart.Quantity++;
                }
            }

            //Get total quantity and price and add to model
            int quantity = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                quantity += item.Quantity;
                price += item.Quantity * item.Price;
            }
            model.Quantity = quantity;
            model.Price = price;

            //Save cart back to session
            Session["cart"] = cart;

            // Return partial view with model
            return PartialView(model);
        }

        //Get: Cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            //Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            
            using(dbConnection db = new dbConnection())
            {
                //Get cartVM from the list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                //Increment quantity
                model.Quantity++;
                //Store needed data
                var result = new { quantity = model.Quantity, price = model.Price };

                //Return json with data
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }
        //GET: cart/DecrementProduct
        public JsonResult DecrementProduct(int productId)
        {
            //Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;


            using (dbConnection db = new dbConnection())
            {
                //Get cartVM from the list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                //Decrement quantity
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }
                //Store needed data
                var result = new { quantity = model.Quantity, price = model.Price };

                //Return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //GET: cart/RemoveProduct
        public void RemoveProduct(int productId)
        {
            //Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (dbConnection db = new dbConnection())
            {
                //Get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Remove model from list
                cart.Remove(model);
            }       
        }

        public ActionResult PayPalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            return PartialView(cart);
        }

        //POST:/Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //Get Cart List
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //Get Username
            string username = User.Identity.Name;

            //Declare OrderId
            int orderId = 0;

            using (dbConnection db = new dbConnection())
            {
                //Init OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                //Get User id
                var query = db.Users.FirstOrDefault(x => x.Username == username);
                var userId = query.Id;
                //Add to OrderDTO and save
                orderDTO.UserId = userId;
                orderDTO.DateCreated = DateTime.Now;

                db.Orders.Add(orderDTO);
                db.SaveChanges();
                //Get Inserted id
                 orderId = orderDTO.OrderId;
                //Init OrderDetailsDTO 
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();
                
                //Add to OrderDetailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }
            }
            //Email Admin
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("925d31c2909ed3", "c98a6aff4e9757"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "New Order", "You have a new order. Order number" + orderId);
            //Reset session
            Session["cart"] = null;
        }
    }
}