# textFilter Overview
TextFilter is a utility designed to make filtering text files more efficient by using either strings or regex patterns. What makes this utility unique as compared to other regex or text viewing utilities is the ability to have multiple filters in multiple filter tabs. This allows for quick toggling between different filters searching for the appropriate information. Creating filters in RegexViewer is simple and with the 'Notes' field, gives the ability to document the reasons for the filter. Filters can be local or remote as well as on a website for easy sharing. Other features that make parsing log files easier is being able to do a dynamic filter (at bottom of window) and the to create a new log tab base on the current tabs view just to name a few. 

## Documentation:
[Installation and Requirements](./installation.md)  
[Functions](./functions.md)  
[Configuration File](./configuration.md)  
[Command Line Arguments](./command-line.md)  
[Creating a Filter](./creating-filter.md)  
[Trace Analysis General Practices](./general-practices.md)  
[Known Issues](./known-issues.md)  


## Features:
- **Command line capability** - filters and files can be opened from command line
- **File Type association and explorer context help** - makes opening text files with different extensions easy without changing file association.
- **Drag and drop capability** - single or multiple files and filters can be dragged into the window for opening.
- **Hotkeys** - most of the common commands have associated hotkeys that are documented in the context and application menus.
- **Cut and paste** - keeps the background and foreground colors for the filter patterns for example when pasting into OneNote.
- **Quick filter** - at bottom of window, allows a quick search that overrides current filter but keeps any colors assigned from current filter. An example of this is having a filtered view with relevant information, then reducing the view further by specifying a time range in the Quick filter.
- **Regex Grouping** - ability to use the regex built-in functionality of grouping to extract data into separate columns. Up to 4 different grouping columns (Group1 -4) are available for this. Once data is in the column, it can be sorted or exported.
- **Window customization** - setting some of the window features such as default background and foreground colors, font and font size.
- **Opening current file view into new view** - this is another way to reduce the data set that is being parsed. An example of this is if you have a multi gigabyte log file but are only interested in a small portion of it. Create a filter like a time filter to narrow down to the area of interest, and open into a new view. Now all new filtering can be done on just the new view which will be faster and use less RAM.
- **Shared filters** - setting 'shared filters to a unc location allows the filters to be centralized for multiple users to use.

[image:window-image-2.png]		
