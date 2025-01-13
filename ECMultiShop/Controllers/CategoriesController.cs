using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ECMultiShop.Models;
using Microsoft.AspNet.Identity;

namespace ECMultiShop.Controllers
{
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Categories
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
                .Where(s => s.Quantity <= 5 && s.IsActive) // njofto vetem <=5
                .Select(s => new LowStockProductViewModel
                {
                    ProductName = s.Product.Name,
                    StockQuantity = s.Quantity
                })
                .OrderBy(s => s.StockQuantity)
                .ToList();

            ViewBag.UnreadNotifications = unreadNotifications; 
            ViewBag.LowStockProducts = lowStockProducts; 

            var categories = db.Categories.Include(c => c.SubCategories).ToList();
            return View(categories);
        }
        [Authorize(Roles = "Admin")]
        // GET: Categories/Details/5
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

            // Fetch low stock products
            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive) // njofto vetem <=5
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
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }
        [Authorize(Roles = "Admin")]
        // GET: Categories/Create
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

            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }
        [Authorize(Roles = "Admin")]
        // GET: Categories/Edit/5
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

            // Fetch low stock products
            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive) // njofto vetem <=5
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
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }
   
        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }
        [Authorize(Roles = "Admin")]
        // GET: Categories/Delete/5
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
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
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
