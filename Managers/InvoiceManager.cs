using AutoCount.Authentication;
using AutoCount.ARAP.APInvoice;
using System;
using System.Collections.Generic;

namespace AutoCountAPInvoiceAPI.Managers
{
    public class InvoiceDetail
    {
        public string AccNo { get; set; }
        public string Description { get; set; }
        public string ProjNo { get; set; }
        public decimal Amount { get; set; }
    }

    public class InvoiceSource
    {
        public string CreditorCode { get; set; }
        public DateTime DocDate { get; set; }
        public string Description { get; set; }
        public string PurchaseAgent { get; set; }
        public string JournalType { get; set; }
        //public bool InclusiveTax { get; set; }
        //public AutoCount.Document.DocumentRoundingMethod RoundingMethod { get; set; }
        public List<InvoiceDetail> Details { get; set; }
    }

    internal class InvoiceManager
    {
        public string NewAPInvoiceEntry(AutoCount.Authentication.UserSession userSession, InvoiceSource invoiceSource)
        {
            if (userSession == null || !userSession.IsLogin)
            {
                Console.WriteLine("Invalid or expired session. Please log in again.");
                return null;
            }

            if (string.IsNullOrEmpty(invoiceSource.CreditorCode))
            {
                Console.WriteLine("Creditor code is required.");
                return null;
            }

            AutoCount.ARAP.APInvoice.APInvoiceDataAccess cmd =
                AutoCount.ARAP.APInvoice.APInvoiceDataAccess.Create(userSession, userSession.DBSetting);
            AutoCount.ARAP.APInvoice.APInvoiceEntity doc = cmd.NewAPInvoice();

            doc.DocNo = "<<New>>";
            doc.CreditorCode = invoiceSource.CreditorCode;
            doc.DocDate = invoiceSource.DocDate;
            doc.Description = invoiceSource.Description;
            doc.PurchaseAgent = invoiceSource.PurchaseAgent == "" ? null : invoiceSource.PurchaseAgent;
            doc.JournalType = invoiceSource.JournalType;
            //doc.InclusiveTax = invoiceSource.InclusiveTax;
            //doc.RoundingMethod = invoiceSource.RoundingMethod;

            foreach (var detail in invoiceSource.Details)
            {
                var dtl = doc.NewDetail();
                dtl.AccNo = detail.AccNo;
                dtl.Description = detail.Description;
                dtl.ProjNo = detail.ProjNo == "" ? null : detail.ProjNo;
                dtl.Amount = detail.Amount;
            }

            try
            {
                cmd.SaveAPInvoice(doc, userSession.LoginUserID);
                Console.WriteLine($"AP Invoice {doc.DocNo} is created successfully.");
                return doc.DocNo;
            }
            catch (AutoCount.AppException ex)
            {
                Console.WriteLine($"Error creating AP Invoice: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return null;
            }
        }
    }
}

