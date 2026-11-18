using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace portfolio.Validation;

public static class ImageUrlRules
{
    public const string Pattern =
        @"^https?://\S+\.(jpg|jpeg|png|gif|webp|JPG|JPEG|PNG|GIF|WEBP)(\?\S*)?$";

    // Checks each line with Regex.IsMatch. Returns the good URLs; bad lines become form errors.
    public static List<string> Validate(string? text, ModelStateDictionary modelState, string key)
    {
        var urls = new List<string>();
        var lines = (text ?? "").Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0) continue;

            if (!Regex.IsMatch(line, Pattern, RegexOptions.IgnoreCase))
            {
                modelState.AddModelError(key,
                    $"Line {i + 1} is not a valid image URL. It must start with http:// or https:// and end in .jpg, .jpeg, .png, .gif or .webp.");
            }
            else
            {
                urls.Add(line);
            }
        }
        return urls;
    }
}