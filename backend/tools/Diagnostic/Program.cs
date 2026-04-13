using Npgsql;

var conn = new NpgsqlConnection("Host=localhost;Port=5432;Database=aimarketingagency;Username=postgres;Password=postgres");
await conn.OpenAsync();

Console.WriteLine("=== IMAGE URLs IN GENERATED CONTENT ===");
await using (var cmd = new NpgsqlCommand(@"SELECT ""Title"", ""ImageUrl"", ""ImagePrompt"" FROM generated_contents ORDER BY ""CreatedAt"" DESC", conn))
await using (var reader = await cmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        var title = reader.GetString(0);
        if (title.Length > 60) title = title.Substring(0, 60);
        var imgUrl = reader.IsDBNull(1) ? "(null)" : reader.GetString(1);
        if (imgUrl.Length > 100) imgUrl = imgUrl.Substring(0, 100);
        var imgPrompt = reader.IsDBNull(2) ? "(null)" : reader.GetString(2);
        if (imgPrompt.Length > 60) imgPrompt = imgPrompt.Substring(0, 60);
        Console.WriteLine($"  Title: {title}");
        Console.WriteLine($"  ImageUrl: {imgUrl}");
        Console.WriteLine($"  ImagePrompt: {imgPrompt}");
        Console.WriteLine();
    }
}

await conn.CloseAsync();
