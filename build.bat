
"%SYSTEMDRIVE%\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" "NET20\Afx.NET20.sln" /t:Rebuild /p:Configuration=Release
"%SYSTEMDRIVE%\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" "NET40\Afx.NET40.sln" /t:Rebuild /p:Configuration=Release
"%SYSTEMDRIVE%\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" "NET452\Afx.NET452.sln" /t:Rebuild /p:Configuration=Release
dotnet build --configuration Release "NETCore2.1\Afx.NETCore2.1.sln"
dotnet build --configuration Release "NETStandard2.0\Afx.NETStandard2.0.sln"