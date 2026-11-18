using Microsoft.AspNetCore.Mvc;
using portfolio.Data;

namespace portfolio.Controllers;

public class ProjectsController : Controller
{
    private readonly ProjectRepository _projects;
    private readonly ProjectImageRepository _images;

    public ProjectsController(ProjectRepository projects, ProjectImageRepository images)
    {
        _projects = projects;
        _images = images;
    }

    public async Task<IActionResult> Details(long? id)
    {
        if (id is null) return NotFound();

        var project = await _projects.GetByIdAsync(id.Value);
        if (project is null) return NotFound();

        project.Images = await _images.GetByProjectAsync(project.ProjectId);
        return View(project);
    }
}