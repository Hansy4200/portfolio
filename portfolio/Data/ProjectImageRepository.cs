
using Npgsql;
using portfolio.Models;

namespace portfolio.Data;

public class ProjectImageRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectImageRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Display images for one project
    public async Task<List<ProjectImage>> GetByProjectIdAsync(long projectId)
    {
        var images = new List<ProjectImage>();

        const string sql = """
            SELECT project_image_id, project_id,
                   image_url, alt_text, display_order
            FROM portfolio.project_image
            WHERE project_id = @project_id
            ORDER BY display_order, project_image_id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("project_id", projectId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            images.Add(new ProjectImage
            {
                ProjectImageId = reader.GetInt64(0),
                ProjectId = reader.GetInt64(1),
                ImageUrl = reader.GetString(2),
                AltText = reader.GetString(3),
                DisplayOrder = reader.GetInt32(4)
            });
        }

        return images;
    }

    // Find one image
    public async Task<ProjectImage?> GetByIdAsync(long id)
    {
        const string sql = """
            SELECT project_image_id, project_id,
                   image_url, alt_text, display_order
            FROM portfolio.project_image
            WHERE project_image_id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new ProjectImage
        {
            ProjectImageId = reader.GetInt64(0),
            ProjectId = reader.GetInt64(1),
            ImageUrl = reader.GetString(2),
            AltText = reader.GetString(3),
            DisplayOrder = reader.GetInt32(4)
        };
    }

    // Add an image
    public async Task<long> AddAsync(ProjectImage image)
    {
        const string sql = """
            INSERT INTO portfolio.project_image
                (project_id, image_url, alt_text, display_order)
            VALUES
                (@project_id, @image_url, @alt_text, @display_order)
            RETURNING project_image_id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        AddParameters(command, image);

        return (long)(await command.ExecuteScalarAsync())!;
    }

    // Update an image
    public async Task<bool> UpdateAsync(ProjectImage image)
    {
        const string sql = """
            UPDATE portfolio.project_image
            SET project_id = @project_id,
                image_url = @image_url,
                alt_text = @alt_text,
                display_order = @display_order
            WHERE project_image_id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        AddParameters(command, image);
        command.Parameters.AddWithValue("id", image.ProjectImageId);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    // Delete an image
    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = """
            DELETE FROM portfolio.project_image
            WHERE project_image_id = @id;
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", id);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    private static void AddParameters(
        NpgsqlCommand command, ProjectImage image)
    {
        command.Parameters.AddWithValue("project_id", image.ProjectId);
        command.Parameters.AddWithValue("image_url", image.ImageUrl);
        command.Parameters.AddWithValue("alt_text", image.AltText);
        command.Parameters.AddWithValue("display_order", image.DisplayOrder);
    }
}
