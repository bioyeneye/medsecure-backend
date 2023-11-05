using MedSecureSystem.Application.Commons;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Domain.Entities;
using MedSecureSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using MedSecureSystem.Application.Features.Users;
using System.Security.Claims;

namespace MedSecureSystem.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class DriversController : BaseIdentityController
    {
        private static readonly EmailAddressAttribute _emailAddressAttribute = new();

        public DriversController(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider) : base(userManager, serviceProvider)
        {
        }

        /**
        Add driver
        **/
        [HttpPost]
        [Authorize(Roles = "SystemAdmin")]
        [SwaggerOperation(Summary = "Create driver", Description = "create driver")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] RegisterRequest registration)
        {

            var userStore = _serviceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
            var emailStore = (IUserEmailStore<ApplicationUser>)userStore;
            var email = registration.Email;

            if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
            {
                return BadRequest(ApiResult<bool>.FailureResult(null, _userManager.ErrorDescriber.InvalidEmail(email).Description));
            }

            var user = new ApplicationUser()
            {
                TenantKey = "SYSTEM",
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                PhoneNumber = registration.PhoneNumber,
                State = registration.State,
                Address = registration.Address,
                Country = registration.Country,
                DateCreated = DateTime.UtcNow
            };

            await userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, registration.Password);

            if (!result.Succeeded)
            {
                return BadRequest(ApiResult<bool>.FailureResult(result.Errors.Select(c => c.Description).ToList(), "Error"));
            }

            await _userManager.AddToRoleAsync(user, UserTypes.Driver.ToString());

            await SendConfirmationEmailAsync(user, email);
            return Ok(ApiResult<bool>.SuccessResult(true, "Driver created successfully"));
        }

        [HttpGet("me")]
        [Authorize(Roles = "Driver")]
        [SwaggerOperation(Summary = "Get driver details", Description = "create driver")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientDeatils()
        {
            if (await _userManager.GetUserAsync(HttpContext.User) is not { } user)
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not a driver"));
            }

            return Ok(ApiResult<UserDto>.SuccessResult(await CreateInfoResponseAsync(user, _userManager)));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SystemAdmin,Driver")]
        [SwaggerOperation(Summary = "Get driver details", Description = "get driver")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientDeatil(string id)
        {
            if (await _userManager.GetUserAsync(HttpContext.User) is not { } user)
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not a driver"));
            }

            string role = GetRole();
            if (user.Id != id && role == UserTypes.Driver.ToString())
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not your profile"));

            }

            var userWithId = await _userManager.FindByIdAsync(id);
            if (userWithId == null)
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not a driver"));
            }

            return Ok(ApiResult<UserDto>.SuccessResult(await CreateInfoResponseAsync(userWithId, _userManager)));
        }


    }
}
