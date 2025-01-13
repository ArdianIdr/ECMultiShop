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
    public class OffersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Offers
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {
                // e pasojme emailin e userit ne view
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
         .Take(10) // limiti 10
         .ToList();

            // marrim stockin qe eshte <=5 dhe aktiv
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

            return View(db.Offers.ToList());
        }

        // GET: Offers/Details/5
        [Authorize(Roles = "Admin")]
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
         .Take(10) // Limit to 10 recent notifications
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

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offer offer = db.Offers.Find(id);
            if (offer == null)
            {
                return HttpNotFound();
            }
            return View(offer);
        }

        [Authorize(Roles = "Admin")]
        // GET: Offers/Create
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
         .Take(10) 
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

        // POST: Offers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Subtitle,ImageUrl,IsActive")] Offer offer, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    // ruajm foton te folderi ne path (offerimages)
                    string fileName = Path.GetFileName(imageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/OfferImages"), fileName);
                    imageFile.SaveAs(path);
                    offer.ImageUrl = "/Content/OfferImages/" + fileName;
                }

                db.Offers.Add(offer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(offer);
        }

        // GET: Offers/Edit/5
        [Authorize(Roles = "Admin")]
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
         .Take(10) 
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

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offer offer = db.Offers.Find(id);
            if (offer == null)
            {
                return HttpNotFound();
            }
            return View(offer);
        }

        // POST: Offers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Subtitle,ImageUrl,IsActive")] Offer offer, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    // ruajm foton e re dhe updayojm image urln
                    string fileName = Path.GetFileName(imageFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/OfferImages"), fileName);
                    imageFile.SaveAs(path);
                    offer.ImageUrl = "/Content/OfferImages/" + fileName;
                }
                else
                {
                    // nese nuk eshgte insertuar ndonje fotografi tjeter mbajme url e vjeter
                    db.Entry(offer).Property(x => x.ImageUrl).IsModified = false;
                }

                db.Entry(offer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(offer);
        }


        // GET: Offers/Delete/5
        [Authorize(Roles = "Admin")]
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
         .Take(10) 
         .ToList();

      
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

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offer offer = db.Offers.Find(id);
            if (offer == null)
            {
                return HttpNotFound();
            }
            return View(offer);
        }

        // POST: Offers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Offer offer = db.Offers.Find(id);
            db.Offers.Remove(offer);
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
