# ============================================================
#  scaffold-all.ps1
#  Scaffolds MVC Controllers + Views for every DbSet in your DbContext
#
#  USAGE:
#    .\scaffold-all.ps1
#    .\scaffold-all.ps1 -DbContextName "AuditorDbContext"
#    .\scaffold-all.ps1 -DbContextName "AuditorDbContext" -DryRun
# ============================================================

param(
    [string]$ProjectPath   = ".",            # Path to your .csproj or project folder
    [string]$DbContextName = "AuDbContext", # Your DbContext class name
    [string]$Namespace     = "",             # Leave empty to auto-detect
    [switch]$DryRun                          # Print commands without executing
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── 1. Resolve project directory ────────────────────────────
if ($ProjectPath -eq ".") {
    $projectDir = Get-Location
} elseif ($ProjectPath.EndsWith(".csproj")) {
    $projectDir = Split-Path $ProjectPath -Parent
} else {
    $projectDir = $ProjectPath
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ASP.NET Core - Scaffold All Entities  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Project dir : $projectDir"
Write-Host "DbContext   : $DbContextName"
Write-Host ""

# ── 2. Ensure dotnet-aspnet-codegenerator is installed ──────
# Write-Host "Checking for dotnet-aspnet-codegenerator..." -ForegroundColor Yellow
# $toolList = dotnet tool list -g 2>$null
# if ($toolList -notmatch "dotnet-aspnet-codegenerator") {
    # Write-Host "Installing dotnet-aspnet-codegenerator globally..." -ForegroundColor Yellow
    # dotnet tool install -g dotnet-aspnet-codegenerator
# } else {
    # Write-Host "dotnet-aspnet-codegenerator already installed." -ForegroundColor Green
# }

# ── 3. Clean obj/bin then restore ───────────────────────────
# NOTE: We skip 'dotnet build' here because the MSBuild static web assets
# target conflicts when run outside VS. The scaffolder triggers its own build.
Write-Host ""
Push-Location $projectDir

Write-Host "Cleaning obj and bin folders..." -ForegroundColor Yellow
@("obj", "bin") | ForEach-Object {
    $p = Join-Path $projectDir $_
    if (Test-Path $p) {
        try {
            Remove-Item -Recurse -Force $p
            Write-Host "  OK - $_ removed." -ForegroundColor Green
        } catch {
            Write-Host "  WARNING - Could not remove $_  (close VS/Rider and retry if scaffolding fails)" -ForegroundColor DarkYellow
        }
    }
}

Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore --nologo
if ($LASTEXITCODE -ne 0) {
    Write-Error "Restore failed. Check your NuGet sources."
    Pop-Location
    exit 1
}
Write-Host "Restore successful." -ForegroundColor Green

# ── 4. Extract DbSet entity names from DbContext ────────────
Write-Host ""
Write-Host "Discovering entities from $DbContextName..." -ForegroundColor Yellow

$dbContextFile = Get-ChildItem -Path $projectDir -Recurse -Filter "*.cs" |
    Where-Object { Select-String -Path $_.FullName -Pattern "class $DbContextName" -Quiet } |
    Select-Object -First 1

if (-not $dbContextFile) {
    Write-Error "Could not find a .cs file containing 'class $DbContextName'. Check -DbContextName parameter."
    Pop-Location
    exit 1
}

Write-Host "Found DbContext at: $($dbContextFile.FullName)" -ForegroundColor Green

$dbSetPattern = 'DbSet<([A-Za-z0-9_]+)>'
$regexMatches = Select-String -Path $dbContextFile.FullName -Pattern $dbSetPattern -AllMatches
$entities = $regexMatches.Matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique

if ($entities.Count -eq 0) {
    Write-Error "No DbSet<T> properties found in $DbContextName. Make sure your entities are registered."
    Pop-Location
    exit 1
}

Write-Host ""
Write-Host "Found $($entities.Count) entities:" -ForegroundColor Green
$entities | ForEach-Object { Write-Host "  - $_" -ForegroundColor White }

# ── 5. Auto-detect namespace if not provided ────────────────
if (-not $Namespace) {
    $csprojFile = Get-ChildItem -Path $projectDir -Filter "*.csproj" | Select-Object -First 1
    if ($csprojFile) {
        $Namespace = [System.IO.Path]::GetFileNameWithoutExtension($csprojFile.Name)
    } else {
        $Namespace = "MyApp"
    }
    Write-Host ""
    Write-Host "Auto-detected namespace: $Namespace" -ForegroundColor Yellow
}

# ── 6. Scaffold each entity ─────────────────────────────────
Write-Host ""
Write-Host "Starting scaffolding..." -ForegroundColor Cyan
Write-Host ""

$success = @()
$failed  = @()

foreach ($entity in $entities) {
    $controllerName = "${entity}sController"
    Write-Host "Scaffolding $entity -> $controllerName" -ForegroundColor Yellow

    $cmd = "dotnet tool run dotnet-aspnet-codegenerator controller " +
           "-name $controllerName " +
           "-m $entity " +
           "-dc $DbContextName " +
           "--relativeFolderPath Controllers " +
           "--useDefaultLayout " +
           "--referenceScriptLibraries"

    if ($DryRun) {
        Write-Host "  [DRY RUN] $cmd" -ForegroundColor DarkGray
        $success += $entity
        continue
    }

    try {
        Invoke-Expression $cmd
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  OK - $entity scaffolded successfully" -ForegroundColor Green
            $success += $entity
        } else {
            Write-Host "  FAIL - $entity failed (exit code $LASTEXITCODE)" -ForegroundColor Red
            $failed += $entity
        }
    } catch {
        Write-Host "  FAIL - $entity threw an exception: $_" -ForegroundColor Red
        $failed += $entity
    }

    Write-Host ""
}

# ── 7. Inject nav links into _Layout.cshtml ─────────────────
if (-not $DryRun -and $success.Count -gt 0) {
    Write-Host "Updating _Layout.cshtml with nav links..." -ForegroundColor Yellow

    $layoutPath = Get-ChildItem -Path $projectDir -Recurse -Filter "_Layout.cshtml" |
        Select-Object -First 1

    if ($layoutPath) {
        $layoutContent = Get-Content $layoutPath.FullName -Raw

        $navLinks = $success | ForEach-Object {
            "                <li class=`"nav-item`">" +
            "<a class=`"nav-link text-dark`" asp-area=`"`" asp-controller=`"${_}s`" asp-action=`"Index`">$_</a>" +
            "</li>"
        }
        $navBlock = "                <!-- BEGIN: scaffolded-entity-links -->`n" +
                    ($navLinks -join "`n") + "`n" +
                    "                <!-- END: scaffolded-entity-links -->"

        if ($layoutContent -match '<!-- BEGIN: scaffolded-entity-links -->') {
            $layoutContent = $layoutContent -replace '<!-- BEGIN: scaffolded-entity-links -->[\s\S]*?<!-- END: scaffolded-entity-links -->', $navBlock
        } else {
            $layoutContent = $layoutContent -replace '(\s*</ul>)', "`n$navBlock`n`$1"
        }

        Set-Content -Path $layoutPath.FullName -Value $layoutContent -Encoding UTF8
        Write-Host "  OK - Nav links added to $($layoutPath.FullName)" -ForegroundColor Green
    } else {
        Write-Host "  WARNING - _Layout.cshtml not found. Add these links manually:" -ForegroundColor DarkYellow
        $success | ForEach-Object {
            Write-Host "    <a asp-controller=`"${_}s`" asp-action=`"Index`">$_</a>" -ForegroundColor Gray
        }
    }
}

Pop-Location

# ── 8. Summary ──────────────────────────────────────────────
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Succeeded : $($success.Count)  ($($success -join ', '))" -ForegroundColor Green

if ($failed.Count -gt 0) {
    Write-Host "  Failed    : $($failed.Count)  ($($failed -join ', '))" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Scaffold failed entities individually with:" -ForegroundColor DarkYellow
    $failed | ForEach-Object {
        Write-Host "  dotnet tool run dotnet-aspnet-codegenerator controller -name ${_}sController -m $_ -dc $DbContextName --useDefaultLayout --referenceScriptLibraries" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Done! Run 'dotnet run' to see your scaffolded app." -ForegroundColor Cyan
Write-Host ""