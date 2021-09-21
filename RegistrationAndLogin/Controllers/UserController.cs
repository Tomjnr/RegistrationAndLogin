using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using RegistrationAndLogin.Models;

namespace RegistrationAndLogin.Controllers
{
    public class UserController : Controller
    {
        // Registration action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        // Registration POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode ")]User user)
        {
            bool Status = false;
            string messsage = "";
            //
            //model validation
            if (ModelState.IsValid)
            {
                #region //email exist or not
                var IsExist = IsEmailExist(user.EmailID);
                if (IsExist)
                {
                    ModelState.AddModelError("EmailExist","Email already Exist");
                    return View(user);
                }
                #endregion
                #region generate Activation code 
                user.ActivationCode = Guid.NewGuid();
                #endregion
                #region password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                #endregion
                user.IsEmailVerified = false;
                #region  save data to database
                using(MyDatabaseEntities dc= new MyDatabaseEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();
                }
                //send email to users
                SenderverificationlinkEmail(user.EmailID, user.ActivationCode.ToString());
                messsage = "Registration successfully done.Account Activation Link" +
                    "has been sent to your email id:" + user.EmailID;
                Status = true;
                #endregion
            }
            else
            {
                messsage = "Invalid Request";
            }
            ViewBag.Message = messsage;
            ViewBag.Status = Status;
   
            return View(user);
        }


        // verify Email
        // verify  Email link
        // Login
        // Login POST
        // Logout
        [NonAction]
        public bool IsEmailExist(string EmailID)
        {
            using (MyDatabaseEntities dc = new MyDatabaseEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == EmailID).FirstOrDefault();
                return v != null;
            }
        }
        [NonAction]
        public void SenderverificationlinkEmail(string emailID,string activationcode)
        {
            var veryUrl = "/User/VerifyAccount/" + activationcode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, veryUrl);

            var fromEmail = new MailAddress("murimatommy@gmail.com", "Tom Murima");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "***********";//replace with your actual password
            string subject = "Your account is Successfullt created";

            string body = "<br/><br/>We are excited to tell you that your account is"+
                "successfully created. Please click on the link below to verify your account"+
               " <br/><br/><a href='"+link+"'>"+link+"</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml=true 
            })
                smtp.Send(message);
        }


    }
}