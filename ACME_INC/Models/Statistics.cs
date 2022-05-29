using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACME_INC.Models
{
    public class Statistics
    {
        private string lable;
        private int sales;

        public Statistics(string lable, int sales)
        {
            this.lable = lable;
            this.sales = sales;
        }

        public string Lable { get => lable; set => lable = value; }
        public int Sales { get => sales; set => sales = value; }
    }
}
