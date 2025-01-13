using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class Carousel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
    public class Offer
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

}