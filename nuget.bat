@echo off
IF NOT EXIST "nupkg" MD "nupkg"
cd nupkg
del /q/s *.nupkg
set nuget="..\tool\nuget"
set nuspec="..\nuget"
for /f "delims=\" %%a in ('dir /b /a-d "%nuspec%\*.nuspec"') do (
%nuget% pack "%nuspec%\%%a"
)
