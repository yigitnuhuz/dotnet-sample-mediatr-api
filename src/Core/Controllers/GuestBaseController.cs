using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Authorize(AuthorizationPolicies.GuestPolicy)]
    public class GuestBaseController : ControllerBase;
}