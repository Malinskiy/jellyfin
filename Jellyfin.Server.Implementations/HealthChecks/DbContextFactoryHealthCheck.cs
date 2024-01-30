using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Jellyfin.Server.Implementations.HealthChecks {
    /// <summary>
    /// </summary>
    public class DbContextFactoryHealthCheck : IHealthCheck
    {
        private readonly JellyfinDbProvider _dbProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextFactoryHealthCheck"/> class.
        /// </summary>
        /// <param name="dbProvider">Instance of the <see cref="JellyfinDbProvider"/>.</param>
        public DbContextFactoryHealthCheck(JellyfinDbProvider dbProvider)
        {
            _dbProvider = dbProvider;
        }

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            await using var dbContext = _dbProvider.CreateContext();
            await using (dbContext.ConfigureAwait(false))
            {
                if (await dbContext.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false))
                {
                    try
                    {
                        await dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
                        return HealthCheckResult.Healthy();
                    }
                    catch (Exception exc)
                    {
                        return HealthCheckResult.Unhealthy(exception: exc);
                    }
                }
            }

            return HealthCheckResult.Unhealthy();
        }
    }
}
