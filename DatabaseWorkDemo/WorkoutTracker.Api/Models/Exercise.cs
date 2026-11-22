using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WorkoutTracker.Api.Models;

public class Exercise
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int ProgramId { get; set; }

    [ForeignKey(nameof(ProgramId))]
    [JsonIgnore]  // ← Добавлено!
    public TrainingProgram? Program { get; set; }

    public bool IsActive { get; set; } = true;

    [JsonIgnore]  // ← Добавлено!
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}