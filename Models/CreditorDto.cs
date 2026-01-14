namespace AutoCountApi.Models
{

    public class CreditorDto : AutoCountAPInvoiceAPI.Models.AutoCountRequestBase
    {
        public string ControlAccount { get; set; }
        public string CompanyName { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string Addr3 { get; set; }
        public string Addr4 { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
    }
}