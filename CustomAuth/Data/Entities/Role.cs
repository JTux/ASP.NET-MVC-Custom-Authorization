using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomAuth.Data.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public virtual List<User> Users { get; set; } = new List<User>();
    }
}