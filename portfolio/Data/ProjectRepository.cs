using Npgsql;
using NpgsqlTypes;
using portfolio.Models;

namespace portfolio.Data;

public class ProjectRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    private const string SelectProjects = """
        SELECT p.project_id, p.course_id, p.title, p.description, p.video_url,
               p.completed_on, c.code, c.title, COUNT(i.image_id)
        FROM portfolio.project p
        JOIN portfolio.course c ON c.course_id = p.course_id
        LEFT JOIN portfolio.project_image i ON i.project_id = p.project_id
        """;

    private static Project ReadProject(NpgsqlDataReader r) => new()
    {
        ProjectId = r.GetInt64(0),
        CourseId = r.GetInt64(1),
        Title = r.GetString(2),
        Description = r.IsDBNull(3) ? null : r.GetString(3),
        VideoUrl = r.IsDBNull(4) ? null : r.GetString(4),
        CompletedOn = r.IsDBNull(5) ? null : r.GetFieldValue<DateOnly>(5),
        CourseCode = r.GetString(6),
        CourseTitle = r.GetString(7),
        ImageCount = Convert.ToInt32(r.GetInt64(8))
    };

    private async Task<List<Project>> QueryAsync(string sql, long? id = null)
    {
        var list = new List<Project>();

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        if (id is not null) command.Parameters.AddWithValue("id", id.Value);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(ReadProject(reader));

        return list;
    }

    public Task<List<Project>> GetAllAsync() =>
        QueryAsync(SelectProjects + "\nGROUP BY p.project_id, c.course_id\nORDER BY c.code, p.title;");

    public Task<List<Project>> GetByCourseIdAsync(long courseId) =>
        QueryAsync(SelectProjects + "\nWHERE p.course_id = @id\nGROUP BY p.project_id, c.course_id\nORDER BY p.title;", courseId);

    public async Task<Project?> GetByIdAsync(long id)
    {
        var list = await QueryAsync(SelectProjects + "\nWHERE p.project_id = @id\nGROUP BY p.project_id, c.course_id;", id);
        return list.FirstOrDefault();
    }

    private static void AddProjectParameters(NpgsqlCommand cmd, Project p)
    {
        cmd.Parameters.AddWithValue("course_id", p.CourseId);
        cmd.Parameters.AddWithValue("title", p.Title);
        cmd.Parameters.AddWithValue("description", (object?)p.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("video_url", (object?)p.VideoUrl ?? DBNull.Value);
        cmd.Parameters.Add(new NpgsqlParameter("completed_on", NpgsqlDbType.Date)
        {
            Value = p.CompletedOn.HasValue ? (object)p.CompletedOn.Value : DBNull.Value
        });
    }

    private static async Task InsertImageAsync(NpgsqlConnection connection, NpgsqlTransaction tx,
        long projectId, string url, int sortOrder)
    {
        const string sql = """
            INSERT INTO portfolio.project_image (project_id, image_url, sort_order)
            VALUES (@project_id, @image_url, @sort_order);
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, tx);
        cmd.Parameters.AddWithValue("project_id", projectId);
        cmd.Parameters.AddWithValue("image_url", url);
        cmd.Parameters.AddWithValue("sort_order", sortOrder);
        await cmd.ExecuteNonQueryAsync();
    }

    // ONE TRANSACTION: the project and all its images, or nothing.
    public async Task<long> AddWithImagesAsync(Project project, List<string> imageUrls)
    {
        const string sql = """
            INSERT INTO portfolio.project
                (course_id, title, description, video_url, completed_on)
            VALUES
                (@course_id, @title, @description, @video_url, @completed_on)
            RETURNING project_id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var tx = await connection.BeginTransactionAsync();

        try
        {
            long projectId;
            await using (var cmd = new NpgsqlCommand(sql, connection, tx))
            {
                AddProjectParameters(cmd, project);
                projectId = (long)(await cmd.ExecuteScalarAsync())!;
            }

            var order = 1;
            foreach (var url in imageUrls)
                await InsertImageAsync(connection, tx, projectId, url, order++);

            await tx.CommitAsync();
            return projectId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ONE TRANSACTION: update the project and replace its images, or change nothing.
    public async Task<bool> UpdateWithImagesAsync(Project project, List<string> imageUrls)
    {
        const string updateSql = """
            UPDATE portfolio.project
            SET course_id = @course_id,
                title = @title,
                description = @description,
                video_url = @video_url,
                completed_on = @completed_on
            WHERE project_id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var tx = await connection.BeginTransactionAsync();

        try
        {
            int rows;
            await using (var cmd = new NpgsqlCommand(updateSql, connection, tx))
            {
                AddProjectParameters(cmd, project);
                cmd.Parameters.AddWithValue("id", project.ProjectId);
                rows = await cmd.ExecuteNonQueryAsync();
            }

            if (rows == 0)
            {
                await tx.RollbackAsync();
                return false;
            }

            await using (var del = new NpgsqlCommand(
                "DELETE FROM portfolio.project_image WHERE project_id = @id;", connection, tx))
            {
                del.Parameters.AddWithValue("id", project.ProjectId);
                await del.ExecuteNonQueryAsync();
            }

            var order = 1;
            foreach (var url in imageUrls)
                await InsertImageAsync(connection, tx, project.ProjectId, url, order++);

            await tx.CommitAsync();
            return true;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM portfolio.project WHERE project_id = @id;";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        return await command.ExecuteNonQueryAsync() > 0;
    }
}