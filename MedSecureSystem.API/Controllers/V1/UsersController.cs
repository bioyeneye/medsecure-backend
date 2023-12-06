namespace MedSecureSystem.API.Controllers.V1
{
    using MedSecureSystem.Application.Commons;
    using MedSecureSystem.Application.Dtos;
    using MedSecureSystem.Application.Features.Users;
    using MedSecureSystem.Domain.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.WebUtilities;
    using Swashbuckle.AspNetCore.Annotations;
    using System.Text.Encodings.Web;
    using System.Text;
    using MedSecureSystem.Shared.Query;
    using Microsoft.EntityFrameworkCore;
    using System;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : BaseIdentityController
    {
        public UsersController(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider) : base(userManager, serviceProvider)
        {
        }

        [HttpPost("ChangePassword")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Change Password", Description = "Change Password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword(InfoRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(ApiResult<bool>.NotFoundResult("User not found."));
            }

            // Check if password change is required
            var token = await _userManager.GetAuthenticationTokenAsync(user, "Default", "PasswordChangeRequired");
            if (token != "Yes")
            {
                return BadRequest(ApiResult<bool>.NotFoundResult("Password change not required."));
            }

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.TemporaryPassword))
                {
                    return BadRequest(ApiResult<bool>.NotFoundResult("The old password is required to set a new password. If the old password is forgotten, use /resetPassword."));
                }
            }

            // Verify the temporary password
            var result = await _userManager.ChangePasswordAsync(user, model.TemporaryPassword, model.NewPassword);
            if (result.Succeeded)
            {
                // Clear the flag
                await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "PasswordChangeRequired");
                return Ok(ApiResult<bool>.SuccessResult(true, "Password changed successfully."));
            }

            return BadRequest(ApiResult<bool>.FailureResult(null, "Failed to change password."));
        }

        [HttpGet("confirmEmail", Name = "confirmEmail")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Confirm email", Description = "confirm email")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> confirmEmail([FromQuery] string userId, [FromQuery] string code, [FromQuery] string? changedEmail)
        {

            if (await _userManager.FindByIdAsync(userId) is not { } user)
            {
                // We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
                return Unauthorized();
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(ApiResult<bool>.FailureResult(null, "Email verified already."));
            }

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return Unauthorized();
            }
            IdentityResult result;

            if (string.IsNullOrEmpty(changedEmail))
            {
                result = await _userManager.ConfirmEmailAsync(user, code);
            }
            else
            {
                // As with Identity UI, email and user name are one and the same. So when we update the email,
                // we need to update the user name.
                result = await _userManager.ChangeEmailAsync(user, changedEmail, code);

                if (result.Succeeded)
                {
                    result = await _userManager.SetUserNameAsync(user, changedEmail);
                }
            }

            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            return Ok(ApiResult<bool>.SuccessResult(true, "Thank you for confirming your email."));
        }

        [HttpGet("resendConfirmationEmail")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Resend Confirmation Email", Description = "Resend Confirmation Email")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest resendRequest)
        {
            if (await _userManager.FindByEmailAsync(resendRequest.Email) is not { } user)
            {
                return Ok(ApiResult<bool>.FailureResult(null, "User not found."));

            }

            await SendConfirmationEmailAsync(user, resendRequest.Email);
            return Ok(ApiResult<bool>.SuccessResult(true, "Email sent successfully."));
        }

        [HttpPost("forgotPassword")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "forgot Password", Description = "forgot Password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<bool>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> forgotPassword([FromBody] ForgotPasswordRequest resetRequest)
        {

            var user = await _userManager.FindByEmailAsync(resetRequest.Email);

            if (user is not null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var emailSender = _serviceProvider.GetRequiredService<IEmailSender<ApplicationUser>>();
                await emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
                return Ok(ApiResult<bool>.SuccessResult(true, "Account reset, kindly check your email"));
            }

            // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
            // returned a 400 for an invalid code given a valid user email.
            return BadRequest(ApiResult<bool>.FailureResult(null, "User not found"));
        }

        [HttpPost("resetPassword")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "reset Password", Description = "reset Password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<BusinessDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> resetPassword([FromBody] ResetPasswordRequest resetRequest)
        {

            var user = await _userManager.FindByEmailAsync(resetRequest.Email);

            if (user is null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
                // returned a 400 for an invalid code given a valid user email.
                return BadRequest(ApiResult<bool>.FailureResult(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken()).Errors.Select(
                    c => c.Description).ToList(), ""));
            }

            IdentityResult result;
            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
                result = await _userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
            }

            if (!result.Succeeded)
            {
                return BadRequest(ApiResult<bool>.FailureResult(result.Errors.Select(
                    c => c.Description).ToList(), ""));
            }

            return Ok(ApiResult<bool>.SuccessResult(true, "Password Reset successfully"));
        }

        [HttpGet]
        [Authorize(Roles = "SystemAdmin")]
        [SwaggerOperation(Summary = "Get users", Description = "Get all users")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBusiness([FromQuery] PaginationQuery query)
        {
            var users = _userManager.Users;
            var count = await users.CountAsync();

            if (query.Page > 0 && query.PageSize > 0)
            {
                users = users.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);
            }

            var items = users?.ToList().Select(c =>
            {
                return CreateInfoResponseAsync(c, _userManager).Result;
            }).ToList() ?? Enumerable.Empty<UserDto>().ToList();

            var result = new PaginatedResult<UserDto>(items, count, query.Page, query.PageSize);

            return Ok(ApiResult<PaginatedResult<UserDto>>.SuccessResult(result));
        }

        [HttpPut]
        [Authorize]
        [SwaggerOperation(Summary = "Update user", Description = "Update user details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResult<UserDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutUser(UpdateUserModel userModel)
        {
            var patientEmail = GetClaimValue("email");

            ApplicationUser? user = await _userManager.FindByEmailAsync(patientEmail);

            if (user == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(userModel.FirstName)) user.FirstName = userModel.FirstName;
            if (!string.IsNullOrWhiteSpace(userModel.LastName)) user.LastName = userModel.LastName;
            if (!string.IsNullOrWhiteSpace(userModel.Address)) user.Address = userModel.Address;
            if (!string.IsNullOrWhiteSpace(userModel.State)) user.State = userModel.State;
            if (!string.IsNullOrWhiteSpace(userModel.Country)) user.Country = userModel.Country;
            if (!string.IsNullOrWhiteSpace(userModel.PhoneNumber)) user.PhoneNumber = userModel.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(ApiResult<bool>.SuccessResult(true, "Account update successfully"));

            }
            return BadRequest(ApiResult<bool>.FailureResult(null, "Account update failed"));
        }

        static async Task<UserDto> CreateInfoResponseAsync(ApplicationUser user, UserManager<ApplicationUser> userManager)
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


        async Task SendConfirmationEmailAsync(ApplicationUser user, string email, bool isChange = false)
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
    }
}
