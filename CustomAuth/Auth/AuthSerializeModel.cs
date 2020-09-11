using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomAuth.Auth
{
    public class AuthSerializeModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string[] Roles { get; set; }
    }
}