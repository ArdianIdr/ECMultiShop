using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        public string Rating { get; set; } 
        public DateTime DatePosted { get; set; } = DateTime.Now;

        public int ProductId { get; set; }
        public Product Product { get; set; }
     


    }
}
