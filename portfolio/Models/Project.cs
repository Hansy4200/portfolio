using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace portfolio.Models;

public class Project
{
    public long ProjectId { get; set; }

    [Display(Name = "Course")]
    [Range(1, int.MaxValue, ErrorMessage = "Choose a course.")]
    public long CourseId { get; set; }

    [Required(ErrorMessage = "Project title is required.")]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Display(Name = "Video link (YouTube)")]
    [StringLength(300)]
    [RegularExpression(@"^https?://(www\.)?(youtube\.com|youtu\.be)/\S+$",
        ErrorMessage = "Video link must be a YouTube link, such as https://www.youtube.com/watch?v=... or https://youtu.be/...")]
    public string? VideoUrl { get; set; }

    [Display(Name = "Completed on")]
    [DataType(DataType.Date)]
    public DateOnly? CompletedOn { get; set; }

    // Form only: one image URL per line
    [Display(Name = "Image URLs (one per line)")]
    public string? ImageUrls { get; set; }

    // Display only
    [ValidateNever] public string? CourseCode { get; set; }
    [ValidateNever] public string? CourseTitle { get; set; }
    public int ImageCount { get; set; }
    [ValidateNever] public List<ProjectImage> Images { get; set; } = new();
}