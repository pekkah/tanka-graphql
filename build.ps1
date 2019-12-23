param (
    [string]$Output = "./artifacts",
    [string]$CurrentBranch = $Env:BUILD_SOURCEBRANCH
 )

# Parameters
"----------------------------------------"
"Output: $Output"
$Location = Get-Location
"Location: $Location"

# Git Information
if ($CurrentBranch -eq '') {
    $CurrentBranch = git branch --show-current | Out-String
}

$Tag = git describe --tags --exact-match 2>$null

if ($null -eq $Tag) {
    $Tag = ''
}

$CurrentBranch = $CurrentBranch.Trim()
$Tag = $Tag.Trim();

"----------------------------------------"
"CurrentBranch: $CurrentBranch"
"Tag: $Tag"

if ($CurrentBranch -eq '' -and $Tag -eq '') {
    Write-Error "Not branch or tag"
    return
}

# Install tools
"----------------------------------------"
"Restoring dotnet tools"
dotnet tool restore

# Get GitVersion
"----------------------------------------"
"Getting GitVersion"
$Version = dotnet gitversion /output json /showvariable SemVer | Out-String
$Version = $Version.Trim()
"Git version: '$Version'"
"##vso[build.updatebuildnumber]$Version"

# Is Master Or Tag?
"----------------------------------------"
"Check is master or tag"
$IsMasterOrTag = $CurrentBranch -eq 'master' -or $Tag -ne ''
"Is tag or master: $IsMasterOrTag"

# Build
"----------------------------------------"
"Build Dotnet"
dotnet build -c Release
dotnet test -c Release -l trx -r $Output

# Build NPM package
"----------------------------------------"
"Build Node"
Set-Location ./src/graphql.server.link
npm i
npm run build
Set-Location $Location
