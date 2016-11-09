rem this script requires node and junit-viewer to be installed.
rem to install junit-viewer, type node install -g junit-viewer
..\Gears.Interpreter.exe Test1.xlsx -junitxml
junit-viewer.cmd --results=\ --save=report.html