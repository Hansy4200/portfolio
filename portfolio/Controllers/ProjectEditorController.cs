using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using portfolio.Data;
using portfolio.Models;
using portfolio.Validation;

namespace portfolio.Controllers;

public class ProjectEditorController : Controller
{
    private readonly ProjectRepository _projects;
    private readonly ProjectImageRepository _images;
    private readonly CourseRepository _courses;

    public ProjectEditorController(ProjectRepository projects, ProjectImageRepository images, CourseRepository courses)
    {
        _projects = projects;
        _images = images;
        _courses = courses;
    }

    private async Task LoadCoursesAsync()
    {
        var courses = await _courses.GetAllAsync();
        ViewBag.Courses = courses
            .Select(c => new SelectListItem($"{c.Code} - {c.Title}", c.CourseId.ToString()))
            .ToList();
    }

    public async Task<IActionResult> Index()
    {
        return View(await _projects.GetAllAsync());
    }

    public async Task<IActionResult> Create(long? courseId)
    {
        await LoadCoursesAsync();
        return View(new Project { CourseId = courseId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        // Server-side check of every image URL, line by line
        var urls = ImageUrlRules.Validate(project.ImageUrls, ModelState, nameof(Project.ImageUrls));

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync();
            return View(project);
        }

        try
        {
            await _projects.AddWithImagesAsync(project, urls);
            TempData["Message"] = "Project added.";
            return RedirectToAction(nameof(Index));
        }
        catch (PostgresException ex)
        {
            // The transaction already rolled back: nothing was saved.
            ModelState.AddModelError("", DbErrorMessages.Describe(ex));
            await LoadCoursesAsync();
            return View(project);
        }
    }

    public async Task<IActionResult> Edit(long? id)
    {
        if (id is null) return NotFound();

        var project = await _projects.GetByIdAsync(id.Value);
        if (project is null) return NotFound();

        var images = await _images.GetByProjectAsync(project.ProjectId);
        project.ImageUrls = string.Join(Environment.NewLine, images.Select(i => i.ImageUrl));

        await LoadCoursesAsync();
        return View(project);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Project project)
    {
        if (id != project.ProjectId) return NotFound();

        var urls = ImageUrlRules.Validate(project.ImageUrls, ModelState, nameof(Project.ImageUrls));

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync();
            return View(project);
        }

        try
        {
            var updated = await _projects.UpdateWithImagesAsync(project, urls);
            if (!updated) return NotFound();

            TempData["Message"] = "Project updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (PostgresException ex)
        {
            ModelState.AddModelError("", DbErrorMessages.Describe(ex));
            await LoadCoursesAsync();
            return View(project);
        }
    }

    public async Task<IActionResult> Delete(long? id)
    {
        if (id is null) return NotFound();

        var project = await _projects.GetByIdAsync(id.Value);
        if (project is null) return NotFound();

        return View(project);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            var deleted = await _projects.DeleteAsync(id);
            if (!deleted) return NotFound();

            TempData["Message"] = "Project deleted.";
            return RedirectToAction(nameof(Index));
        }
        catch (PostgresException ex)
        {
            var project = await _projects.GetByIdAsync(id);
            if (project is null) return NotFound();

            ModelState.AddModelError("", DbErrorMessages.Describe(ex));
            return View(project);
        }
    }
}