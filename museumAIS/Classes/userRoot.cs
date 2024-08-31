using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace museumAIS.Classes
{
    public class userRoot
    {
        private static helpForDB db = new helpForDB();

        public bool checkUsers()
        {
            DataTable table = db.selectData("SELECT * FROM users WHERE privilege_user = 1");
            if(table == null)
            {
                callMessageBox.ShowError("Ошибка подключения к базе данных!");
                return false;
            }
            if(table.Rows.Count == 0)
            {
                return true;
            }
            return false;
        }
    }
}
