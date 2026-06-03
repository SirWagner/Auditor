# ============================================================
#  STEP 3 - Pull images and start the application
#  Run as Administrator in PowerShell
# ============================================================

$composePath = "$PSScriptRoot\2-docker-compose.yml"

docker-compose -f $composePath pull
docker-compose -f $composePath up -d

Write-Host ""
Write-Host "============================================================"
Write-Host " DONE. Open your browser and go to: http://localhost:8080"
Write-Host "============================================================"
Write-Host ""
pause
