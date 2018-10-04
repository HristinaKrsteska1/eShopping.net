using eShopping.NET.Models.Data;
using eShopping.NET.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eShopping.NET.Controllers
{
    public class PagesController : Controller
    {

        // GET: Index/{page}
        [HttpGet]
        public ActionResult Index(string page="")
        {
            //Get/set page or slug
            if (page == "")
            {
                page = "home";
            }

            //Declare model and dto
            PageVM model;
            pageDTO dto;

            //Check if page exists
            using(dbConnection db = new dbConnection())
            {
                if (!db.Pages.Any(x=> x.Slug.Equals(page)))
                {
                    return RedirectToAction("Index", new { page = ""});
                }
            }

            //Get page dto
            using(dbConnection db = new dbConnection())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            //Set page title
            ViewBag.PageTitle = dto.Title;

            //Check for sidebar
            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";

            }
            //Init model
            model = new PageVM(dto);

            //Return view with model
            return View(model);
        }


        public ActionResult PagesMenuPartial()
        {
            //Declare list of pageVM
            List<PageVM> pageVMlist;

            using(dbConnection db = new dbConnection())
            {
                //Get all pages exept home
                pageVMlist = db.Pages.ToArray().OrderBy(x=>x.Sorting).Where(x => x.Slug != "home").Select(x => new PageVM(x)).ToList();
            }

            //Return partial view with list
            return PartialView(pageVMlist);
        }

        public ActionResult SidebarPartial()
        {
            //Declare model
            SidebarVM model;
            //Init model
            using (dbConnection db = new dbConnection())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                model = new SidebarVM(dto);
            }

            //Return partial view with model
            return PartialView(model);
        }



        // GET: Pages/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Pages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Pages/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Pages/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Pages/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Pages/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Pages/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
