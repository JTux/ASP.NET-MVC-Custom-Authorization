using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomAuth.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public Guid ActivationCode { get; set; }
        public virtual List<Role> Roles { get; set; } = new List<Role>();
    }
}