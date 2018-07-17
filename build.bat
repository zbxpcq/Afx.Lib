@echo off
set Build="%SYSTEMDRIVE%\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe"
if exist publish rd /s /q publish
%Build% "NET20\Afx.NET20.sln" /t:Rebuild /p:Configuration=Release
%Build% "NET40\Afx.NET40.sln" /t:Rebuild /p:Configuration=Release
%Build% "NET452\Afx.NET452.sln" /t:Rebuild /p:Configuration=Release
dotnet build "NETCore2.1\Afx.NETCore2.1.sln" -c Release 
dotnet build "NETStandard2.0\Afx.NETStandard2.0.sln" -c Release 
cd Publish
del /q/s *.pdb