using Microsoft.AspNetCore.Mvc;
using Npgsql;
using portfolio.Data;
using portfolio.Models;
using portfolio.Validation;

namespace portfolio.Controllers;

public class ImageEditorController : Controller
{
    private readonly ProjectRepository _projects;
    private readonly ProjectImageRepository _images;

    public ImageEditorController(ProjectRepository projects, ProjectImageRepository images)
    {
        _projects = projects;
        _images = images;
    }

    public async Task<IActionResult> Index(long? projectId)
    {
        if (projectId is null) return NotFound();

        var project = await _projects.GetByIdAsync(projectId.Value);
        if (project is null) return NotFound();

        project.Images = await _images.GetByProjectAsync(project.ProjectId);
        return View(project);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectImage image)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" ",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index), new { projectId = image.ProjectId });
        }

        try
        {
            await _images.AddAsync(image);
            TempData["Message"] = "Image added.";
        }
        catch (PostgresException ex)
        {
            TempData["Error"] = DbErrorMessages.Describe(ex);
        }

        return RedirectToAction(nameof(Index), new { projectId = image.ProjectId });
    }

    public async Task<IActionResult> Edit(long? id)
    {
        if (id is null) return NotFound();

        var image = await _images.GetByIdAsync(id.Value);
        if (image is null) return NotFound();

        return View(image);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, ProjectImage image)
    {
        if (id != image.ImageId) return NotFound();
        if (!ModelState.IsValid) return View(image);

        try
        {
            var updated = await _images.UpdateAsync(image);
            if (!updated) return NotFound();

            TempData["Message"] = "Image updated.";
            return RedirectToAction(nameof(Index), new { projectId = image.ProjectId });
        }
        catch (PostgresException ex)
        {
            ModelState.AddModelError("", DbErrorMessages.Describe(ex));
            return View(image);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, long projectId)
    {
        try
        {
            var deleted = await _images.DeleteAsync(id);
            if (!deleted) return NotFound();

            TempData["Message"] = "Image deleted.";
        }
        catch (PostgresException ex)
        {
            TempData["Error"] = DbErrorMessages.Describe(ex);
        }

        return RedirectToAction(nameof(Index), new { projectId });
    }
}