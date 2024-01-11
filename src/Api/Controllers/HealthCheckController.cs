using System.Net;
using Core.Dtos;
using Core.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Health;

namespace Api.Controllers
{
    public class HealthCheckController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Returns application status.
        /// </summary>
        /// <response code="500">If an unexpected error occurs.</response>
        /// <response code="503">If endpoint is temporarily unavailable.</response>
        [HttpGet]
        [AllowAnonymous]
        [Route("")]
        [ProducesResponseType(typeof(ApiResponseDto<string>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiErrorDto), (int) HttpStatusCode.BadRequest)]
        public IActionResult GetStatus()
        {
            return ActionResultHelper.Ok("Healthy");
        }

        /// <summary>
        /// Returns application health check status with client headers.
        /// </summary>
        /// <response code="500">If an unexpected error occurs.</response>
        /// <response code="503">If endpoint is temporarily unavailable.</response>
        [HttpGet]
        [AllowAnonymous]
        [Route("health")]
        [ProducesResponseType(typeof(ApiResponseDto<HealthCheckResponse>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiErrorDto), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> HealthCheck()
        {
            var result = await mediator.Send(new HealthCheck());

            return ActionResultHelper.Ok(result);
        }
    }
}