# Solidworks Scripting
Interface SolidWorks programmatically 

## Installation

* Make sure you have `nuget` and `dotnet` with sdk installed
* If you are going to build in Linux (WSL 2), also make sure that aforementioned utilities are in PATH
  * Why do you might want to build a Windows exe from Linux? To interface Solidworks from something like github/gitlab runner or Docker!


* First, clone this repo

  `git clone https://github.com/Innopolis-UAV-Team/solidworks_script`
* In the project directory, install the dependencies with
  
  `nuget.exe install ./sw_exporter/packages.config -OutputDirectory ./packages`
  
  Pay attention to slashes - Windows can understand both forward- and back-slashes, while Linux can not.
* Then build the project

  `dotnet.exe build`

* Your executable should be now in `./sw_exporter/bin/Debug/sw_exporter.exe`
* You can then run the Solidworks Interface with something like this

  `./sw_exporter/bin/Debug/sw_exporter.exe -f <full_path_to_asm> --timeout=40 --out=out.json`
  
  This will put parsed data into `./out.json` file
