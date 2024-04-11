using System.Security.Claims;
using Authorization;
using Authorization.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TelegramEvents.Fasad;
using DataBase.Entities.Entities_DBContext;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TelegramEvents.Controllers
{
    [Route("api")]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.SessionCookie)]
    public class AuthController : ControllerBase
    {
        private readonly IUserFacade _userFacade;

        public AuthController(
            IConfiguration configuration,
            IUserFacade userFacade
        )
        {
            _userFacade = userFacade;
        }

        // [Route("login")]
        // [HttpGet]
        // [AllowAnonymous]
        // [SwaggerOperation(Summary = "Signs in as a User account", Description = "You must use User account in order to interact with the rest of the API")]
        // //[ProducesResponseType(, StatusCodes.Status200OK)]
        // public async Task<IActionResult> LoginAsync()
        // {
        //     try
        //     {
        //         string sessionUID = HttpContext.Request.Cookies["SessionUID"];
        //
        //         if (string.IsNullOrEmpty(sessionUID))
        //         {
        //             return BadRequest("SessionUID cookie is missing.");
        //         }
        //
        //         var result = await _userFacade.LogInAsync(sessionUID);
        //
        //         if(result is null)
        //         {
        //             return BadRequest("User is missing.");
        //         }
        //
        //         return Ok(result.FirstName);
        //     }
        //     catch (System.Exception e)
        //     {
        //         return BadRequest(e.Message);
        //     }
        // }
        //
        //
        // [Route("login")]
        // [HttpPost]
        // [AllowAnonymous]
        // [SwaggerOperation(Summary = "Signs in as a User account", Description = "You must use User account in order to interact with the rest of the API")]
        // //[ProducesResponseType(, StatusCodes.Status200OK)]
        // public async Task<IActionResult> LoginAsync([FromBody] LoginDto login_request)
        // {
        //     try
        //     {
        //         var result = await _userFacade.AuthenticateAsync(login_request);
        //     
        //         if (result.succes)
        //         {
        //             HttpContext.Response.Cookies.Append("SessionUID", result.session, new CookieOptions
        //             {
        //                 SameSite = SameSiteMode.Lax,
        //                 Expires = DateTimeOffset.Now.AddDays(1),
        //                 IsEssential = true
        //             });
        //     
        //             return Ok(GetUserForFront(result.user));
        //         }
        //         else
        //         {
        //             return BadRequest(result);
        //         }
        //     }
        //     catch (System.Exception e)
        //     {
        //         return BadRequest(e.Message);
        //     }
        // }
        
        [Route("loginTelegram")]
        [HttpGet]
        // [AllowAnonymous]
        //[SwaggerOperation(Summary = "Signs in as a User account", Description = "You must use User account in order to interact with the rest of the API")]
        //[ProducesResponseType(, StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoginTelegramAsync()
        {
            try
            {
                string? sessionUID = HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "SessionUID")?.Value;
                string? userIdRaw = HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                long? userId = !string.IsNullOrWhiteSpace(userIdRaw) ? Convert.ToInt64(userIdRaw) : null;
                
                if (string.IsNullOrEmpty(sessionUID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "SessionUID cookie is missing"
                    });
                }
                
                // Perform the actual logout action using _userService.LogoutAsync
                var user = await _userFacade.LogInAsync(sessionUID, userId);

                if (user is null)
                {
                    // Clear the "SessionUID" cookie to log the user out
                    Response.Cookies.Delete("SessionUID");
                    
                    return BadRequest(new
                    {
                        success = false,
                        message = "User is missing"
                    });
                }
                
                return Ok(GetUserForFront(user));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("loginTelegram")]
        [HttpPost]
        [AllowAnonymous]
        //[SwaggerOperation(Summary = "Signs in as a User account", Description = "You must use User account in order to interact with the rest of the API")]
        //[ProducesResponseType(, StatusCodes.Status200OK)]
        public async Task<IActionResult> PostLoginTelegramAsync([FromBody] string requestLoginTelegram)
        {
            try
            {
                var result = await _userFacade.AuthenticateTelegramAsync(requestLoginTelegram);
                
                if(result.succes)
                {
                    // Создаем утверждения (claims) пользователя
                    var claims = new List<Claim>
                    {
                        new Claim("SessionUID", result.session),
                        new Claim("UserId", result.user.Id.ToString()),
                        new Claim("Role", result.user.Role),
                        // Другие утверждения, если необходимо
                    };

                    // Создаем объект ClaimsIdentity на основе утверждений (claims)
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Создаем объект AuthenticationProperties
                    var authProperties = new AuthenticationProperties
                    {
                        // Настройки аутентификации, например, время жизни куки
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24),
                        // Другие настройки, если необходимо
                    };

                    // Вызываем метод SignInAsync для создания и отправки аутентификационных куки
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                    
                    return Ok(GetUserForFront(result.user));
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (System.Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [Route("logout")]
        [HttpPost]
        [SwaggerOperation(Summary = "Signs out as a User account", Description = "You must use User account in order to interact with the rest of the API")]
        //[ProducesResponseType(typeof(LoginSend_model), StatusCodes.Status200OK)]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                string? sessionUID = HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "SessionUID")?.Value;
                
                if (string.IsNullOrEmpty(sessionUID))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "SessionUID cookie is missing"
                    });
                }

                // Perform the actual logout action using _userService.LogoutAsync
                await _userFacade.LogoutAsync(sessionUID);

                // Clear the "SessionUID" cookie to log the user out
                Response.Cookies.Delete("SessionUID");

                return Ok(new
                {
                    success = true,
                    message = "User logged out successfully."
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = e.Message
                });
            }
        }

        private object GetUserForFront(User user)
        {
            return new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                login = user.UserName,
                email = user.Email,
                isActive = !user.IsDeleted,
                role = user.Role,
                avatar = ""
            };

            /*
             * id: string;
    avatar: string;
    firstName: string;
    lastName: string;
    login: string;
    email: string;
    isActive: boolean;
             */
        }

    }
}
