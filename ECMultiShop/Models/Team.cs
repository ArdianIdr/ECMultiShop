using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class Team
    {
        public int TeamId { get; set; }
        public string EmployeeImage{ get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }
}