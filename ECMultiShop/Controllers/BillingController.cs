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
    public class BillingController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Billing
        public ActionResult Index()
        {
            return View(db.BillingAddresses.ToList());
        }

        // GET: Billing/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillingAddress billingAddress = db.BillingAddresses.Find(id);
            if (billingAddress == null)
            {
                return HttpNotFound();
            }
            return View(billingAddress);
        }

        // GET: Billing/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Billing/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,Email,MobileNo,AddressLine1,AddressLine2,City,Municipality,Country,ZipCode")] BillingAddress billingAddress)
        {
            if (ModelState.IsValid)
            {
                db.BillingAddresses.Add(billingAddress);
                db.SaveChanges();
                return RedirectToAction("CreateCheckoutSession", "Orders");
            }

            return View(billingAddress);
        }

        [HttpPost]
        public JsonResult SaveBillingAddress(BillingAddress billingAddress)
        {
            if (ModelState.IsValid)
            {
                db.BillingAddresses.Add(billingAddress);
                db.SaveChanges();

             
                var userId = User.Identity.GetUserId();
                var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserId == userId);
                if (cart != null)
                {
                    cart.BillingAddressId = billingAddress.Id;
                    db.SaveChanges();
                }

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Invalid billing address data" });
        }


        // GET: Billing/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillingAddress billingAddress = db.BillingAddresses.Find(id);
            if (billingAddress == null)
            {
                return HttpNotFound();
            }
            return View(billingAddress);
        }

        // POST: Billing/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,Email,MobileNo,AddressLine1,AddressLine2,City,Municipality,Country,ZipCode")] BillingAddress billingAddress)
        {
            if (ModelState.IsValid)
            {
                db.Entry(billingAddress).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(billingAddress);
        }

        // GET: Billing/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillingAddress billingAddress = db.BillingAddresses.Find(id);
            if (billingAddress == null)
            {
                return HttpNotFound();
            }
            return View(billingAddress);
        }

        // POST: Billing/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BillingAddress billingAddress = db.BillingAddresses.Find(id);
            db.BillingAddresses.Remove(billingAddress);
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
