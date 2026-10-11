using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using System.IO;


namespace AiAssistant.Infrastructure.Tools.Notes;


public class CreateNotes
{
    private static string GetDatabasePath()
    {
        var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var databaseDirectory = Path.Combine(localAppDataPath, "AiAssistant", "Notes");
        Directory.CreateDirectory(databaseDirectory);
        var bilderDB = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(databaseDirectory, "notes.db"),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Default
        };
        return bilderDB.ToString();
    }

    private static void CreateDatabase(string databasePath)
    {
        using (var connection = new SqliteConnection(databasePath))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Notes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    LastUpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );
            ";
            command.ExecuteNonQuery();
        }
    }

    private static void UpdateNotes(string databasePath, string elementId, string title, string content)
    {
        using (var connection = new SqliteConnection(databasePath))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
                            @"
                UPDATE Notes
                SET Title = @Title, 
                    Content = @Content, 
                    LastUpdatedAt = CURRENT_TIMESTAMP
                WHERE Id = @Id
            ";
            command.Parameters.AddWithValue("@Id", elementId);
            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Content", content);
            command.ExecuteNonQuery();
        }
    }

    private static void Main(string[] args)
    {
        var databasePath = GetDatabasePath();
        CreateDatabase(databasePath);
        Console.WriteLine($"Database created at: {databasePath}");
    }



}
