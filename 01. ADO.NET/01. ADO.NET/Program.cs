using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace _01._ADO.NET
{
    public class Program
    {
        public const string sqlConnectionString = @"Server=DESKTOP-3PBD2BD\SQLEXPRESS;Database=MinionsDB;Integrated Security=true";

        public static void Main(string[] args)
        {
            using var connection = new SqlConnection(sqlConnectionString);
            connection.Open();

            // 01.
            //InitialSetup(connection);

            // 02.
            //GetVillainNames(connection);

            // 03.
            //SqlCommand command = MinionNames(connection);

            // 04.
            //AddMinion(connection);

            // 05.
            //SqlCommand updateCommand = ChangeTownNamesCasing(connection);

            // 06.
            //SqlCommand sqlCommand;
            //RemoveVillain(connection, out sqlCommand);

            // 07.
            //SqlCommand selectCommand;
            //SqlDataReader reader;
            //PrintAllMinionNames(connection, out selectCommand, out reader);

            // 08.
            //SqlCommand selectCommand;
            //SqlDataReader reader;
            //IncreaseMinionAge(connection, out selectCommand, out reader);

            // 09.
            //SqlCommand command = IncreaseAgeStoredProcedure(connection);
        }

        private static SqlCommand IncreaseAgeStoredProcedure(SqlConnection connection)
        {
            int id = int.Parse(Console.ReadLine());

            string query = @"EXEC usp_GetOlder @id";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();

            string selectQuery = @"SELECT Name, Age FROM Minions WHERE Id = @Id";

            var selectCommand = new SqlCommand(selectQuery, connection);
            selectCommand.Parameters.AddWithValue("@id", id);
            var reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader[0]} - {reader[1]} years old");
            }

            return command;
        }

        private static void IncreaseMinionAge(SqlConnection connection, out SqlCommand selectCommand, out SqlDataReader reader)
        {
            int[] minionIds = Console.ReadLine().Split().Select(int.Parse).ToArray();

            string updateMinionsQuery = @" UPDATE Minions
   SET Name = UPPER(LEFT(Name, 1)) + SUBSTRING(Name, 2, LEN(Name)), Age += 1
 WHERE Id = @Id";

            foreach (var id in minionIds)
            {
                using var sqlCommand = new SqlCommand(updateMinionsQuery, connection);
                sqlCommand.Parameters.AddWithValue(@"Id", id);
                sqlCommand.ExecuteNonQuery();
            }

            var selectMinionsQuery = @"SELECT Name, Age FROM Minions";
            selectCommand = new SqlCommand(selectMinionsQuery, connection);
            reader = selectCommand.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader[0]} {reader[1]}");
            }
        }

        private static void PrintAllMinionNames(SqlConnection connection, out SqlCommand selectCommand, out SqlDataReader reader)
        {
            var minionsQuery = @"SELECT Name FROM Minions";
            selectCommand = new SqlCommand(minionsQuery, connection);
            reader = selectCommand.ExecuteReader();
            var minions = new List<string>();
            while (reader.Read())
            {
                minions.Add((string)reader[0]);
            }

            int counter = 0;

            for (int i = 0; i < minions.Count / 2; i++)
            {
                Console.WriteLine(minions[i]);
                Console.WriteLine(minions[minions.Count - 1 - counter]);
                counter++;
            }

            if (minions.Count % 2 != 0)
            {
                Console.WriteLine(minions[minions.Count / 2]);
            }
        }

        private static void RemoveVillain(SqlConnection connection, out SqlCommand sqlCommand)
        {
            int value = int.Parse(Console.ReadLine());
            string evilNameQuery = "SELECT Name FROM Villains WHERE Id = @villainId";
            sqlCommand = new SqlCommand(evilNameQuery, connection);
            sqlCommand.Parameters.AddWithValue(@"villainId", value);
            var name = (string)sqlCommand.ExecuteScalar();

            if (name == null)
            {
                Console.WriteLine("No such villain was found.");
                return;
            }
            var deleteMinionsVillainsQuery = @"DELETE FROM MinionsVillains 
      WHERE VillainId = @villainId";

            var sqlDeleteMVCommand = new SqlCommand(deleteMinionsVillainsQuery, connection);
            sqlDeleteMVCommand.Parameters.AddWithValue(@"villainId", value);
            var affectedRows = sqlDeleteMVCommand.ExecuteNonQuery();

            var deleteVillainsQuery = @"DELETE FROM Villains
      WHERE Id = @villainId";

            var sqlDeleteVillainsCommand = new SqlCommand(deleteVillainsQuery, connection);
            sqlDeleteVillainsCommand.Parameters.AddWithValue(@"villainId", value);
            sqlDeleteVillainsCommand.ExecuteNonQuery();

            Console.WriteLine($"{name} was deleted.");
            Console.WriteLine($"{affectedRows} minions were released.");
        }

        private static SqlCommand ChangeTownNamesCasing(SqlConnection connection)
        {
            string updateTownsNamesQuery = @"UPDATE Towns
   SET Name = UPPER(Name)
 WHERE CountryCode = (SELECT c.Id FROM Countries AS c WHERE c.Name = @countryName)";

            string countryName = Console.ReadLine();

            string selectTownNamesQuery = @"SELECT t.Name 
   FROM Towns as t
   JOIN Countries AS c ON c.Id = t.CountryCode
  WHERE c.Name = @countryName";
            var updateCommand = new SqlCommand(updateTownsNamesQuery, connection);
            updateCommand.Parameters.AddWithValue("@countryName", countryName);
            var affectedRows = updateCommand.ExecuteNonQuery();
            if (affectedRows == 0)
            {
                Console.WriteLine("No town names were affected.");
            }
            else
            {
                Console.WriteLine($"{affectedRows} town names were affected.");
                using var selectCommand = new SqlCommand(selectTownNamesQuery, connection);
                selectCommand.Parameters.AddWithValue("@countryName", countryName);
                using var reader = selectCommand.ExecuteReader();
                var towns = new List<string>();
                while (reader.Read())
                {
                    towns.Add((string)reader[0]);
                }
                Console.WriteLine($"[{string.Join(", ", towns)}]");
            }

            return updateCommand;
        }

        private static void AddMinion(SqlConnection connection)
        {
            string[] minionInfo = Console.ReadLine().Split();

            string[] villainInfo = Console.ReadLine().Split();

            string minionName = minionInfo[1];
            int age = int.Parse(minionInfo[2]);
            string town = minionInfo[3];

            int? townId = GetTownId(connection, town);

            if (townId == null)
            {
                string createTownQuery = "INSERT INTO Towns (Name) VALUES (@townName)";
                using var sqlCommand = new SqlCommand(createTownQuery, connection);
                sqlCommand.Parameters.AddWithValue("@townName", town);
                sqlCommand.ExecuteNonQuery();
                townId = GetTownId(connection, town);
                Console.WriteLine($"Town {town} was added to the database.");
            }

            string villainName = villainInfo[1];
            int? villaindId = GetVillainId(connection, villainName);

            if (villaindId == null)
            {
                string createVillainQuery = "INSERT INTO Villains (Name, EvilnessFactorId)  VALUES (@villainName, 4)";
                using var sqlCommand = new SqlCommand(createVillainQuery, connection);
                sqlCommand.Parameters.AddWithValue("@villainName", villainName);
                sqlCommand.ExecuteNonQuery();
                villaindId = GetVillainId(connection, villainName);
                Console.WriteLine($"Villain {villainName} was added to the database.");
            }

            CreateMinion(connection, minionName, age, townId);
            var minionId = GetMinionId(connection, minionName);
            InsertMinionVillain(connection, villaindId, minionId);

            Console.WriteLine($"Successfully added {minionName} to be minion of {villainName}.");
        }

        private static void InsertMinionVillain(SqlConnection connection, int? villaindId, int? minionId)
        {
            var insertIntoMinVilQuery = "INSERT INTO MinionsVillains (MinionId, VillainId) VALUES (@villainId, @minionId)";
            var sqlCommand = new SqlCommand(insertIntoMinVilQuery, connection);
            sqlCommand.Parameters.AddWithValue("@villainId", villaindId);
            sqlCommand.Parameters.AddWithValue("@minionId", minionId);
            sqlCommand.ExecuteNonQuery();
        }

        private static int? GetMinionId(SqlConnection connection, string minionName)
        {
            var minionIdQuery = "SELECT Id FROM Minions WHERE Name = @Name";
            var sqlCommand = new SqlCommand(minionIdQuery, connection);
            sqlCommand.Parameters.AddWithValue("@Name", minionName);
            var minionId = sqlCommand.ExecuteScalar();
            return (int?)minionId;
        }

        private static void CreateMinion(SqlConnection connection, string minionName, int age, int? townId)
        {
            string createMinionQuery = "INSERT INTO Minions (Name, Age, TownId) VALUES (@name, @age, @townId)";
            using var sqlCommand = new SqlCommand(createMinionQuery, connection);
            sqlCommand.Parameters.AddWithValue("@name", minionName);
            sqlCommand.Parameters.AddWithValue("@age", age);
            sqlCommand.Parameters.AddWithValue("@townId", townId);
            sqlCommand.ExecuteNonQuery();
        }

        private static int? GetVillainId(SqlConnection connection, string villainName)
        {
            string query = "SELECT Id FROM Villains WHERE Name = @Name";
            using var sqlCommand = new SqlCommand(query, connection);
            sqlCommand.Parameters.AddWithValue("@Name", villainName);
            var villainId = sqlCommand.ExecuteScalar();
            return (int)villainId;
        }

        private static int GetTownId(SqlConnection connection, string town)
        {
            string townIdQuery = "SELECT Id FROM Villains WHERE Name = @townName";
            using var sqlCommand = new SqlCommand(townIdQuery, connection);
            sqlCommand.Parameters.AddWithValue("@townName", town);
            var townId = sqlCommand.ExecuteScalar();
            return (int)townId;
        }

        private static SqlCommand MinionNames(SqlConnection connection)
        {
            int id = int.Parse(Console.ReadLine());
            string villainNameQuery = "SELECT Name FROM Villains WHERE Id = @Id";
            var command = new SqlCommand(villainNameQuery, connection);
            command.Parameters.AddWithValue(@"Id", id);
            var result = command.ExecuteScalar();

            string minionsQuery = @"
                SELECT ROW_NUMBER() OVER (ORDER BY m.Name) as RowNum,
                                         m.Name, 
                                         m.Age
                                    FROM MinionsVillains AS mv
                                    JOIN Minions As m ON mv.MinionId = m.Id
                                   WHERE mv.VillainId = @Id
                                ORDER BY m.Name";

            if (result == null)
            {
                Console.WriteLine($"No villain with ID {id} exists in the database.");
            }
            else
            {
                Console.WriteLine($"Villain: {result}");
                using var minionCommand = new SqlCommand(minionsQuery, connection);
                minionCommand.Parameters.AddWithValue(@"Id", id);
                using var reader = minionCommand.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("(no minions)");
                    }
                    Console.WriteLine($"{reader[0]}. {reader[1]} {reader[2]}");
                }
            }

            return command;
        }

        private static object ExecuteScalar(SqlConnection connection, string query, params KeyValuePair<string, string>[] keyValuePairs)
        {
            using var command = new SqlCommand(query, connection);
            foreach (var item in keyValuePairs)
            {
                command.Parameters.AddWithValue(item.Key, item.Value);
            }
            var result = command.ExecuteScalar();
            return result;
        }

        private static void GetVillainNames(SqlConnection connection)
        {
            string query = @"SELECT v.[Name], COUNT(mv.MinionId) FROM Villains v
                JOIN MinionsVillains mv ON mv.VillainId = v.Id
                GROUP BY v.Id, v.[Name]
                HAVING COUNT(mv.MinionId) > 3
                ORDER BY COUNT(mv.MinionId) DESC";

            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader[0];
                var count = reader[1];
                Console.WriteLine($"{name} - {count}");
            }
        }

        private static void InitialSetup(SqlConnection connection)
        {
            var createTableStatements = GetCreateTableStatements();

            foreach (var query in createTableStatements)
            {
                ExecuteNonQuery(connection, query);
            }

            var insertStatements = GetInsertDataStatements();

            foreach (var query in insertStatements)
            {
                ExecuteNonQuery(connection, query);
            }
        }

        private static void ExecuteNonQuery(SqlConnection connection, string query)
        {
            using var command = new SqlCommand(query, connection);
            var result = command.ExecuteNonQuery();
        }

        private static string[] GetInsertDataStatements()
        {
            var result = new string[]
            {
                "INSERT INTO Countries ([Name]) VALUES ('Bulgaria'),('England'),('Cyprus'),('Germany'),('Norway')",
                "INSERT INTO Towns ([Name], CountryCode) VALUES ('Plovdiv', 1),('Varna', 1),('Burgas', 1),('Sofia', 1),('London', 2),('Southampton', 2),('Bath', 2),('Liverpool', 2),('Berlin', 3),('Frankfurt', 3),('Oslo', 4)",
                "INSERT INTO Minions (Name,Age, TownId) VALUES('Bob', 42, 3),('Kevin', 1, 1),('Bob ', 32, 6),('Simon', 45, 3),('Cathleen', 11, 2),('Carry ', 50, 10),('Becky', 125, 5),('Mars', 21, 1),('Misho', 5, 10),('Zoe', 125, 5),('Json', 21, 1)",
                "INSERT INTO EvilnessFactors (Name) VALUES ('Super good'),('Good'),('Bad'), ('Evil'),('Super evil')",
                "INSERT INTO Villains (Name, EvilnessFactorId) VALUES ('Gru',2),('Victor',1),('Jilly',3),('Miro',4),('Rosen',5),('Dimityr',1),('Dobromir',2)",
                "INSERT INTO MinionsVillains (MinionId, VillainId) VALUES (4,2),(1,1),(5,7),(3,5),(2,6),(11,5),(8,4),(9,7),(7,1),(1,3),(7,3),(5,3),(4,3),(1,2),(2,1),(2,7)"
            };
            return result;
        }

        private static string[] GetCreateTableStatements()
        {
            var result = new string[]
            {
                "CREATE TABLE Countries(Id INT PRIMARY KEY IDENTITY, [Name] NVARCHAR(50) NOT NULL)",
                "CREATE TABLE Towns(Id INT PRIMARY KEY IDENTITY, [Name] NVARCHAR(50) NOT NULL, CountryCode INT FOREIGN KEY REFERENCES Countries(Id) NOT NULL)",
                "CREATE TABLE Minions(Id INT PRIMARY KEY IDENTITY, [Name] NVARCHAR(50) NOT NULL, Age INT NOT NULL, TownId INT FOREIGN KEY REFERENCES Towns(Id) NOT NULL)",
                "CREATE TABLE EvilnessFactors(Id INT PRIMARY KEY IDENTITY, [Name] NVARCHAR(20) NOT NULL)",
                "CREATE TABLE Villains(Id INT PRIMARY KEY IDENTITY, [Name] NVARCHAR(50) NOT NULL, EvilnessFactorId INT FOREIGN KEY REFERENCES EvilnessFactors(Id))",
                "CREATE TABLE MinionsVillains(MinionId INT FOREIGN KEY REFERENCES Minions(Id) NOT NULL, VillainId INT FOREIGN KEY REFERENCES Villains(Id) NOT NULL, CONSTRAINT PK_Minions_Villains PRIMARY KEY (MinionId, VillainId))"
            };
            return result;
        }
    }
}
