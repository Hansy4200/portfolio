
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models;

public class ProjectImage
{
    public long ProjectImageId { get; set; }

    [Required]
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "Image URL is required.")]
    [StringLength(1000)]
    public string ImageUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Alternative text is required.")]
    [StringLength(150)]
    public string AltText { get; set; } = string.Empty;

    [Range(0, 1000)]
    public int DisplayOrder { get; set; }
}
