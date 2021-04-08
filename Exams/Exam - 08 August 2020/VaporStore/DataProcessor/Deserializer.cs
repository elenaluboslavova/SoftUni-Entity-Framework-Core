namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			var games = JsonConvert.DeserializeObject<GameJsonImportModel[]>(jsonString);

			var sb = new StringBuilder();

            foreach (var game in games)
            {
                if (!IsValid(game) || game.Tags.Count() == 0)
                {
					sb.AppendLine("Invalid Data");
					continue;
                }
				DateTime releaseDate;
				var isReleaseDateValid = DateTime.TryParseExact(game.ReleaseDate, "yyyy-MM-dd",
				   CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDate);

				if (!isReleaseDateValid)
				{
					sb.AppendLine("Invalid Data");

					continue;
				}

				var genre = context.Genres.FirstOrDefault(x => x.Name == game.Genre);

                if (genre == null)
                {
					genre = new Genre
					{
						Name = game.Genre
					};
                }

				var developer = context.Developers.FirstOrDefault(x => x.Name == game.Developer);

                if (developer == null)
                {
					developer = new Developer
					{
						Name = game.Developer
					};
                }

				var validGame = new Game
				{
					Name = game.Name,
					Price = game.Price,
					Genre = genre,
					Developer = developer,
					ReleaseDate = releaseDate,
				};

                foreach (var currentTag in game.Tags)
                {
					var tag = context.Tags.FirstOrDefault(x => x.Name == currentTag);

                    if (tag == null)
                    {
						tag = new Tag
						{
							Name = currentTag
						};
                    }
					validGame.GameTags.Add(new GameTag { Tag = tag });
                }
				sb.AppendLine($"Added {validGame.Name} ({validGame.Genre.Name}) with {validGame.GameTags.Count} tags");
				context.Games.Add(validGame);
				context.SaveChanges();
            }

			return sb.ToString().TrimEnd();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
			var users = JsonConvert.DeserializeObject<UserJsonInputModel[]>(jsonString);
			var sb = new StringBuilder();
			var usersList = new List<User>();

			foreach (var currentUser in users)
            {
                if (!IsValid(currentUser) || !currentUser.Cards.All(IsValid))
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

				var user = new User
				{
					FullName = currentUser.FullName,
					Username = currentUser.Username,
					Email = currentUser.Email,
					Age = currentUser.Age,
					Cards = currentUser.Cards.ToList().Select(c => new Card
					{
						Number = c.Number,
						Cvc = c.CVC,
						Type = Enum.Parse<CardType>(c.Type)
					})
					.ToList()
				};
				usersList.Add(user);
				sb.AppendLine($"Imported {user.Username} with {user.Cards.Count} cards");
            }

			context.Users.AddRange(usersList);
			context.SaveChanges();
			return sb.ToString().TrimEnd();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			var sb = new StringBuilder();
			var purchases = XmlConverter.Deserializer<PurchaseXmlInputModel>(xmlString, "Purchases");

            foreach (var currentPurchase in purchases)
            {
                if (!IsValid(currentPurchase))
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

				DateTime date;
				bool isDateValid = DateTime.TryParseExact(currentPurchase.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                if (!isDateValid)
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

                var game = context.Games.FirstOrDefault(x => x.Name == currentPurchase.Title);
                if (game == null)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

				var card = context.Cards.FirstOrDefault(x => x.Number == currentPurchase.Card);
                if (card == null)
                {
					sb.AppendLine("Invalid Data");
					continue;
				}

				var purchase = new Purchase
				{
					Type = Enum.Parse<PurchaseType>(currentPurchase.Type),
					Date = date,
					Game = game,
					Card = card,
					ProductKey = currentPurchase.Key
				};

				context.Purchases.Add(purchase);
				context.SaveChanges();
				sb.AppendLine($"Imported {purchase.Game.Name} for {purchase.Card.User.Username}");
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