using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACME_INC.Models
{
    public class Product
    {
        private string productID;
        private string productName;
        private string productDesc;
        private string productPrice;
        private string productImageURL;
        private string productCategory;

        public Product(string productID, string productName, string productDesc, string productPrice, string productImageURL, string productCategory)
        {
            this.productID = productID;
            this.productName = productName;
            this.productDesc = productDesc;
            this.productPrice = productPrice;
            this.productImageURL = productImageURL;
            this.productCategory = productCategory;
        }

        public string ProductID { get => productID; set => productID = value; }
        public string ProductName { get => productName; set => productName = value; }
        public string ProductDesc { get => productDesc; set => productDesc = value; }
        public string ProductPrice { get => productPrice; set => productPrice = value; }
        public string ProductImageURL { get => productImageURL; set => productImageURL = value; }
        public string ProductCategory { get => productCategory; set => productCategory = value; }
    }
}
