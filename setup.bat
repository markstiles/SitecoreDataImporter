
Powershell.exe -executionpolicy remotesigned -File  scripts\nant\LocalProperties.ps1

@scripts\nant\nant-0.90\bin\nant.exe -buildfile:scripts\nant\solution.build init 

call npm install
