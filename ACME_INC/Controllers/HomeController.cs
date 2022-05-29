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
using Microsoft.AspNetCore.Authorization;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.EntityFrameworkCore.Internal;
using System.Runtime.CompilerServices;

namespace ACME_INC.Controllers
{
    public class HomeController : Controller
    {
        //declaring firebase refrences
        private readonly FirebaseClient firebaseClient;
        private readonly FirebaseAuthProvider auth;

        public HomeController()
        {
            //setting refrences to connect to the database
            firebaseClient = new FirebaseClient("https://acme-inc-3ee35.firebaseio.com/");
            auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyDHkoYBp0M6ust1A-X1T8FYp4zrPdjHB3U"));
        }

        //IactionResult methot to return all the products that is in the store for first customers 
        //and returning customers
        //-------------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var uToken = HttpContext.Request.Cookies["UserToken"];
            var uRole = HttpContext.Request.Cookies["UserRole"];
            
            if (uToken == null)
            {
                //Retrieve data from Firebase
                var productsInFB = await firebaseClient
                  .Child("Products")
                  .OnceAsync<Product>();

                List<Product> products = new List<Product>();

                foreach (var item in productsInFB)
                {
                    products.Add(new Product(item.Object.ProductID, item.Object.ProductName, item.Object.ProductDesc, item.Object.ProductPrice, item.Object.ProductImageURL, item.Object.ProductCategory));
                }

                ViewBag.Products = products;

                return View();
            }
            else
            {
                if (uRole == "customer")
                {
                    //Retrieve data from Firebase
                    var productsInFB = await firebaseClient
                      .Child("Products")
                      .OnceAsync<Product>();

                    List<Product> products = new List<Product>();

                    foreach (var item in productsInFB)
                    {
                        products.Add(new Product(item.Object.ProductID, item.Object.ProductName, item.Object.ProductDesc, item.Object.ProductPrice, item.Object.ProductImageURL, item.Object.ProductCategory));
                    }

                    ViewBag.Products = products;

                    return View();
                }
                else
                {
                    return RedirectToAction("Stats", "Admin");
                }
            }
        }
        //-------------------------------------------------------------------

        //IActionResult method to remove a product from the customer cart
        //-------------------------------------------------------------------
        public IActionResult RemoveFromCart(string id)
        {
            CookieOptions cookieOptions = new CookieOptions();
            cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddDays(7));

            string newCartItems = "";

            var uCart = HttpContext.Request.Cookies["Cart"];

            string[] cartItems = uCart.Split(";");

            foreach (var item in cartItems)
            {
                if (item == id)
                {
                    newCartItems = uCart.ToString().Replace(";" + item , "");
                }
            }

            if (newCartItems == "" || newCartItems == ";" || cartItems.Length == 1)
            {
                HttpContext.Response.Cookies.Delete("Cart");
            }
            else
            {
                HttpContext.Response.Cookies.Append("Cart", newCartItems, cookieOptions);
            }

            return RedirectToAction("ProductInfo", "Home", new { id = id });
        }
        //-------------------------------------------------------------------

        //IActionResult method to add a product to a customer product
        //-------------------------------------------------------------------
        public IActionResult AddToCart(string id)
        {
            CookieOptions cookieOptions = new CookieOptions();
            cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddDays(7));

            string newCartItems = "";

            var uCart = HttpContext.Request.Cookies["Cart"];

            if (uCart == null)
            {
                newCartItems = ";" + id;
                HttpContext.Response.Cookies.Append("Cart", newCartItems, cookieOptions);
            }
            else
            {
                string[] cartItems = uCart.Split(";");

                Boolean check = false;

                foreach (var item in cartItems)
                {
                    if (item == id)
                    {
                        check = true;
                    }
                }

                if (check != true)
                {
                    newCartItems = uCart.ToString() + ";" + id;
                    HttpContext.Response.Cookies.Append("Cart", newCartItems, cookieOptions);
                }
                else
                {
                    check = false;
                }
            }
            return RedirectToAction("ProductInfo", "Home", new { id = id });
        }
        //-------------------------------------------------------------------

        //IActionResult method to to retrive a spesific product that the customer wants to see
        //-------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> SearchResults(string search)
        {
            List<Product> products = new List<Product>();
            if (search != null)
            {
                products.Clear();

                //Retrieve data from Firebase
                var productsInFB = await firebaseClient
                  .Child("Products")
                  .OnceAsync<Product>();

                foreach (var item in productsInFB)
                {
                    if (search.Equals(item.Object.ProductName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        products.Add(new Product(item.Object.ProductID, item.Object.ProductName, item.Object.ProductDesc, item.Object.ProductPrice, item.Object.ProductImageURL, item.Object.ProductCategory));
                    }
                    if (search.Equals(item.Object.ProductCategory, StringComparison.InvariantCultureIgnoreCase))
                    {
                        products.Add(new Product(item.Object.ProductID, item.Object.ProductName, item.Object.ProductDesc, item.Object.ProductPrice, item.Object.ProductImageURL, item.Object.ProductCategory));
                    }
                }
                ViewBag.SearchText = search.ToString();
                ViewBag.SearchProducts = products;
                return View();
            }
            else
            {
                ViewBag.SearchText = " ";
                ViewBag.SearchProducts = products;
                return View();
            } 
        }
        //-------------------------------------------------------------------

        //IActionResult method to to retrive all the products that the user added to their cart
        //-------------------------------------------------------------------
        public async Task<IActionResult> Cart()
        {
            List<Product> products = new List<Product>();

            products.Clear();

            var cart = HttpContext.Request.Cookies["Cart"];

            if (cart != null)
            {
                //Retrieve data from Firebase
                var productsInFB = await firebaseClient
                  .Child("Products")
                  .OnceAsync<Product>();

                string[] cartItems = cart.Split(";");

                foreach (var pItem in productsInFB)
                {
                    foreach (var cItems in cartItems)
                    {
                        if (cItems.Equals(pItem.Object.ProductID))
                        {
                            products.Add(new Product(pItem.Object.ProductID, pItem.Object.ProductName, pItem.Object.ProductDesc, pItem.Object.ProductPrice, pItem.Object.ProductImageURL, pItem.Object.ProductCategory));
                        }
                    }
                }
            }

            ViewBag.ProductsInCart = products;

            return View();
        }
        //-------------------------------------------------------------------

        //IActionResult method to enable the user to pay for the products in their cart
        //-------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Pay()
        {
            string currentUser = "";
            var uCart = HttpContext.Request.Cookies["Cart"];
            var uToken = HttpContext.Request.Cookies["UserToken"];

            //Retrieve data from Firebase
            var usersInFB = await firebaseClient
              .Child("Users")
              .OnceAsync<Models.User>();

            foreach (var item in usersInFB)
            {
                if (uToken.ToString() == item.Object.UserEmail)
                {
                    currentUser = item.Object.UserID;
                }
            }

            Transaction transA = new Transaction("", "", "", "", "");

            var setfirebaseContainer = await firebaseClient
                .Child("Transactions")
                .PostAsync<Transaction>(transA);

            //Retrieve data from Firebase
            var productsInFB = await firebaseClient
              .Child("Products")
              .OnceAsync<Product>();

            double total = 0;

            foreach (var pr in productsInFB)
            {
                foreach (var item in uCart.Split(";"))
                {
                    if (item == pr.Object.ProductID)
                    {
                        total = total + Convert.ToDouble(pr.Object.ProductPrice);
                    }
                }
            }

            foreach (var item in usersInFB)
            {
                if (uToken.ToString() == item.Object.UserEmail)
                {
                    currentUser = item.Object.UserID;
                }
            }

            string transactionID = setfirebaseContainer.Key;

            DateTime dateTime = DateTime.Now;

            transA = new Transaction(transactionID, currentUser, uCart.ToString(), dateTime.ToString(), total.ToString());

            var userTransactionsRefInFirebase = firebaseClient.Child("Transactions");

            await userTransactionsRefInFirebase.Child(transactionID).PutAsync<Transaction>(transA);

            HttpContext.Response.Cookies.Delete("Cart");

            return RedirectToAction("PaymentSuccess", "Home", new { id = transactionID }) ;
        }
        //-------------------------------------------------------------------

        //Display the product that info the customer choose
        //-------------------------------------------------------------------------
        public async Task<IActionResult> ProductInfo(string id)
        {
            //Retrieve data from Firebase
            var productsInFB = await firebaseClient
              .Child("Products")
              .OnceAsync<Product>();

            List<Product> products = new List<Product>();

            foreach (var item in productsInFB)
            {
                if (item.Object.ProductID == id)
                {
                    products.Add(new Product(item.Object.ProductID, item.Object.ProductName, item.Object.ProductDesc, item.Object.ProductPrice, item.Object.ProductImageURL, item.Object.ProductCategory));
                }
            }

            ViewBag.Products = products;

            //redirect to a succes page
            return View();
        }
        //-------------------------------------------------------------------------

        //IActionResult method to retrive all the previous transactions of the custmer that is logged in
        //-------------------------------------------------------------------------
        public async Task<IActionResult> Transactions()
        {
            var uToken = HttpContext.Request.Cookies["UserToken"];

            //Retrieve data from Firebase
            var productsInFB = await firebaseClient
              .Child("Products")
              .OnceAsync<Product>();

            //Retrieve data from Firebase
            var usersInFB = await firebaseClient
              .Child("Users")
              .OnceAsync<Models.User>();

            //Retrieve data from Firebase
            var transactionsInFB = await firebaseClient
              .Child("Transactions")
              .OnceAsync<Transaction>();

            List<TransactionProduct> products = new List<TransactionProduct>();

            List<string[]> userTranactions = new List<string[]>();

            List<string> dates = new List<string>();

            List<string> productdates = new List<string>();

            int i = 0;

            foreach (var user in usersInFB)
            {
                foreach (var transA in transactionsInFB)
                {
                    if (user.Object.UserEmail == uToken.ToString())
                    {
                        if (user.Object.UserID == transA.Object.UserID)
                        {
                            userTranactions.Add(transA.Object.ProductID.Split(";"));
                            dates.Add(transA.Object.TransactionDate);
                            foreach (var item in userTranactions[i])
                            {
                                productdates.Add(transA.Object.TransactionDate + ";" + item + ";" + transA.Object.TransactionID + ";" + transA.Object.TransactionTotal);
                            }
                            i++;
                        }
                    }
                }
            }

            foreach (var pr in productsInFB)
            {
                foreach (var list in productdates)
                {
                    string[] dd = list.Split(";");

                    if (dd[1] == pr.Object.ProductID)
                    {
                        products.Add(new TransactionProduct(pr.Object.ProductID, pr.Object.ProductName, dd[3], dd[0], dd[2]));
                    }
                }
            }

            ViewBag.Dates = dates;
            ViewBag.Transactions = products;

            return View();
        }
        //-------------------------------------------------------------------------

        //Method to return the payment sucsess page
        //-------------------------------------------------------------------------
        public IActionResult PaymentSuccess(string id)
        {
            ViewBag.OderNum = id;
            return View();
        }
        //-------------------------------------------------------------------------

        //IActionResult method to redirect the user back to the home page after the sucsess page
        //-------------------------------------------------------------------------
        [HttpPost]
        public IActionResult PaymentSuccess()
        {
            return RedirectToAction("Index", "Home");
        }
        //-------------------------------------------------------------------------

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}