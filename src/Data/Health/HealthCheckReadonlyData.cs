using Data.Utils;
using MediatR;

namespace Data.Health;

public class HealthCheckReadonlyData : IRequest<bool>;

public class HealthCheckReadonlyDataHandler(IDbHelper dbHelper) : IRequestHandler<HealthCheckReadonlyData, bool>
{
    public async Task<bool> Handle(HealthCheckReadonlyData request, CancellationToken cancellationToken)
    {
        return await dbHelper.HealthCheckReadOnlyAsync(cancellationToken);
    }
}