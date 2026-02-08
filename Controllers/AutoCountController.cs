using AutoCount.Authentication;
using AutoCountApi.Models;
using AutoCountAPInvoiceAPI.Managers;
using AutoCountAPInvoiceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Xml.Linq;
using static AutoCountAPInvoiceAPI.Models.PurchaseInvoiceDto;

namespace AutoCountApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoCountController : ControllerBase
    {
        private readonly LoginManager _loginManager;
        private readonly CreditorManager _creditorManager;
        private readonly InvoiceManager _invoiceManager;
        private readonly ChartOfAccountManager _chartOfAccountManager;
        private readonly PurchaseManager _purchaseManager;
        private readonly ItemManager _itemManager;
        // Hardcoded for simplicity; consider using configuration for production
        private readonly string _serverName = "26.156.97.227\\A2006";   // "SAM\\Z2006";
        private readonly string _sqlUserLogin = "weekei";           // SQL Server login
        private readonly string _sqlUserPasswd = "weekeidb"; // SQL Server password
        private readonly string _autoCountUser = "ADMIN";       // AutoCount user
        private readonly string _autoCountPass = "admin";       // AutoCount password

        public AutoCountController()
        {
            _loginManager = new LoginManager();
            _creditorManager = new CreditorManager();
            _invoiceManager = new InvoiceManager();
            _chartOfAccountManager = new ChartOfAccountManager();
            _purchaseManager = new PurchaseManager();
            _itemManager = new ItemManager();
        }

        [HttpPost("creditor")]
        public IActionResult CreateCreditor([FromBody] CreditorDto creditorDto)
        {
            var session = _loginManager.InitiateUserSessionUnattended(
                _serverName, 
                creditorDto.DbName, 
                _autoCountUser, 
                _autoCountPass, 
                _sqlUserLogin, 
                _sqlUserPasswd);

            if (session == null || !session.IsLogin)
            {
                return BadRequest("Login failed. Please check credentials or database settings.");
            }

            var creditorSource = new CreditorSource
            {
                ControlAccount = creditorDto.ControlAccount,
                CompanyName = creditorDto.CompanyName,
                Addr1 = creditorDto.Addr1,
                Addr2 = creditorDto.Addr2,
                Addr3 = creditorDto.Addr3,
                Addr4 = creditorDto.Addr4,
                Phone = creditorDto.Phone,
                Mobile = creditorDto.Mobile,
                ContactPerson = creditorDto.ContactPerson,
                Email = creditorDto.Email
            };

            var creditorCode = _creditorManager.NewCreditor(session, creditorSource);
            if (creditorCode == null)
            {
                return BadRequest("Failed to create creditor.");
            }

            return Ok(new { CreditorCode = creditorCode });
        }

        [HttpPost("invoice")]
        public IActionResult CreateInvoice([FromBody] InvoiceDto invoiceDto)
        {
            var session = _loginManager.InitiateUserSessionUnattended(
                _serverName, 
                invoiceDto.DbName, 
                _autoCountUser, 
                _autoCountPass, 
                _sqlUserLogin, 
                _sqlUserPasswd);
            if (session == null || !session.IsLogin)
            {
                return BadRequest("Login failed. Please check credentials or database settings.");
            }

            var invoiceSource = new InvoiceSource
            {
                CreditorCode = invoiceDto.CreditorCode,
                DocDate = invoiceDto.DocDate,
                Description = invoiceDto.Description,
//                PurchaseAgent = invoiceDto.PurchaseAgent,
                JournalType = invoiceDto.JournalType,
                //InclusiveTax = invoiceDto.InclusiveTax,
                //RoundingMethod = Enum.Parse<AutoCount.Document.DocumentRoundingMethod>(invoiceDto.RoundingMethod),
                Details = invoiceDto.Details.ConvertAll(d => new InvoiceDetail
                {
                    AccNo = d.AccNo,
                    Description = d.Description,
                    ProjNo = d.ProjNo,
                    Amount = d.Amount
                })
            };

            try
            {
                var invoiceNo = _invoiceManager.NewAPInvoiceEntry(session, invoiceSource);
                if (invoiceNo == null)
                {
                    return BadRequest(new { Message = "Failed to create invoice.", Error = "Invoice number is null." });
                }
                return Ok(new { Message = "Invoice created successfully.", InvoiceNo = invoiceNo });
            }
            catch (Exception ex)
            {
                // Optionally log the exception here
                return BadRequest(new { Message = "Failed to create invoice.", Error = ex.Message });
            }
        }

        [HttpPost("purchase-order")]
        public IActionResult CreatePurchaseOrder([FromBody] PurchaseOrderDto purchaseOrderDto)
        {
            // Validate input data first
            if (purchaseOrderDto == null)
            {
                return BadRequest(new { Message = "Purchase order data is required.", Error = "PurchaseOrderDto is null." });
            }

            if (string.IsNullOrEmpty(purchaseOrderDto.CreditorCode))
            {
                return BadRequest(new { Message = "Failed to create purchase order.", Error = "CreditorCode is required." });
            }

            if (purchaseOrderDto.Details == null || purchaseOrderDto.Details.Count == 0)
            {
                return BadRequest(new { Message = "Failed to create purchase order.", Error = "Purchase order details are required." });
            }

            var session = _loginManager.InitiateUserSessionUnattended(
                _serverName, 
                purchaseOrderDto.DbName, 
                _autoCountUser, 
                _autoCountPass, 
                _sqlUserLogin, 
                _sqlUserPasswd);
            if (session == null || !session.IsLogin)
            {
                return BadRequest("Login failed. Please check credentials or database settings.");
            }

            var purchaseOrderSource = new PurchaseOrderSource
            {
                DocDate = purchaseOrderDto.DocDate,
                CreditorCode = purchaseOrderDto.CreditorCode,
                Description = purchaseOrderDto.Description,
                PurchaseAgent = purchaseOrderDto.PurchaseAgent,
                Details = purchaseOrderDto.Details.ConvertAll(d => new PurchaseOrderDetail
                {
                    ItemCode = d.ItemCode,
                    Description = d.Description,
                    UOM = d.UOM,
                    Location = d.Location,
                    Qty = d.Qty,
                    UnitPrice = d.UnitPrice,
                    DeliveryDate = d.DeliveryDate,
                    EstimatedDeliveryDate = d.EstimatedDeliveryDate
                })
            };

            try
            {
                var purchaseOrderNo = _purchaseManager.NewPurchaseOrder(session, purchaseOrderSource);
                if (purchaseOrderNo == null)
                {
                    return BadRequest(new { Message = "Failed to create purchase order.", Error = "Purchase order number is null." });
                }
                return Ok(new { Message = "Purchase order created successfully.", PurchaseOrderNo = purchaseOrderNo });
            }
            catch (Exception ex)
            {
                // Optionally log the exception here
                return BadRequest(new { Message = "Failed to create purchase order.", Error = ex.Message });
            }
        }

        [HttpPost("purchase-invoice")]
        public IActionResult CreatePurchaseInvoice([FromBody] PurchaseInvoiceDto purchaseInvoiceDto)
        {
            // Validate input data first
            if (purchaseInvoiceDto == null)
            {
                return BadRequest(new { Message = "Purchase invoice data is required.", Error = "PurchaseInvoiceDto is null." });
            }

            if (string.IsNullOrEmpty(purchaseInvoiceDto.CreditorCode))
            {
                return BadRequest(new { Message = "Failed to create purchase invoice.", Error = "CreditorCode is required." });
            }

            if (purchaseInvoiceDto.Details == null || purchaseInvoiceDto.Details.Count == 0)
            {
                return BadRequest(new { Message = "Failed to create purchase invoice.", Error = "Purchase invoice details are required." });
            }

            var session = _loginManager.InitiateUserSessionUnattended(
                _serverName,
                purchaseInvoiceDto.DbName,
                _autoCountUser,
                _autoCountPass,
                _sqlUserLogin,
                _sqlUserPasswd);
            if (session == null || !session.IsLogin)
            {
                return BadRequest("Login failed. Please check credentials or database settings.");
            }

            var purchaseInvoiceSource = new PurchaseInvoiceSource
            {
                DocDate = purchaseInvoiceDto.DocDate,
                CreditorCode = purchaseInvoiceDto.CreditorCode,
                Description = purchaseInvoiceDto.Description,
                CreditorInvoiceNo = purchaseInvoiceDto.CreditorInvoiceNo,
                Details = purchaseInvoiceDto.Details.ConvertAll(d => new PurchaseInvoiceDetail
                {
                    ItemCode = d.ItemCode,
                    Description = d.Description,
                    UOM = d.UOM,
                    Location = d.Location,
                    Qty = d.Qty,
                    UnitPrice = d.UnitPrice,
                    TaxRate = d.TaxRate
                })
            };

            try
            {
                var purchaseInvoiceNo = _purchaseManager.NewPurchaseInvoice(session, purchaseInvoiceSource);
                if (purchaseInvoiceNo == null)
                {
                    return BadRequest(new { Message = "Failed to create purchase invoice.", Error = "Purchase invoice number is null." });
                }
                return Ok(new { Message = "Purchase invoice created successfully.", PurchaseInvoiceNo = purchaseInvoiceNo });
            }
            catch (Exception ex)
            {
                // Optionally log the exception here
                return BadRequest(new { Message = "Failed to create purchase invoice.", Error = ex.Message });
            }
        }

        //[HttpPost("creditor-and-invoice")]
        //public IActionResult CreateCreditorAndInvoice([FromBody] CreateCreditorAndInvoiceDto request)
        //{
        //    var session = _loginManager.InitiateUserSessionUnattended(_serverName, CreateCreditorAndInvoiceDto.CreditorDto.DbName, _userLogin, _userPasswd);
        //    if (session == null || !session.IsLogin)
        //    {
        //        return BadRequest("Login failed. Please check credentials or database settings.");
        //    }

        //    // Create Creditor
        //    var creditorSource = new CreditorSource
        //    {
        //        ControlAccount = request.Creditor.ControlAccount,
        //        CompanyName = request.Creditor.CompanyName,
        //        Addr1 = request.Creditor.Addr1,
        //        Addr2 = request.Creditor.Addr2,
        //        Addr3 = request.Creditor.Addr3,
        //        Addr4 = request.Creditor.Addr4,
        //        Phone = request.Creditor.Phone,
        //        Mobile = request.Creditor.Mobile,
        //        ContactPerson = request.Creditor.ContactPerson,
        //        Email = request.Creditor.Email
        //    };

        //    var creditorCode = _creditorManager.NewCreditor(session, creditorSource);
        //    if (creditorCode == null)
        //    {
        //        return BadRequest("Failed to create creditor.");
        //    }

        //    // Create Invoice
        //    var invoiceSource = new InvoiceSource
        //    {
        //        CreditorCode = creditorCode,
        //        DocDate = request.Invoice.DocDate,
        //        Description = request.Invoice.Description,
        //        PurchaseAgent = request.Invoice.PurchaseAgent,
        //        JournalType = request.Invoice.JournalType,
        //        InclusiveTax = request.Invoice.InclusiveTax,
        //        RoundingMethod = Enum.Parse<AutoCount.Document.DocumentRoundingMethod>(request.Invoice.RoundingMethod),
        //        Details = request.Invoice.Details.ConvertAll(d => new InvoiceDetail
        //        {
        //            AccNo = d.AccNo,
        //            Description = d.Description,
        //            ProjNo = d.ProjNo,
        //            Amount = d.Amount
        //        })
        //    };

        //    _invoiceManager.NewAPInvoiceEntry(session, invoiceSource);
        //    return Ok(new { CreditorCode = creditorCode, Message = "Creditor and invoice created successfully." });
        //}



        [HttpGet("creditor/find-by-company")]
        public IActionResult FindCreditorByCompanyName([FromQuery] string companyName, [FromQuery] string dbName)
        {
            var session = _loginManager.InitiateUserSessionUnattended(_serverName, dbName, _autoCountUser, _autoCountPass, _sqlUserLogin, _sqlUserPasswd);
            if (session == null || !session.IsLogin)
            {
                return BadRequest("Login failed. Please check credentials or database settings.");
            }

            var creditor = _creditorManager.FindCreditorByCompanyName(session, companyName);
            if (creditor == null)
            {
                return Ok(new
                {
                    AccNo = string.Empty,
                    CompanyName = string.Empty,
                    Address1 = string.Empty,
                    Address2 = string.Empty,
                    Address3 = string.Empty,
                    Address4 = string.Empty,
                    Phone1 = string.Empty,
                    Phone2 = string.Empty,
                    Attention = string.Empty,
                    EmailAddress = string.Empty,
                    CurrencyCode = string.Empty
                });
            }

            return Ok(new
            {
                creditor.AccNo,
                creditor.CompanyName,
                creditor.Address1,
                creditor.Address2,
                creditor.Address3,
                creditor.Address4,
                creditor.Phone1,
                creditor.Phone2,
                creditor.Attention,
                creditor.EmailAddress,
                creditor.CurrencyCode
            });
        }

        [HttpGet("creditor/get-creditor-data")]
        public IActionResult GetCreditorData([FromQuery] string dbName)
        {
            var session = _loginManager.InitiateUserSessionUnattended(_serverName, dbName, _autoCountUser, _autoCountPass, _sqlUserLogin, _sqlUserPasswd);
            if (session == null || !session.IsLogin)
            {
                return BadRequest("Login failed. Please check credentials or database settings.");
            }

            var creditors = _creditorManager.GetCreditorData(session);
            if (creditors == null || creditors.Count == 0)
            {
                return Ok(new List<object>());
            }

            return Ok(creditors);
        }


        [HttpGet("item/find-by-description")]
        public IActionResult GetItemCodeByDescription([FromQuery] string description, [FromQuery] string dbName, [FromQuery] bool exactMatch = false)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return BadRequest(new { Message = "Description is required.", Error = "Description parameter is null or empty." });
            }

            if (string.IsNullOrWhiteSpace(dbName))
            {
                return BadRequest(new { Message = "Database name is required.", Error = "DbName parameter is null or empty." });
            }

            var session = _loginManager.InitiateUserSessionUnattended(
                _serverName, 
                dbName, 
                _autoCountUser, 
                _autoCountPass, 
                _sqlUserLogin, 
                _sqlUserPasswd);
            if (session == null || !session.IsLogin)
            {
                return BadRequest("Login failed. Please check credentials or database settings.");
            }

            try
            {
                var itemCodes = _itemManager.GetItemCodesByDescription(session, description, exactMatch);

                if (itemCodes == null || itemCodes.Count == 0)
                {
                    return NotFound(new
                    {
                        Message = $"No items found with description: {description}",
                        Count = 0,
                        ItemCodes = new List<string>()
                    });
                }

                return Ok(new
                {
                    Message = $"Found {itemCodes.Count} item(s) matching description.",
                    Count = itemCodes.Count,
                    ItemCodes = itemCodes
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error finding items.", Error = ex.Message });
            }
        }

        [HttpGet("item/find-all-by-description")]
        public IActionResult GetAllItemsByDescription([FromQuery] string description, [FromQuery] string dbName, [FromQuery] bool exactMatch = false)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return BadRequest(new { Message = "Description is required.", Error = "Description parameter is null or empty." });
            }

            if (string.IsNullOrWhiteSpace(dbName))
            {
                return BadRequest(new { Message = "Database name is required.", Error = "DbName parameter is null or empty." });
            }

            var session = _loginManager.InitiateUserSessionUnattended(
                _serverName, 
                dbName, 
                _autoCountUser, 
                _autoCountPass, 
                _sqlUserLogin, 
                _sqlUserPasswd);
            if (session == null || !session.IsLogin)
            {
                return BadRequest("Login failed. Please check credentials or database settings.");
            }

            try
            {
                var items = _itemManager.GetAllItemsByDescription(session, description, exactMatch);
                
                return Ok(new
                {
                    Message = $"Found {items.Count} item(s) matching description.",
                    Count = items.Count,
                    Items = items
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error finding items.", Error = ex.Message });
            }
        }

        [HttpGet("item/get-item-data")]
        public IActionResult GetItemData([FromQuery] string dbName)
        {
            var session = _loginManager.InitiateUserSessionUnattended(_serverName, dbName, _autoCountUser, _autoCountPass, _sqlUserLogin, _sqlUserPasswd);
            if (session == null || !session.IsLogin)
            {
                return BadRequest("Login failed. Please check credentials or database settings.");
            }

            var items = _itemManager.GetItemData(session);
            if (items == null || items.Count == 0)
            {
                return Ok(new List<object>());
            }

            return Ok(items);
        }

        [HttpGet("item/get-alt-item-data")]
        public IActionResult GetAltItemData([FromQuery] string dbName)
        {
            var session = _loginManager.InitiateUserSessionUnattended(_serverName, dbName, _autoCountUser, _autoCountPass, _sqlUserLogin, _sqlUserPasswd);
            if (session == null || !session.IsLogin)
            {
                return BadRequest("Login failed. Please check credentials or database settings.");
            }

            var items = _itemManager.GetAltItemData(session);
            if (items == null || items.Count == 0)
            {
                return Ok(new List<object>());
            }

            return Ok(items);
        }

        [HttpGet("accounts")]
        public IActionResult GetChartOfAccounts([FromQuery] string dbName)
        {
            var session = _loginManager.InitiateUserSessionUnattended(_serverName, dbName, _autoCountUser, _autoCountPass, _sqlUserLogin, _sqlUserPasswd);
            if (session == null || !session.IsLogin)
            {
                return BadRequest("Login failed. Please check credentials or database settings.");
            }

            var accounts = _chartOfAccountManager.GetChartOfAccounts(session);

            return Ok(accounts);
        }

        [HttpPost("accounts/create")]
        public IActionResult Create([FromBody] ChartOfAccountDto dto)
        {
            var session = _loginManager.InitiateUserSessionUnattended(_serverName, dto.DbName, _autoCountUser, _autoCountPass, _sqlUserLogin, _sqlUserPasswd);
            var accountSource = new AccountSource
            {
                AccNo = dto.AccNo,
                Description = dto.Description,
                AccType = dto.AccountType // e.g., "AST", "EXP", etc.
            };
            try
            {
                var success = _chartOfAccountManager.NewAccountEntry(session, accountSource);
                return Ok("Account created.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }

    public class CreateCreditorAndInvoiceDto
    {
        public CreditorDto Creditor { get; set; }
        public InvoiceDto Invoice { get; set; }
    }
}