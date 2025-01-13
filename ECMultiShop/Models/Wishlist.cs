using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class Wishlist
    {
        public int WishlistId { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; } 


        public virtual Product Product { get; set; }
    }
}
