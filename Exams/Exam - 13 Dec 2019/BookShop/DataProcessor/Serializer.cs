namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var authors = context.Authors
                .ToList()
                .Select(x => new
                {
                    AuthorName = x.FirstName + " " + x.LastName,
                    Books = x.AuthorsBooks
                    .ToList()
                    .OrderByDescending(b => b.Book.Price)
                    .Select(b => new
                    {
                        BookName = b.Book.Name,
                        BookPrice = b.Book.Price.ToString("F2")
                    })
                    .ToList()
                })
                .ToList()
                .OrderByDescending(x => x.Books.Count)
                .ThenBy(x => x.AuthorName)
                .ToList();

            var result = JsonConvert.SerializeObject(authors, Formatting.Indented);
            return result;
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            var books = context.Books
                .ToList()
                .Where(x => x.PublishedOn < date && x.Genre == Genre.Science)
                .ToArray()
                .OrderByDescending(x => x.Pages)
                .ThenByDescending(x => x.PublishedOn)
                .Select(x => new BookXmlExportModel
                {
                    Name = x.Name,
                    Date = x.PublishedOn.ToString("d", CultureInfo.InvariantCulture),
                    Pages = x.Pages
                })
                .Take(10)
                .ToArray();

            var result = XmlConverter.Serialize<BookXmlExportModel[]>(books, "Books");

            return result;
        }
    }
}