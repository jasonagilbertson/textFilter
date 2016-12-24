![](../TextFilter/Images/ico.png)

# Trace Analysis General Practices

Though it helps, you don’t necessarily have to know what you are looking for. You will find this to be the case more often than not. So how do I find the issue when I don’t know what I am looking for? This is what I do in general and seems to be common practice:

### 1. Get a bad trace

### 2. Get context of the bad trace

### 3. Get a good trace for compare
- Getting a good trace from customer is best as it will be the most comparable. OS, File, Configuration, and Environment will all be the same.
- If a good trace is not available, then create a good one off an internal repro for use as reference.

### 4. Typically, get as much data as you can.
- There are two schools of thought on this. Neither is generically right or wrong and each case needs to be taken into account. You can either:
- Get large amount of data and parse through it (that is what the utility is for :))
  - Advantages: 
    - Limits the number of times you have to request data from customer
    - Hopefully gives a more complete picture of issue
  - Disadvantages:
    - Can take longer to parse through trace
    - Can cause stress to environment
    - Can drop events
- Get limited / targeted data
  - Advantages:
    - Less to filter through
    - Less stress to environment. 
    - Less chance to drop events.
  - Disadvantages:
    - How do you know you got the right information?
    - If you didn’t get right information, then you have to request another set from customer. If issue is intermittent, getting multiple sets can be difficult and frustrate customer.

### 5. Start with common known error words:
- Error
- Fail
- Warning
- Exception
- Error codes (or error patterns {"0x[0-9a-fA-F]{8}"} for example)

### 6. Some lesser known words:
- Timeout
- Terminate
- Dialog
- Unexpected
- Stop 
- Start

### 7. Search for context words:
- User names
- Machine names
- Ip addresses
- Time of day

### 8. Search for process words:
- PID
- TID
- Handle or some other unique ID which you will see frequently used to identify a task
- %Process name%

### 9. Filter out noise:
- Once you start to filter down the data using some of the keywords / methods above you will undoubtedly still have additional traces that are viewable clouding the issue. Seeing 10's or 100's of the same type of trace, for example: unable to read registry key probably means this is NOT your issue and it can be filtered. To do this make an exclusion filter and set to a low (0) index so that it gets processed at a higher level.
- If in doubt if trace is an issue or not, mark it as so in the 'Notes' field until you can confirm.
- Compare questionable traces to a known good trace to see if the same trace statements show in a good trace.
- Most likely you will be looking for only a couple of unique trace statements that will identify the issue.

### 10. Look for patterns
- The easiest way to do this is to filter and color the filters that are of interest

### 11. Do what works for you

## ETW specific information:
- Make sure traces are being parsed.
- There are three types of ETW tracing (not really important)
- **WPP** - windows pre processor requires a TMF file which is generated only off of a private .pdb
  - Original type of tracing.
  - C / C ++
- **MOF** - defined in WMI \root\wmi namespace
  - Used for kernel tracing
- **MANIFEST** - trace statements are built into and consumed from the binary itself.
  - Event logs I think are all Manifest based.
  - .net
- Traces that did not get parsed will be displayed with some type of trace / tmf error
- Make sure a large number of traces are not being dropped
- After parsing trace with ETWBench, look at the INF.TXT file. This will display how many events were dropped
- Sometimes it is also good to not how many traces of each guid was traced.
- For example, if comparing a good vs bad trace and say the bad trace showed a guid with only a couple of hits but the good trace didn’t show that guid at all, this may be a clue.
