![](https://github.com/jagilber/regexViewer/blob/master/RegexViewer/images/RegexViewer_128x128.png)  
# regexViewer  

**Current Download:**  
[RegexViewer.0.5.412.zip](https://github.com/jagilber/regexViewer/blob/master/RegexViewer/download/regexViewer.0.4.105.zip) 

![](https://github.com/jagilber/regexViewer/blob/master/RegexViewer/images/rv-05412-window-image-1.png)		

**Overview:**

RegexViewer is a utility designed to make filtering text files more efficient by using either strings or regex patterns. What makes this utility unique as compared to other regex or text viewing utilities is the ability to have multiple filters in multiple filter tabs. This allows for quick toggling between different filters searching for the appropriate information. Creating filters in RegexViewer is simple and with the 'Notes' field, gives the ability to document the reasons for the filter. Filters can be local or remote as well as on a website for easy sharing. Other features that make parsing log files easier is being able to do a dynamic filter (at bottom of window) and the to create a new log tab base on the current tabs view just to name a few. 

**Features:**

- Command line capability - filters and files can be opened from command line.
- File Type association and explorer context help - makes opening text files with different extensions easy without changing file association.
- Drag and drop capability - single or multiple files and filters can be dragged into the window for opening.
- Hotkeys - most of the common commands have associated hotkeys that are documented in the context and application menus.
- Cut and paste - keeps the background and foreground colors for the filter patterns for example when pasting into OneNote.
- AND / Quick filter - at bottom of window, allows a quick search that overrides current filter but keeps any colors assigned from current filter. An example of this is having a filtered view with relevant information, then reducing the view further by specifying a time range in the Quick filter.
- Window customization - setting some of the window features such as default background and foreground colors, font and font size.
- Opening current file view into new view - this is another way to reduce the data set that is being parsed. An example of this is if you have a multi gigabyte log file but are only interested in a small portion of it. Create a filter like a time filter to narrow down to the area of interest, and open into a new view. Now all new filtering can be done on just the new view which will be faster and use less RAM.

**Notes:**
- The text files currently load into RAM regardless of size using standard .net. This is good as its fast to parse but can be bad depending on size of file the amount of RAM it uses can be sizeable. A 1GB file averages around 2GB of RAM.

**Feedback:**
- If you have suggestions for features you would like to see or any issues, please let me know


