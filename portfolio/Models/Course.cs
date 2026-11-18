
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models;

public class Course
{
    public long CourseId { get; set; }

    [Required(ErrorMessage = "Course code is required.")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Course title is required.")]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Term is required.")]
    [StringLength(30)]
    public string Term { get; set; } = string.Empty;

    [Required(ErrorMessage = "School year is required.")]
    [StringLength(20)]
    public string SchoolYear { get; set; } = string.Empty;

    public int ProjectCount { get; set; }
}
