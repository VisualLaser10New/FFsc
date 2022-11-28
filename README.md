# FFsc
*Fast File search console* is a tool to enhance the Windows files and folders search.

## Requirements
To run it:
- .Net Framework 5.0 library
- SQL Server for LocalDB

To compile it:
- I advice to use VS 2022 or 2019
- C# support
- .Net Framework > 5.0
- SQL Server

## How it works
It works with a symply structure: ther're two program, the first is a daemon, the seacond is the callable program. At setup the first will setted in autostart, it scans all folders respecting the config.xml file, when it's finished it runs in background as analyzer: it wait for a filesystem event and captures it. Everything is then stored in a Database, that runs with SQL server.

### FFscw
This is the daemon, "w" stands for *Worker*. 
At it's *first start* it scan the disk following the precompiled, by the user, config file, and then analyze the filesystem, for a changing, when an event is fired it update the database. Else it starts at the windows logon in background and keep analyzing the disks.

Parameters:
- Nothing: works as described above
- -r: reset the database and starts as the *first start*
- -v: enable the verbosity of program

### FFsc
This is the callable, from terminal, command. It provvide to search the files and directory specified, by connecting to database and getting datas.

Parameters:
- Nothing: show the guide
- -p [string]: The path from where the search starts (optional, if there isn't, use the cwd as path)
- -s [string]: The regex pattern of what are searching
- -r: Use recursive mode to search in the folders too

### Config file
The config file is in the config folder and is the file that ffscw follows to scans the directory.
It supports multiple configs, and it will validated by the program. If either config.xml or config.xsd are deleted the program doesn't start.
Foreach config, set an progressive ID, a root directory (from where the program begins to scan), a boolean value that indicate if config is active.

Inside the config element are two elements: directory and files, each one contains others two elements: excluded and included.
You can create several excluded or included elements to sets what files or dirs must to be excluded, foreach element it's possible to set the path (to exclude) or the name (of files or dirs to exclude), then specify the xsi-schema and the xsi:type (must be, respectively to what you choose, "path" or "name").

The included elements prevail over the excluded elements, this is used to exclude a directory but include only some files contained in it.


## How to run
To runs it perform the follow instructions.
### Fill the config.xml
In the config folder there's a config.xml file. It's the file that FFscw respects to scanning the disks, so before run anything fill it. Read the Config File section to know more.

### Setup
Run setup.bat, i advice to runs it as admin, but it's not necessary. The bat file will remember to fill the config file, if i'ts done press any key.
After the installation FFscw starts automatically, it's begin to scan hard drives and to analyze the changing.

If something went wrong, just re-run the setup.bat.

### Searching file and folder
After the FFscw.exe has scanned the disks it will hide itself. So the program is ready to do a research.
Open cmd on desktop and try to call the "ffsc" as command if it works it will show the guide, else restart the pc.
When ffsc is callable from any folder let's do a search by passing the correct parameters:
```
ffsc -p "C:\\folder\to\search" -s %.txt -r
```
Rules:
- Don't type the backslash after the last name of folder in -p parameter
- Put the path between double-quotes
- Use double-backslash after the disk name
- The regex of -s args is not normal regex, in reality is *SQL Wildcard*
- If you don't write the -p parameter it shoulds search from the cwd
- The program search only in the scanned path