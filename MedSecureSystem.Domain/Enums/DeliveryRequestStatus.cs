using System.ComponentModel;

namespace MedSecureSystem.Domain.Enums
{
    public enum DeliveryRequestStatus
    {
        [Description("Requested")]
        Requested,
        [Description("Preparing for pickup")]
        Preparing,
        [Description("Pickup Ready")]
        ReadyForPickup,
        [Description("Delivery on the Way")]
        OnWayForDelivery,
        DriverCompletedDelivery,
        Delivered,
        Canceled
    }
}
