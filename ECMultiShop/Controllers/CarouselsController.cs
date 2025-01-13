using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ECMultiShop.Models;
using Microsoft.AspNet.Identity;

namespace ECMultiShop.Controllers
{
    public class CarouselsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        [Authorize(Roles = "Admin")]
        // GET: Carousels
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {

                ViewBag.UserName = user.UserName;
            }
            else
            {
                ViewBag.UserName = "Unknown User";
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            var unreadNotifications = db.Notifications
         .Where(n => !n.IsRead)
         .OrderByDescending(n => n.CreatedAt)
         .Take(10) // 10 njoftimet e fundit
         .ToList();

            // Fetch low stock products
            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive)// njofto vetem kur eshte <=5
                .Select(s => new LowStockProductViewModel
                {
                    ProductName = s.Product.Name,
                    StockQuantity = s.Quantity
                })
                .OrderBy(s => s.StockQuantity)
                .ToList();

            ViewBag.UnreadNotifications = unreadNotifications; 
            ViewBag.LowStockProducts = lowStockProducts;

            return View(db.Carousels.ToList());
        }
        [Authorize(Roles = "Admin")]
        // GET: Carousels/Details/5
        public ActionResult Details(int? id)
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {
               
                ViewBag.UserName = user.UserName; 
            }
            else
            {
                ViewBag.UserName = "Unknown User";
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
          
            var unreadNotifications = db.Notifications
         .Where(n => !n.IsRead)
         .OrderByDescending(n => n.CreatedAt)
         .Take(10) // 10 njoftimet e fundit
         .ToList();


            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive) // njofto vetem kur eshte <=5
                .Select(s => new LowStockProductViewModel
                {
                    ProductName = s.Product.Name,
                    StockQuantity = s.Quantity
                })
                .OrderBy(s => s.StockQuantity)
                .ToList();

            ViewBag.UnreadNotifications = unreadNotifications;
            ViewBag.LowStockProducts = lowStockProducts; 

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Carousel carousel = db.Carousels.Find(id);
            if (carousel == null)
            {
                return HttpNotFound();
            }
            return View(carousel);
        }
        [Authorize(Roles = "Admin")]
        // GET: Carousels/Create
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {
               
                ViewBag.UserName = user.UserName;
            }
            else
            {
                ViewBag.UserName = "Unknown User";
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            var unreadNotifications = db.Notifications
         .Where(n => !n.IsRead)
         .OrderByDescending(n => n.CreatedAt)
         .Take(10) // 10 njoftimet e fundit
         .ToList();

            // Fetch low stock products
            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive)
                .Select(s => new LowStockProductViewModel
                {
                    ProductName = s.Product.Name,
                    StockQuantity = s.Quantity
                })
                .OrderBy(s => s.StockQuantity)
                .ToList();

            ViewBag.UnreadNotifications = unreadNotifications; 
            ViewBag.LowStockProducts = lowStockProducts; 

            return View();
        }

        // POST: Carousels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,ImageUrl,IsActive")] Carousel carousel, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    // Save foton tek folderi (path) 
                    string fileName = Path.GetFileName(imageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/CarouselImages"), fileName);
                    imageFile.SaveAs(path);
                    carousel.ImageUrl = "/Content/CarouselImages/" + fileName;
                }

                db.Carousels.Add(carousel);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(carousel);
        }

        [Authorize(Roles = "Admin")]
        // GET: Carousels/Edit/5
        public ActionResult Edit(int? id)
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {
               
                ViewBag.UserName = user.UserName; 
            }
            else
            {
                ViewBag.UserName = "Unknown User";
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            var unreadNotifications = db.Notifications
         .Where(n => !n.IsRead)
         .OrderByDescending(n => n.CreatedAt)
         .Take(10) // 10 njoftimet e fundit
         .ToList();

            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive) // njofto vetem kur <=5
                .Select(s => new LowStockProductViewModel
                {
                    ProductName = s.Product.Name,
                    StockQuantity = s.Quantity
                })
                .OrderBy(s => s.StockQuantity)
                .ToList();

            ViewBag.UnreadNotifications = unreadNotifications; 
            ViewBag.LowStockProducts = lowStockProducts; 

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Carousel carousel = db.Carousels.Find(id);
            if (carousel == null)
            {
                return HttpNotFound();
            }
            return View(carousel);
        }

        // POST: Carousels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,ImageUrl,IsActive")] Carousel carousel, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingCarousel = db.Carousels.Find(carousel.Id);
                if (existingCarousel == null)
                {
                    return HttpNotFound();
                }

                // update
                existingCarousel.Title = carousel.Title;
                existingCarousel.Description = carousel.Description;
                existingCarousel.IsActive = carousel.IsActive;

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    // ruajm foton
                    string fileName = Path.GetFileName(imageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/CarouselImages"), fileName);
                    imageFile.SaveAs(path);

                    // efshijme te vjetren
                    if (!string.IsNullOrEmpty(existingCarousel.ImageUrl))
                    {
                        string oldImagePath = Server.MapPath(existingCarousel.ImageUrl);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    existingCarousel.ImageUrl = "/Content/CarouselImages/" + fileName;
                }

                db.Entry(existingCarousel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(carousel);
        }

        [Authorize(Roles = "Admin")]
        // GET: Carousels/Delete/5
        public ActionResult Delete(int? id)
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {
                
                ViewBag.UserName = user.UserName; 
            }
            else
            {
                ViewBag.UserName = "Unknown User";
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            var unreadNotifications = db.Notifications
         .Where(n => !n.IsRead)
         .OrderByDescending(n => n.CreatedAt)
         .Take(10) // 10 njoftimet e fundit
         .ToList();

            // Fetch low stock products
            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive) // njofto vetem kur <=5
                .Select(s => new LowStockProductViewModel
                {
                    ProductName = s.Product.Name,
                    StockQuantity = s.Quantity
                })
                .OrderBy(s => s.StockQuantity)
                .ToList();

            ViewBag.UnreadNotifications = unreadNotifications; 
            ViewBag.LowStockProducts = lowStockProducts; 

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Carousel carousel = db.Carousels.Find(id);
            if (carousel == null)
            {
                return HttpNotFound();
            }
            return View(carousel);
        }

        // POST: Carousels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Carousel carousel = db.Carousels.Find(id);
            db.Carousels.Remove(carousel);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
