namespace Cinema.DataProcessor
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json.Serialization;
    using Cinema.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;

    public class Serializer
    {
        public static string ExportTopMovies(CinemaContext context, int rating)
        {
            //Export top 10 movies which have rating more or equal to the given and have at least one projection with sold tickets.
            //For each movie, export its name, rating formatted to the second digit, total incomes formatted same way and customers.
            //For each customer, export its first name, last name and balance formatted to the second digit.
            //Order the customers by balance(descending by the formatted string, not the balance itselft), then by first name(ascending) and last name(ascending).
            //Take first 10 records ordered by rating(descending), then by total incomes(descending).

            var movies = context.Movies
                .ToList()
                .Where(x => x.Rating >= rating && x.Projections.Any(p => p.Tickets.Count > 0))
                .Select(x => new
                {
                    MovieName = x.Title,
                    Rating = x.Rating.ToString("F2"),
                    TotalIncomes = x.Projections.Sum(p => p.Tickets.Sum(t => t.Price)).ToString("F2"),
                    Customers = x.Projections
                        .ToList()
                        .SelectMany(p => p.Tickets.ToList().Select(t => new
                        {
                            FirstName = t.Customer.FirstName,
                            LastName = t.Customer.LastName,
                            Balance = t.Customer.Balance.ToString("F2")
                        })
                        .ToList()
                        )
                        .ToList()
                        .OrderByDescending(p => p.Balance)
                        .ThenBy(p => p.FirstName)
                        .ThenBy(p => p.LastName)
                        .ToList()
                })
                .ToList()
                .OrderByDescending(x => x.Rating)
                .Take(10)
                .OrderByDescending(x => decimal.Parse(x.TotalIncomes))
                .ToList();

            //var movies = context.Projections
            //    .ToList()
            //    .Where(x => x.movie.rating >= rating && x.tickets.count > 0)
            //    .select(x => new
            //    {
            //        moviename = x.movie.title,
            //        rating = x.movie.rating.tostring("f2"),
            //        totalincomes = x.tickets.sum(t => t.price).tostring("f2"),
            //        customers = x.tickets.tolist().select(t => new
            //        {
            //            firstname = t.customer.firstname,
            //            lastname = t.customer.lastname,
            //            balance = t.customer.balance.tostring("f2")
            //        })
            //                .orderbydescending(p => p.balance)
            //                .thenby(p => p.firstname)
            //                .thenby(p => p.lastname)
            //                .tolist()
            //    })
            //    .orderbydescending(x => x.rating)
            //    .thenbydescending(x => x.totalincomes)
            //    .take(10)
            //    .tolist();

            var result = JsonConvert.SerializeObject(movies, Formatting.Indented);
            return result;
        }

        public static string ExportTopCustomers(CinemaContext context, int age)
        {
            return "TODO";
            ////Export customers with age above or equal to the given. For each customer, export their first name, last name, spent money for tickets(formatted to the second digit) and spent time(in format: "hh\:mm\:ss").Take first 10 records and order the result by spent money in descending order.

            //var customers = context.Customers
            //    .ToList()
            //    .Where(x => x.Age >= age)
            //    .Select(x => new CustomerXmlExportModel
            //    {
            //        FirstName = x.FirstName,
            //        LastName = x.LastName,
            //        SpentMoney = decimal.Parse(x.Tickets.Sum(t => t.Price).ToString("F2")),
            //        SpentTime = TimeSpan.ParseExact(new TimeSpan(x.Tickets.Sum(t => t.Projection.Movie.Duration.Ticks)), @"hh\:mm\:ss", CultureInfo.InvariantCulture)
            //    })
            //    .Take(10)
            //    .OrderByDescending(x => x.SpentMoney)
            //    .ToList();

            //var result = XmlConverter.Serialize(customers, "Customers");
            //return result;
        }
    }
}