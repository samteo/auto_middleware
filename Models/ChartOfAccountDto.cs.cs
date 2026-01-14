namespace AutoCountAPInvoiceAPI.Models
{


        public class ChartOfAccountDto : AutoCountAPInvoiceAPI.Models.AutoCountRequestBase
        {
            public string AccNo { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string AccountType { get; set; } = string.Empty; // "Asset", "Liability", "Income", "Expense"
        }
  
}
