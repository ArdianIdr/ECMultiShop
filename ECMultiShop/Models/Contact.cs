using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class Contact
    {
        public int ContactID { get; set; }
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        public string Subject { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } 


    }

    public class MessageViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string MinutesAgo { get; set; } 
    }

}