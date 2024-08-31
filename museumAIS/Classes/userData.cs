using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace museumAIS
{
    public class userData
    {
        private static string userName = "root";

        private static string userID = "1";

        public static string UserName { get => userName; set => userName = value; }
        public static string UserID { get => userID; set => userID = value; }
    }
}
