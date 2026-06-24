# TaskFlow — To-Do Task Management Application

A full-stack to-do task management application built as a take-home assessment. Demonstrates clean architecture, RESTful API design, and modern React frontend development.

## Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Backend** | .NET (ASP.NET Core Web API) | 9.0 |
| **ORM** | Entity Framework Core | 9.0.2 |
| **Database** | SQLite | File-based (`todos.db`) |
| **Frontend** | React (Vite) | React 19.x, Vite 8.x |
| **Styling** | Vanilla CSS | Custom design system |
| **API Docs** | Swagger / OpenAPI | Swashbuckle 6.x |

---

## Quick Start

### Prerequisites

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) (includes npm)

### 1. Start the Backend

```bash
cd backend/ToDoApp.Api
dotnet run
```

- API runs at `http://localhost:5090`
- Swagger UI: `http://localhost:5090/swagger`
- SQLite database (`todos.db`) is auto-created on first launch with seed data

### 2. Start the Frontend

```bash
cd frontend
npm install
npm run dev
```

- Frontend runs at `http://localhost:5173`
- API calls are proxied via Vite: `/api/*` → `http://localhost:5090/api/*`

> **Note**: Both servers must be running simultaneously. Open two terminal windows.

---

## Project Structure

```
ToDoApp/
├── backend/
│   ├── ToDoApp.sln                       # Solution file
│   └── ToDoApp.Api/
│       ├── Controllers/
│       │   └── TodosController.cs        # REST API endpoints (CRUD + filters + summary)
│       ├── Data/
│       │   ├── AppDbContext.cs            # EF Core context with Fluent API configuration
│       │   └── Queries/                   # Raw SQL reference queries
│       │       ├── 01_CreateTable.sql
│       │       ├── 02_SeedData.sql
│       │       └── 03_CRUD_Queries.sql
│       ├── DTOs/
│       │   ├── CreateTodoDto.cs           # Input DTO for creation
│       │   ├── UpdateTodoDto.cs           # Input DTO for updates
│       │   └── TodoResponseDto.cs         # Output DTO + summary DTO
│       ├── Models/
│       │   └── TodoItem.cs               # Domain entity + enums
│       ├── Program.cs                     # App bootstrap, middleware, DI
│       └── appsettings.json              # Configuration (connection string)
├── frontend/
│   ├── src/
│   │   ├── App.jsx                       # Main UI (components inline)
│   │   ├── App.css                       # Component styles
│   │   ├── index.css                     # Design system & global styles
│   │   ├── main.jsx                      # React entry point
│   │   └── services/
│   │       └── todoApi.js                # API client with error handling
│   ├── index.html                        # HTML shell with SEO meta tags
│   └── vite.config.js                    # Dev server proxy configuration
├── docs/
│   └── DATABASE_DESIGN.md               # Database schema documentation
└── README.md                            # This file
```

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/todos` | List all todos (supports `?status=`, `?priority=`, `?category=`, `?search=` query filters) |
| `GET` | `/api/todos/{id}` | Get a single todo by ID |
| `POST` | `/api/todos` | Create a new todo |
| `PUT` | `/api/todos/{id}` | Full update of an existing todo |
| `PATCH` | `/api/todos/{id}/toggle` | Toggle completion status |
| `DELETE` | `/api/todos/{id}` | Permanently delete a todo |
| `GET` | `/api/todos/summary` | Get dashboard statistics (counts by status + overdue) |

### Request/Response Examples

<details>
<summary><strong>POST /api/todos</strong> — Create a task</summary>

**Request:**
```json
{
  "title": "Complete API documentation",
  "description": "Write comprehensive API docs with examples",
  "priority": "High",
  "category": "Documentation",
  "dueDate": "2026-07-01"
}
```

**Response (201 Created):**
```json
{
  "id": 5,
  "title": "Complete API documentation",
  "description": "Write comprehensive API docs with examples",
  "status": "Pending",
  "statusLabel": "Pending",
  "isCompleted": false,
  "priority": "High",
  "priorityLabel": "High",
  "category": "Documentation",
  "dueDate": "2026-07-01T00:00:00",
  "createdAt": "2026-06-21T23:00:00Z",
  "updatedAt": "2026-06-21T23:00:00Z"
}
```
</details>

<details>
<summary><strong>GET /api/todos/summary</strong> — Dashboard stats</summary>

**Response (200 OK):**
```json
{
  "totalTasks": 4,
  "pending": 2,
  "inProgress": 1,
  "completed": 1,
  "cancelled": 0,
  "overdue": 0
}
```
</details>

---

## Architecture & Design Decisions

### Backend Architecture

- **Layered separation**: Models → DTOs → Controller. Domain entities are never directly exposed to the API consumer.
- **DTO pattern**: `CreateTodoDto` (write), `UpdateTodoDto` (write), `TodoResponseDto` (read) ensure a clean API contract that can evolve independently of the domain model.
- **Fluent API over Data Annotations**: While Data Annotations handle basic validation, EF Core Fluent API provides fine-grained control over column types, indexes, default values, and seed data — all in one centralized location (`AppDbContext.OnModelCreating`).
- **Global exception handling**: A middleware catches unhandled exceptions and returns consistent JSON error responses instead of exposing stack traces.
- **Structured logging**: Request/response logging via middleware; operation-level logging in the controller.

### Frontend Architecture

- **Single-file component approach**: For an MVP of this scope, all components live in `App.jsx` to minimize boilerplate and maximize readability during code review. In a larger project, each component would be extracted to its own file.
- **Service layer**: `todoApi.js` abstracts all HTTP calls behind a clean async interface with centralized error handling.
- **Optimistic UX**: After each mutation (create/update/delete/toggle), the app re-fetches from the server to ensure data consistency.
- **Debounced search**: Search queries are debounced (300ms) to avoid flooding the API.
- **CSS design system**: Custom properties (variables) in `index.css` define the entire design language — colors, spacing, typography, animations. Component styles in `App.css` consume these tokens.

### Database Design

- **SQLite** chosen for zero-configuration setup — no database server installation required. The file (`todos.db`) is auto-created at app startup.
- **`EnsureCreated()`** over migrations for MVP simplicity. In production, this would be replaced with `Database.Migrate()` and proper migration history.
- **5 strategic indexes** cover the most common query patterns (status+priority composite, due date, category, created date, completion status).
- **Enum storage**: `TodoStatus` and `TodoPriority` are stored as integers with `HasConversion<int>()` for storage efficiency while maintaining type safety in C#.
- Full schema documentation available in [`docs/DATABASE_DESIGN.md`](docs/DATABASE_DESIGN.md).

---

## Assumptions & Trade-offs

| Decision | Rationale | Trade-off |
|----------|-----------|-----------|
| **SQLite over SQL Server** | Zero setup, portable, perfect for take-home evaluation | No concurrent write scaling; would switch to PostgreSQL/SQL Server in production |
| **`EnsureCreated()` over Migrations** | Eliminates need for `dotnet ef` tooling installation | No schema evolution support; must delete DB to change schema |
| **Inline React components** | Faster review, clear data flow, minimal file jumping | Would not scale to 20+ components; extract when team grows |
| **No authentication** | Not required for MVP scope | Every user sees all tasks; add JWT/OAuth for multi-user |
| **No pagination** | Manageable for typical todo list sizes (< 1000 items) | Would add cursor-based pagination for production datasets |
| **Server-side filtering** | Reduces data transfer, leverages database indexes | More API surface to maintain; could use OData for complex queries |
| **Vite proxy for CORS** | Simplest local development setup | Production would need proper CORS policy or reverse proxy |
| **Hard delete** | Simple and expected for todo items | Could implement soft delete (`IsDeleted` flag) for audit trail |
| **Dark theme only** | Opinionated design choice for the MVP | Would add theme toggle (light/dark) in production |

---

## Production MVP Features Included

- ✅ Full CRUD API with proper HTTP status codes (200, 201, 204, 400, 404, 500)
- ✅ Input validation with Data Annotations and model state checking
- ✅ DTO pattern separating internal models from API contracts
- ✅ Swagger/OpenAPI documentation auto-generated
- ✅ Global exception handling with safe error responses
- ✅ Request logging middleware
- ✅ Server-side filtering and search
- ✅ Dashboard summary statistics endpoint
- ✅ Responsive dark-theme UI with micro-animations
- ✅ Task prioritization (Low/Medium/High) and status workflow (Pending → InProgress → Completed)
- ✅ Category-based organization
- ✅ Due date tracking with overdue detection
- ✅ Delete confirmation dialog
- ✅ Inline editing via modal
- ✅ Debounced search
- ✅ CORS configuration
- ✅ Database indexes for query performance
- ✅ Seed data for immediate testing

---

## Deployment (Fly.io & GitHub Actions)

This project is configured for automated CI/CD deployment to [Fly.io](https://fly.io):

- **Dockerized**: A 3-stage `Dockerfile` builds both the React frontend and .NET backend into a single lightweight alpine container.
- **Persistent SQLite**: A Fly.io volume (`taskflow_data`) is mounted to `/data` ensuring `todos.db` survives container restarts and deployments.
- **CI/CD Pipeline**: GitHub Actions (`.github/workflows/fly.yml`) automatically builds and deploys to Fly.io whenever code is pushed to the `main` branch.

---

## Future Improvements (TODO)

### 🔒 Security & Access Control (Planned Architecture)
To restrict access and prevent bot abuse, the following security measures are planned:
- [ ] **Google OAuth Integration**: Users must authenticate via Google to access the app.
- [ ] **Email Allowlist (Whitelist)**: A database table of allowed emails. Only pre-approved Google accounts can log in.
- [ ] **API Rate Limiting**: ASP.NET Core rate limiting middleware to prevent brute-force attacks and bot abuse.

### Other Improvements
- [ ] **Pagination & Sorting** — Cursor-based pagination with configurable sort fields for large datasets.
- [ ] **Unit & Integration Tests** — xUnit tests for controller logic and React Testing Library for components.
- [ ] **Soft Delete** — `IsDeleted` + `DeletedAt` fields instead of hard delete.
- [ ] **Real-time Updates** — SignalR WebSocket integration for live task updates across browser tabs.

---

## License

This project was created as a take-home assessment and is provided as-is for evaluation purposes.
