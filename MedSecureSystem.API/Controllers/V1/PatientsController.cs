using MedSecureSystem.Application.Commons;
using MedSecureSystem.Application.Dtos;
using MedSecureSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using MedSecureSystem.Domain.Enums;
using MedSecureSystem.Application.Features.Users;
using System.Security.Claims;

namespace MedSecureSystem.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class PatientsController : BaseIdentityController
    {

        public PatientsController(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider) : base(userManager, serviceProvider)
        {

        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin")]
        [SwaggerOperation(Summary = "Create patient", Description = "create patient")]
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

            await _userManager.AddToRoleAsync(user, UserTypes.Patient.ToString());

            await SendConfirmationEmailAsync(user, email);
            return Ok(ApiResult<bool>.SuccessResult(true, "Patient created successfully"));
        }

        [HttpGet("me")]
        [Authorize(Roles = "Patient")]
        [SwaggerOperation(Summary = "Get patient details", Description = "create patient")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientDeatils()
        {
            if (await _userManager.GetUserAsync(HttpContext.User) is not { } user)
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not a patient"));
            }

            return Ok(ApiResult<UserDto>.SuccessResult(await CreateInfoResponseAsync(user, _userManager)));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SystemAdmin,Patient")]
        [SwaggerOperation(Summary = "Get patient details", Description = "create patient")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientDeatil(string id)
        {
            if (await _userManager.GetUserAsync(HttpContext.User) is not { } user)
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not a patient"));
            }

            var role = GetClaimValue(ClaimTypes.Role);
            if (user.Id != id && role == UserTypes.Patient.ToString())
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not your profile"));

            }

            var userWithId = await _userManager.FindByIdAsync(id);
            if (userWithId == null)
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not a patient"));
            }

            return Ok(ApiResult<UserDto>.SuccessResult(await CreateInfoResponseAsync(userWithId, _userManager)));
        }

    }
}
