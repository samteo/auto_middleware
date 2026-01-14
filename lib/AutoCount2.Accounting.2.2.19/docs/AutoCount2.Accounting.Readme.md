# Description

AutoCount2.Accounting is the accounting core library of AutoCount Accounting version 2. This package is required by any AutoCount plug-in project or any applications integrated with AutoCount Accounting version 2 through its programming interface.

**This package does not contain any WinForms controls.**

This package contains core logic for ARAP, GL, Stock, Invoicing, and etc.

Here, you can create a AutoCount.MainEntry.Startup instance and call the SubProjectStartup method when you start a sub-project of AutoCount Accounting version 2.

```C#
var startup = new AutoCount.MainEntry.Startup();
startup.SubProjectStartup(userSession);
```

# Commonly Used Types:
AutoCount.GL.CashBook.CashBookCommand  
AutoCount.GL.JournalEntry.JournalEntryCommand  
AutoCount.ARAP.ARInvoice.ARInvoiceDataAccess  
AutoCount.ARAP.ARCN.ARCNDataAccess  
AutoCount.ARAP.ARDN.ARDNDataAccess  
AutoCount.ARAP.ARPayment.ARPaymentDataAccess  
AutoCount.ARAP.APInvoice.APInvoiceDataAccess  
AutoCount.ARAP.APCN.APCNDataAccess  
AutoCount.ARAP.APDN.APDNDataAccess  
AutoCount.ARAP.APPayment.APPaymentDataAccess  
