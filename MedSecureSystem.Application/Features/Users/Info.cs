using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedSecureSystem.Application.Features.Users
{
    public sealed class InfoRequest
    {
        /// <summary>
        /// The optional new email address for the authenticated user. This will replace the old email address if there was one. The email will not be updated until it is confirmed.
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// The optional new password for the authenticated user. If a new password is provided, the <see cref="OldPassword"/> is required.
        /// If the user forgot the old password, use the "/forgotPassword" endpoint instead.
        /// </summary>
        public string? NewPassword { get; init; }

        /// <summary>
        /// The old password for the authenticated user. This is only required if a <see cref="NewPassword"/> is provided.
        /// </summary>
        public string? TemporaryPassword { get; init; }
    }
}
