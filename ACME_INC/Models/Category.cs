using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACME_INC.Models
{
    public class Category
    {
        private string catID;
        private string catName;

        public Category(string catID, string catName)
        {
            this.catID = catID;
            this.catName = catName;
        }

        public string CatID { get => catID; set => catID = value; }
        public string CatName { get => catName; set => catName = value; }
    }
}
