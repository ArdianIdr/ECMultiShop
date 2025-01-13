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
    public class WishlistsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Wishlists
        public ActionResult Index()
        {
            var wishlists = db.Wishlists.Include(w => w.Product);
            return View(wishlists.ToList());
        }

        [HttpPost]
        public ActionResult AddToWishlist(int productId)
        {
            var userId = User.Identity.GetUserId();

            // shikjojm nese produkti eshte ne wishlist already
            var existingWishlistItem = db.Wishlists.FirstOrDefault(w => w.ProductId == productId && w.UserId == userId);

            if (existingWishlistItem == null)
            {
                var wishlistItem = new Wishlist
                {
                    UserId = userId,
                    ProductId = productId
                };

                try
                {
                    db.Wishlists.Add(wishlistItem);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                  
                    return Json(new { success = false, message = "An error occurred while adding to wishlist." });
                }
            }

          
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult RemoveWishlistItem(int id)
        {
            try
            {
           
                var userId = User.Identity.GetUserId(); 

                // marrim nga databaza produktin dhe useridn
                var wishlistItem = db.Wishlists
                                           .FirstOrDefault(w => w.ProductId == id && w.UserId == userId);

                if (wishlistItem != null)
                {
                    db.Wishlists.Remove(wishlistItem);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Item not found in the wishlist." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


        // GET: Wishlists/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Wishlist wishlist = db.Wishlists.Find(id);
            if (wishlist == null)
            {
                return HttpNotFound();
            }
            return View(wishlist);
        }

        // GET: Wishlists/Create
        public ActionResult Create()
        {
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name");
            return View();
        }

        // POST: Wishlists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "WishlistId,UserId,ProductId")] Wishlist wishlist)
        {
            if (ModelState.IsValid)
            {
                db.Wishlists.Add(wishlist);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", wishlist.ProductId);
            return View(wishlist);
        }

        // GET: Wishlists/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Wishlist wishlist = db.Wishlists.Find(id);
            if (wishlist == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", wishlist.ProductId);
            return View(wishlist);
        }

        // POST: Wishlists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "WishlistId,UserId,ProductId")] Wishlist wishlist)
        {
            if (ModelState.IsValid)
            {
                db.Entry(wishlist).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", wishlist.ProductId);
            return View(wishlist);
        }

        // GET: Wishlists/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Wishlist wishlist = db.Wishlists.Find(id);
            if (wishlist == null)
            {
                return HttpNotFound();
            }
            return View(wishlist);
        }

        // POST: Wishlists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Wishlist wishlist = db.Wishlists.Find(id);
            db.Wishlists.Remove(wishlist);
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
