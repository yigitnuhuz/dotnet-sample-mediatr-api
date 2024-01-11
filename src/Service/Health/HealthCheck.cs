using System.Diagnostics;
using Core.Context;
using Data.Health;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Service.Health
{
    public class HealthCheck : IRequest<HealthCheckResponse>;

    public class HealthCheckHandler(
        IHttpContextAccessor contextAccessor,
        ICoreAppContext coreAppContext,
        IMediator mediator)
        : IRequestHandler<HealthCheck, HealthCheckResponse>
    {
        private Stopwatch _sw;

        public async Task<HealthCheckResponse> Handle(HealthCheck serviceRequest, CancellationToken cancellationToken)
        {
            _sw = Stopwatch.StartNew();

            var response = new HealthCheckResponse
            {
                ServiceUp = true,
                DataUp = true,
                DataNodes = new Dictionary<string, bool>(),
                Ip = string.Empty,
                Headers = new List<string>(),
                Duration = 0
            };

            #region Data Requests

            try
            {
                response.DataNodes.Add(nameof(HealthCheckData), await mediator.Send(new HealthCheckData(), cancellationToken));
            }
            catch
            {
                response.DataNodes.Add(nameof(HealthCheckData), false);
                response.DataUp = false;
            }

            try
            {
                response.DataNodes.Add(nameof(HealthCheckReadonlyData), await mediator.Send(new HealthCheckReadonlyData(), cancellationToken));
            }
            catch
            {
                response.DataNodes.Add(nameof(HealthCheckReadonlyData), false);
                response.DataUp = false;
            }

            #endregion

            response.Ip = coreAppContext.Ip();

            response.Headers = contextAccessor.HttpContext.Request.Headers.Select(s => $"ClientKey: {s.Key} | Value: {s.Value}").ToList();

            if (!response.DataUp) response.ServiceUp = false;

            response.Duration = _sw.Elapsed.Milliseconds;

            return response;
        }
    }

    public class HealthCheckResponse
    {
        public bool ServiceUp { get; set; }
        public bool DataUp { get; set; }
        public Dictionary<string, bool> DataNodes { get; set; }
        public string Ip { get; set; }
        public List<string> Headers { get; set; }
        public int Duration { get; set; }
    }
}