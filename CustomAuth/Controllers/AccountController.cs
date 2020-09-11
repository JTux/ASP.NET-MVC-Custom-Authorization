using CustomAuth.Auth;
using CustomAuth.Data;
using CustomAuth.Data.Entities;
using CustomAuth.Models.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace CustomAuth.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login(string returnUrl = "")
        {
            if (User.Identity.IsAuthenticated)
                return LogOut();

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl = "")
        {
            if (ModelState.IsValid && Membership.ValidateUser(model.Email, model.Password))
            {
                var user = (AuthMembershipUser)Membership.GetUser(model.Email, false);
                if (user != null)
                {
                    var userModel = new AuthSerializeModel()
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Roles = user.Roles.Select(r => r.RoleName).ToArray()
                    };

                    var userData = JsonConvert.SerializeObject(userModel);
                    var authTicket = new FormsAuthenticationTicket(1, model.Email, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData);
                    var encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var cookie = new HttpCookie("AuthCookie", encryptedTicket);
                    Response.Cookies.Add(cookie);
                }

                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            ModelState.AddModelError("", "User or Password is invalid.");
            return View(model);
        }

        [HttpGet]
        public ActionResult Register(string returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = Membership.GetUserNameByEmail(model.Email);
            if (!string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("Invalid Email", "Account already exists.");
                return View(model);
            }

            using (var context = new AppDbContext())
            {
                var user = new User()
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Password = model.Password,
                    ActivationCode = Guid.NewGuid()
                };

                context.Users.Add(user);
                if (context.SaveChanges() != 1)
                {
                    ModelState.AddModelError("Account Creation Error", "Could not create account.");
                    return View(model);
                }

                VerifyEmail(model.Email, user.ActivationCode.ToString());

                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult LogOut()
        {
            Response.Cookies.Add(new HttpCookie("AuthCookie", "")
            {
                Expires = DateTime.Now.AddYears(-1)
            });

            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult ActivateAccount(string id)
        {
            using (var context = new AppDbContext())
            {
                var userAccount = context.Users.FirstOrDefault(u => u.ActivationCode.ToString().Equals(id));
                if (userAccount is null)
                {
                    ViewBag.Message = "Something went wrong.";
                    return View();
                }

                if (userAccount.IsActive)
                {
                    ViewBag.Message = "Account already active";
                    return View();
                }

                userAccount.IsActive = true;
                ViewBag.Message = context.SaveChanges() == 1 ? "Account activated!" : "Could not activate account.";

                return View();
            }
        }

        [NonAction]
        public void VerifyEmail(string email, string activationCode)
        {
            var url = $"/Account/ActivateAccount/{activationCode}";
            var activationLink = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, url);

            var sender = new MailAddress("email", "Account Activation");
            var senderPassword = "password";

            var recipient = new MailAddress(email);

            var subject = "Activate Account";
            var body = $"Click this link to activate your account:<br/><a href='{activationLink}'>Activate Account</a>";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(sender.Address, senderPassword)
            };

            using (var message = new MailMessage(sender, recipient)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }
    }
}