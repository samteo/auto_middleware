# Description

AutoCount2.MainEntry contains the Main Entry of AutoCount Accounting version 2. This package is required by any AutoCount plug-in project or any applications integrated with AutoCount Accounting version 2 through its programming interface.

**This package requires [DevExpress WinForms component version 19.2.10.](https://www.devexpress.com)**

Here, you can call the SubProjectStartup method of AutoCount.MainEntry.MainStartup.Default instance when you start a sub-project of AutoCount Accounting version 2.

```C#
AutoCount.MainEntry.MainStartup.Default.SubProjectStartup(userSession);
```

In case you need to work with system report or backup and restore with Rar, you may need to copy the following files from the target's bin folder into your Debug or Release folder.

report.dat  
unrar.wdl  
unrar64.wdl  
