namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            var books = XmlConverter.Deserializer<BookXmlImportModel>(xmlString, "Books");
            var sb = new StringBuilder();

            foreach (var currentBook in books)
            {
                if (!IsValid(currentBook))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                bool isDateValid = DateTime.TryParseExact(currentBook.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);

                if (!isDateValid)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                var book = new Book
                {
                    Name = currentBook.Name,
                    Pages = currentBook.Pages,
                    Genre = (Genre)Enum.Parse(typeof(Genre), currentBook.Genre),
                    Price = currentBook.Price,
                    PublishedOn = date
                };

                sb.AppendLine($"Successfully imported book {book.Name} for {book.Price:F2}.");
                context.Books.Add(book);
                context.SaveChanges();
            }
            return sb.ToString().TrimEnd();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var authors = JsonConvert.DeserializeObject<List<AuthorJsonInputModel>>(jsonString);
            var sb = new StringBuilder();

            foreach (var currentAuthor in authors)
            {
                if (!IsValid(currentAuthor) || !currentAuthor.Books.All(IsValid))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                if (context.Authors.Any(x => x.Email == currentAuthor.Email))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                var author = new Author
                {
                    FirstName = currentAuthor.FirstName,
                    LastName = currentAuthor.LastName,
                    Email = currentAuthor.Email,
                    Phone = currentAuthor.Phone
                };

                foreach (var currentBook in currentAuthor.Books)
                {
                    var book = context.Books.FirstOrDefault(x => x.Id == currentBook.Id);

                    if (book == null)
                    {
                        continue;
                    }
                    author.AuthorsBooks.Add(new AuthorBook { BookId = book.Id });
                }

                if (author.AuthorsBooks.Count == 0)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }
                context.Authors.Add(author);
                context.SaveChanges();
                sb.AppendLine($"Successfully imported author - {author.FirstName} {author.LastName} with {author.AuthorsBooks.Count} books.");
            }
            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}