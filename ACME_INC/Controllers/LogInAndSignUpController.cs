using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ACME_INC.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Newtonsoft.Json;
using Firebase.Auth;
using Firebase.Database;
using Microsoft.AspNetCore.Authorization;
using Firebase.Database.Query;

namespace ACME_INC.Controllers
{
    public class LogInAndSignUpController : Controller
    {
        //declaring firebase refrences
        private readonly FirebaseClient firebaseClient;
        private readonly FirebaseAuthProvider auth;

        public LogInAndSignUpController()
        {
            //setting refrences to connect to the database
            firebaseClient = new FirebaseClient("https://acme-inc-3ee35.firebaseio.com/");
            auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyDHkoYBp0M6ust1A-X1T8FYp4zrPdjHB3U"));
        }

        //IActionResult that will be used for the log in of a user
        //-------------------------------------------------------------------------
        public IActionResult Login()
        {
            return View();
        }
        //-------------------------------------------------------------------------

        //IActionResult (HttpPost) that will be used for the log in of a user to check if the user is in the database
        //-------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> login(string tbxUserEmail, string tbxUserPassword)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    CookieOptions cookieOptions = new CookieOptions();
                    cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddDays(7));

                    var user = await auth.SignInWithEmailAndPasswordAsync(tbxUserEmail, tbxUserPassword);

                    //Retrieve data from Firebase
                    var dbLogins = await firebaseClient
                      .Child("Users")
                      .OnceAsync<Models.User>();

                    string userRole = "";

                    //Convert JSON data to original datatype
                    foreach (var login in dbLogins)
                    {
                        if (login.Object.UserEmail.Equals(tbxUserEmail))
                        {
                            userRole = login.Object.UserRole;
                        }
                    }

                    HttpContext.Response.Cookies.Append("UserToken", user.User.Email, cookieOptions);
                    HttpContext.Response.Cookies.Append("UserRole", userRole, cookieOptions);

                    
                    if (userRole == "customer")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Stats", "Admin");
                    }
                }
                catch (Exception)
                {
                    ViewBag.Error = "Username or Password is Incorrect";
                    return View();
                }  
            }
            return View();
        }
        //-------------------------------------------------------------------------

        //Sign out of the application
        //-------------------------------------------------------------------------
        public IActionResult SignOut()
        {
            HttpContext.Response.Cookies.Delete("UserToken");
            HttpContext.Response.Cookies.Delete("UserRole");
            HttpContext.Response.Cookies.Delete("Cart");
            return RedirectToAction("Index", "Home");
        }
        //-------------------------------------------------------------------------
    }
}