using eShopping.NET.Models.Data;
using eShopping.NET.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eShopping.NET.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {

            //Declare list of VM
            List<PageVM> pagesList;
           
            using(dbConnection  db = new dbConnection())
            {
                //Initialise the list

                pagesList = db.Pages.ToArray().OrderBy(p => p.Sorting).Select(x => new PageVM(x)).ToList();
            }
            //Return view with list
            return View(pagesList);
        }

        //GET:Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {

            return View();
        }

        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            using(dbConnection db = new dbConnection())
            {
                //Declare slug
                string slug;

                //Init pageDto
                pageDTO pageDTO = new pageDTO();

                //DtoTitle
                pageDTO.Title = model.Title;

                //Check for and set slug if need be 
                if (String.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }
                //Make sure title and slug are unique
                if(db.Pages.Any(x=> x.Title==model.Title) || db.Pages.Any(c => c.Slug == slug))
                {
                    ModelState.AddModelError("", "The title or slug already exists");
                    return View(model);
                }


                //dto the rest

                pageDTO.Slug = slug;
                pageDTO.Body = model.Body;
                pageDTO.HasSidebar = model.HasSidebar;
                pageDTO.Sorting = 100;

                //save dto
                db.Pages.Add(pageDTO);
                db.SaveChanges();
                
            }
            //Set TempData message
            TempData["successMessage"] = "You have added a new page!";


            //Redirect
            return RedirectToAction("AddPage");
        }

        //GET:Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Declare pageVM
            PageVM model;

            //Get the page
            using(dbConnection db = new dbConnection())
            {
                pageDTO dto = db.Pages.Find(id);

                //Confirm that page exists
                if(dto == null)
                {
                    return Content("Page does not exists");
                }

                //Init PageVM
                model = new PageVM(dto);
            }
            return View(model);
        }

        //Post:Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
                  
            using(dbConnection db= new dbConnection())
            {
                //Get page id
                int id = model.Id;
               //Init slug
                string slug = "home";

                //Get the page
                pageDTO dto = db.Pages.Find(id);

                //DTO the title
                dto.Title = model.Title;

                //Check for slug and set it if need be
                if(model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();

                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();

                    }
                }
                    //Make sure title and slug are unique
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                   db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists");
                    return View(model);
                }

                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //Save DTO
                db.SaveChanges();
             
            }
            //Set TempData message
            TempData["SuccessMessage"] = "You have edited the page";

            //Redirect
            return RedirectToAction("EditPage");

        }

        //Get: Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            //Declare pageVm
            PageVM model;
            
            using(dbConnection db= new dbConnection())
            {
                //Get the page
                pageDTO dto = db.Pages.Find(id);
                //Confirm page exists
                if(dto == null)
                {
                    return Content("Page does not exits");
                }
                //Init pageVm
                model = new PageVM(dto);
            }
         //Return view with model
            return View(model);
        }

        //Get: Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using(dbConnection db= new dbConnection())
            {
                //Get the page
                pageDTO dto = db.Pages.Find(id);
                //Remove the page
                if(dto == null)
                {
                    return Content("Page does not exisrs");
                }

                db.Pages.Remove(dto);
                //Save
                db.SaveChanges();
            }


            //Redirect
            return RedirectToAction("Index");
        }

        //Get: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int [] id)
        {
            using(dbConnection db= new dbConnection())
            {
                //Set initial count
                int count = 1;
                //Declare Pagedto

                pageDTO dto;

                //Set sorting for each page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
            
        }
        //Get: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Declare model
            SidebarVM model;
            using(dbConnection db = new dbConnection())
            {
                //Get dto
                SidebarDTO dto = db.Sidebar.Find(1);
                //Init model
                model = new SidebarVM(dto);
            }

            //Return view with model
            return View(model);
        }

        //Post: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
           
            using(dbConnection db = new dbConnection())
            {
                //Get dto
                SidebarDTO dto = db.Sidebar.Find(1);

                //DTO the body             
                dto.Body = model.Body;

                //Save
                db.SaveChanges();

            }
            //Set TempData message
            TempData["successMessage"] ="You have edited the sidebar";

            //Redirect
            return RedirectToAction("EditSidebar");
        }
    }
}