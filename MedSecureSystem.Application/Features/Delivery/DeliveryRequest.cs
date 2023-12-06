using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedSecureSystem.Application.Features.Delivery
{
    public class CreateDeliveryRequestModel
    {
        public long BusinessId { get; set; }
        public List<DeliveryRequestItemModel> Items { get; set; }
    }

    public class DeliveryRequestItemModel
    {
        public string Name { get; set; }
        public string Quantity { get; set; }
        public string Note { get; set; }
    }
}
