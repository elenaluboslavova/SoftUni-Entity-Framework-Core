namespace BookShop
{
    using BookShop.Models;
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using BookShopContext db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            Console.WriteLine(RemoveBooks(db));
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(x => x.Copies < 4200)
                .ToList();

            int counter = 0;
            foreach (var book in books)
            {
                context.Books.Remove(book);
                counter++;
            }
            context.SaveChanges();
            return counter;
        }

        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books
                .Where(x => x.ReleaseDate.Value.Year < 2010)
                .ToList();

            foreach (var book in books)
            {
                book.Price += 5;
            }
            context.SaveChanges();
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categoryBooks = context.Categories
                .Select(category => new
                {
                    category.Name,
                    Books = category.CategoryBooks.Select(b => new
                    {
                        b.Book.Title,
                        b.Book.ReleaseDate.Value
                    })
                    .OrderByDescending(b => b.Value)
                    .Take(3)
                    .ToList()
                })
                .OrderBy(x => x.Name)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var cat in categoryBooks)
            {
                sb.AppendLine($"--{cat.Name}");
                foreach (var book in cat.Books)
                {
                    sb.AppendLine($"{book.Title} ({book.Value.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categories = context.Categories
                .Select(c => new
                {
                    c.Name,
                    Profit = c.CategoryBooks.Sum(b => b.Book.Copies * b.Book.Price)
                })
                .OrderByDescending(x => x.Profit)
                .ThenBy(x => x.Name)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var category in categories)
            {
                sb.AppendLine($"{category.Name} ${category.Profit:F2}");
            }
            return sb.ToString().TrimEnd();
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context.Authors.Select(x => new
            {
                FullName = x.FirstName + " " + x.LastName,
                Books = x.Books.Sum(b => b.Copies)
            })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var author in authors.OrderByDescending(x => x.Books))
            {
                sb.AppendLine($"{author.FullName} - {author.Books}");
            }
            return sb.ToString().TrimEnd();

        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var count = context.Books.Where(x => x.Title.Length > lengthCheck).Count();
            return count;
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context.Books.Where(x => x.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .OrderBy(x => x.BookId)
                .Select(x => new
                {
                    x.Title,
                    x.Author.FirstName,
                    x.Author.LastName
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} ({book.FirstName} {book.LastName})");
            }
            return sb.ToString().TrimEnd();
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var books = context.Books.Where(x => x.Title.ToLower().Contains(input.ToLower()))
                .OrderBy(x => x.Title)
                .Select(x => x.Title)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book);
            }
            return sb.ToString().TrimEnd();

        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors.Where(x => x.FirstName.EndsWith(input))
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var author in authors)
            {
                sb.AppendLine($"{author.FirstName} {author.LastName}");
            }
            return sb.ToString().TrimEnd();

        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var releaseDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var books = context.Books
                .OrderByDescending(x => x.ReleaseDate)
                .Where(x => x.ReleaseDate.Value < releaseDate && x.ReleaseDate != null)
                .Select(x => new
                {
                    x.Title,
                    x.EditionType,
                    x.Price
                }).ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:F2}");
            }
            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categories = input.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);

            var books = context.Books
                .Where(x => x.BookCategories
                .Any(c => categories.Contains(c.Category.Name.ToLower())))
                .OrderBy(x => x.Title)
                .ToList();

            StringBuilder sb = new StringBuilder();
            foreach (var book in books)
            {
                sb.AppendLine(book.Title);
            }
            return sb.ToString().TrimEnd();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books.Where(x => x.ReleaseDate.HasValue && x.ReleaseDate.Value.Year != year)
                .OrderBy(x => x.BookId)
                .Select(x => x.Title)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book);
            }
            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books.Where(x => x.Price > 40)
                .OrderByDescending(x => x.Price)
                .Select(x => new
                {
                    x.Title,
                    x.Price
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:F2}");
            }
            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var ageRestriction = Enum.Parse<AgeRestriction>(command, true);
            var books = context.Books.Where(x => x.AgeRestriction == ageRestriction)
                .OrderBy(x => x.Title)
                .Select(x => x.Title)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book);
            }
            return sb.ToString().TrimEnd();
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            var books = context.Books.Where(x => x.EditionType == EditionType.Gold && x.Copies < 5000)
                .OrderBy(x => x.BookId)
                .Select(x => x.Title)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book);
            }
            return sb.ToString().TrimEnd();
        }
    }
}
