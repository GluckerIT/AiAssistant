using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AiAssistant.Infrastructure.Tools.Notes;

namespace ASPAiAssistant.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotesController : ControllerBase
{
    private readonly NotesRepository _notesRepository;

    public NotesController(NotesRepository notesRepository)
    {
        _notesRepository = notesRepository;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<NotesRepository.Note>> GetAllNotes()
    {
        var notes = _notesRepository.GetAllNotes();
        return Ok(notes);
    }

    [HttpGet("{id:long}")]
    public ActionResult<NotesRepository.Note> GetNoteById(long id)
    {
        var note = _notesRepository.GetNoteById(id);
        if (note == null)
        {
            return NotFound();
        }
        return Ok(note);
    }

    [HttpDelete("{id:long}")]
    public IActionResult DeleteNote(long id)
    {
        var deleted = _notesRepository.DeleteNote(id);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost]
    public ActionResult CreateNote([FromBody] NotesRepository.Note note)
    {
        if (note == null || string.IsNullOrWhiteSpace(note.Title) || string.IsNullOrWhiteSpace(note.Content))
        {
            return BadRequest("Note title and content cannot be empty.");
        }
        var createdNote = _notesRepository.CreateNote(note.Title, note.Content);
        return Ok(createdNote);
    }

    [HttpPut("{id:long}")]
    public ActionResult UpdateNote(long id, [FromBody] NotesRepository.Note note)
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
