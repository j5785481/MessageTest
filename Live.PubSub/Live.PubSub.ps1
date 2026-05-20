# 取得admin權限 請先設定*.ps1預設開啟為powershell
if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Start-Process PowerShell -Verb RunAs "-NoProfile -ExecutionPolicy Bypass -Command `"cd '$pwd'; & '$PSCommandPath';`"";
    exit;
}

# NuGet路徑
$nugetUrl = "http://10.100.5.171:8089"
$specName = $MyInvocation.MyCommand.Name.Substring(0, $MyInvocation.MyCommand.Name.indexof(".ps1")) + ".nuspec"

$paths=@(
    "bin",
    "obj",
    "*.nupkg"
)

foreach($path in $paths){
    if(Test-Path $path){
        Remove-Item $path
    }
}

$spec = (Get-Content $specName)

# 取版本號 (ex: 1.0.0 要符合此命名規範)
$version = $spec[4].Substring($spec[4].indexof("<version>") + 9)
$version = $version.Substring(0, $version.indexof("</version>")).split(".")
# 版本號+1
$version[2] = [int]$version[2] + 1

$ofs = "."
$spec[4] = "<version>$version</version>"
# 回寫版本號
Set-Content $specName -Value $spec

# 記得加windows path
nuget.exe restore ..\Live.PubSub.sln
MSBuild.exe
nuget.exe pack

nuget.exe push *.nupkg -Source $nugetUrl

read-host