# Auditor — Installation

You only need **Docker** (Docker Desktop on Windows/Mac, or Docker Engine with the Compose plugin on Linux) and the file `docker-compose.yml`.

## 1. Set your passwords (recommended)

Create a file called `.env` in the same folder as `docker-compose.yml`:

```
SA_PASSWORD=Choose-A-Strong-Sql-Password1!
ADMIN_EMAIL=admin@yourcompany.com
ADMIN_PASSWORD=Choose-An-Admin-Password1!
```

`SA_PASSWORD` must be at least 8 characters with upper case, lower case, a number and a symbol (SQL Server rule). Set both passwords **before the first start**; the administrator account is only created once.

## 2. Start

```
docker compose up -d
```

The first start downloads the images (about 2.5 GB), creates the database, and loads the ACSA airports, question bank, audit templates and sites. Allow a few minutes.

## 3. Sign in

Open **http://SERVER:8580** and sign in with `ADMIN_EMAIL` / `ADMIN_PASSWORD`
(defaults: `admin@auditor.local` / `Admin#2026!`). Change the password from the account menu (top right), then create users under **Users**.

| Role    | Can do |
|---------|--------|
| Admin   | Everything, including user management |
| Manager | Airports, sites, questions, templates, schedules; reviews submitted audits |
| Auditor | Accepts and completes the audits assigned to them |
| Viewer  | Read-only dashboard and audit results |

## Ports

| Service    | Port  | Change with |
|------------|-------|-------------|
| Web app    | 8580  | `WEB_PORT`  |
| SQL Server | 14333 | `SQL_PORT`  |

Non-default ports are used so the stack doesn't clash with an existing SQL Server (1433) or web server (80/8080).

## Day-to-day

| Task | Command |
|------|---------|
| Update to the latest version | `docker compose pull` then `docker compose up -d` (database upgrades run automatically) |
| View the app log | `docker compose logs -f web` |
| Stop | `docker compose down` (data is kept) |
| Back up the database | `docker exec auditor-sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<SA_PASSWORD>" -C -Q "BACKUP DATABASE [Auditor] TO DISK='/var/opt/mssql/backup/Auditor.bak' WITH INIT"` |

Data lives in two Docker volumes: `auditor_sqldata` (database) and `auditor_appdata` (audit photos/attachments and sign-in keys). Back up both.

For access over the internet, put the app behind an HTTPS reverse proxy (IIS, nginx, Caddy, or a cloud load balancer) that forwards to port 8580.
