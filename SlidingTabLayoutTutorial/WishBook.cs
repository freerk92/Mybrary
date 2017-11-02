using System;
using System.Collections.Generic;
using SQLite;

namespace SlidingTabLayoutTutorial
{
    public class WishBook
    {
        [PrimaryKey]
        public string Isbn { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Edition { get; set; }
        public string Publisher { get; set; }
        public string Year { get; set; }
        public string Length { get; set; }

        public WishBook()
        {

        }

        public WishBook(string isbn, string title, string author, string edition, string publisher, string year, string length)
        {
            Isbn = isbn;
            Title = title;
            Author = author;
            Edition = edition;
            Publisher = publisher;
            Year = year;
            Length = length;
        }

        public string GetInformation()
        {
            string info = "Titel: " + Title + "\n";
            if (Author != null) { info += "Auteur: " + Author + "\n"; }
            if (Edition != null) { info += "Editie: " + Edition + "\n"; }
            if (Publisher != null) { info += "Uitgever: " + Publisher + "\n"; }
            if (Year != null) { info += "Jaar van uitgave: " + Year + "\n"; }
            if (Length != null) { info += "Lengte: " + Length + "\n"; }
            return info;
        }

    }
}