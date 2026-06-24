-- ============================================================
-- ToDoApp — DML: Seed Data
-- Database: SQLite
-- ============================================================

INSERT INTO "TodoItems" ("Id", "Title", "Description", "Status", "IsCompleted", "Priority", "Category", "DueDate", "CreatedAt", "UpdatedAt")
VALUES
(1, 'Complete initial project setup', 'Set up backend .NET 9 + frontend React/Vite project structure', 2, 1, 2, 'Development', NULL, '2026-06-21 00:00:00', '2026-06-21 00:00:00'),
(2, 'Implement CRUD API controller', 'Build Create, Read, Update, Delete, and Toggle endpoints in TodosController', 1, 0, 2, 'Development', NULL, '2026-06-21 00:00:00', '2026-06-21 00:00:00'),
(3, 'Design frontend UI', 'Build todo list and input form UI using React components', 0, 0, 1, 'Design', '2026-07-01 00:00:00', '2026-06-21 00:00:00', '2026-06-21 00:00:00'),
(4, 'Write unit tests', 'Create xUnit tests for API endpoints and business logic', 0, 0, 0, 'Testing', '2026-07-15 00:00:00', '2026-06-21 00:00:00', '2026-06-21 00:00:00');
