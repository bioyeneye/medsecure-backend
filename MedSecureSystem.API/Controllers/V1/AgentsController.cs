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
using MedSecureSystem.Application.Interfaces;

namespace MedSecureSystem.API.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class AgentsController : BaseIdentityController
    {
        private static readonly EmailAddressAttribute _emailAddressAttribute = new();

        public AgentsController(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider) : base(userManager, serviceProvider)
        {
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin,BusinessAdmin")]
        [SwaggerOperation(Summary = "Create agent", Description = "create agent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] AgentRegisterRequest registration)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

                return BadRequest(ApiResult<UserDto>.FailureResult(errors, "Request error"));
            }


            var userStore = _serviceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
            var businessService = _serviceProvider.GetRequiredService<IBusinessService>();

            string role = GetRole();
            var businessid = GetClaimValue("businessid");
            if (role == UserTypes.BusinessAdmin.ToString())
            {
                if (string.IsNullOrWhiteSpace(businessid) || !long.TryParse(businessid, out long longBusinessId) || longBusinessId != registration.BusinessId)
                {
                    return BadRequest(ApiResult<UserDto>.FailureResult(null, "Sorry, you can not create an agent"));
                }
            }

            var businessResponse = await businessService.GetBusinessByIdAsync(registration.BusinessId);
            if (!businessResponse.Success)
            {
                return HandleBusinessResult(businessResponse);
            }

            var emailStore = (IUserEmailStore<ApplicationUser>)userStore;
            var email = registration.Email;
            if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
            {
                return BadRequest(ApiResult<bool>.FailureResult(null, _userManager.ErrorDescriber.InvalidEmail(email).Description));
            }

            var user = new ApplicationUser()
            {
                TenantKey = businessResponse.Data.BusinessUniqueKey,
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

            await _userManager.AddToRoleAsync(user, UserTypes.BusinessAgent.ToString());

            await SendConfirmationEmailAsync(user, email);
            return Ok(ApiResult<bool>.SuccessResult(true, "Agent created successfully"));
        }

        [HttpGet("me")]
        [Authorize(Roles = "BusinessAgent")]
        [SwaggerOperation(Summary = "Get agent details", Description = "get driver details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientDeatils()
        {
            if (await _userManager.GetUserAsync(HttpContext.User) is not { } user)
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not an agent"));
            }

            return Ok(ApiResult<UserDto>.SuccessResult(await CreateInfoResponseAsync(user, _userManager)));
        }

        // SD
        [HttpGet("{id}")]
        [Authorize(Roles = "SystemAdmin,BusinessAgent")]
        [SwaggerOperation(Summary = "Get agent details", Description = "get agent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientDeatil(string id)
        {
            if (await _userManager.GetUserAsync(HttpContext.User) is not { } user)
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not an agent"));
            }

            string role = GetRole();
            if (user.Id != id && role == UserTypes.BusinessAgent.ToString())
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not your profile"));
            }

            var userWithId = await _userManager.FindByIdAsync(id);
            if (userWithId == null)
            {
                return NotFound(ApiResult<UserDto>.NotFoundResult("Not an agent"));
            }

            return Ok(ApiResult<UserDto>.SuccessResult(await CreateInfoResponseAsync(userWithId, _userManager)));
        }


    }
}
