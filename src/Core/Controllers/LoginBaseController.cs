using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [ApiController]
    [Authorize(AuthorizationPolicies.LoginPolicy)]
    public class LoginBaseController : ControllerBase;
}