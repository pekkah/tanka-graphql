param (
    [string]$Output = "./artifacts",
    [string]$CurrentBranch ='',
    [bool]$OnlyBuild = $False
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
"OnlyBuild: $OnlyBuild"

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

# Build and test
"----------------------------------------
"Build"
dotnet restore
dotnet build -c Release --no-restore
EnsureLastExitCode("dotnet build failed")

if ($OnlyBuild -eq $False) {
    "----------------------------------------"
    "Run tests"
    dotnet test -c Release --results-directory $Output --no-restore --no-build --collect:"XPlat Code Coverage"
    EnsureLastExitCode("dotnet test failed")

    "----------------------------------------"
    "Pack NuGet"
    dotnet pack -c Release -p:Version=$Version -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
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

}
"----------------------------------------"
"DONE"
Set-Location $Location
