using Npgsql;
var conn = new NpgsqlConnection("Host=localhost;Port=5432;Database=aimarketingagency;Username=postgres;Password=postgres");
conn.Open();

Console.WriteLine("=== ALL TENANTS ===");
using (var cmd = new NpgsqlCommand("SELECT \"Id\", \"Name\" FROM tenants", conn))
using (var reader = cmd.ExecuteReader()) {
    while (reader.Read()) Console.WriteLine($"  Tenant: {reader[0]} | {reader[1]}");
}

Console.WriteLine("\n=== ALL SUBSCRIPTIONS ===");
using (var cmd = new NpgsqlCommand("SELECT \"Id\", \"TenantId\", \"PlanTier\", \"MaxJobsPerMonth\", \"CurrentPeriodEnd\" FROM subscriptions", conn))
using (var reader = cmd.ExecuteReader()) {
    while (reader.Read()) Console.WriteLine($"  Sub: {reader[0]} | Tenant={reader[1]} | Tier={reader[2]} | MaxJobs={reader[3]} | PeriodEnd={reader[4]}");
}
if (true) { } // force flush

Console.WriteLine("\n=== JOBS PER TENANT THIS MONTH ===");
using (var cmd = new NpgsqlCommand(@"
    SELECT j.""TenantId"", t.""Name"", COUNT(*) as job_count, 
           COUNT(*) FILTER (WHERE j.""CreatedAt"" >= date_trunc('month', NOW() AT TIME ZONE 'UTC')) as jobs_this_month
    FROM agent_jobs j 
    LEFT JOIN tenants t ON t.""Id"" = j.""TenantId""
    GROUP BY j.""TenantId"", t.""Name""", conn))
using (var reader = cmd.ExecuteReader()) {
    while (reader.Read()) Console.WriteLine($"  Tenant={reader[0]} | Name={reader[1]} | TotalJobs={reader[2]} | ThisMonth={reader[3]}");
}

Console.WriteLine("\n=== ALL USERS ===");
using (var cmd = new NpgsqlCommand("SELECT \"Id\", \"Email\", \"TenantId\", \"FullName\" FROM users", conn))
using (var reader = cmd.ExecuteReader()) {
    while (reader.Read()) Console.WriteLine($"  User: {reader[0]} | {reader[1]} | Tenant={reader[2]} | {reader[3]}");
}

conn.Close();
