
using Npgsql;
using portfolio.Models;

namespace portfolio.Data;

public class CourseRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public CourseRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Display all courses
    public async Task<List<Course>> GetAllAsync()
    {
        var courses = new List<Course>();

        const string sql = """
            SELECT c.course_id, c.code, c.title,
                   c.description, c.term, c.school_year,
                   COUNT(p.project_id) AS project_count
            FROM portfolio.course c
            LEFT JOIN portfolio.project p
                ON p.course_id = c.course_id
            GROUP BY c.course_id
            ORDER BY c.school_year DESC, c.code;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            courses.Add(new Course
            {
                CourseId = reader.GetInt64(0),
                Code = reader.GetString(1),
                Title = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                Term = reader.GetString(4),
                SchoolYear = reader.GetString(5),
                ProjectCount = Convert.ToInt32(reader.GetInt64(6))
            });
        }

        return courses;
    }

    // Find one course
    public async Task<Course?> GetByIdAsync(long id)
    {
        const string sql = """
            SELECT course_id, code, title, description,
                   term, school_year
            FROM portfolio.course
            WHERE course_id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new Course
        {
            CourseId = reader.GetInt64(0),
            Code = reader.GetString(1),
            Title = reader.GetString(2),
            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
            Term = reader.GetString(4),
            SchoolYear = reader.GetString(5)
        };
    }

    // Add a course
    public async Task<long> AddAsync(Course course)
    {
        const string sql = """
            INSERT INTO portfolio.course
                (code, title, description, term, school_year)
            VALUES
                (@code, @title, @description, @term, @school_year)
            RETURNING course_id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("code", course.Code);
        command.Parameters.AddWithValue("title", course.Title);
        command.Parameters.AddWithValue("description",
            (object?)course.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("term", course.Term);
        command.Parameters.AddWithValue("school_year", course.SchoolYear);

        return (long)(await command.ExecuteScalarAsync())!;
    }

    // Update a course
    public async Task<bool> UpdateAsync(Course course)
    {
        const string sql = """
            UPDATE portfolio.course
            SET code = @code,
                title = @title,
                description = @description,
                term = @term,
                school_year = @school_year
            WHERE course_id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", course.CourseId);
        command.Parameters.AddWithValue("code", course.Code);
        command.Parameters.AddWithValue("title", course.Title);
        command.Parameters.AddWithValue("description",
            (object?)course.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("term", course.Term);
        command.Parameters.AddWithValue("school_year", course.SchoolYear);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    // Delete a course
    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = """
            DELETE FROM portfolio.course
            WHERE course_id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", id);

        return await command.ExecuteNonQueryAsync() > 0;
    }
}
