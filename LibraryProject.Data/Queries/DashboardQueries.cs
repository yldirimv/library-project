using Dapper;
using LibraryProject.Model.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LibraryProject.Data.Queries
{
    public class DashboardQueries
    {
        private readonly string _connectionString;

        public DashboardQueries(IConfiguration configuration)
            => _connectionString = configuration.GetConnectionString("Default")!;

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM Reservations WHERE Status = 1) AS CurrentlyInside,
                    (SELECT COUNT(*) FROM Reservations WHERE Status = 2) AS OnBreak,
                    (SELECT COUNT(*) FROM Reservations
                        WHERE StartTime >= CAST(GETDATE() AS date)
                          AND StartTime <  DATEADD(day, 1, CAST(GETDATE() AS date))) AS TodaysReservations,
                    (SELECT COUNT(*) FROM BookLoans WHERE Status = 0) AS ActiveLoans,
                    (SELECT COUNT(*) FROM BookLoans
                        WHERE Status = 0 AND DueDate < GETDATE()) AS OverdueLoans,
                    (SELECT COUNT(*) FROM AspNetUsers u
                        INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                        INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                        WHERE r.Name = 'Visitor') AS TotalVisitors,
                    (SELECT COUNT(*) FROM GiftRequests WHERE Status = 0) AS PendingGiftRequests";

            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<AdminDashboardDto>(sql);
        }
    }
}