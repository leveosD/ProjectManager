# Sibers Project & Task Management System

Test task implementation for the C# / ASP.NET Developer position at Sibers.

## Technology Stack

- **Backend**: .NET 8 Web API, Entity Framework Core Code First, SQLite
- **Authentication**: ASP.NET Core Identity + JWT Bearer tokens
- **Frontend**: React 19 SPA with Vite, React Router DOM
- **Testing**: xUnit, Moq

## Architecture

```text
Sibers/
├── Sibers.Core            # Domain entities, DTOs, interfaces, core business services
├── Sibers.Infrastructure  # EF Core DbContext, repositories, file storage, JWT auth service
├── Sibers                 # ASP.NET Core Web API host (controllers, Program.cs)
├── Sibers.Tests           # Unit tests for core business logic
└── Sibers.Client          # React SPA frontend
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (version 18 or higher)
- A modern web browser

## Database

The application uses SQLite. The database file `sibers.db` is created automatically on first run in the `Sibers` folder.

To re-create the database from migrations, run from the solution root:

```bash
dotnet ef database update --project Sibers.Infrastructure/Sibers.Infrastructure.csproj --startup-project Sibers/Sibers.csproj
```

## Running the Application

### 1. Run the Backend API

From the solution root directory:

```bash
dotnet run --project Sibers/Sibers.csproj
```

By default, the API runs on `https://localhost:7183` (check the console output for the exact URL).

### 2. Run the Frontend

In a separate terminal:

```bash
cd Sibers.Client
npm install
npm run dev
```

The React app runs by default on `http://localhost:5173`. Configure the API URL in `Sibers.Client/src/api.js` or via the `VITE_API_URL` environment variable.

### 3. Default Seed Users

The application seeds the following demo users on startup:

| Email                 | Password     | Role             |
|-----------------------|--------------|------------------|
| director@sibers.com   | Password123! | Director         |
| pm@sibers.com         | Password123! | ProjectManager   |
| employee1@sibers.com  | Password123! | Employee         |
| employee2@sibers.com  | Password123! | Employee         |

## API Documentation

Once the API is running, Swagger UI is available at `https://localhost:7183/swagger` in Development mode.

## Main Features

### Projects
- Create / view / edit / delete projects
- Filter by date range, priority range, search term
- Sort by name, start date, end date, priority

### Employees
- Create / view / edit / delete employees
- AJAX live employee search for project manager and executor selection

### Project Wizard
- 5-step project creation wizard:
  1. Basic info (name, dates, priority)
  2. Customer and executing companies
  3. Project manager selection (AJAX search)
  4. Executors selection (AJAX multi-select search)
  5. Document upload with HTML5 drag & drop

### Tasks
- Create / view / edit / delete tasks per project
- Status workflow: ToDo → InProgress → Done
- Assign executor from project team members
- Filter by project, status; sort by title, priority, status

### Documents
- Upload and download project documents
- Drag & drop file upload in the project wizard

### Roles & Access Control
- **Director**: full access to all features
- **Project Manager**: manage own projects, team members, tasks; cannot add new employees
- **Employee**: view assigned projects and tasks; update task status

## Testing

Run the unit tests with:

```bash
dotnet test Sibers.Tests/Sibers.Tests.csproj
```

## Build Production Frontend

```bash
cd Sibers.Client
npm run build
```

The production build is output to `Sibers.Client/dist`.
