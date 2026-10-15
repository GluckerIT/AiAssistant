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
            using(var command= connection.CreateCommand())
            {
                command.CommandText = "PRAGMA journal_mode = WAL;";
                command.ExecuteNonQuery();
            }
            using (var command = connection.CreateCommand())
            {
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
    }

    public bool UpdateNote( long noteId, string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            Console.WriteLine("Заголовок и содержание заметки не могут быть пустыми.");
            return false;
        }
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
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
                command.ExecuteNonQuery();
                return true;
            }
        }
    }

    public bool DeleteNote(long noteId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                                @"
                    DELETE FROM Notes
                    WHERE Id = @Id
            ";
                command.Parameters.AddWithValue("@Id", noteId);
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }

    public sealed record Note(
        long Id,
        string Title,
        string Content,
        DateTime LastUpdatedAt
    );

    public IReadOnlyList<Note> GetAllNotes()
    {
        var notes = new List<Note>();
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                                @"
                    SELECT Id, Title, Content, LastUpdatedAt
                    FROM Notes
                    ORDER BY LastUpdatedAt DESC
            ";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
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
        }
        return notes;
    }

    public long CreateNote(string title, string content)
    {
        if(string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            Console.WriteLine("Заголовок и содержание заметки не могут быть пустыми.");
            return -1;
        }
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                                @"
                    INSERT INTO Notes (Title, Content, LastUpdatedAt)
                    VALUES (@Title, @Content, CURRENT_TIMESTAMP);
                    SELECT last_insert_rowid();
            ";
                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@Content", content);
                var newNoteId = command.ExecuteScalar();
                return (long)newNoteId;
            }
        }
    }

    public Note? GetNoteById(long noteId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                                @"
                    SELECT Id, Title, Content, LastUpdatedAt
                    FROM Notes
                    WHERE Id = @Id
                ";
                command.Parameters.AddWithValue("@Id", noteId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
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
        }
        return null;
    }

    public IReadOnlyList<Note> SearchNotes (string searchTerm)
    {
        if(string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<Note>();
        }
        var notesList = GetAllNotes();
        var results = new List<Note>();
        foreach (var note in notesList)
        {
            if (note.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                note.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(note);
            }
            
        }
        return results;
    }

    public void Main()
    {
        var notesRepository = new NotesRepository();
        List<Note> notes = notesRepository.GetAllNotes().ToList();
        foreach(var note in notes)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine($"Id: {note.Id}");
            Console.WriteLine($"Title: {note.Title}");
            Console.WriteLine($"Content: {note.Content}");
            Console.WriteLine($"Last Updated At: {note.LastUpdatedAt}");
        }

        //List<Note> searchResults = notesRepository.SearchNotes("test").ToList();
        //foreach (var note in searchResults)
        //{
        //    Console.WriteLine("--------------------");
        //    Console.WriteLine($"Id: {note.Id}");
        //    Console.WriteLine($"Title: {note.Title}");
        //    Console.WriteLine($"Content: {note.Content}");
        //    Console.WriteLine($"Last Updated At: {note.LastUpdatedAt}");
        //}

        //Console.WriteLine("\nВведите Id заметки, которую хотите отредактировать:");
        //int noteId;
        //while(!int.TryParse(Console.ReadLine(), out noteId) || notesRepository.GetNoteById(noteId) == null)
        //{
        //    Console.Write("Заметка с таким id не найдена.Введите другой id: ");
        //}

        //Note noteById = notesRepository.GetNoteById(noteId);

        //Console.WriteLine("Изменить заголовок заметки? (да/нет)");
        //string response = Console.ReadLine();
        //string newTitle = noteById.Title;
        //string newContent = noteById.Content;
        //while (response?.ToLower() != "да" && response?.ToLower() != "нет")
        //{
        //    Console.WriteLine("Введите корректный ответ (да/нет):");
        //    response = Console.ReadLine();
        //}
        //if (response?.ToLower() == "да")
        //{
        //    Console.WriteLine($"Старый заголовок заметки:{noteById.Title}");
        //    Console.Write("Введите новый заголовок заметки:");
        //    newTitle = Console.ReadLine();
        //}

        //Console.WriteLine("Изменить содержание заметки? (да/нет)");
        //string response2 = Console.ReadLine();
        //while (response2?.ToLower() != "да" && response2?.ToLower() != "нет")
        //{
        //    Console.WriteLine("Введите корректный ответ (да/нет):");
        //    response2 = Console.ReadLine();
        //}
        //if (response2?.ToLower() == "да")
        //{
        //    Console.WriteLine($"Старое содержание заметки:{noteById.Content}");
        //    Console.Write("Введите новое содержание заметки:");
        //    newContent = Console.ReadLine();
        //}
        //notesRepository.UpdateNote(noteId, newTitle, newContent);







        //Console.WriteLine("\nВведите Id заметки, которую хотите удалить:");
        //int number;
        //while (!int.TryParse(Console.ReadLine(), out number))
        //{
        //    Console.Write("Введите корректный id: ");
        //}


        //var deletedNote = notesRepository.DeleteNote(number);
        //if (deletedNote)
        //{
        //    Console.WriteLine($"Заметка с Id {number} удалена успешно.");
        //}
        //else
        //{
        //    Console.WriteLine($"Заметка с Id {number} не найдена.");
        //}
    }
}
