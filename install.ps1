param
(
  [bool]$install = $False,
  [bool]$package = $False,
  [bool]$reset = $False
)

function reset_package()
{
    Write-Host "===Reset Intalled Templates==="
    Write-Host ""

    Invoke-Expression "dotnet new --debug:reinit"

    Write-Host "Removed user-defined templates."
    Write-Host ""
}

function make_package()
{
    Write-Host "===Packaging Templates==="
    Write-Host ""

    Invoke-Expression "nuget pack multiplatform/xamarin-templates-multiplatform.nuspec"
    Invoke-Expression "nuget pack ios/xamarin-templates-ios.nuspec"

    Write-Host "Successfully packaged templates."
    Write-Host ""
}

function install_package()
{
    $install_path = Get-ChildItem -path $Directory -Recurse -Include *.nupkg
    Write-Host "===Installing Templates==="
    Write-Host ""

    Write-Host ""
    Invoke-Expression "dotnet new --install $install_path"
    Write-Host ""
    Write-Host "Successfully installed templates."
    Write-Host ""
}

If ($install -eq $False -and $reset -eq $False -and $package -eq $False) {
    Set-Variable -Name "install" -Value $True
    Set-Variable -Name "reset" -Value $True
    Set-Variable -Name "package" -Value $True
}

Write-Host ""
Write-Host "===Prerequisites==="

$TARGETDIR = 'C:\Program Files\dotnet\sdk'
if (!(Test-Path -Path $TARGETDIR))
{
    Write-Host "Hold up..."
    Write-Host "dotnet isn't installed. You must install .NET Core to use the dotnet new command."
    Write-Host "Go do that and try again."
    exit
}

Write-Host "Found the dotnet CLI at C:\Program Files\dotnet\sdk."
Write-Host ""

# --reset

if ($reset -eq $True)
{
    reset_package
}

# --package
if ($package -eq $True)
{
    make_package
}

# --install
if ($install -eq $True)
{
    install_package
}