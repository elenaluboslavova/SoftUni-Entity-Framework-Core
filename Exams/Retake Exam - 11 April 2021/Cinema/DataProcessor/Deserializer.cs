namespace Cinema.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Cinema.Data.Models;
    using Cinema.Data.Models.Enums;
    using Cinema.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfulImportMovie 
            = "Successfully imported {0} with genre {1} and rating {2}!";

        private const string SuccessfulImportProjection 
            = "Successfully imported projection {0} on {1}!";

        private const string SuccessfulImportCustomerTicket 
            = "Successfully imported customer {0} {1} with bought tickets: {2}!";

        public static string ImportMovies(CinemaContext context, string jsonString)
        {
            var movies = JsonConvert.DeserializeObject<MovieJsonImportModel[]>(jsonString);
            var sb = new StringBuilder();

            foreach (var currentMovie in movies)
            {
                if (!IsValid(currentMovie))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }
                var movie = context.Movies.FirstOrDefault(x => x.Title == currentMovie.Title);
                if (movie != null)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                bool isDurationValid = TimeSpan.TryParseExact(currentMovie.Duration, "c", CultureInfo.InvariantCulture, out TimeSpan validDuration);
                if (!isDurationValid)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                movie = new Movie
                {
                    Director = currentMovie.Director,
                    Duration = validDuration,
                    Genre = (Genre)Enum.Parse(typeof(Genre), currentMovie.Genre),
                    Rating = currentMovie.Rating,
                    Title = currentMovie.Title
                };

                context.Movies.Add(movie);
                context.SaveChanges();
                sb.AppendLine($"Successfully imported {movie.Title} with genre {movie.Genre} and rating {movie.Rating:F2}!");
            }
            return sb.ToString().TrimEnd();
        }

        public static string ImportProjections(CinemaContext context, string xmlString)
        {
            var projections = XmlConverter.Deserializer<ProjectionXmlImportModel>(xmlString, "Projections");

            var sb = new StringBuilder();

            foreach (var currentProjection in projections)
            {
                if (!IsValid(currentProjection))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                var movie = context.Movies.FirstOrDefault(x => x.Id == currentProjection.MovieId);

                if (movie == null)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                bool isDateValid = DateTime.TryParseExact(currentProjection.DateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validDate);

                if (!isDateValid)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                var projection = new Projection
                {
                    DateTime = validDate,
                    MovieId = movie.Id
                };

                context.Projections.Add(projection);
                context.SaveChanges();
                sb.AppendLine($"Successfully imported projection {movie.Title} on {projection.DateTime:MM/dd/yyyy}!");
            }
            return sb.ToString().TrimEnd();
        }

        public static string ImportCustomerTickets(CinemaContext context, string xmlString)
        {
            var customers = XmlConverter.Deserializer<CustomerTicketXmlImportModel>(xmlString, "Customers");
            var sb = new StringBuilder();

            foreach (var currentCustomer in customers)
            {
                if (!IsValid(currentCustomer) || !currentCustomer.Tickets.All(IsValid))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                var customer = new Customer
                {
                    Age = currentCustomer.Age,
                    Balance = currentCustomer.Balance,
                    FirstName = currentCustomer.FirstName,
                    LastName = currentCustomer.LastName,
                    Tickets = currentCustomer.Tickets.Select(t => new Ticket
                    {
                        ProjectionId = t.ProjectionId,
                        Price = t.Price
                    })
                    .ToList()
                };

                context.Customers.Add(customer);
                context.SaveChanges();
                sb.AppendLine($"Successfully imported customer {customer.FirstName} {customer.LastName} with bought tickets: {customer.Tickets.Count}!");
            }

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}