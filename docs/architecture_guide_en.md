# 📘 TaskFlow Project Technical Specification & Architecture Guide

This document is a comprehensive guide detailing every technical aspect of the TaskFlow To-Do application, including the frontend, backend, database design, cloud deployment pipeline, and the upcoming security architecture.

---

## 1. 🏗️ Tech Stack & Overall Structure

TaskFlow is designed with a modern Full-Stack architecture to maximize performance, maintainability, and ease of deployment.

| Area | Technology/Tool | Role & Rationale |
|---|---|---|
| **Frontend** | React 19, Vite 8 | Fast, reactive UI rendering with an ultra-fast local build environment. |
| **Styling** | Vanilla CSS | A custom design system built using pure CSS variables without relying on heavy external libraries like Tailwind. |
| **Backend** | .NET 9.0 (ASP.NET Core API) | Provides strong type safety, high-performance multi-threading, and a secure REST API. |
| **ORM** | Entity Framework Core (EF Core) | Safely translates object-oriented C# code into database queries. |
| **Database** | SQLite | A file-based database that requires no server installation, optimized for small-to-medium personal apps. |
| **Container** | Docker | Packages the frontend and backend into a single container environment, ensuring it runs identically anywhere. |
| **Cloud** | Fly.io | A cloud platform that runs the Docker container and makes it accessible globally. |
| **CI/CD** | GitHub Actions | A bot that automatically updates the server without manual intervention whenever code is pushed. |

---

## 2. 🎨 Frontend Architecture (React & Vite)

The frontend focuses on a seamless User Experience (UX) and intuitive code separation.

### 2.1. Separation of Concerns (API Service Layer)
The code that renders the UI (`App.jsx`) is strictly separated from the code that communicates with the server (`todoApi.js`).
* **`todoApi.js`**: Uses the `fetch` API to talk to the backend, standardizes error handling, and passes clean data to the UI.
* This allows UI components to focus solely on "how to display data beautifully" rather than "how to fetch data."

### 2.2. State Management & Optimistic Updates
By leveraging React's state management, the screen updates smoothly and instantly without full page reloads when adding or toggling tasks.
When a mutation (Create/Update/Delete) occurs, an API request is sent in the background. Upon success, `fetchTodos()` is called immediately to re-sync the UI with the server data.

### 2.3. Pure CSS (Vanilla CSS) Design System
Instead of using heavy UI frameworks, we defined **design tokens (CSS Variables)** in `index.css`.
* Variables like `--bg-primary`, `--text-primary`, and `--accent-color` enforce a consistent Dark Mode design system across the entire application.

---

## 3. ⚙️ Backend Architecture (ASP.NET Core Web API)

The backend acts as a strict gatekeeper, protecting the system from malicious requests and safely processing data.

### 3.1. DTO (Data Transfer Object) Pattern
We never directly expose the actual database models (`TodoItem.cs`) to the browser.
Instead, we use **DTOs (`CreateTodoDto`, `TodoResponseDto`, etc.)** as dedicated "delivery boxes."
* **Benefits**: It prevents users from tampering with protected fields (like `CreatedAt`) and ensures only the necessary data is transmitted, optimizing payload size.

### 3.2. Data Annotations & Validation
The server rigorously checks user inputs.
* Example: "Title is required and cannot exceed 100 characters", "DueDate format is invalid."
* If validation fails, the backend immediately returns a `400 Bad Request` without ever touching the database.

### 3.3. Global Exception Handling
If an unexpected error occurs internally, a global middleware intercepts it. It wraps the error in a clean, standardized JSON response so the server doesn't crash and the internal stack trace is never exposed to hackers.

---

## 4. 🗄️ Database Design (SQLite & EF Core)

### 4.1. Why SQLite?
While large commercial services use heavy databases like PostgreSQL or MySQL, this app uses SQLite. Everything is stored in a single file (`todos.db`), making it overwhelmingly superior for maintenance and deployment for a personal/small-scale app.

### 4.2. Index Optimization
To ensure fast searching, we optimized the database at the design phase.
* **Indexes** are applied to heavily queried columns (Status, Priority, DueDate) so the database doesn't have to scan every single row when sorting or filtering tasks, maximizing query speed.

---

## 5. 📦 Docker Architecture (3-Stage Build)

Our `Dockerfile` is elegantly structured into **3 Stages**.

1. **Stage 1 (Frontend Build)**: Uses a Node.js environment to compile the React code into highly compressed static HTML/JS/CSS files.
2. **Stage 2 (Backend Build)**: Uses the .NET SDK to compile the C# backend code into high-performance executables (Publish).
3. **Stage 3 (Runtime Assembly)**: We discard all the heavy build tools from the previous stages! We use a minimal runtime environment (Alpine Linux), and copy **only the compressed frontend files and backend executables into one lightweight container.**
   - The frontend files are placed inside the backend's `wwwroot` folder, allowing a single .NET server to serve both the React app and the API simultaneously.

---

## 6. 🌐 Deployment & Pipeline (Fly.io & GitHub Actions)

### 6.1. Flawless Data Persistence (Fly.io Volume Mount)
Docker containers are ephemeral—they are destroyed and recreated during updates. If the `todos.db` file remained inside the container, all task data would be lost on every deploy.
* **Solution**: We provisioned a persistent virtual hard drive (**Volume**) on Fly.io named `taskflow_data`.
* We instructed `fly.toml` to always mount this external drive to the container's `/data` directory. Thus, the database safely survives unlimited server updates.

### 6.2. 100% Automated CI/CD Pipeline
You never have to type manual deployment commands again. The `.github/workflows/fly.yml` file automates everything:
1. The developer pushes code to the `main` branch.
2. GitHub Actions detects the change, spins up a temporary Linux runner, and downloads the latest code.
3. It uses the `FLY_API_TOKEN` (hidden safely in GitHub Secrets) to authenticate with Fly.io.
4. The remote Fly.io builder creates the new Docker container and replaces the old server with zero downtime.

---

## 7. 🔒 Security & Scalability Architecture (Roadmap)

Currently, anyone who knows the URL (`taskflow-todo.fly.dev`) can access and manipulate data. We will implement a **3-Layer Security System** to prevent this.

### Layer 1: Google OAuth (Social Login)
When users open the app, they will be greeted with a Google Login prompt. This acts as the first line of defense, as bots and crawlers cannot easily bypass Google's authentication.

### Layer 2: Email Allowlist (Authorization System)
Logging in via Google is not enough. We will create an `AllowedEmails` table in the database. 
The backend will verify if the authenticated Google email matches a pre-approved email in this table. Unauthorized users will be rejected with an "Access Denied" screen, ensuring the app remains completely private.

### Layer 3: API Rate Limiting (DDoS & Spam Protection)
To prevent malicious scripts from spamming the server (e.g., trying to create 10,000 tasks per second), we will implement ASP.NET Core's powerful **Rate Limiter middleware**.
* Example rule: "If the same IP or User makes more than 10 requests per second, block them for 1 minute (HTTP 429 Too Many Requests)."
