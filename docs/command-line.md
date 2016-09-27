# textFilter Command Line Arguments

TextFilter does support some command line arguments. These arguments are used to setup the file type association and explorer context menu as well as specify which filters and files to open. Additionally a different config file that controls textFilter itself can be specified. This is useful if trying to automate / batch steps that are used repeatedly. One of the nice features of command line use that is not available in the gui is the ability to use a wildcard when specifying either filter file(s) or log file(s). For example, if there are multiple event filters used for viewing event logs and / or there are multiple event log files that are to be viewed, using a wildcard on the command line makes this easy to open assuming they are named in a proper format. See below for examples of this.

## List of currently available command line arguments:

```
C:\temp\textFilter>textFilter /?

C:\temp\textFilter>This utility parses text files using regex and string patterns.
Multiple filter files in xml format can be used at the same time.
Multiple log files can be viewed at the same time.
Command line options are:
/config:
/filter:
/log:
/register
/unregister
/?
Press Enter to continue...

C:\temp\textFilter>
```
## To setup file type association and explorer context menu:
- Open an elevated command prompt and navigate to directory where textFilter is located.
- Type `textFilter.exe /register`
- Notice how .csv is now associated to textFilter and `textFilter` is now in the explorer context menu.
	

	
## To remove file type association and explorer context menu:

- Open an elevated command prompt and navigate to directory where textFilter is located.
- Type `textFilter.exe /unregister`
- Notice how .csv is not associated with textFilter and `textFilter` is not in the explorer context menu.

			

## Using /config: argument:
By default and only when launching textFilter from gui, textFilter.exe.config is used as the application configuration file. From the command prompt however, another configuration file can be specified. This is useful if wanting to use and maintain different config files for different purposes. An example of this is having a different list of filters which can be maintained in the config file under `CurrentFilterFiles`
Example command: `textFilter.exe /config: example.exe.config`

## Using /filter: and /log: arguments:

As mentioned previously the command line arguments can be used to override values in config file as well as used to specify multiple files with the use of comma `,` separator and /or wildcards `*`. When using wildcards, the full path should be specified (or %cd%).

**Note:** using command line arguments work best with `SaveSessionInformation` set to `False` in config file Configuration file textFilter.exe.config

For the following examples we will be using this directory / file structure
```
	C:\temp\textFilter>tree /a /f %cd%
	Folder PATH listing
	Volume serial number is 5846-29FB
	C:\TEMP\textFilter
	|   ascii.txt
	|   ascii2.txt
	|   eventlog-filter.xml
	|   eventlog-filter2.xml
	|   textFilter.exe
	|   textFilter.exe.config
	|
	+---filters
	|       eventlog-filter3.xml
	|       eventlog-filter4.xml
	|
	+---i love folder spaces
	|       ascii5.txt
	|       ascii6.txt
	|       eventlog-filter5.xml
	|       eventlog-filter6.xml
	|
	\---logs
	        ascii3.txt
	        ascii4.txt
```
## Filter Files:
**Example 1**: open a specified filter file.
- `C:\temp\textFilter>textFilter.exe /filter: eventlog-filter.xml`
- For folders with spaces: `C:\temp\textFilter>textFilter.exe /filter: "i love folder spaces\eventlog-filter5.xml"`
	
**Example 2**: open multiple specified filter files.
- `C:\temp\textFilter>textFilter.exe /filter: eventlog-filter.xml,eventlog-filter2.xml`
- For folders with spaces: `C:\temp\textFilter>textFilter.exe /filter: "i love folder spaces\eventlog-filter5.xml";"i love folder spaces\eventlog-filter6.xml"`

**Example 3**: open multiple filter files using wildcard
- `C:\temp\textFilter>textFilter.exe /filter: %cd%\event**.xml`

## Filter and Log Files:
**Example 4**: Building on Example 1: open a specified filter file and specified log file
- `C:\temp\textFilter>textFilter.exe /filter: eventlog-filter.xml /log: ascii.txt`
- For folders with spaces: `C:\temp\textFilter>textFilter.exe /filter: "i love folder spaces\eventlog-filter5.xml" /log: "i love folder spaces\ascii5.txt"`

**Example 5**: Building on Example 2: open multiple specified filter files and multiple specified logs.
- `C:\temp\textFilter>textFilter.exe /filter: eventlog-filter.xml;eventlog-filter2.xml /log: ascii.txt;ascii2.txt`
- For folders with spaces: `C:\temp\textFilter>textFilter.exe /filter: "i love folder spaces\eventlog-filter5.xml";"i love folder spaces\eventlog-filter6.xml" /log: "i love folder spaces\ascii5.txt";"i love folder spaces\ascii6.txt"`


**Example 6**: Building on Example 3: open multiple filter files using wildcard and multiple log files using wildcard
- `C:\temp\textFilter>textFilter.exe /filter: %cd%\eventlog-filter*.xml /log: %cd%\ascii*.txt`

