using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Api.Data;
using WorkoutTracker.Api.Models;

namespace WorkoutTracker.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExercisesController : ControllerBase
{
    private readonly WorkoutDbContext _db;

    public ExercisesController(WorkoutDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// DTO для создания упражнения.
    /// </summary>
    public record CreateExerciseDto(
        string Name,
        int ProgramId,
        bool IsActive = true);

    /// <summary>
    /// DTO для обновления упражнения.
    /// </summary>
    public record UpdateExerciseDto(
        string Name,
        int ProgramId,
        bool? IsActive = null);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly = null)
    {
        var query = _db.Exercises
            .Include(e => e.Program)
            .AsQueryable();

        if (activeOnly == true)
        {
            query = query.Where(e => e.IsActive);
        }

        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var exercise = await _db.Exercises
            .Include(e => e.Program)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exercise == null)
        {
            return NotFound();
        }

        return Ok(exercise);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExerciseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Название упражнения обязательно.");
        }

        if (dto.Name.Length > 100)
        {
            return BadRequest("Название упражнения не должно превышать 100 символов.");
        }

        var program = await _db.TrainingPrograms.FindAsync(dto.ProgramId);

        if (program == null)
        {
            return BadRequest("Программа не найдена.");
        }

        var exercise = new Exercise
        {
            Name = dto.Name,
            ProgramId = dto.ProgramId,
            IsActive = dto.IsActive
        };

        _db.Exercises.Add(exercise);

        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = exercise.Id },
            exercise);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExerciseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Название упражнения обязательно.");
        }

        if (dto.Name.Length > 100)
        {
            return BadRequest("Название упражнения не должно превышать 100 символов.");
        }

        var exercise = await _db.Exercises.FindAsync(id);

        if (exercise == null)
        {
            return NotFound();
        }

        var program = await _db.TrainingPrograms.FindAsync(dto.ProgramId);

        if (program == null)
        {
            return BadRequest("Программа не найдена.");
        }

        exercise.Name = dto.Name;
        exercise.ProgramId = dto.ProgramId;

        if (dto.IsActive.HasValue)
        {
            exercise.IsActive = dto.IsActive.Value;
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var exercise = await _db.Exercises.FindAsync(id);

        if (exercise == null)
        {
            return NotFound();
        }

        // Мягкое удаление, чтобы не ломать существующие активности.
        exercise.IsActive = false;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}