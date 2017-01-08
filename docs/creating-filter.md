![](../TextFilter/Images/ico.png)

# TextFilter Creating a Filter
## String filters
TextFilter now defaults to 'string' filters instead of 'regex' filters to allow easier filtering.
String filters are literal filters with no 'escaping' or pattern matching abilities.
There are two operands that can be used in this utility however which are 'AND' and 'OR'
### Example file data:
1. this is a test
2. this is another test
3. this is the last test
4. this is now finished

### Example string filters:
- 'this' would match first 3 lines in above file data
- 'another' would match the 2nd line in above file data
- 'another OR finished' would match the 2nd and 4th lines in above file data
- 'this AND finished' would match the 4th line in above file data
- 'this OR is AND test' would match the first 3 lines in above file data

## Regex filters
dCreating a filter using regex is not as scary as it looks. Filters can be as easy or as complicated as you want them to be. The same goes for regex in general. Remember you can always start off with simple strings and work your way into the true capabilities of regex. 

Regex is a common text filtering type language across many programming languages. PowerShell, C++, .net, java, pearl, and even findstr.exe in command prompt all use regex and have very similar syntax just to name a few. Learning how to use regex will make parsing log files quicker and easier. There are many sites that have documentation around Regex. To be clear, I only know a fraction of the capability provided by regex but what little I know helps tremendously. 

Here are a couple of sites that I have used:  
[regexr.com](http://www.regexr.com/) - nice navigation. Has cheat sheet, examples, and intellisense.  
[myregextester.com](https://www.myregextester.com/) - a great site to test expressions though more geared towards developing. Providing sample text and expression will return the results. It even will provide pseudo code.  
[regular-expressions.info](http://www.regular-expressions.info/) - great source of documentation for regex in general.  
[regular-expressions.info/quickstart](http://www.regular-expressions.info/quickstart.html)  
[regular-expressions.info/refquick](http://www.regular-expressions.info/refquick.html)  
[regular-expressions.info/refreplacecharacters](http://www.regular-expressions.info/refreplacecharacters.html)  

## Some basic Regex commands that provide immediate value:
### Wildcards
- `.` - matches any single character
- `*` - matches preceding character 0 or more times. (to be used with `.`)
- `+` - matches preceding character 1 or more times. (to be used with `.`) 
- **Note: It is much more efficient to use `+` when searching if at all possible. `*` can cause searching to be inefficient.**
  - For example: `this is a test` can be matched by `this.+test`
- There is a way to AND two searches statements in order. For true AND`ing in any order, see below.

### AND'ing
- AND`ing uses a non capturing group syntax which allows matches to be in any order.
- The basic syntax is `(?=.+%match%)(?=.+%match2%)(?=.+%match3%)...`
  - **Note: any matches that may be at start of line will need `.*`**
  - For example: `this is a test` 
- The syntax could be `(?=this)(?=.+is)(?=.+test)`
- The syntax could be `(?=.*this)(?=.+is)(?=.+test)`
- The syntax could be `(?=.+test)(?=.+is)(?=.*this)`

### OR'ing
- Simply use pipe `|` to or one or more expressions
  - For example: if I want filter a file for any lines that have the words `this` or `test` in any order then my expression would be {"`this|test`"}

### Escaping
- Escaping is the way to tell the parser to process the following character as a literal and NOT as a part of the regex expression syntax.
- To build regex expressions, there are multiple `special characters` (like in any language) that have a different meaning. 
- For Regex, the escape character is a blackslash `\` (which is the same as C#).
- From the above wildcards and or`ing, we have already identified 3 special characters (4 counting the escape character itself). (.,*,|,\)
  - **Example 1**: I want to search for a `.` in a trace. `.` is a special character that matches any single character. Given the sample text `this is a test. Where is the period?` we can use the escape character `\` to specify that the `.` is the literal character we are searching for. The regular expression would be `\.` to search for the period in the trace.
  - **Example 2**: I want to search for a trace that has a `\` in it. `\` is a special character that is the escape character. Given the sample text `HKEY_LOCAL_MACHINE\SOFTWARE` we again use the escape character `\` to specify the following character will be a literal. The regular expression would be `\\` to search for a trace with a backslash in it.
- In general if the character being searched for is non alpha-numeric, there is a decent chance the character is a special character in regex.

### Grouping
- Grouping characters are the parenthesis characters `(` and `)`
- Grouping is a way to group together multiple expressions OR `|`
  - **Example**: this pattern: `this (is|a) test` would match input string: `this is test` or `this a test` but not `this is a test`. Further, it defines the OR pattern to be is OR a. If this pattern was used: this is|a test, then input string `this is a test` would be matches twice for `this is` and for `a test` because the OR patterns are now `this is` OR `a test`.
- It can also be used to extract information from a string (a capturing group)
- Lastly it can be used to specify a named group.
- This is useful if multiple groups are being used as they can be identified by name. 
  - **Example**: for input string: `this is a test`, named groups `(?<Group1>this is)(?<Group2>a test)`
- **Note: Any name can be used that complies with general code rules. for textFilter however, either use no name or names Group1 - Group4.**

### Other useful 'Special Characters'
- `^` - matches at the beginning of the string. 
  - **Example**: for input string: `this is a test`, this pattern: `^test` would not be a match, but `test` would be a match.
- `$` - matches an the end of the string
  - **Example**: for input string: `this is a test`, this pattern: `this$` would not be a match, but `this` would be a match.
- `\w` - matches any word character (non white space)
- `\d` - matches any digit (0-9)
- `\s` - matches any white space character such as {space} or {tab}
- `\b` - matches any word boundary which is going from a non white space character to a white space or vice versa


