using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using ZXing.Mobile;
using HtmlAgilityPack;
using Java.Sql;
using Org.Apache.Http.Client.Methods;
using Org.Apache.Http.Impl.Client;
using Org.Apache.Http.Util;
using SQLite;
using ZXing;
using Debug = System.Diagnostics.Debug;
using Environment = System.Environment;
using Result = ZXing.Result;

namespace SlidingTabLayoutTutorial
{
    [Activity(Label = "ScanActivity")]
    public class ScanActivity : Activity
    {
        //private string temp = "9023953606";
        private Book b;
        private TextView results;
        private Button addButton;
        private Button wishButton;
        private TextView exists;
        private ImageView imageView;

        IdbCon con = new DatabaseConnection();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            MobileBarcodeScanner.Initialize(Application);
            //path string for the databasefile
            // create DB path
            con.createDatabases();
            string s = Intent.GetStringExtra("isbn");
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.scan_page);
            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.btnScan);
            
            results = FindViewById<TextView>(Resource.Id.Results);
            addButton = FindViewById<Button>(Resource.Id.btnGot);
            addButton.Enabled = true;
            wishButton = FindViewById<Button>(Resource.Id.btnNeed);
            wishButton.Enabled = true;
            exists = FindViewById <TextView>(Resource.Id.textExists);
            imageView = FindViewById<ImageView>(Resource.Id.imageView1);

            setInfo(s);

                button.Click += async delegate
            {
                var scanner = new MobileBarcodeScanner();
                var result = await scanner.Scan();

                if (result == null) return;


                setInfo(result.Text);
            };

            addButton.Click += delegate
            {
                var y = "";
                try
                {
                    DateTime t = DateTime.Now;
                    var result = con.insertUpdateData(b);
                    DateTime t2 = DateTime.Now;
                    results.Text += "\nDuur insert: " + (t2 - t)+"\n";
                    
                    t = DateTime.Now;
                    var x = GetAlternativeIsbn(b.Isbn);
                    t2 = DateTime.Now;
                    results.Text += "Duur getalt: " + (t2 - t) + "\n";
                    //results.Text = "Alternatieve ISBN's worden toegevoegd";

                    t = DateTime.Now;
                    y = con.insertAlternativeData(x, this);
                    
                    t2= DateTime.Now;
                    results.Text += "Duur insertalt: " + (t2 - t) + "\n";
                    results.Text += "\n"+con.findNumberRecords("alt", "SELECT Count(*) FROM AlternativeISBN");
                    //results.Text = b.Title+" toegevoegd aan database";
                }
                catch
                {
                    results.Text += "\nfail";
                }
            };

            wishButton.Click += delegate
            {
                try
                {
                    results.Text = "Word toegevoegd aan wishlist";
                    var result = con.insertWishData(b);
                    results.Text = b.Title + " toegevoegd aan wishlist";
                }
                catch
                {

                }
            };
        }

        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }
        
        private void setInfo(string s)
        {
            b = createinfostring(s);

            results.Text = b.GetInformation() + "\nAlternatieve versies: " + GetAlternativeIsbn(b.Isbn).Count;
            checkDuplicates(b.Isbn);
        }

        public void checkDuplicates(string s)
        {
            exists.Text = "";
            if (con.findNumberRecords("normal", "SELECT Count(*) FROM Book WHERE Isbn ==" + s) > 0)
            {
                exists.SetTextColor(Android.Graphics.Color.Green);
                exists.Text = "Al in uw bezit";
                addButton.Enabled = false;
                wishButton.Enabled = false;
            }
            else if (con.getAltDatabaseInfo("SELECT * FROM AlternativeISBN WHERE AltISBN =="+s).Count > 0)
            {
                exists.SetTextColor(Android.Graphics.Color.Orange);
                exists.Text = "U heeft een alternatieve versie van dit boek";
            }
        }

        public string GetISBN13(string ISBN)
        {
            string isbn10 = "978" + ISBN.Substring(0, 9);
            int isbn10_1 = Convert.ToInt32(isbn10.Substring(0, 1));
            int isbn10_2 = Convert.ToInt32(Convert.ToInt32(isbn10.Substring(1, 1)) * 3);
            int isbn10_3 = Convert.ToInt32(isbn10.Substring(2, 1));
            int isbn10_4 = Convert.ToInt32(Convert.ToInt32(isbn10.Substring(3, 1)) * 3);
            int isbn10_5 = Convert.ToInt32(isbn10.Substring(4, 1));
            int isbn10_6 = Convert.ToInt32(Convert.ToInt32(isbn10.Substring(5, 1)) * 3);
            int isbn10_7 = Convert.ToInt32(isbn10.Substring(6, 1));
            int isbn10_8 = Convert.ToInt32(Convert.ToInt32(isbn10.Substring(7, 1)) * 3);
            int isbn10_9 = Convert.ToInt32(isbn10.Substring(8, 1));
            int isbn10_10 = Convert.ToInt32(Convert.ToInt32(isbn10.Substring(9, 1)) * 3);
            int isbn10_11 = Convert.ToInt32(isbn10.Substring(10, 1));
            int isbn10_12 = Convert.ToInt32(Convert.ToInt32(isbn10.Substring(11, 1)) * 3);
            int k = (isbn10_1 + isbn10_2 + isbn10_3 + isbn10_4 + isbn10_5 + isbn10_6 + isbn10_7 + isbn10_8 + isbn10_9 + isbn10_10 + isbn10_11 + isbn10_12);
            int checkdigit = 10 - ((isbn10_1 + isbn10_2 + isbn10_3 + isbn10_4 + isbn10_5 + isbn10_6 + isbn10_7 + isbn10_8 + isbn10_9 + isbn10_10 + isbn10_11 + isbn10_12) % 10);
            if (checkdigit == 10)
                checkdigit = 0;
            return isbn10 + checkdigit.ToString();
        }

        private Book createinfostring(string isbn)
        {
            string title;
            string author = null;
            string edition = null;
            string publisher = null;
            string year = null;
            string length = null;
            List<string> getBookName = GetBookName(isbn);
            var v = getBookName[getBookName.Count - 1];
            if (v.Equals("/googlebooks/images/no_cover_thumb.gif"))
            {
                
            }
            else
            {
                var x = GetImageBitmapFromUrl(v);
                imageView.SetImageBitmap(x);
            }
            //Add title
            title = getBookName[1];

            for (int i = 0; i < getBookName.Count; i++)
            {
                if (getBookName[i].Equals("Auteur") || getBookName[i].Equals("Auteurs"))
                {
                    author = getBookName[i + 1];
                    i += 2;
                }

                if (getBookName[i].Equals("Editie"))
                {
                    edition = getBookName[i + 1];
                    i += 2;
                }

                if (getBookName[i].Equals("Uitgever"))
                {

                    string[] uitgever = getBookName[i + 1].Split(',');
                    publisher = uitgever[0];
                    year = uitgever[1];
                }

                if (getBookName[i].Equals("Lengte"))
                {
                    length = getBookName[i + 1];
                }

            }
            return new Book(isbn, title, author, edition, publisher, year, length);
        }

        private List<AlternativeISBN> GetAlternativeIsbn(string isbn)
        {
            var Url = "http://www.librarything.com/api/thingISBN/" + isbn;
            string s = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);


            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                s = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }


            s = s.Replace("</isbn>", "\n");
            s = s.Replace("<isbn>", "");
            s = s.Replace("<idlist>", "");
            s = s.Replace("</idlist>", "");

            List<string> result = s.Split('\n').ToList();
            List<AlternativeISBN> results = new List<AlternativeISBN>();
            result.RemoveAt(0);
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].Equals(""))
                {
                    result.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].Equals(isbn))
                {
                    result.RemoveAt(i);
                }
                else if (result[i].Length < 13)
                {
                    results.Add(new AlternativeISBN(GetISBN13(result[i]), isbn));
                }
                else
                {
                    results.Add(new AlternativeISBN(result[i], isbn));
                }
                
            }
            return results;
        }
        
        private List<string> GetBookName(string s)
        {

            List<string> book;
            try
            {
                var url = "https://books.google.nl/books?vid=isbn" + s;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream;

                    if (response.CharacterSet == null)
                    {
                        readStream = new StreamReader(receiveStream);
                    }
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    }

                    s = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();
                }

                book = ExtractString(s);
                book.Add(ExtractImageUrl(s));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                book = null;
            }

            for (int i = 0; i < book.Count; i++)
            {
                if (book[i].Contains("&amp;"))
                {
                    book[i] = book[i].Replace("&amp;", "&");
                }
            }
            return book;

        }

        public string ExtractImageUrl(string s)
        {
            var startTag = "<div class=\"bookcover\">";
            var endTag = "\" alt";
            int startIndex = s.IndexOf(startTag, StringComparison.Ordinal) + startTag.Length;
            int endIndex = s.IndexOf(endTag, startIndex, StringComparison.Ordinal);
            var x = s.Substring(startIndex, endIndex - startIndex);
            var y = Regex.Replace(x, "<img src=\"", "");
            return y;
        }

        List<string> ExtractString(string s)
        {
            var startTag = "<table id=\"metadata_content_table\">";
            var endTag = "&nbsp";
            int startIndex = s.IndexOf(startTag, StringComparison.Ordinal) + startTag.Length;
            int endIndex = s.IndexOf(endTag, startIndex, StringComparison.Ordinal);
            var x = s.Substring(startIndex, endIndex - startIndex);
            var y = Regex.Replace(x, "<.*?>", "\n");
            var z = Regex.Replace(y, @"^\s*$[\r\n]*", "", RegexOptions.Multiline);
            z = z.Replace("&#39;", "'");
            List<string> result = z.Split('\n').ToList();
            Console.WriteLine(result[1]);
            return result;
        }

    }
    
}