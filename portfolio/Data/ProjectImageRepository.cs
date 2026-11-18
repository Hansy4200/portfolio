using Npgsql;
using portfolio.Models;

namespace portfolio.Data;

public class ProjectImageRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectImageRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    private static ProjectImage Read(NpgsqlDataReader r) => new()
    {
        ImageId = r.GetInt64(0),
        ProjectId = r.GetInt64(1),
        ImageUrl = r.GetString(2),
        Caption = r.IsDBNull(3) ? null : r.GetString(3),
        SortOrder = r.GetInt32(4)
    };

    public async Task<List<ProjectImage>> GetByProjectAsync(long projectId)
    {
        const string sql = """
            SELECT image_id, project_id, image_url, caption, sort_order
            FROM portfolio.project_image
            WHERE project_id = @project_id
            ORDER BY sort_order, image_id;
            """;

        var list = new List<ProjectImage>();

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("project_id", projectId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(Read(reader));

        return list;
    }

    public async Task<ProjectImage?> GetByIdAsync(long id)
    {
        const string sql = """
            SELECT image_id, project_id, image_url, caption, sort_order
            FROM portfolio.project_image
            WHERE image_id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Read(reader) : null;
    }

    public async Task<long> AddAsync(ProjectImage image)
    {
        const string sql = """
            INSERT INTO portfolio.project_image (project_id, image_url, caption, sort_order)
            VALUES (@project_id, @image_url, @caption, @sort_order)
            RETURNING image_id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("project_id", image.ProjectId);
        command.Parameters.AddWithValue("image_url", image.ImageUrl);
        command.Parameters.AddWithValue("caption", (object?)image.Caption ?? DBNull.Value);
        command.Parameters.AddWithValue("sort_order", image.SortOrder);

        return (long)(await command.ExecuteScalarAsync())!;
    }

    public async Task<bool> UpdateAsync(ProjectImage image)
    {
        const string sql = """
            UPDATE portfolio.project_image
            SET image_url = @image_url,
                caption = @caption,
                sort_order = @sort_order
            WHERE image_id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", image.ImageId);
        command.Parameters.AddWithValue("image_url", image.ImageUrl);
        command.Parameters.AddWithValue("caption", (object?)image.Caption ?? DBNull.Value);
        command.Parameters.AddWithValue("sort_order", image.SortOrder);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM portfolio.project_image WHERE image_id = @id;";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        return await command.ExecuteNonQueryAsync() > 0;
    }
}