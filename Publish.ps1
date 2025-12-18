<#
.SYNOPSIS
    Publishes the Divoom.Api NuGet package to nuget.org

.DESCRIPTION
    This script performs the following steps:
    1. Checks for clean git working directory (no uncommitted changes)
    2. Determines the Nerdbank GitVersioning version
    3. Validates nuget-key.txt exists, has content, and is gitignored
    4. Runs unit tests (unless -SkipTests is specified)
    5. Publishes to nuget.org

.PARAMETER SkipTests
    Skip running unit tests before publishing

.EXAMPLE
    .\Publish.ps1
    
.EXAMPLE
    .\Publish.ps1 -SkipTests
#>

[CmdletBinding()]
param(
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Colors for output
function Write-Step { param($Message) Write-Host "`n=== $Message ===" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "[SUCCESS] $Message" -ForegroundColor Green }
function Write-Failure { param($Message) Write-Host "[FAILED] $Message" -ForegroundColor Red }
function Write-Info { param($Message) Write-Host "[INFO] $Message" -ForegroundColor Yellow }

# Track script start time
$scriptStartTime = Get-Date

try {
    # Step 1: Check for clean git working directory (porcelain)
    Write-Step "Checking git working directory status"
    
    $gitStatus = git status --porcelain 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to get git status"
        exit 1
    }
    
    if ($gitStatus) {
        Write-Failure "Git working directory is not clean. Uncommitted changes detected:"
        Write-Host $gitStatus
        Write-Host "`nPlease commit or stash your changes before publishing." -ForegroundColor Yellow
        exit 1
    }
    
    Write-Success "Git working directory is clean"

    # Step 2: Determine Nerdbank GitVersioning version
    Write-Step "Determining Nerdbank GitVersioning version"
    
    # Ensure nbgv tool is available
    $nbgvPath = Get-Command nbgv -ErrorAction SilentlyContinue
    if (-not $nbgvPath) {
        Write-Info "Installing nbgv dotnet tool..."
        dotnet tool install --global nbgv 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Failure "Failed to install nbgv tool"
            exit 1
        }
    }
    
    $versionJson = nbgv get-version --format json 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to get version from nbgv"
        Write-Host $versionJson
        exit 1
    }
    
    $versionInfo = $versionJson | ConvertFrom-Json
    $nugetVersion = $versionInfo.NuGetPackageVersion
    $semVer = $versionInfo.SemVer2
    
    Write-Success "Version: $semVer (NuGet: $nugetVersion)"

    # Step 3: Validate nuget-key.txt
    Write-Step "Validating nuget-key.txt"
    
    $nugetKeyPath = Join-Path $PSScriptRoot "nuget-key.txt"
    
    # Check file exists
    if (-not (Test-Path $nugetKeyPath)) {
        Write-Failure "nuget-key.txt not found at: $nugetKeyPath"
        Write-Host "Please create nuget-key.txt with your NuGet API key." -ForegroundColor Yellow
        exit 1
    }
    
    # Check file has content
    $nugetKey = (Get-Content $nugetKeyPath -Raw).Trim()
    if ([string]::IsNullOrWhiteSpace($nugetKey)) {
        Write-Failure "nuget-key.txt is empty"
        Write-Host "Please add your NuGet API key to nuget-key.txt" -ForegroundColor Yellow
        exit 1
    }
    
    # Check file is gitignored
    $gitIgnorePath = Join-Path $PSScriptRoot ".gitignore"
    if (Test-Path $gitIgnorePath) {
        $gitIgnoreContent = Get-Content $gitIgnorePath -Raw
        if ($gitIgnoreContent -notmatch 'nuget-key\.txt') {
            Write-Failure "nuget-key.txt is not in .gitignore"
            Write-Host "Please add 'nuget-key.txt' to .gitignore to prevent accidental commits." -ForegroundColor Yellow
            exit 1
        }
    } else {
        Write-Failure ".gitignore file not found"
        exit 1
    }
    
    # Double-check with git check-ignore
    $isIgnored = git check-ignore "nuget-key.txt" 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "nuget-key.txt is not being ignored by git"
        Write-Host "Please ensure 'nuget-key.txt' is properly added to .gitignore" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Success "nuget-key.txt is valid and gitignored"

    # Step 4: Run unit tests (unless -SkipTests)
    if ($SkipTests) {
        Write-Step "Skipping unit tests (-SkipTests specified)"
    } else {
        Write-Step "Running unit tests"
        
        $testResult = dotnet test --configuration Release --verbosity minimal 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Failure "Unit tests failed"
            Write-Host $testResult
            exit 1
        }
        
        Write-Success "All unit tests passed"
    }

    # Step 5: Build and pack
    Write-Step "Building and packing NuGet package"
    
    $projectPath = Join-Path $PSScriptRoot "Divoom.Api" "Divoom.Api.csproj"
    
    # Clean and build in Release mode
    dotnet clean $projectPath --configuration Release --verbosity minimal 2>&1 | Out-Null
    
    $buildOutput = dotnet build $projectPath --configuration Release --verbosity minimal 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Build failed"
        Write-Host $buildOutput
        exit 1
    }
    
    # Pack (this generates the .nupkg)
    $packOutput = dotnet pack $projectPath --configuration Release --no-build --verbosity minimal 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Pack failed"
        Write-Host $packOutput
        exit 1
    }
    
    Write-Success "Package built successfully"

    # Find the generated package
    $packageDir = Join-Path $PSScriptRoot "Divoom.Api" "bin" "Release"
    $nupkgFile = Get-ChildItem -Path $packageDir -Filter "Divoom.Api.$nugetVersion.nupkg" -ErrorAction SilentlyContinue | Select-Object -First 1
    
    if (-not $nupkgFile) {
        # Try finding any nupkg file
        $nupkgFile = Get-ChildItem -Path $packageDir -Filter "*.nupkg" -ErrorAction SilentlyContinue | 
                     Where-Object { $_.Name -notmatch '\.symbols\.' } | 
                     Select-Object -First 1
    }
    
    if (-not $nupkgFile) {
        Write-Failure "Could not find generated .nupkg file in $packageDir"
        exit 1
    }
    
    Write-Info "Package: $($nupkgFile.Name)"

    # Step 6: Publish to NuGet.org
    Write-Step "Publishing to NuGet.org"
    
    $pushOutput = dotnet nuget push $nupkgFile.FullName --api-key $nugetKey --source "https://api.nuget.org/v3/index.json" --skip-duplicate 2>&1
    if ($LASTEXITCODE -ne 0) {
        # Check if it's a duplicate error (which is acceptable with --skip-duplicate)
        if ($pushOutput -match "already exists|skip-duplicate") {
            Write-Info "Package version already exists on NuGet.org (skipped)"
        } else {
            Write-Failure "Failed to publish to NuGet.org"
            Write-Host $pushOutput
            exit 1
        }
    } else {
        Write-Success "Published $($nupkgFile.Name) to NuGet.org"
    }

    # Summary
    $elapsed = (Get-Date) - $scriptStartTime
    Write-Host "`n" -NoNewline
    Write-Host "========================================" -ForegroundColor Green
    Write-Host " PUBLISH COMPLETED SUCCESSFULLY" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host " Package: Divoom.Api" -ForegroundColor White
    Write-Host " Version: $nugetVersion" -ForegroundColor White
    Write-Host " Time:    $($elapsed.ToString('mm\:ss'))" -ForegroundColor White
    Write-Host "========================================" -ForegroundColor Green
    
    exit 0
}
catch {
    Write-Failure "An unexpected error occurred"
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    exit 1
}
