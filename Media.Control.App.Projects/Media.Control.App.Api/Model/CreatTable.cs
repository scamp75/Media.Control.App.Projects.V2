
using Microsoft.AspNetCore.Http.HttpResults;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

namespace Media.Control.App.Api.Model
{
    public static class CreatTable
    {

        public static string CreateMediaDataInfoTable()
        {
            return
                $" CREATE TABLE mediainfotb( " +
                $" Num INTEGER PRIMARY KEY AUTOINCREMENT," +
                $" MediaId TEXT," + 
                $" Name TEXT," +
                $" Duration TEXT," +
                $" Frame INTEGER," +
                $" CreateDate TEXT," +
                $" InPoint INTEGER," +
                $" InTimeCode TEXT," +
                $" OutPoint INTEGER," +
                $" OutTimeCode TEXT," +
                $" Type TEXT," +
                $" Creator TEXT," +
                $" Proxy TEXT," +
                $" Image TEXT," +
                $" Path TEXT," +
                $" Fps INTEGER," +
                $" Des TEXT," +
                $" State TEXT ); ";
        }

        public static string CreateLogDataTable()
        {
            return 
                " CREATE TABLE logdatatb( " +
                " Num INTEGER PRIMARY KEY AUTOINCREMENT," +
                " Type TEXT," +
                " Title TEXT," +
                " Message TEXT," +
                " Channel TEXT," +
                " CreateDate TEXT," +
                " Time TEXT ); ";
        }
    }
}