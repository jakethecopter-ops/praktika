using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WorkoutTracker.Api.Models;

public class TrainingProgram
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [JsonIgnore]  // ← Добавлено!
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}