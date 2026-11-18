using Microsoft.AspNetCore.Mvc;
using portfolio.Data;

namespace portfolio.Controllers;

public class CoursesController : Controller
{
    private readonly CourseRepository _courses;
    private readonly ProjectRepository _projects;

    public CoursesController(CourseRepository courses, ProjectRepository projects)
    {
        _courses = courses;
        _projects = projects;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _courses.GetAllAsync());
    }

    public async Task<IActionResult> Details(long? id)
    {
        if (id is null) return NotFound();

        var course = await _courses.GetByIdAsync(id.Value);
        if (course is null) return NotFound();

        course.Projects = await _projects.GetByCourseIdAsync(id.Value);
        return View(course);
    }
}