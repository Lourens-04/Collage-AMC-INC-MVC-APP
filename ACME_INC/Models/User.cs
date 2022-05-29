using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACME_INC.Models
{
    public class User
    {
        private string userID;
        private string userEmail;
        private string userFirstName;
        private string userLastName;
        private string userRole;

        public User(string userID, string userEmail, string userFirstName, string userLastName, string userRole)
        {
            this.userID = userID;
            this.userEmail = userEmail;
            this.userFirstName = userFirstName;
            this.userLastName = userLastName;
            this.userRole = userRole;
        }

        public string UserID { get => userID; set => userID = value; }
        public string UserEmail { get => userEmail; set => userEmail = value; }
        public string UserFirstName { get => userFirstName; set => userFirstName = value; }
        public string UserLastName { get => userLastName; set => userLastName = value; }
        public string UserRole { get => userRole; set => userRole = value; }
    }
}
