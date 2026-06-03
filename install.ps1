# ============================================================
#  AUDITOR - FULL INSTALLATION SCRIPT
#  Run this file as Administrator in PowerShell
# ============================================================

# 1 - Install Chocolatey
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# 2 - Install Docker Desktop
choco install docker-desktop -y

Write-Host ""
Write-Host "============================================================"
Write-Host " Docker installed. RESTART YOUR PC, then run this script"
Write-Host " again starting from STEP 3 (comment out steps 1 and 2)."
Write-Host "============================================================"
Write-Host ""
pause

# 3 - Create folder and docker-compose file
mkdir C:\Auditor -Force
Set-Location C:\Auditor

@"
version: "3.9"

services:

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: auditor-db
    restart: always
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "YourStrong!Passw0rd"
      MSSQL_PID: "Express"
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    networks:
      - auditor-net

  app:
    image: sirwagner44/auditor:latest
    container_name: auditor-app
    restart: always
    depends_on:
      - db
    environment:
      ConnectionStrings__AudConnection: "Server=db,1433;Database=Auditor2;User Id=sa;Password=YourStrong!Passw0rd;MultipleActiveResultSets=True;TrustServerCertificate=True"
      ASPNETCORE_ENVIRONMENT: "Production"
      ASPNETCORE_URLS: "http://+:80"
    ports:
      - "8080:80"
    networks:
      - auditor-net

volumes:
  sqldata:

networks:
  auditor-net:
    driver: bridge
"@ | Out-File -Encoding utf8 C:\Auditor\docker-compose.yml

# 4 - Pull images and start
docker-compose -f C:\Auditor\docker-compose.yml pull
docker-compose -f C:\Auditor\docker-compose.yml up -d

Write-Host ""
Write-Host "============================================================"
Write-Host " DONE. Open your browser and go to: http://localhost:8080"
Write-Host "============================================================"
Write-Host ""
