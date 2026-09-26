#Requires -Version 7.0
<#
.SYNOPSIS
  Compute next SemVer from Conventional Commits since the last vX.Y.Z tag.
.DESCRIPTION
  Writes GitHub Actions outputs:
    skip    - 'true' when no feat/fix/breaking since last tag
    version - X.Y.Z (no v prefix)
    tag     - vX.Y.Z
  Matches WinSpot constitution Principle VIII (stable releases on main only).
#>
param(
    [string]$MatchTagPattern = 'v[0-9]*.[0-9]*.[0-9]*'
)

$ErrorActionPreference = 'Stop'

function Write-GitHubOutput {
    param([string]$Name, [string]$Value)
    if ($env:GITHUB_OUTPUT) {
        Add-Content -Path $env:GITHUB_OUTPUT -Value "$Name=$Value"
    }
    Write-Host "output $Name=$Value"
}

function Get-LatestStableTag {
    $tags = git tag --list $MatchTagPattern --sort=-v:refname 2>$null
    if (-not $tags) {
        return $null
    }
    foreach ($t in ($tags | ForEach-Object { $_.Trim() } | Where-Object { $_ })) {
        if ($t -match '^v\d+\.\d+\.\d+$') {
            return $t
        }
    }
    return $null
}

$latest = Get-LatestStableTag
$major = 0
$minor = 0
$patch = 0

if ($latest) {
    if ($latest -notmatch '^v(\d+)\.(\d+)\.(\d+)$') {
        throw "Unrecognized tag format: $latest"
    }
    $major = [int]$Matches[1]
    $minor = [int]$Matches[2]
    $patch = [int]$Matches[3]
    $range = "$latest..HEAD"
    Write-Host "Last stable tag: $latest ($major.$minor.$patch)"
}
else {
    $range = 'HEAD'
    Write-Host 'No stable tag found; baselining from 0.0.0'
}

$subjects = @(git log $range --pretty=format:%s --no-merges 2>$null)
$bodies = @(git log $range --pretty=format:%b --no-merges 2>$null)

# Also include merge commit subjects (squash merges often appear as a single commit without --no-merges exclusion issues)
if (-not $subjects -or $subjects.Count -eq 0) {
    $subjects = @(git log $range --pretty=format:%s 2>$null)
    $bodies = @(git log $range --pretty=format:%B 2>$null)
}

$bump = $null # major | minor | patch

function Consider-Bump {
    param([string]$Level)
    if ($Level -eq 'major') {
        $script:bump = 'major'
        return
    }
    if ($Level -eq 'minor' -and $script:bump -ne 'major') {
        $script:bump = 'minor'
        return
    }
    if ($Level -eq 'patch' -and -not $script:bump) {
        $script:bump = 'patch'
    }
}

$allText = (@($subjects) + @($bodies)) -join "`n"
if ($allText -match '(?m)^BREAKING CHANGE:') {
    Consider-Bump 'major'
}

foreach ($subject in $subjects) {
    if ([string]::IsNullOrWhiteSpace($subject)) { continue }
    $s = $subject.Trim()
    Write-Host "commit: $s"

    if ($s -match '^[a-zA-Z]+(\([^)]*\))?!:') {
        Consider-Bump 'major'
        continue
    }
    if ($s -match '^feat(\([^)]*\))?:') {
        Consider-Bump 'minor'
        continue
    }
    if ($s -match '^fix(\([^)]*\))?:') {
        Consider-Bump 'patch'
        continue
    }
}

if (-not $bump) {
    Write-Host 'No feat/fix/breaking commits — skipping release.'
    Write-GitHubOutput -Name 'skip' -Value 'true'
    Write-GitHubOutput -Name 'version' -Value ''
    Write-GitHubOutput -Name 'tag' -Value ''
    exit 0
}

switch ($bump) {
    'major' { $major++; $minor = 0; $patch = 0 }
    'minor' { $minor++; $patch = 0 }
    'patch' { $patch++ }
}

$version = "$major.$minor.$patch"
$tag = "v$version"
Write-Host "Next version: $tag (bump=$bump)"

Write-GitHubOutput -Name 'skip' -Value 'false'
Write-GitHubOutput -Name 'version' -Value $version
Write-GitHubOutput -Name 'tag' -Value $tag
