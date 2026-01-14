namespace AutoCountApi.Models
{
    public class InvoiceDetailDto
    {
        public string AccNo { get; set; }
        public string Description { get; set; }
        public string ProjNo { get; set; }
        public decimal Amount { get; set; }
    }
}