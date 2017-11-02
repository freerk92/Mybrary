using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace SlidingTabLayoutTutorial
{
    class AlternativeISBN
    {
        [PrimaryKey]
        public string AltISBN { get; set; }
        public string ISBN { get; set; }


        public AlternativeISBN()
        {
        }

        public AlternativeISBN(string altIsbn, string isbn)
        {
            AltISBN = altIsbn;
            ISBN = isbn;
        }
    }
}