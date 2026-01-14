namespace AutoCountAPInvoiceAPI.Models
{
    public class PurchaseInvoiceDto : AutoCountRequestBase
    {
        public DateTime DocDate { get; set; } = DateTime.Today;
        public string CreditorCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreditorInvoiceNo { get; set; } = string.Empty;
        public List<PurchaseInvoiceDetailDto> Details { get; set; } = new List<PurchaseInvoiceDetailDto>();
    }

    public class PurchaseInvoiceDetailDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UOM { get; set; } = "UNIT";
        public string Location { get; set; } = "HQ";
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; } = 0;
    }
}