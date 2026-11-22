using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Api.Data;
using WorkoutTracker.Api.Models;

namespace WorkoutTracker.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProgramsController : ControllerBase
{
    private readonly WorkoutDbContext _db;
    public ProgramsController(WorkoutDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.TrainingPrograms.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(TrainingProgram program)
    {
        _db.TrainingPrograms.Add(program);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = program.Id }, program);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TrainingProgram program)
    {
        if (id != program.Id) return BadRequest();
        _db.Entry(program).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var program = await _db.TrainingPrograms.FindAsync(id);
        if (program == null) return NotFound();
        _db.TrainingPrograms.Remove(program);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}