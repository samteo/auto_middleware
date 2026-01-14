using System;

namespace AutoCountApi.Models
{
    public class PurchaseOrderDetailDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UOM { get; set; } = "UNIT";
        public string Location { get; set; } = "HQ";
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string EstimatedDeliveryDate { get; set; } = string.Empty;
    }
}