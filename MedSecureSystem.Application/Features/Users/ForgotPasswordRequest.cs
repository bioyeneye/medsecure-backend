using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

namespace MedSecureSystem.Application.Features.Users
{
    /// <summary>
    /// The response type for the "/forgotPassword" endpoint added by <see cref="IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi"/>.
    /// </summary>
    public sealed class ForgotPasswordRequest
    {
        /// <summary>
        /// The email address to send the reset password code to if a user with that confirmed email address already exists.
        /// </summary>
        public required string Email { get; init; }
    }

    public class RegisterRequest
    {
        /// <summary>
        /// The user's email address which acts as a user name.
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// The user's password.
        /// </summary>
        public required string Password { get; init; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
    }

    public sealed class AgentRegisterRequest : RegisterRequest
    {
        [Required]
        public long BusinessId { get; set; }
    }

    public class UpdateUserModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
    }
}
