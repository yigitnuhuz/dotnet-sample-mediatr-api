using Data.Utils;
using MediatR;

namespace Data.Health;

public class HealthCheckData : IRequest<bool>;

public class HealthCheckDataHandler(IDbHelper dbHelper) : IRequestHandler<HealthCheckData, bool>
{
    public async Task<bool> Handle(HealthCheckData request, CancellationToken cancellationToken)
    {
        return await dbHelper.HealthCheckAsync(cancellationToken);
    }
}