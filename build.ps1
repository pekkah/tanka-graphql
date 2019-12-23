param (
    [string]$Output = "./artifacts",
    [string]$CurrentBranch = $Env:BUILD_SOURCEBRANCH
 )

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

# Get GitVersion
"----------------------------------------"
"Getting GitVersion"
$Version = dotnet gitversion /output json /showvariable SemVer | Out-String -NoNewline
$Version = $Version.Trim()
"Git version: '$Version'"
"##vso[build.updatebuildnumber]$Version"

$PreReleaseTag = dotnet gitversion /output json /showvariable PreReleaseTag | Out-String
$PreReleaseTag = $PreReleaseTag.Trim()
$IsPreRelease = $PreReleaseTag -ne '' -or $null -ne $PreReleaseTag
"PreReleaseTag: $PreReleaseTag, IsPreRelease: $IsPreRelease"

# Is Master
"----------------------------------------"
"Check is master"
$IsMaster = $CurrentBranch -eq 'master'
"Is master: $IsMaster"

# Build
"----------------------------------------"
"Build Dotnet"
dotnet build -c Release
dotnet test -c Release -l trx -r $Output

"----------------------------------------"
"Pack NuGet"
dotnet pack -c Release -o $Output -p:Version=$Version -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg

"----------------------------------------"
"Pack NPM"
$Exclude = [string[]]@("node_modules")
Copy-Item -Recurse -Exclude $Exclude ./src/graphql.server.link/ $Output/graphql.server.link
Set-Location $Output/graphql.server.link
npm i
npm run build
npm --no-git-tag-version --allow-same-version version $Version
Set-Location $Location
Set-Location $Output
npm pack ./graphql.server.link

Set-Location $Location

"----------------------------------------"
"Benchmarks"
$BechmarkCmd = "dotnet run --project ./benchmarks/graphql.benchmarks/graphql.benchmarks.csproj --configuration Release --framework netcoreapp31 -- -i -m --filter *"

if ($IsPreRelease) {
    $BechmarkCmd += " --job short"
}

Invoke-Expression $BechmarkCmd

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

"----------------------------------------"
"DONE"
Set-Location $Location