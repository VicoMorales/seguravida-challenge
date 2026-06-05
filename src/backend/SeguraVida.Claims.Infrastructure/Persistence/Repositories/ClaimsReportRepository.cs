using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SeguraVida.Claims.Application.Reports;

namespace SeguraVida.Claims.Infrastructure.Persistence.Repositories;

public sealed class ClaimsReportRepository : IClaimsReportRepository
{
    private readonly ClaimsDbContext _dbContext;

    public ClaimsReportRepository(ClaimsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ClaimSummaryRow>> GetClaimsSummaryAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        await using var command = _dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = "dbo.GetClaimsSummary";
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@FromDate", (object?)fromDate?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@ToDate", (object?)toDate?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value));

        await _dbContext.Database.OpenConnectionAsync(cancellationToken);

        var rows = new List<ClaimSummaryRow>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ClaimSummaryRow(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetDecimal(3)));
        }

        return rows;
    }
}
