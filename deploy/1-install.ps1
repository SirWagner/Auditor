# ============================================================
#  STEP 1 - Install Chocolatey and Docker Desktop
#  Run as Administrator in PowerShell
# ============================================================

Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

choco install docker-desktop -y

Write-Host ""
Write-Host "============================================================"
Write-Host " Done. RESTART YOUR PC, then run 3-start.ps1"
Write-Host "============================================================"
Write-Host ""
pause
