using Asp.Versioning;
using Core.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers.V1
{
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}")]
    public class DemoController() : GuestBaseController
    {
        /// <summary>
        /// Template of one endpoint
        /// </summary>
        /// <returns>
        /// Hello message
        /// </returns>
        [EnableRateLimiting("LOGIN_LIMITER")]
        [AllowAnonymous]
        [HttpGet("hello")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Hello()
        {
            var response = "Hello World!";
            return Ok(response);
        }
        
    }
}