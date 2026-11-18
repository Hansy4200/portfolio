using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace portfolio.Models;

public class Course
{
    public long CourseId { get; set; }

    [Required(ErrorMessage = "Course code is required.")]
    [StringLength(20)]
    [RegularExpression(@"^[A-Z]{2,6}[ -]?\d{2,4}[A-Z]?$",
        ErrorMessage = "Use a code like IT101 or CS-201 (capital letters, then numbers).")]
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
    [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "School year must look like 2025-2026.")]
    public string SchoolYear { get; set; } = string.Empty;

    public int ProjectCount { get; set; }

    [ValidateNever]
    public List<Project> Projects { get; set; } = new();
}