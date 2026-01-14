using AutoCount.Authentication;
using AutoCount.Invoicing.Purchase.PurchaseOrder;
using AutoCount.Invoicing.Purchase.PurchaseInvoice;
using System;
using System.Collections.Generic;

namespace AutoCountAPInvoiceAPI.Managers
{
    public class PurchaseOrderDetail
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

    public class PurchaseOrderSource
    {
        public DateTime DocDate { get; set; } = DateTime.Today;
        public string CreditorCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PurchaseAgent { get; set; } = string.Empty;
        public List<PurchaseOrderDetail> Details { get; set; } = new List<PurchaseOrderDetail>();
    }

    public class PurchaseInvoiceDetail
    {
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UOM { get; set; } = "UNIT";
        public string Location { get; set; } = "HQ";
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; } = 0;
    }

    public class PurchaseInvoiceSource
    {
        public DateTime DocDate { get; set; } = DateTime.Today;
        public string CreditorCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreditorInvoiceNo { get; set; } = string.Empty;
        public List<PurchaseInvoiceDetail> Details { get; set; } = new List<PurchaseInvoiceDetail>();
    }

    public class PurchaseManager
    {
        public string NewPurchaseOrder(UserSession userSession, PurchaseOrderSource purchaseOrderSource)
        {
            if (userSession == null || !userSession.IsLogin)
            {
                throw new InvalidOperationException("Invalid or expired session. Please log in again.");
            }

            if (string.IsNullOrEmpty(purchaseOrderSource.CreditorCode))
            {
                throw new ArgumentException("Creditor code is required.");
            }

            if (purchaseOrderSource.Details == null || purchaseOrderSource.Details.Count == 0)
            {
                throw new ArgumentException("Purchase order details are required.");
            }

            try
            {
                PurchaseOrderCommand cmd = PurchaseOrderCommand.Create(userSession, userSession.DBSetting);
                PurchaseOrder doc = cmd.AddNew();

                // Set required fields first
                doc.DocNo = "<<New>>";
                //doc.SubmitInvoiceNow = false;
                
                // Add purchase order details BEFORE setting CreditorCode
                foreach (var detail in purchaseOrderSource.Details)
                {
                    var dtl = doc.AddDetail();
                    dtl.ItemCode = detail.ItemCode;
                    dtl.Description = detail.Description;
                    dtl.UOM = detail.UOM;
                    dtl.Location = detail.Location;
                    dtl.Qty = detail.Qty;
                    dtl.UnitPrice = detail.UnitPrice;
  
                    if (!string.IsNullOrEmpty(detail.EstimatedDeliveryDate))
                    {
                        dtl.EstimatedDeliveryDate = detail.EstimatedDeliveryDate;
                    }
                    
                    if (detail.DeliveryDate.HasValue)
                    {
                        dtl.DeliveryDate = detail.DeliveryDate.Value;
                    }
                }

                // Set header information AFTER details to minimize creditor lookup issues
                doc.DocDate = purchaseOrderSource.DocDate;
                doc.Description = purchaseOrderSource.Description;
                doc.CreditorCode = purchaseOrderSource.CreditorCode;

                // Save the purchase order
                doc.Save();
                
                Console.WriteLine($"Purchase Order {doc.DocNo} is created successfully.");
                return doc.DocNo;
            }
            catch (AutoCount.AppException ex)
            {
                Console.WriteLine($"Error creating Purchase Order: {ex.Message}");
                throw new InvalidOperationException($"AutoCount error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }
        }

        public string NewPurchaseInvoice(UserSession userSession, PurchaseInvoiceSource purchaseInvoiceSource)
        {
            if (userSession == null || !userSession.IsLogin)
            {
                throw new InvalidOperationException("Invalid or expired session. Please log in again.");
            }

            if (string.IsNullOrEmpty(purchaseInvoiceSource.CreditorCode))
            {
                throw new ArgumentException("Creditor code is required.");
            }

            if (purchaseInvoiceSource.Details == null || purchaseInvoiceSource.Details.Count == 0)
            {
                throw new ArgumentException("Purchase invoice details are required.");
            }

            try
            {
                PurchaseInvoiceCommand cmd = PurchaseInvoiceCommand.Create(userSession, userSession.DBSetting);
                PurchaseInvoice doc = cmd.AddNew();

                // Set required fields first
                doc.DocNo = "<<New>>";
                
                // Add purchase invoice details BEFORE setting CreditorCode
                foreach (var detail in purchaseInvoiceSource.Details)
                {
                    var dtl = doc.AddDetail();
                    dtl.ItemCode = detail.ItemCode;
                    dtl.Description = detail.Description;
                    dtl.UOM = detail.UOM;
                    dtl.Location = detail.Location;
                    dtl.Qty = detail.Qty;
                    dtl.UnitPrice = detail.UnitPrice;
                    dtl.TaxRate = detail.TaxRate;
                }

                // Set header information AFTER details to minimize creditor lookup issues
                doc.DocDate = purchaseInvoiceSource.DocDate;
                doc.Description = purchaseInvoiceSource.Description;
                doc.SupplierInvoiceNo = purchaseInvoiceSource.CreditorInvoiceNo;
                doc.CreditorCode = purchaseInvoiceSource.CreditorCode;

                // Save the purchase invoice
                doc.Save();
                
                Console.WriteLine($"Purchase Invoice {doc.DocNo} is created successfully.");
                return doc.DocNo;
            }
            catch (AutoCount.AppException ex)
            {
                Console.WriteLine($"Error creating Purchase Invoice: {ex.Message}");
                throw new InvalidOperationException($"AutoCount error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }
        }
    }
}