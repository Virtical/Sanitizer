using FluentResults.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Sanitizer.Api.Controllers.Internal.Client.Requests;
using Sanitizer.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Sanitizer.Api.Controllers.Internal;

[Route("api/login")]
public class LoginController(UsersService usersService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(Summary = "Авторизация")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request) => (await usersService.LoginAsync(request)).ToActionResult();
}