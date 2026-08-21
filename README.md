# Project Management System

A production-oriented project management backend built with **.NET 10**, **ASP.NET Core Web API**, **Entity Framework Core**, **SQLite**, **JWT authentication**, **refresh token rotation**, **Docker**, and **integration tests**.

The system includes project and task management, role and membership authorization, comments, time tracking, task history, dashboard metrics, user management, database backup support, and an internal mailbox with secure file attachments.

---

## Features

### Authentication & Security

- JWT access tokens
- Refresh token rotation
- Refresh token reuse detection
- Refresh token revocation
- Token versioning
- Logout and logout-all
- Password change and admin password reset
- Role-based authorization
- Project membership authorization
- Global exception handling
- CORS
- Rate limiting
- Health checks

> Access tokens are not stored in the database. Refresh tokens are stored only as hashes.

### Projects

- Create, update, delete and list projects
- Project detail view
- Project member management
- Membership roles: `Member`, `Contributor`, `Viewer`

### Tasks

- Create, update and delete tasks
- Assign users
- Change task status
- Priority management
- Due dates
- Estimated / actual hours
- Overdue tracking

Task statuses:

- `Todo`
- `InProgress`
- `InReview`
- `Done`

Task priorities:

- `Low`
- `Medium`
- `High`
- `Critical`

### Comments, Time Logs & History

- Task comments
- Time log creation and summaries
- Task history records for important changes

### Dashboard

Includes project and task statistics such as:

- Total / active / completed projects
- Task counts by status
- Overdue tasks
- Assigned tasks
- Estimated vs actual hours
- Completion percentages

### Internal Mailbox

Users can send internal messages to other registered users.

Supported capabilities:

- Inbox
- Sent messages
- Multiple recipients
- Read / unread state
- User-specific deletion
- Search and pagination
- Attachment filtering
- Protected file downloads

Supported attachment types:

- PDF
- DOC
- DOCX
- ZIP
- PNG
- JPG / JPEG

Attachment rules:

- Up to 10 files per message
- Maximum single file size: 200 MB
- Maximum total attachment size per message: 200 MB
- Files are stored on disk, not as database BLOBs
- Expired physical files are cleaned while message metadata remains
- File extension, MIME type and file signature validation are applied

---

## Tech Stack

### Backend

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- SQLite
- FluentValidation
- Swagger / OpenAPI

### Infrastructure

- Docker
- Docker Compose
- Persistent SQLite volume
- Local database backup
- Local mailbox file storage
- Background hosted services

### Testing

- xUnit
- `Microsoft.AspNetCore.Mvc.Testing`
- SQLite in-memory integration tests
- FluentAssertions

---

## Solution Structure

```text
ProjectManagementSystem/
├── ProjectManagement.Api/
├── ProjectManagement.Application/
├── ProjectManagement.Domain/
├── ProjectManagement.Infrastructure/
├── ProjectManagement.Api.IntegrationTests/
├── docker-compose.yml
├── .env.example
├── .gitignore
└── README.md
```

The project follows a layered architecture:

```text
API
 ↓
Application
 ↓
Domain
 ↑
Infrastructure
```

---

## Requirements

### Recommended

- Docker Desktop
- Git

### Local development without Docker

- .NET 10 SDK

---

## Quick Start with Docker

Clone the repository:

```bash
git clone https://github.com/alperensarac1/ProjectManagementSystem.git
cd ProjectManagementSystem
```

Create your environment file.

### Windows PowerShell

```powershell
Copy-Item .env.example .env
```

### Linux / macOS

```bash
cp .env.example .env
```

Open `.env` and replace the placeholder JWT secret with a strong random value.

Then start the application:

```bash
docker compose --env-file .env up -d --build
```

Check container status:

```bash
docker compose ps
```

View logs:

```bash
docker compose logs -f
```

---

## Swagger

After startup, Swagger is available at:

```text
http://localhost:8080/swagger
```

---

## Health Check

Readiness endpoint:

```text
http://localhost:8080/health/ready
```

---

## Database

The application uses SQLite.

Inside Docker, the database is stored in a persistent volume so container recreation does not remove application data.

To stop the project safely:

```bash
docker compose down
```

> **Warning:** `docker compose down -v` may delete the SQLite Docker volume and therefore application data.

---

## Environment Configuration

An example file is included as `.env.example`.

Important values include:

```env
JWT_SECRET=CHANGE_ME_WITH_A_LONG_RANDOM_SECRET
JWT_ISSUER=ProjectManagement.Api
JWT_AUDIENCE=ProjectManagement.Client

CORS_ALLOWED_ORIGINS=http://localhost:5173

MAILBOX_UPLOAD_HOST_PATH=./mailbox-uploads
MAILBOX_RETENTION_MONTHS=1
MAILBOX_CLEANUP_INTERVAL_HOURS=6
MAILBOX_CLEANUP_ENABLED=true

DATABASE_BACKUP_HOST_PATH=./backups
```

Never commit your real `.env` file or production secrets.

---

## Mailbox Storage

Mailbox attachments are stored outside SQLite.

Docker path:

```text
/app/uploads/mailbox
```

The host path can be configured through `.env`.

Example:

```env
MAILBOX_UPLOAD_HOST_PATH=./mailbox-uploads
```

---

## Database Backups

Backup files are stored outside the repository and should never be committed to Git.

Example host path:

```env
DATABASE_BACKUP_HOST_PATH=./backups
```

---

## Migrations

Create a new migration:

```bash
dotnet ef migrations add MigrationName \
  --project ProjectManagement.Infrastructure \
  --startup-project ProjectManagement.Api
```

Apply migrations:

```bash
dotnet ef database update \
  --project ProjectManagement.Infrastructure \
  --startup-project ProjectManagement.Api
```

---

## Build

```bash
dotnet build
```

---

## Tests

Run all integration tests:

```bash
dotnet test ProjectManagement.Api.IntegrationTests/ProjectManagement.Api.IntegrationTests.csproj
```

Run only mailbox tests:

```bash
dotnet test \
  ProjectManagement.Api.IntegrationTests/ProjectManagement.Api.IntegrationTests.csproj \
  --filter "FullyQualifiedName~MailboxFlowTests"
```

---

## Authentication

Protected endpoints use bearer authentication:

```http
Authorization: Bearer YOUR_ACCESS_TOKEN
```

System roles:

- `Admin`
- `ProjectManager`
- `TeamMember`

---

## Main API Areas

```text
/api/Auth
/api/Users
/api/Projects
/api/Tasks
/api/Dashboard
/api/Mailbox
```

Mailbox endpoints include:

```text
POST   /api/Mailbox/messages
GET    /api/Mailbox/inbox
GET    /api/Mailbox/sent
GET    /api/Mailbox/messages/{messageId}
PATCH  /api/Mailbox/messages/{messageId}/read
PATCH  /api/Mailbox/messages/{messageId}/unread
DELETE /api/Mailbox/messages/{messageId}
GET    /api/Mailbox/messages/{messageId}/attachments/{attachmentId}/download
```

---

## Frontend / Mobile Clients

The backend can be consumed independently by web and mobile clients.

Typical local web API address:

```text
http://localhost:8080
```

Android Emulator can reach the host machine with:

```text
http://10.0.2.2:8080
```

For a physical device on the same local network, use the host machine's LAN IP address.

---

## Security Notes

- Keep `.env` outside version control
- Use a long, random JWT secret
- Use HTTPS in production
- Do not commit SQLite database files
- Do not commit backup files
- Do not commit uploaded mailbox files
- Rotate any credential that has ever been exposed publicly

---

## Repository Status

The backend is actively developed and includes comprehensive integration test coverage for core authentication, project membership, task workflows and mailbox flows.

---

## License

No open-source license has been assigned yet.
