namespace VaporStore.DataProcessor
{
	using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.DataProcessor.Dto.Export;

    public static class Serializer
	{
		public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
		{
			var games = context.Genres
				.ToList()
				.Where(x => genreNames.Contains(x.Name))
				.Select(x => new
				{
					Id = x.Id,
					Genre = x.Name,
					Games = x.Games.Where(g => g.Purchases.Count > 0).Select(g => new
					{
						Id = g.Id,
						Title = g.Name,
						Developer = g.Developer.Name,
						Tags = string.Join(", ", g.GameTags.Select(gt => gt.Tag.Name)),
						Players = g.Purchases.Count
					})
					.ToList()
					.OrderByDescending(g => g.Players)
					.ThenBy(g => g.Id),
					TotalPlayers = x.Games.Sum(g => g.Purchases.Count)
				})
				.ToList()
				.OrderByDescending(x => x.TotalPlayers)
				.ThenBy(x => x.Id);

			var result = JsonConvert.SerializeObject(games, Formatting.Indented);
			return result;
		}

		public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
		{
			var userPurchases = context.Users.ToList()
				.Where(x => x.Cards.Any(c => c.Purchases.Any()))
				 .Select(x => new UserXmlExportModel
				 {
					 Username = x.Username,
					 TotalSpent = x.Cards.Sum(c => c.Purchases.Where(p => p.Type.ToString() == storeType).Sum(p => p.Game.Price)),

					 Purchases = x.Cards.SelectMany(c => c.Purchases)
					 .Where(p => p.Type.ToString() == storeType)
					 .Select(p => new PurchaseXmlExportModel
					 {
						 Card = p.Card.Number,
						 CVC = p.Card.Cvc,
						 Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
						 Game = new GameXmlExportModel
						 {
							 Genre = p.Game.Genre,
							 Price = p.Game.Price,
							 Title = p.Game.Name
						 }
					 })
					 .OrderBy(p => p.Date)
					 .ToArray()
				 })
				 .OrderByDescending(x => x.TotalSpent)
				 .ThenBy(x => x.Username)
				 .ToArray();

			var result = XmlConverter.Serialize(userPurchases, "Users");
			return result;
		}
	}
}