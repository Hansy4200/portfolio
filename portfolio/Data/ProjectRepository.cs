
using Npgsql;
using portfolio.Models;

namespace portfolio.Data;

public class ProjectRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Display all projects
    public async Task<List<Project>> GetAllAsync()
    {
        var projects = new List<Project>();

        const string sql = """
            SELECT p.project_id, p.course_id, c.title,
                   p.title, p.summary, p.description,
                   p.technologies, p.video_url,
                   p.completed_on, p.featured,
                   COUNT(i.project_image_id) AS image_count
            FROM portfolio.project p
            INNER JOIN portfolio.course c
                ON c.course_id = p.course_id
            LEFT JOIN portfolio.project_image i
                ON i.project_id = p.project_id
            GROUP BY p.project_id, c.title
            ORDER BY p.completed_on DESC NULLS LAST;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            projects.Add(ReadProject(reader));
        }

        return projects;
    }

    // Find one project
    public async Task<Project?> GetByIdAsync(long id)
    {
        const string sql = """
            SELECT p.project_id, p.course_id, c.title,
                   p.title, p.summary, p.description,
                   p.technologies, p.video_url,
                   p.completed_on, p.featured,
                   COUNT(i.project_image_id) AS image_count
            FROM portfolio.project p
            INNER JOIN portfolio.course c
                ON c.course_id = p.course_id
            LEFT JOIN portfolio.project_image i
                ON i.project_id = p.project_id
            WHERE p.project_id = @id
            GROUP BY p.project_id, c.title;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return ReadProject(reader);
    }

    // Add a project
    public async Task<long> AddAsync(Project project)
    {
        const string sql = """
            INSERT INTO portfolio.project
                (course_id, title, summary, description,
                 technologies, video_url, completed_on, featured)
            VALUES
                (@course_id, @title, @summary, @description,
                 @technologies, @video_url, @completed_on, @featured)
            RETURNING project_id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        AddParameters(command, project);

        return (long)(await command.ExecuteScalarAsync())!;
    }

    // Update a project
    public async Task<bool> UpdateAsync(Project project)
    {
        const string sql = """
            UPDATE portfolio.project
            SET course_id = @course_id,
                title = @title,
                summary = @summary,
                description = @description,
                technologies = @technologies,
                video_url = @video_url,
                completed_on = @completed_on,
                featured = @featured
            WHERE project_id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        AddParameters(command, project);
        command.Parameters.AddWithValue("id", project.ProjectId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    // Delete a project
    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = """
            DELETE FROM portfolio.project
            WHERE project_id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    private static void AddParameters(NpgsqlCommand command, Project project)
    {
        command.Parameters.AddWithValue("course_id", project.CourseId);
        command.Parameters.AddWithValue("title", project.Title);
        command.Parameters.AddWithValue("summary",
            (object?)project.Summary ?? DBNull.Value);
        command.Parameters.AddWithValue("description",
            (object?)project.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("technologies",
            (object?)project.Technologies ?? DBNull.Value);
        command.Parameters.AddWithValue("video_url",
            (object?)project.VideoUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("completed_on",
            (object?)project.CompletedOn ?? DBNull.Value);
        command.Parameters.AddWithValue("featured", project.Featured);
    }

    private static Project ReadProject(NpgsqlDataReader reader)
    {
        return new Project
        {
            ProjectId = reader.GetInt64(0),
            CourseId = reader.GetInt64(1),
            CourseTitle = reader.GetString(2),
            Title = reader.GetString(3),
            Summary = reader.IsDBNull(4) ? null : reader.GetString(4),
            Description = reader.IsDBNull(5) ? null : reader.GetString(5),
            Technologies = reader.IsDBNull(6) ? null : reader.GetString(6),
            VideoUrl = reader.IsDBNull(7) ? null : reader.GetString(7),
            CompletedOn = reader.IsDBNull(8) ? null : reader.GetFieldValue<DateOnly>(8),
            Featured = reader.GetBoolean(9),
            ImageCount = Convert.ToInt32(reader.GetInt64(10))
        };
    }
}
