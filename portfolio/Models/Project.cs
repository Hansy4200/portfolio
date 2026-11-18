
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models;

public class Project
{
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "Please select a course.")]
    public long CourseId { get; set; }

    public string? CourseTitle { get; set; }

    [Required(ErrorMessage = "Project title is required.")]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Summary { get; set; }

    public string? Description { get; set; }

    [StringLength(250)]
    public string? Technologies { get; set; }

    [StringLength(500)]
    public string? VideoUrl { get; set; }

    public DateOnly? CompletedOn { get; set; }

    public bool Featured { get; set; }

    public int ImageCount { get; set; }

    public List<ProjectImage> Images { get; set; } = new();
}
