using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WorkoutTracker.Api.Models;

public class Activity
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    [Range(1, 1440)]
    public int DurationMinutes { get; set; }

    [MaxLength(255)]
    public string? Notes { get; set; }

    public int ExerciseId { get; set; }

    [ForeignKey(nameof(ExerciseId))]
    [JsonIgnore]  // ← Добавлено!
    public Exercise? Exercise { get; set; }
}