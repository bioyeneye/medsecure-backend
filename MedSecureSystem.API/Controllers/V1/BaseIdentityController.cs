using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using MedSecureSystem.Application.Commons;
using System.Security.Claims;

namespace MedSecureSystem.API.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BaseIdentityController : ControllerBase
    {
        protected static readonly EmailAddressAttribute _emailAddressAttribute = new();
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly IServiceProvider _serviceProvider;

        public BaseIdentityController(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        [NonAction]
        public string GetClaimValue(string key)
        {
            var claim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == key);
            return claim?.Value;
        }

        protected static async Task<UserDto> CreateInfoResponseAsync(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            return new()
            {
                Id = user.Id,
                Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
                IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = new UserAddressDto
                {
                    Address = user.Address,
                    Country = user.Country,
                    State = user.State,
                },
                Role = (await userManager.GetRolesAsync(user)).FirstOrDefault(),
            };
        }

        [NonAction]
        protected IActionResult HandleBusinessResult<T>(ApiResult<T> result)
        {
            return result.Success
                ? Ok(result)
                : (result.ApiResultStatus == ApiResultStatus.NotFound
                    ? NotFound(result)
                    : BadRequest(result));
        }

        protected async Task SendConfirmationEmailAsync(ApplicationUser user, string email, bool isChange = false)
        {
            var emailSender = _serviceProvider.GetRequiredService<IEmailSender<ApplicationUser>>();
            var linkGenerator = _serviceProvider.GetRequiredService<LinkGenerator>();


            var code = isChange
                ? await _userManager.GenerateChangeEmailTokenAsync(user, email)
                : await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var userId = await _userManager.GetUserIdAsync(user);
            var routeValues = new RouteValueDictionary()
            {
                ["userId"] = userId,
                ["code"] = code,
            };

            if (isChange)
            {
                // This is validated by the /confirmEmail endpoint on change.
                routeValues.Add("changedEmail", email);
            }

            var confirmEmailUrl = linkGenerator.GetUriByName(HttpContext, "confirmEmail", routeValues)
                ?? throw new NotSupportedException($"Could not find endpoint named 'confirmEmail'.");

            await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
        }

        protected string GetRole()
        {
            var role = GetClaimValue("role");
            if (string.IsNullOrEmpty(role))
            {
                role = GetClaimValue(ClaimTypes.Role);
            }

            return role;
        }
    }
}
