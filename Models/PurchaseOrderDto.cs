using System;
using System.Collections.Generic;

namespace AutoCountApi.Models
{
    public class PurchaseOrderDto : AutoCountAPInvoiceAPI.Models.AutoCountRequestBase
    {
        public DateTime DocDate { get; set; } = DateTime.Today;
        public string CreditorCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PurchaseAgent { get; set; } = string.Empty;
        public List<PurchaseOrderDetailDto> Details { get; set; } = new List<PurchaseOrderDetailDto>();
    }
}