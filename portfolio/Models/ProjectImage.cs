using System.ComponentModel.DataAnnotations;
using portfolio.Validation;

namespace portfolio.Models;

public class ProjectImage
{
    public long ImageId { get; set; }
    public long ProjectId { get; set; }

    [Display(Name = "Image URL")]
    [Required(ErrorMessage = "Image URL is required.")]
    [StringLength(500)]
    [RegularExpression(ImageUrlRules.Pattern,
        ErrorMessage = "Image URL must start with http:// or https:// and end in .jpg, .jpeg, .png, .gif or .webp.")]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Caption { get; set; }

    [Display(Name = "Sort order")]
    [Range(0, 9999, ErrorMessage = "Sort order must be 0 or more.")]
    public int SortOrder { get; set; }
}