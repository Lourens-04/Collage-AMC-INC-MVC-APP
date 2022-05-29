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
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ACME_INC.Controllers
{
    public class AdminController : Controller
    {
        //declaring firebase refrences
        private readonly FirebaseClient firebaseClient;
        private readonly FirebaseStorage firebaseStorage;
        private readonly FirebaseAuthProvider auth;
        private static List<SelectListItem> categoryList = new List<SelectListItem>();

        public AdminController()
        {
            //setting refrences to connect to the database
            firebaseClient = new FirebaseClient("https://acme-inc-3ee35.firebaseio.com/");
            firebaseStorage = new FirebaseStorage("acme-inc-3ee35.appspot.com");
            auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyDHkoYBp0M6ust1A-X1T8FYp4zrPdjHB3U"));
        }

        //IActionResult that will retrive the statistics for the product sales as well as catagory sales
        //-------------------------------------------------------------------------
        public async Task<IActionResult> Stats()
        {
            int categorySalesNum = 0;
            int productSalesNum = 0;

            //Retrieve data from Firebase
            var productsInFB = await firebaseClient
              .Child("Products")
              .OnceAsync<Product>();

            //Retrieve data from Firebase
            var allTransactionsInFB = await firebaseClient
              .Child("Transactions")
              .OnceAsync<Transaction>();

            //Retrieve data from Firebase
            var allCategories = await firebaseClient
              .Child("Categories")
              .OnceAsync<string>();

            List<Statistics> productsSlales = new List<Statistics>();
            List<Statistics> categorySlales = new List<Statistics>();
            
            foreach (var pr in productsInFB)
            {
                productSalesNum = 0;

                foreach (var list in allTransactionsInFB)
                {
                    foreach (var item in list.Object.ProductID.Split(";"))
                    {
                        if (item == pr.Object.ProductID)
                        {
                            productSalesNum++;
                        }
                    }
                }
                productsSlales.Add(new Statistics(pr.Object.ProductName, productSalesNum));
            }

            foreach (var category in allCategories)
            {
                categorySalesNum = 0;
                foreach (var dd in productsInFB)
                {
                    if (category.Object == dd.Object.ProductCategory)
                    {
                        categorySalesNum++;
                    }
                }
                categorySlales.Add(new Statistics(category.Object, categorySalesNum));
            }

            ViewBag.CategorySales = categorySlales;
            ViewBag.ProductSales = productsSlales;

            //redirect to a succes page
            return View();
        }
        //-------------------------------------------------------------------------

        // GET: Item/Create
        //IActionResult that will retrive all the products that the store has
        //-------------------------------------------------------------------------
        public async Task<IActionResult> StoreContent()
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

            //redirect to a succes page
            return View();
        }

        // GET: Item/Create
        //Get the create product page
        //-------------------------------------------------------------------------
        public async Task<IActionResult> Create()
        {
            categoryList.Clear();

            //Retrieve data from Firebase
            var allCategories = await firebaseClient
              .Child("Categories")
              .OnceAsync<string>();

            int i = 0;
            foreach (var item in allCategories)
            {
                categoryList.Add(new SelectListItem(item.Object, i.ToString()));
                i++;
            }

            ViewBag.CategoryNames = categoryList;

            //return the view to create a new Item
            return View();
        }
        //-------------------------------------------------------------------------

        // POST: Item/Create
        //Save the product that the employee created
        //-------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string productName, string productDesc, string productPrice, string productImageURL, string productCategory, IFormFile image)
        {
            //checks if the model state is good the it will enter the if statement
            if (ModelState.IsValid)
            {
                //FileStream st = new FileStream(Path.Combine)
                ////using that will copy a image into a memory stream to save to the database
                var stream = new MemoryStream();

                //copy the picture into a stream
                await image.CopyToAsync(stream);

                //var aa = new FileStream(@"C:\Users\TheGreenOPK\Downloads\computer.jpg", FileMode.Open);

                var gg = new MemoryStream(stream.GetBuffer());

                var UUID = Guid.NewGuid();

                // Construct FirebaseStorage with path to where you want to upload the file and put it there
                var task = new FirebaseStorage("acme-inc-3ee35.appspot.com")
                 .Child("Product-Images")
                 .Child(image.FileName + "-" + UUID)
                 .PutAsync(gg);

                //Track progress of the upload
                task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                //Await the task to wait until upload is completed and get the download url
                var downloadUrl = await task;

                int i = 0;
                string productCategorySelected = "";
                foreach (var item in categoryList)
                {
                    if (Convert.ToInt32(productCategory) == i)
                    {
                        productCategorySelected = item.Text;
                    }
                    i++;
                }

                Product product = new Product("", "", "", "", "", "");

                var setfirebaseContainer = await firebaseClient
                    .Child("Products")
                    .PostAsync<Product>(product);

                string productID = setfirebaseContainer.Key;

                product = new Product(productID, productName, productDesc, productPrice, downloadUrl, productCategorySelected);

                var productRefInFirebase = firebaseClient.Child("Products");

                await productRefInFirebase.Child(productID).PutAsync<Product>(product);

                return RedirectToAction(nameof(StoreContent));
            }
            return View();
        }
        //-------------------------------------------------------------------------

        // GET: Item/Edit/5
        //get the product information that the employee wants to edit
        //-------------------------------------------------------------------------
        public async Task<IActionResult> Edit(string id)
        {
            List<Product> products = new List<Product>();

            categoryList.Clear();

            //checks if the id is null
            if (id == null)
            {
                return NotFound();
            }

            //Retrieve data from Firebase
            var productsInFB = await firebaseClient
              .Child("Products")
              .OnceAsync<Product>();

            foreach (var item in productsInFB)
            {
                if (item.Object.ProductID == id)
                {
                    products.Add(new Product(item.Object.ProductID, item.Object.ProductName, item.Object.ProductDesc, item.Object.ProductPrice, item.Object.ProductImageURL, item.Object.ProductCategory));
                }
            }

            categoryList.Clear();

            //Retrieve data from Firebase
            var allCategories = await firebaseClient
              .Child("Categories")
              .OnceAsync<string>();

            int i = 0;
            foreach (var item in allCategories)
            {
                categoryList.Add(new SelectListItem(item.Object, i.ToString()));
                i++;
            }

            ViewBag.CategoryNames = categoryList;

            ViewBag.Products = products;

            //redirect to a succes page
            return View();
        }
        //-------------------------------------------------------------------------

        // POST: Item/Edit/5
        //Save the changes made to the product the employee choose
        //-------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string PIURL, string productName, string productDesc, string productPrice, string productImageURL, string productCategory, IFormFile image)
        {
            //checks if the model state is good the it will enter the if statement
            if (ModelState.IsValid)
            {
                string productCategorySelected = "";
                if (image != null)
                {
                    ////using that will copy a image into a memory stream to save to the database
                    var stream = new MemoryStream();

                    //copy the picture into a stream
                    await image.CopyToAsync(stream);

                    var imageStreamBuffer = new MemoryStream(stream.GetBuffer());

                    var UUID = Guid.NewGuid();

                    // Construct FirebaseStorage with path to where you want to upload the file and put it there
                    var task = new FirebaseStorage("acme-inc-3ee35.appspot.com")
                     .Child("Product-Images")
                     .Child(image.FileName + "-" + UUID)
                     .PutAsync(imageStreamBuffer);

                    //Track progress of the upload
                    task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                    //Await the task to wait until upload is completed and get the download url
                    var downloadUrl = await task;

                    int i = 0;
                    foreach (var item in categoryList)
                    {
                        if (Convert.ToInt32(productCategory) == i)
                        {
                            productCategorySelected = item.Text;
                        }
                        i++;
                    }

                    Product product = new Product(id, productName, productDesc, productPrice, downloadUrl, productCategorySelected);

                    var productRefInFirebase = firebaseClient.Child("Products");

                    await productRefInFirebase.Child(id).PutAsync<Product>(product);

                    return RedirectToAction(nameof(StoreContent));
                }
                else
                {
                    int i = 0;
                    foreach (var item in categoryList)
                    {
                        if (Convert.ToInt32(productCategory) == i)
                        {
                            productCategorySelected = item.Text;
                        }
                        i++;
                    }

                    Product product = new Product(id, productName, productDesc, productPrice, PIURL, productCategorySelected);

                    var productRefInFirebase = firebaseClient.Child("Products");

                    await productRefInFirebase.Child(id).PutAsync<Product>(product);

                    return RedirectToAction(nameof(StoreContent));
                }
            }
            //return to the edit view
            return View();
        }
        //-------------------------------------------------------------------------

        //Display the product info the user choose
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

        //IActionResult method to to retrive a spesific product that the employee wants to see
        //-------------------------------------------------------------------------
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