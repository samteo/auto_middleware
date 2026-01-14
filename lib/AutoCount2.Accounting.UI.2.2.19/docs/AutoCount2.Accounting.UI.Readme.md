# Description

AutoCount2.Accounting.UI is the accounting UI core library of AutoCount Accounting version 2. This package is required by any AutoCount plug-in project or any applications integrated with AutoCount Accounting version 2 through its programming interface.

This package contains the core logic for ARAP, GL, Stock, Invoicing, and etc.

Here, you can create a AutoCount.MainEntry.UIStartup instance and call the SubProjectStartup method when you start a sub-project of AutoCount Accounting version 2.

```C#
var startup = new AutoCount.MainEntry.UIStartup();
startup.SubProjectStartup(userSession);
```

**This package requires [DevExpress WinForms component version 19.2.10.](https://www.devexpress.com)**

