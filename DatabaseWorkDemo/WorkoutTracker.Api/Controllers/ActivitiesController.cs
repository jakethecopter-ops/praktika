using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Api.Data;
using WorkoutTracker.Api.Models;

namespace WorkoutTracker.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivitiesController : ControllerBase
{
    private readonly WorkoutDbContext _db;

    public ActivitiesController(WorkoutDbContext db) => _db = db;

    /// <summary>
    /// DTO для создания активности
    /// </summary>
    public record CreateActivityDto(
        DateOnly Date,
        int DurationMinutes,
        string? Notes,
        int ExerciseId);

    /// <summary>
    /// DTO для обновления активности (без ExerciseId — его менять нельзя)
    /// </summary>
    public record UpdateActivityDto(
        DateOnly Date,
        int DurationMinutes,
        string? Notes);

    /// <summary>
    /// DTO для визуализации стикера
    /// </summary>
    public record DailySummaryDto(
        DateOnly Date,
        int TotalMinutes,
        string StickerColor);

    /// <summary>
    /// Получить список активностей с фильтрацией
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateOnly? date = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var query = _db.Activities
            .Include(a => a.Exercise)
            .ThenInclude(e => e!.Program)
            .AsQueryable();

        if (date.HasValue)
        {
            query = query.Where(a => a.Date == date.Value);
        }
        else if (year.HasValue && month.HasValue)
        {
            query = query.Where(a => a.Date.Year == year && a.Date.Month == month);
        }

        var result = await query.OrderByDescending(a => a.Date).ToListAsync();
        return Ok(result);
    }

    /// <summary>
    /// Получить сводку за день с цветным стикером
    /// </summary>
    [HttpGet("daily-summary")]
    public async Task<IActionResult> GetDailySummary([FromQuery] DateOnly date)
    {
        var totalMinutes = await _db.Activities
            .Where(a => a.Date == date)
            .SumAsync(a => a.DurationMinutes);

        var color = totalMinutes < 30 ? "Yellow" :
                    totalMinutes <= 90 ? "Green" : "Red";

        return Ok(new DailySummaryDto(date, totalMinutes, color));
    }

    /// <summary>
    /// Создать новую активность
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActivityDto dto)
    {
        // 1. Проверка: упражнение должно существовать и быть активным
        var exercise = await _db.Exercises.FindAsync(dto.ExerciseId);
        if (exercise == null)
        {
            return BadRequest("Упражнение не найдено");
        }
        if (!exercise.IsActive)
        {
            return BadRequest("Нельзя добавить активность для неактивного упражнения");
        }

        // 2. Проверка: лимит 1440 минут в день
        var dayTotal = await _db.Activities
            .Where(a => a.Date == dto.Date)
            .SumAsync(a => a.DurationMinutes);

        if (dayTotal + dto.DurationMinutes > 1440)
        {
            return BadRequest("Превышен лимит активности: максимум 1440 минут в сутки");
        }

        // 3. Создание активности
        var activity = new Activity
        {
            Date = dto.Date,
            DurationMinutes = dto.DurationMinutes,
            Notes = dto.Notes,
            ExerciseId = dto.ExerciseId
        };

        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = activity.Id }, activity);
    }

    /// <summary>
    /// Обновить активность (ExerciseId менять нельзя)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateActivityDto dto)
    {
        var activity = await _db.Activities
            .Include(a => a.Exercise)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity == null)
        {
            return NotFound();
        }

        // Если упражнение стало неактивным — запрещаем менять ExerciseId
        // (в DTO его и нет, но на всякий случай проверяем)
        if (activity.Exercise != null && !activity.Exercise.IsActive)
        {
            // ExerciseId менять нельзя — это уже учтено в DTO
        }

        // Проверка лимита 1440 минут
        var dayTotal = await _db.Activities
            .Where(a => a.Date == dto.Date && a.Id != id)
            .SumAsync(a => a.DurationMinutes);

        if (dayTotal + dto.DurationMinutes > 1440)
        {
            return BadRequest("Превышен лимит активности: максимум 1440 минут в сутки");
        }

        activity.Date = dto.Date;
        activity.DurationMinutes = dto.DurationMinutes;
        activity.Notes = dto.Notes;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Удалить активность
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var activity = await _db.Activities.FindAsync(id);
        if (activity == null)
        {
            return NotFound();
        }

        _db.Activities.Remove(activity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}