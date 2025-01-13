using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class Coupon
    {
        public int Id { get; set; } 

        [Required]
        [StringLength(20)]
        public string Code { get; set; } 

        public decimal DiscountPercentage { get; set; } 

        public bool IsActive { get; set; } 

        public DateTime ExpirationDate { get; set; }

        public int? UsageLimit { get; set; } 

        public int? UsageCount { get; set; } 

        public bool IsValid()
        { 
            return IsActive && ExpirationDate > DateTime.Now &&
                   (UsageLimit == null || UsageCount < UsageLimit);
        }
    }

}