using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACME_INC.Models
{
    public class Transaction
    {
        private string transactionID;
        private string userID;
        private string productID;
        private string transactionDate;
        private string transactionTotal;

        public Transaction(string transactionID, string userID, string productID, string transactionDate, string transactionTotal)
        {
            this.TransactionID = transactionID;
            this.UserID = userID;
            this.ProductID = productID;
            this.TransactionDate = transactionDate;
            this.TransactionTotal = transactionTotal;
        }

        public string TransactionID { get => transactionID; set => transactionID = value; }
        public string UserID { get => userID; set => userID = value; }
        public string ProductID { get => productID; set => productID = value; }
        public string TransactionDate { get => transactionDate; set => transactionDate = value; }
        public string TransactionTotal { get => transactionTotal; set => transactionTotal = value; }
    }
}
