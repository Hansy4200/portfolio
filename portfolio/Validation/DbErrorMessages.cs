using Npgsql;

namespace portfolio.Validation;

public static class DbErrorMessages
{
    public static string Describe(PostgresException ex)
    {
        switch (ex.SqlState)
        {
            case PostgresErrorCodes.UniqueViolation: // 23505
                return ex.ConstraintName switch
                {
                    "uq_project_image_url" => "The same image URL appears more than once for this project. Nothing was saved.",
                    "uq_project_course_title" => "This course already has a project with that title.",
                    "uq_course_code" => "That course code is already used.",
                    _ => "That value already exists. Codes, titles and image URLs must be unique."
                };

            case PostgresErrorCodes.ForeignKeyViolation: // 23503
                return "This change breaks a link between records. A course that still has projects cannot be deleted, and a project needs an existing course.";

            case PostgresErrorCodes.CheckViolation: // 23514
                return ex.ConstraintName switch
                {
                    "ck_project_video_url" => "The video link must be a YouTube link.",
                    "ck_project_image_url" => "An image URL must start with http(s):// and end in .jpg, .jpeg, .png, .gif or .webp.",
                    "ck_course_school_year" => "School year must look like 2025-2026.",
                    "ck_course_term" => "Term must be 1st Semester, 2nd Semester or Summer.",
                    "ck_course_code_format" => "Use a code like IT101 or CS-201.",
                    "ck_project_title_not_blank" => "The project title cannot be blank.",
                    _ => "A value broke a database rule. Please check your entries."
                };

            case PostgresErrorCodes.NotNullViolation:
                return "A required value is missing.";

            default:
                return "The database refused this change. Please try again.";
        }
    }
}