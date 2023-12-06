namespace MedSecureSystem.API.Controllers.V1
{
    using MedSecureSystem.Application.Commons;
    using MedSecureSystem.Application.Dtos;
    using MedSecureSystem.Application.Features.Businesses.Commands;
    using MedSecureSystem.Application.Interfaces;
    using MedSecureSystem.Domain.Entities;
    using MedSecureSystem.Domain.Enums;
    using MedSecureSystem.Shared.Query;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using System.Security.Claims;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class BusinessController : BaseIdentityController
    {
        private readonly IBusinessService _businessService;

        public BusinessController(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider, IBusinessService businessService) : base(userManager, serviceProvider)
        {
            _businessService = businessService;
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin")]
        [SwaggerOperation(Summary = "Create business", Description = "Initial business creation process")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessCommand request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}"; // Construct the base URL
            var verificationUrl = $"{baseUrl}/api/v1/business/complete";

            var businessResult = await _businessService.CreateBusinessAsync(request, verificationUrl);
            return businessResult.Success ? Ok(businessResult) : BadRequest(businessResult);
        }

        [HttpGet]
        [Authorize(Roles = "SystemAdmin,Patient")]
        [SwaggerOperation(Summary = "Get businesses", Description = "Get all business")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessDetailsDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessDetailsDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBusiness([FromQuery] PaginationQuery query)
        {
            var businessResult = await _businessService.GetBusinessessAsync(query);
            return businessResult.Success ? Ok(businessResult) : BadRequest(businessResult);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SystemAdmin,BusinessAdmin")]
        [SwaggerOperation(Summary = "Get business", Description = "Get single business")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessDetailsDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessDetailsDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBusiness([FromRoute] int id)
        {
            var role = GetClaimValue("role");
            var businessid = GetClaimValue("businessid");
            if (role == UserTypes.BusinessAdmin.ToString())
            {
                if (string.IsNullOrWhiteSpace(businessid) || !long.TryParse(businessid, out long longBusinessId) || longBusinessId != id)
                {
                    return BadRequest(ApiResult<UserDto>.FailureResult(null, "Sorry, business detail cant be queried"));
                }
            }

            var businessResult = await _businessService.GetBusinessByIdAsync(id);
            return businessResult.Success
                ? Ok(businessResult)
                : (businessResult.ApiResultStatus == ApiResultStatus.NotFound ? NotFound(businessResult) : BadRequest(businessResult));
        }

        [HttpGet("complete")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Comlete creation business", Description = "Complete business creation process")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompleteBusinessRegistration([FromQuery] CompleteRegistrationCommand model)
        {

            var result = await _businessService.CompleteVerificationAsync(model);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("{id}/credentials")]
        [Authorize(Roles = "SystemAdmin,BusinessAdmin")]
        [SwaggerOperation(Summary = "Create business credential", Description = "Initial business credential creation process")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessCredentialsDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessCredentialsDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBusinessCredentials([FromRoute] long id)
        {
            string role = GetRole();
            var businessid = GetClaimValue("businessid");
            if (role == UserTypes.BusinessAdmin.ToString())
            {
                if (string.IsNullOrWhiteSpace(businessid) || !long.TryParse(businessid, out long longBusinessId) || longBusinessId != id)
                {
                    return BadRequest(ApiResult<UserDto>.FailureResult(null, "Sorry, you can create a business credential"));
                }
            }

            var businessResult = await _businessService.GenerateCredential(id);
            return HandleBusinessResult(businessResult);
        }

        [HttpGet("{id}/credentials")]
        [Authorize(Roles = "BusinessAdmin")]
        [SwaggerOperation(Summary = "Get business credentials", Description = "Get business credentials")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessCredentialsDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessCredentialsDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBusinessCredentials([FromRoute] long id)
        {
            string role = GetRole();
            var businessid = GetClaimValue("businessid");
            if (role == UserTypes.BusinessAdmin.ToString())
            {
                if (string.IsNullOrWhiteSpace(businessid) || !long.TryParse(businessid, out long longBusinessId) || longBusinessId != id)
                {
                    return BadRequest(ApiResult<UserDto>.FailureResult(null, "Sorry, you can not retrieve the business credentials"));
                }

            }

            var businessResult = await _businessService.GetCredentials(id);
            return HandleBusinessResult(businessResult);
        }

        [HttpGet("{id}/credentials/{clientid}")]
        [Authorize(Roles = "BusinessAdmin")]
        [SwaggerOperation(Summary = "Get business credential", Description = "Get business credential")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessCredentialsDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessCredentialsDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBusinessCredentials([FromRoute] long id, [FromRoute] string clientid)
        {
            string role = GetRole();
            var businessid = GetClaimValue("businessid");
            if (role == UserTypes.BusinessAdmin.ToString())
            {
                if (string.IsNullOrWhiteSpace(businessid) || !long.TryParse(businessid, out long longBusinessId) || longBusinessId != id)
                {
                    return BadRequest(ApiResult<UserDto>.FailureResult(null, "\"Sorry, you can not retrieve the business credential"));
                }

            }

            var businessResult = await _businessService.GetCredentialById(id, clientid);
            return HandleBusinessResult(businessResult);
        }

        [HttpPut("{id}/credentials/{clientid}/disable")]
        [Authorize(Roles = "BusinessAdmin")]
        [SwaggerOperation(Summary = "Disable business credential", Description = "Disable business credential")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessCredentialsDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessCredentialsDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DisableBusinessCredential([FromRoute] long id, [FromRoute] string clientid)
        {
            string role = GetRole();
            var businessid = GetClaimValue("businessid");
            if (role == UserTypes.BusinessAdmin.ToString())
            {
                if (string.IsNullOrWhiteSpace(businessid) || !long.TryParse(businessid, out long longBusinessId) || longBusinessId != id)
                {
                    return BadRequest(ApiResult<UserDto>.FailureResult(null, "\"Sorry, you can not retrieve the business credential"));
                }

            }

            var businessResult = await _businessService.EnableCredential(id, clientid, false);
            return HandleBusinessResult(businessResult);
        }

        [HttpPut("{id}/credentials/{clientid}/enable")]
        [Authorize(Roles = "BusinessAdmin")]
        [SwaggerOperation(Summary = "Enable business credential", Description = "Enable business credential")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessCredentialsDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessCredentialsDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EnableBusinessCredential([FromRoute] long id, [FromRoute] string clientid)
        {
            string role = GetRole();
            var businessid = GetClaimValue("businessid");
            if (role == UserTypes.BusinessAdmin.ToString())
            {
                if (string.IsNullOrWhiteSpace(businessid) || !long.TryParse(businessid, out long longBusinessId) || longBusinessId != id)
                {
                    return BadRequest(ApiResult<UserDto>.FailureResult(null, "\"Sorry, you can not retrieve the business credential"));
                }

            }

            var businessResult = await _businessService.EnableCredential(id, clientid, true);
            return HandleBusinessResult(businessResult);
        }
    }
}
