using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;
using Environment = System.Environment;

namespace SlidingTabLayoutTutorial
{
    interface IdbCon
    {
        string createDatabases();
        string deleteData(string path, string query);
        string insertUpdateData(Book data);
        string insertAlternativeData(List<AlternativeISBN> data, ScanActivity s);
        int findNumberRecords(string path, string query);
        List<Book> getDatabaseInfo(string query);
        List<AlternativeISBN> getAltDatabaseInfo(string query);
        List<WishBook> getWishDatabaseInfo(string query);
        string insertWishData(Book data);
        void dropTable();
    }

    class DatabaseConnection : IdbCon
    {
        static string docsFolder = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string pathToDatabase = Path.Combine(docsFolder, "db_sqlnet.db");
        string altPath = Path.Combine(docsFolder, "altISBN.db");
        private string pathToWishbase = Path.Combine(docsFolder, "wishlist.db");

        public string createDatabases()
        {
            try
            {
                var connection = new SQLiteAsyncConnection(pathToDatabase);
                connection.CreateTableAsync<Book>();

                connection = new SQLiteAsyncConnection(altPath);
                connection.CreateTableAsync<AlternativeISBN>();

                connection = new SQLiteAsyncConnection(pathToWishbase);
                connection.CreateTableAsync<WishBook>();
                return "Database created";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        public string insertUpdateData(Book data)
        {
            try
            {
                var db = new SQLiteAsyncConnection(pathToDatabase);
                if (db.InsertAsync(data).Result != 0)
                    db.UpdateAsync(data);
                return "Single data file inserted or updated";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        public string insertWishData(Book data)
        {
            try
            {
                var db = new SQLiteAsyncConnection(pathToWishbase);
                if (db.InsertAsync(data).Result != 0)
                    db.UpdateAsync(data);
                return "Single data file inserted or updated";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }

        public void dropTable()
        {
            try
            {
                var db = new SQLiteAsyncConnection(altPath);
                db.DropTableAsync<AlternativeISBN>();

                db = new SQLiteAsyncConnection(pathToDatabase);
                db.DropTableAsync<Book>();
            }
            catch
            {
                
            }
        }

        public string deleteData(string path, string query)
        {
            path = pathSwitch(path);
            try
            {
                var db = new SQLiteConnection(path);
                // this counts all records in the database, it can be slow depending on the size of the database
                var info = db.Query<Book>(query);

                return "succes";
            }
            catch
            {
                return null;
            }
        }

        public string insertAlternativeData(List<AlternativeISBN> list, ScanActivity t)
        {
            TextView v = t.FindViewById<TextView>(Resource.Id.Results);
            try
            {
                var db = new SQLiteConnection(altPath);


                string s = "INSERT INTO AlternativeISBN (AltISBN, Isbn) VALUES (\"" + list[0].AltISBN + "\", \"" + list[0].ISBN + "\")";


                for (int i = 1; i < list.Count; i++)
                {
                    s += ",(\""+list[i].AltISBN+"\", \""+list[i].ISBN+"\")";
                    var x = list.Count - i;
                }

                s += ";";

                db.Query<AlternativeISBN>(s);
                //for (var i = 0; i < list.Count; i++)
                //{
                //    var data = list[i];
                //    if (db.InsertAsync(data).Result != 0)
                //        db.UpdateAsync(data);
                //    v.Text = "nog " + (list.Count - i);
                //}
                return "Single data file inserted or updated";
            }
            catch (SQLiteException ex)
            {
                return ex.Message;
            }
        }
        
        public int findNumberRecords(string pathVar, string query)
        {
            string path = pathSwitch(pathVar);
                
            try
            {
                var db = new SQLiteConnection(path);
                // this counts all records in the database, it can be slow depending on the size of the database
                var count = db.ExecuteScalar<int>(query);

                // for a non-parameterless query
                // var count = db.ExecuteScalar<int>("SELECT Count(*) FROM Person WHERE FirstName="Amy");

                return count;
            }
            catch (SQLiteException)
            {
                return -1;
            }
        }

        public List<Book> getDatabaseInfo(string query)
        {
            string path = pathToDatabase;
            try
            {
                var db = new SQLiteConnection(path);
                // this counts all records in the database, it can be slow depending on the size of the database
                var info = db.Query<Book>(query);

                return info;
            }
            catch
            {
                return null;
            }
        }

        public List<AlternativeISBN> getAltDatabaseInfo(string query)
        {
            string path = altPath;
            try
            {
                var db = new SQLiteConnection(path);
                // this counts all records in the database, it can be slow depending on the size of the database
                var info = db.Query<AlternativeISBN>(query);

                return info;
            }
            catch
            {
                return null;
            }
        }

        public List<WishBook> getWishDatabaseInfo(string query)
        {
            string path = pathToWishbase;
            try
            {
                var db = new SQLiteConnection(path);
                // this counts all records in the database, it can be slow depending on the size of the database
                var info = db.Query<WishBook>(query);

                return info;
            }
            catch
            {
                return null;
            }
        }

        private string pathSwitch(string s)
        {
            if (s.Equals("normal"))
            {
                return pathToDatabase;
            }
            else if (s.Equals("wish"))
            {
                return pathToWishbase;
            }
            return altPath;
        }
    }
}