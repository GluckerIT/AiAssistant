using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;


namespace AiAssistant.Infrastructure.Tools.Notes;


public class NotesRepository
{
    private readonly string _connectionString;

    public NotesRepository()
    {
        _connectionString = PrepareConnectionString();
        InitializeDatabase();
    }

    private string PrepareConnectionString()
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
        using (var connection = new SqliteConnection(_connectionString))
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
        using (var connection = new SqliteConnection(_connectionString))
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
        using (var connection = new SqliteConnection(_connectionString))
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
        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
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
                    var lastUpdatedAt = DateTime.SpecifyKind(
                            DateTime.Parse(
                                reader.GetString(3),
                                CultureInfo.InvariantCulture), DateTimeKind.Utc).ToLocalTime();

                    var note = new Note(
                        reader.GetInt64(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        lastUpdatedAt
                    );
                    notes.Add(note);
                }
            }
        }
        return notes;
    }

    public async Task<long> CreateNoteAsync(string title, string content)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText =
                            @"
                INSERT INTO Notes (Title, Content, LastUpdatedAt)
                VALUES (@Title, @Content, CURRENT_TIMESTAMP);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Content", content);
            var newNoteId = await command.ExecuteScalarAsync();
            return (long)newNoteId;
        }
    }

    public async Task<Note?> GetNoteByIdAsync(long noteId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText =
                            @"
                SELECT Id, Title, Content, LastUpdatedAt
                FROM Notes
                WHERE Id = @Id
            ";
            command.Parameters.AddWithValue("@Id", noteId);
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var lastUpdatedAt = DateTime.SpecifyKind(
                            DateTime.Parse(
                                reader.GetString(3),
                                CultureInfo.InvariantCulture), DateTimeKind.Utc).ToLocalTime();
                    return new Note(
                        reader.GetInt64(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        lastUpdatedAt
                    );
                }
            }
        }
        return null;
    }

    public void Main()
    {
        var notesRepository = new NotesRepository();
        var noteId = notesRepository.CreateNoteAsync("Sample Note", "This is a sample note content.").Result;
        Console.WriteLine($"Created Note with ID: {noteId}");
        var note = notesRepository.GetNoteByIdAsync(noteId).Result;
        if (note != null)
        {
            Console.WriteLine($"Retrieved Note: {note.Title} - {note.Content}");
        }
        notesRepository.UpdateNoteAsync(noteId, "Updated Sample Note", "This is the updated content.").Wait();
        Console.WriteLine("Note updated.");
        var allNotes = notesRepository.GetAllNotesAsync().Result;
        Console.WriteLine($"Total Notes: {allNotes.Count}");
        
    }
}
