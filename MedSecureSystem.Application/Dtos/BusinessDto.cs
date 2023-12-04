using MedSecureSystem.Domain.Entities;
using MedSecureSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MedSecureSystem.Application.Features.Delivery;

namespace MedSecureSystem.Application.Dtos
{
    public class BusinessDto
    {
        public string BusinessUniqueKey { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public long Id { get; set; }

    }

    public class BusinessDetailsDto
    {
        public long Id { get; set; }
        public string Admin { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class InfoResponse
    {
        /// <summary>
        /// The email address associated with the authenticated user.
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// Indicates whether or not the <see cref="Email"/> has been confirmed yet.
        /// </summary>
        public required bool IsEmailConfirmed { get; init; }
    }

    public class DeliveryDto
    {
        public long Id { get; set; }
        public string Status { get; set; }

        public string BusinessName { get; set; }

        public string DriverName { get; set; }
        public string AgentName { get; set; }
        public string PatientName { get; set; }


        public DateTime? AgentAcceptedTime { get; set; }
        public DateTime? AgentCompletedTime { get; set; }

        // Timestamps for driver actions
        public DateTime? DriverAcceptedTime { get; set; }
        public DateTime? DriverStartedDeliveryTime { get; set; }
        public DateTime? DriverCompletedTime { get; set; }

        public DateTime? PatientReceivedTime { get; set; }
        public DateTime CreatedAt { get; set; }


        public virtual List<DeliveryRequestItemModel> Items { get; set; }
    }


    public class UserDto
    {
        public string Email { get; init; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public UserAddressDto Address { get; set; }
        public string Id { get; set; }
        public string? Role { get; set; }
    }

    public class UserAddressDto
    {
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
    }

    public class BusinessCredentialsDto
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool IsActive { get; set; }
    }


    public class FeedbackModel
    {
        public long Id { get; set; }
        public long DeliveryRequestId { get; set; }
        public string UserType { get; set; } // Enum indicating whether the user is a Patient, Agent, or Driver
        public string Commenter { get; set; }
        public string Comments { get; set; }
    }
}