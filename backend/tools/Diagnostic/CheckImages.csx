using Npgsql;
var conn = new NpgsqlConnection("Host=localhost;Port=5432;Database=aimarketingagency;Username=postgres;Password=postgres");
await conn.OpenAsync();
Console.WriteLine("=== IMAGE URLs IN GENERATED CONTENT ===");
await using var cmd = new NpgsqlCommand(@"SELECT ""Title"", ""ImageUrl"", ""ImagePrompt"" FROM generated_contents ORDER BY ""CreatedAt"" DESC", conn);
await using var reader = await cmd.ExecuteReaderAsync();
while (await reader.ReadAsync()) {
    var title = reader.GetString(0).Substring(0, Math.Min(60, reader.GetString(0).Length));
    var imgUrl = reader.IsDBNull(1) ? "(null)" : reader.GetString(1).Substring(0, Math.Min(80, reader.GetString(1).Length));
    var imgPrompt = reader.IsDBNull(2) ? "(null)" : reader.GetString(2).Substring(0, Math.Min(60, reader.GetString(2).Length));
    Console.WriteLine($"  Title: {title}");
    Console.WriteLine($"  ImageUrl: {imgUrl}");
    Console.WriteLine($"  ImagePrompt: {imgPrompt}");
    Console.WriteLine();
}
await conn.CloseAsync();
