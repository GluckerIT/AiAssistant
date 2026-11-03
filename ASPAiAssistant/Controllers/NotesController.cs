using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AiAssistant.Infrastructure.Tools.Notes;
using System.ComponentModel.DataAnnotations;

namespace ASPAiAssistant.Controllers;

public class NoteRequest
{
    [Required]
    public string Title { get; set; } = "";
    [Required]
    public string Content { get; set; } = "";
}


[Route("api/[controller]")]
[ApiController]
public class NotesController : ControllerBase
{
    private readonly NotesRepository _notesRepository;

    public NotesController(NotesRepository notesRepository)
    {
        _notesRepository = notesRepository;
    }

    [HttpGet("GetAllNotes")]
    public ActionResult<IReadOnlyList<NotesRepository.Note>> GetAllNotes()
    {
        var notes = _notesRepository.GetAllNotes();
        return Ok(notes);
    }

    [HttpGet("GetNote/{id:long}")]
    public ActionResult<NotesRepository.Note> GetNoteById(long id)
    {
        var note = _notesRepository.GetNoteById(id);
        if (note == null)
        {
            return NotFound();
        }
        return Ok(note);
    }

    [HttpDelete("DeleteNote/{id:long}")]
    public IActionResult DeleteNote(long id)
    {
        var deleted = _notesRepository.DeleteNote(id);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("CreateNote")]
    public ActionResult CreateNote([FromBody] NoteRequest note)
    {
        if (note == null || string.IsNullOrWhiteSpace(note.Title) || string.IsNullOrWhiteSpace(note.Content))
        {
            return BadRequest("Note title and content cannot be empty.");
        }
        var createdNote = _notesRepository.CreateNote(note.Title, note.Content);
        if (createdNote < 0)
        {
            return BadRequest("Note was not created.");
        }

        var created = _notesRepository.GetNoteById(createdNote);
        return CreatedAtAction(nameof(GetNoteById), new { id = createdNote }, created);
    }

    [HttpPut("UpdateNote/{id:long}")]
    public ActionResult UpdateNote(long id, [FromBody] NoteRequest note)
    {
        if (note == null || string.IsNullOrWhiteSpace(note.Title)|| string.IsNullOrWhiteSpace(note.Content))
        {
            return BadRequest("Note title and content cannot be empty.");
        }

        var updated = _notesRepository.UpdateNote(id, note.Title, note.Content);

        if (!updated)
        {
            return NotFound();
        }
        return NoContent();
    }
}

