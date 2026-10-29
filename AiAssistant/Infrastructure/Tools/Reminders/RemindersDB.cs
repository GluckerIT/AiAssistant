using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace AiAssistant.Infrastructure.Tools.Reminders;

internal class RemindersDB
{
    private readonly string _connectionString;
    public RemindersDB()
    {
        _connectionString = PrepareConnectionString();
        InitializeDatabase();
    }

    private string PrepareConnectionString()
    {
        var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var databaseDirectory = Path.Combine(localAppDataPath, "AiAssistant", "Reminders");
        Directory.CreateDirectory(databaseDirectory);
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(databaseDirectory, "Reminders.db"),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Default
        };
        return connectionStringBuilder.ToString();
    }

    private void InitializeDatabase()
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using(var command= connection.CreateCommand())
            {
                command.CommandText = "PRAGMA journal_mode = WAL;";
                command.ExecuteNonQuery();
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                @"
                CREATE TABLE IF NOT EXISTS Reminders (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Content TEXT NOT NULL,
                    CreateTime INTEGER NOT NULL,
                    ReminderTime INTEGER NOT NULL,
                    Status TEXT NOT NULL DEFAULT 'Not completed'
                );
            ";
                command.ExecuteNonQuery();
            }
        }
    }

    public bool UpdateReminder(long ReminderId, string content, long updateTime)
    {
        long nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (updateTime < nowSeconds || string.IsNullOrWhiteSpace(content))
        {
            Console.WriteLine("время не может быть меньше текущего и содержание напоминалки не могут быть пустыми.");
            return false;
        }
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                                @"
                    UPDATE Reminders 
                    SET Content = @Content, 
                        ReminderTime = @updateTime,
                        Status = 'Not completed'
                    WHERE Id = @Id
            ";
                command.Parameters.AddWithValue("@Id", ReminderId);
                command.Parameters.AddWithValue("@updateTime", updateTime);
                command.Parameters.AddWithValue("@Content", content);
                command.ExecuteNonQuery();
                return true;
            }
        }
    }

    public bool DeleteReminder(long ReminderId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                                @"
                    DELETE FROM Reminders
                    WHERE Id = @Id
            ";
                command.Parameters.AddWithValue("@Id", ReminderId);
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }

    public long CreateReminder(long time, string content)
    {
        long nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (time <nowSeconds || string.IsNullOrWhiteSpace(content))
        {
            Console.WriteLine("время не может быть меньше текущего и содержание напоминалки не могут быть пустыми.");
            return -1;
        }
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                                @"
                    INSERT INTO Reminders (Content, CreateTime, ReminderTime)
                    VALUES (@Content,@CreateTime,@ReminderTime);
                    SELECT last_insert_rowid();
            ";
                command.Parameters.AddWithValue("@CreateTime", nowSeconds);
                command.Parameters.AddWithValue("@ReminderTime", time);
                command.Parameters.AddWithValue("@Content", content);
                var newNoteId = command.ExecuteScalar();
                return (long)newNoteId;
            }
        }
    }


}
