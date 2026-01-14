using System;
using System.Collections.Generic;

namespace AutoCountApi.Models
{
    public class InvoiceDto : AutoCountAPInvoiceAPI.Models.AutoCountRequestBase
    {
        public string CreditorCode { get; set; }
        public DateTime DocDate { get; set; }
        public string Description { get; set; }
        public string PurchaseAgent { get; set; }
        public string JournalType { get; set; }
        //public bool InclusiveTax { get; set; }
        //public string RoundingMethod { get; set; } // Use string to map to AutoCount enum
        public List<InvoiceDetailDto> Details { get; set; }
    }
}