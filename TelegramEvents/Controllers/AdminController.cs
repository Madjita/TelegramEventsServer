using Authorization;
using Authorization.Bearer.Authorization;
using CQRS.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyLoggerNamespace;
using MyLoggerNamespace.Enums;
using DataBase.Entities.Entities_DBContext;
using Microsoft.AspNetCore.Authorization;

namespace TelegramEvents.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.SessionCookie)]
public class AdminController : ControllerBase
{
    private readonly IMyLogger _logger;
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _logger = MyLoggerNamespace.Logger.InitLogger(this.GetType().Name);
        _mediator = mediator;
    }
    
    [Route("getTelegramUsers")]
    [HttpGet]
    [Authorize(Policy = nameof(AdminRoleRequirement))]
    public async Task<IActionResult> GetUsers()
    {
        List<User> telegramUSersDto = await _mediator.Send(new GetUserAllQuery());
        
        _logger.WriteLine(MessageType.Debug,$"telegramUSersDto=[{telegramUSersDto}]");
        
        return Ok(telegramUSersDto); 
    }
}