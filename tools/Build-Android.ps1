param([string]$AndroidSdkDirectory="C:\Program Files (x86)\Android\android-sdk",[string]$JavaSdkDirectory="C:\Program Files\Eclipse Adoptium\jdk-17.0.15.6-hotspot")
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot -Parent
$mobileTools=Join-Path $repo '.artifacts/maui-toolchain'
$env:DOTNET_ROOT=Join-Path $mobileTools 'dotnet'
$env:DOTNET_CLI_HOME=Join-Path $mobileTools 'cli'
$env:NUGET_PACKAGES=Join-Path $mobileTools 'packages'
$env:NUGET_HTTP_CACHE_PATH=Join-Path $mobileTools 'http-cache'
$env:TEMP=Join-Path $mobileTools 'tmp'
$env:TMP=$env:TEMP
$dotnet=Join-Path $env:DOTNET_ROOT 'dotnet.exe'
if(!(Test-Path -LiteralPath $dotnet)){throw 'Falta el SDK .NET MAUI aislado. Instala .NET 10 y maui-android en .artifacts/maui-toolchain/dotnet antes de compilar.'}
& $dotnet build (Join-Path $repo 'src/ForensicEdu.Mobile/ForensicEdu.Mobile.csproj') -f net10.0-android --artifacts-path (Join-Path $repo '.artifacts/mobile-android') "-p:AndroidSdkDirectory=$AndroidSdkDirectory" "-p:JavaSdkDirectory=$JavaSdkDirectory" -v:minimal
if($LASTEXITCODE -ne 0){throw "La compilación terminó con código $LASTEXITCODE"}

$apkSource=Join-Path $repo '.artifacts/mobile-android/bin/ForensicEdu.Mobile/debug_net10.0-android/pe.forense.academia-Signed.apk'
$apkDirectory=Join-Path $repo '.artifacts/apk'
New-Item -ItemType Directory -Path $apkDirectory -Force | Out-Null
Copy-Item -LiteralPath $apkSource -Destination (Join-Path $apkDirectory 'FORENSIQ.apk') -Force
Write-Output (Join-Path $apkDirectory 'FORENSIQ.apk')
