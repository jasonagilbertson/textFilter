![](https://github.com/jasonagilbertson/textFilter/blob/master/TextFilter/Images/ico.png)  

# textFilter  

**Current Download:**  
[textFilter.0.7.001.zip](https://github.com/jasonagilbertson/textFilter/releases/download/textFilter.0.7.001.zip/textFilter.0.7.001.zip) 

![](https://github.com/jasonagilbertson/textFilter/blob/master/TextFilter/Images/tf-window-image-1.png)		

**Overview:**

textFilter is a utility designed to make filtering text files more efficient by using either strings or regex patterns. What makes this utility unique as compared to other regex or text viewing utilities is the ability to have multiple filters in multiple filter tabs. This allows for quick toggling between different filters searching for the appropriate information. Creating filters in textFilter is simple and with the 'Notes' field, gives the ability to document the reasons for the filter. Filters can be local or remote as well as on a website for easy sharing. Other features that make parsing log files easier is being able to do a Quick Filter at top of window with string search operators. Optionally enable central file location of filter files (shared filters)  to be shared with team.

**Features:**
- Command line capability - filters and files can be opened from command line.
- File Type association and explorer context help - makes opening text files with different extensions easy without changing file association.
- Drag and drop capability - single or multiple files and filters can be dragged into the window for opening.
- Hotkeys - most of the common commands have associated hotkeys that are documented in the context and application menus.
- Cut and paste - keeps the background and foreground colors for the filter patterns for example when pasting into OneNote.
- Quick filter - at top of window, allows a quick search that can either override current filters in filter view, compliment filters in filter view, or filter for lines that match both Quick filter and individual filters in filter view.
- Regex Grouping - ability to use the regex built-in functionality of grouping to extract data into separate columns. Up to 4 different grouping columns (Group1 -4) are available for this. Once data is in the column, it can be sorted or exported.
- Window customization - setting some of the window features such as default background and foreground colors, font and font size.
- Opening current file view into new view - this is another way to reduce the data set that is being parsed. An example of this is if you have a multi gigabyte log file but are only interested in a small portion of it. Create a filter like a time filter to narrow down to the area of interest, and open into a new view. Now all new filtering can be done on just the new view which will be faster and use less RAM.

**Notes:**
- The text files currently load into RAM regardless of size using standard .net. This is good as its fast to parse but can be bad depending on size of file the amount of RAM it uses can be sizeable. A 1GB file averages around 2GB of RAM.

**Feedback:**
- If you have suggestions for features you would like to see or have any issues, please let me know.

thanks



