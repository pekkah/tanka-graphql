param (
    [string]$Output = "./artifacts",
    [string]$CurrentBranch = $Env:BUILD_SOURCEBRANCH
 )

# Utils
function EnsureLastExitCode($message){
    if ($LASTEXITCODE -ne 0) {
        throw $message
    } 
}

# Parameters
"----------------------------------------"
"Output: $Output"
$Location = Get-Location
"Location: $Location"

if ((Test-Path $output) -eq $True) {
    "Clean: $Output"
    Remove-Item -Recurse -Force $Output
}

# Git Information
if ($CurrentBranch -eq '') {
    $CurrentBranch = git branch --show-current | Out-String
    EnsureLastExitCode("git branch --show-current failed")
}

$CurrentBranch = $CurrentBranch.Trim()

"----------------------------------------"
"CurrentBranch: $CurrentBranch"

if ($CurrentBranch -eq '') {
    Write-Error "Not branch or tag"
    return
}

# Install tools
"----------------------------------------"
"Restoring dotnet tools"
dotnet tool restore

EnsureLastExitCode("Restore failed")

# Get GitVersion
"----------------------------------------"
"Getting GitVersion"
$Version = dotnet gitversion /output json /showvariable SemVer | Out-String -NoNewline
EnsureLastExitCode("Could not get SemVer from gitversion")
$Version = $Version.Trim()
"Git version: '$Version'"
"##vso[build.updatebuildnumber]$Version"

$PreReleaseTag = dotnet gitversion /output json /showvariable PreReleaseTag | Out-String -NoNewline
EnsureLastExitCode("Could not get PrReleaseTag from gitversion")
$PreReleaseTag = $PreReleaseTag.Trim()
$IsPreRelease = $PreReleaseTag -ne ''
"PreReleaseTag: $PreReleaseTag, IsPreRelease: $IsPreRelease"

# Is Master
"----------------------------------------"
"Check is master"
$IsMaster = $CurrentBranch -eq 'master'
"Is master: $IsMaster"

# Build and test
"----------------------------------------"
"Build Dotnet"
dotnet build -c Release
EnsureLastExitCode("dotnet build failed")

dotnet test -c Release -l trx -r $Output
EnsureLastExitCode("dotnet test failed")

"----------------------------------------"
"Pack NuGet"
dotnet pack -c Release -o $Output -p:Version=$Version -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
EnsureLastExitCode("dotnet pack failed")

"----------------------------------------"
"Pack NPM"
$Exclude = [string[]]@("node_modules")
Copy-Item -Recurse -Exclude $Exclude ./src/graphql.server.link/ $Output/graphql.server.link
Set-Location $Output/graphql.server.link
npm i
npm run build
EnsureLastExitCode("npm run build failed")
npm --no-git-tag-version --allow-same-version version $Version
Set-Location $Location
Set-Location $Output
npm pack ./graphql.server.link
EnsureLastExitCode("npm pack failed")

Set-Location $Location

"----------------------------------------"
"Benchmarks"
$BechmarkCmd = "dotnet run --project ./benchmarks/graphql.benchmarks/graphql.benchmarks.csproj --configuration Release --framework netcoreapp31 -- -i -m --filter *"

if ($IsPreRelease) {
    $BechmarkCmd += " --job short"
}

Invoke-Expression $BechmarkCmd
EnsureLastExitCode("dotnet benchmarks failed")

"----------------------------------------"
"Docs"
$DocsOutput = "$Output/gh-pages"
$Basepath = "/tanka-graphql/"

if ($IsPreRelease) {
    $DocsOutput += "/beta"
    $Basepath += "beta/"
}

"Output: $DocsOutput"
"BasePath: $Basepath"

dotnet tanka-docs --output $DocsOutput --basepath $Basepath
EnsureLastExitCode("dotnet tanka-docs failed")

"----------------------------------------"
"DONE"
Set-Location $Location