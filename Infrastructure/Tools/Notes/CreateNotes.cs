using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;


namespace AiAssistant.Infrastructure.Tools.Notes;


public class NotesRepository
{
    private readonly string _databasePath;

    public NotesRepository()
    {
        _databasePath = BuildConnectionString();
        InitializeDatabase();
    }

    private string BuildConnectionString()
    {
        var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var databaseDirectory = Path.Combine(localAppDataPath, "AiAssistant", "Notes");
        Directory.CreateDirectory(databaseDirectory);
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(databaseDirectory, "notes.db"),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Default
        };
        return connectionStringBuilder.ToString();
    }

    private void InitializeDatabase()
    {
        using (var connection = new SqliteConnection(_databasePath))
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

    public async Task UpdateNoteAsync( long noteId, string title, string content)
    {
        using (var connection = new SqliteConnection(_databasePath))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText =
                            @"
                UPDATE Notes
                SET Title = @Title, 
                    Content = @Content, 
                    LastUpdatedAt = CURRENT_TIMESTAMP
                WHERE Id = @Id
            ";
            command.Parameters.AddWithValue("@Id", noteId);
            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Content", content);
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task DeleteNoteAsync(long noteId)
    {
        using (var connection = new SqliteConnection(_databasePath))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText =
                            @"
                DELETE FROM Notes
                WHERE Id = @Id
            ";
            command.Parameters.AddWithValue("@Id", noteId);
            await command.ExecuteNonQueryAsync();
        }
    }

    public sealed record Note(
        long Id,
        string Title,
        string Content,
        DateTime LastUpdatedAt
    );

    public async Task<IReadOnlyList<Note>> GetAllNotesAsync()
    {
        var notes = new List<Note>();
        using (var conection = new SqliteConnection(_databasePath))
        {
            await conection.OpenAsync();
            var command = conection.CreateCommand();
            command.CommandText =
                            @"
                SELECT Id, Title, Content, LastUpdatedAt
                FROM Notes
                ORDER BY LastUpdatedAt DESC
            ";
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var note = new Note(
                        reader.GetInt64(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        DateTime.Parse(reader.GetString(3))
                    );
                    notes.Add(note);
                }
            }
        }
        return notes;
    }

}
