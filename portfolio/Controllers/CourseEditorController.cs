using Microsoft.AspNetCore.Mvc;
using Npgsql;
using portfolio.Data;
using portfolio.Models;
using portfolio.Validation;

namespace portfolio.Controllers;

public class CourseEditorController : Controller
{
    private readonly CourseRepository _courses;

    public CourseEditorController(CourseRepository courses) => _courses = courses;

    public async Task<IActionResult> Index()
    {
        return View(await _courses.GetAllAsync());
    }

    public IActionResult Create() => View(new Course());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        if (!ModelState.IsValid) return View(course);

        try
        {
            await _courses.AddAsync(course);
            TempData["Message"] = "Course added.";
            return RedirectToAction(nameof(Index));
        }
        catch (PostgresException ex)
        {
            ModelState.AddModelError("", DbErrorMessages.Describe(ex));
            if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                ModelState.AddModelError(nameof(Course.Code), "This course code is already used.");
            return View(course);
        }
    }

    public async Task<IActionResult> Edit(long? id)
    {
        if (id is null) return NotFound();

        var course = await _courses.GetByIdAsync(id.Value);
        if (course is null) return NotFound();

        return View(course);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Course course)
    {
        if (id != course.CourseId) return NotFound();
        if (!ModelState.IsValid) return View(course);

        try
        {
            var updated = await _courses.UpdateAsync(course);
            if (!updated) return NotFound();

            TempData["Message"] = "Course updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (PostgresException ex)
        {
            ModelState.AddModelError("", DbErrorMessages.Describe(ex));
            if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                ModelState.AddModelError(nameof(Course.Code), "This course code is already used.");
            return View(course);
        }
    }

    public async Task<IActionResult> Delete(long? id)
    {
        if (id is null) return NotFound();

        var course = await _courses.GetByIdAsync(id.Value);
        if (course is null) return NotFound();

        return View(course);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            var deleted = await _courses.DeleteAsync(id);
            if (!deleted) return NotFound();

            TempData["Message"] = "Course deleted.";
            return RedirectToAction(nameof(Index));
        }
        catch (PostgresException ex)
        {
            var course = await _courses.GetByIdAsync(id);
            if (course is null) return NotFound();

            ModelState.AddModelError("", DbErrorMessages.Describe(ex));
            return View(course);
        }
    }
}