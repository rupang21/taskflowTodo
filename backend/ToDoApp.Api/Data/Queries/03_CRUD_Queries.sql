-- ============================================================
-- ToDoApp — CRUD Queries
-- Database: SQLite
-- Common query patterns used by the application
-- ============================================================


-- ╔══════════════════════════════════════╗
-- ║          READ OPERATIONS             ║
-- ╚══════════════════════════════════════╝

-- 1. Get all todos (ordered by creation date, newest first)
SELECT *
FROM "TodoItems"
ORDER BY "CreatedAt" DESC;

-- 2. Get a single todo by ID
SELECT *
FROM "TodoItems"
WHERE "Id" = @Id;

-- 3. Get all pending (incomplete) todos
SELECT *
FROM "TodoItems"
WHERE "Status" = 0  -- Pending
ORDER BY "Priority" DESC, "DueDate" ASC;

-- 4. Get all completed todos
SELECT *
FROM "TodoItems"
WHERE "Status" = 2  -- Completed
ORDER BY "UpdatedAt" DESC;

-- 5. Get todos by category
SELECT *
FROM "TodoItems"
WHERE "Category" = @Category
ORDER BY "Priority" DESC, "CreatedAt" DESC;

-- 6. Get overdue todos (past due date and not completed)
SELECT *
FROM "TodoItems"
WHERE "DueDate" < datetime('now')
  AND "Status" NOT IN (2, 3)  -- Not Completed or Cancelled
ORDER BY "DueDate" ASC;

-- 7. Get todos by priority (e.g., all High priority)
SELECT *
FROM "TodoItems"
WHERE "Priority" = @Priority  -- 0=Low, 1=Medium, 2=High
ORDER BY "DueDate" ASC, "CreatedAt" DESC;

-- 8. Get todos due within the next N days
SELECT *
FROM "TodoItems"
WHERE "DueDate" BETWEEN datetime('now') AND datetime('now', '+7 days')
  AND "Status" NOT IN (2, 3)
ORDER BY "DueDate" ASC;

-- 9. Search todos by title (partial match)
SELECT *
FROM "TodoItems"
WHERE "Title" LIKE '%' || @SearchTerm || '%'
ORDER BY "CreatedAt" DESC;

-- 10. Get todo count grouped by status
SELECT
    "Status",
    COUNT(*) AS "Count"
FROM "TodoItems"
GROUP BY "Status";

-- 11. Get todo count grouped by category
SELECT
    COALESCE("Category", 'Uncategorized') AS "Category",
    COUNT(*) AS "Count"
FROM "TodoItems"
GROUP BY "Category";


-- ╔══════════════════════════════════════╗
-- ║         WRITE OPERATIONS             ║
-- ╚══════════════════════════════════════╝

-- 12. Create a new todo
INSERT INTO "TodoItems" ("Title", "Description", "Status", "IsCompleted", "Priority", "Category", "DueDate", "CreatedAt", "UpdatedAt")
VALUES (@Title, @Description, 0, 0, @Priority, @Category, @DueDate, datetime('now'), datetime('now'));

-- Return the newly created record
SELECT * FROM "TodoItems" WHERE "Id" = last_insert_rowid();


-- 13. Update a todo (full update)
UPDATE "TodoItems"
SET "Title"       = @Title,
    "Description" = @Description,
    "Status"      = @Status,
    "IsCompleted" = @IsCompleted,
    "Priority"    = @Priority,
    "Category"    = @Category,
    "DueDate"     = @DueDate,
    "UpdatedAt"   = datetime('now')
WHERE "Id" = @Id;


-- 14. Toggle completion status
UPDATE "TodoItems"
SET "IsCompleted" = CASE WHEN "IsCompleted" = 0 THEN 1 ELSE 0 END,
    "Status"      = CASE WHEN "IsCompleted" = 0 THEN 2 ELSE 0 END,  -- Completed(2) or Pending(0)
    "UpdatedAt"   = datetime('now')
WHERE "Id" = @Id;

-- Return the updated record
SELECT * FROM "TodoItems" WHERE "Id" = @Id;


-- 15. Update status only
UPDATE "TodoItems"
SET "Status"      = @Status,
    "IsCompleted" = CASE WHEN @Status = 2 THEN 1 ELSE 0 END,
    "UpdatedAt"   = datetime('now')
WHERE "Id" = @Id;


-- 16. Update priority only
UPDATE "TodoItems"
SET "Priority"  = @Priority,
    "UpdatedAt" = datetime('now')
WHERE "Id" = @Id;


-- ╔══════════════════════════════════════╗
-- ║        DELETE OPERATIONS             ║
-- ╚══════════════════════════════════════╝

-- 17. Delete a single todo by ID
DELETE FROM "TodoItems"
WHERE "Id" = @Id;

-- 18. Delete all completed todos (bulk cleanup)
DELETE FROM "TodoItems"
WHERE "Status" = 2;  -- Completed

-- 19. Delete all cancelled todos
DELETE FROM "TodoItems"
WHERE "Status" = 3;  -- Cancelled


-- ╔══════════════════════════════════════╗
-- ║     ANALYTICS / DASHBOARD QUERIES    ║
-- ╚══════════════════════════════════════╝

-- 20. Dashboard summary statistics
SELECT
    COUNT(*)                                             AS "TotalTasks",
    SUM(CASE WHEN "Status" = 0 THEN 1 ELSE 0 END)      AS "Pending",
    SUM(CASE WHEN "Status" = 1 THEN 1 ELSE 0 END)      AS "InProgress",
    SUM(CASE WHEN "Status" = 2 THEN 1 ELSE 0 END)      AS "Completed",
    SUM(CASE WHEN "Status" = 3 THEN 1 ELSE 0 END)      AS "Cancelled",
    SUM(CASE WHEN "DueDate" < datetime('now')
              AND "Status" NOT IN (2, 3) THEN 1 ELSE 0 END) AS "Overdue"
FROM "TodoItems";

-- 21. Priority distribution
SELECT
    CASE "Priority"
        WHEN 0 THEN 'Low'
        WHEN 1 THEN 'Medium'
        WHEN 2 THEN 'High'
    END AS "PriorityLabel",
    COUNT(*) AS "Count"
FROM "TodoItems"
WHERE "Status" NOT IN (2, 3)
GROUP BY "Priority"
ORDER BY "Priority" DESC;

-- 22. Completion rate by category
SELECT
    COALESCE("Category", 'Uncategorized') AS "Category",
    COUNT(*) AS "Total",
    SUM(CASE WHEN "Status" = 2 THEN 1 ELSE 0 END) AS "Completed",
    ROUND(
        CAST(SUM(CASE WHEN "Status" = 2 THEN 1 ELSE 0 END) AS REAL)
        / NULLIF(COUNT(*), 0) * 100, 1
    ) AS "CompletionRate"
FROM "TodoItems"
GROUP BY "Category"
ORDER BY "CompletionRate" DESC;
