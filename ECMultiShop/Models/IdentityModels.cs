using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ECMultiShop.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<ECMultiShop.Models.Category> Categories { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.Product> Products { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.SubCategory> SubCategories { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.ProductPhoto> ProductPhotoes { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.Stock> Stocks { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // Prevent cascade delete on Product -> SubCategory relationship
            modelBuilder.Entity<Product>()
                .HasRequired(p => p.SubCategory)
                .WithMany(sc => sc.Products)
                .HasForeignKey(p => p.SubCategoryId)
                .WillCascadeOnDelete(false);

            // Prevent cascade delete on SubCategory -> Category relationship
            modelBuilder.Entity<SubCategory>()
                .HasRequired(sc => sc.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(sc => sc.CategoryId)
                .WillCascadeOnDelete(false);

            // Configure the StockItem foreign key with no cascade delete to avoid cycles
            modelBuilder.Entity<CartItem>()
                .HasRequired(c => c.StockItem)
                .WithMany()
                .HasForeignKey(c => c.StockItemId)
                .WillCascadeOnDelete(false); // Prevent cascade delete on StockItem


        }


    

        public System.Data.Entity.DbSet<ECMultiShop.Models.Contact> Contacts { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.Team> Teams { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.Wishlist> Wishlists { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.Review> Reviews { get; set; }

        public System.Data.Entity.DbSet<BillingAddress> BillingAddresses { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.ShoppingCart> ShoppingCarts { get; set; }

        public System.Data.Entity.DbSet<CartItem> CartItems { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.Coupon> Coupons { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.Order> Orders { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.OrderItem> OrderItems { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.ToDoTask> ToDoTasks { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.Carousel> Carousels { get; set; }

        public System.Data.Entity.DbSet<ECMultiShop.Models.Offer> Offers { get; set; }
    }
}