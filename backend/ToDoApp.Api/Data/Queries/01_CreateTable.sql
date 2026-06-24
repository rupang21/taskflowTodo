-- ============================================================
-- ToDoApp — DDL: Create TodoItems Table
-- Database: SQLite
-- Generated from EF Core Fluent API configuration
-- ============================================================

CREATE TABLE IF NOT EXISTS "TodoItems" (
    "Id"          INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Title"       TEXT     NOT NULL DEFAULT '',           -- Required, max 200 chars
    "Description" TEXT     NULL,                          -- Optional, max 1000 chars
    "Status"      INTEGER  NOT NULL DEFAULT 0,            -- 0=Pending, 1=InProgress, 2=Completed, 3=Cancelled
    "IsCompleted" INTEGER  NOT NULL DEFAULT 0,            -- 0=false, 1=true (legacy convenience flag)
    "Priority"    INTEGER  NOT NULL DEFAULT 1,            -- 0=Low, 1=Medium, 2=High
    "Category"    TEXT     NULL,                          -- Optional, max 100 chars
    "DueDate"     TEXT     NULL,                          -- ISO 8601 datetime string
    "CreatedAt"   TEXT     NOT NULL DEFAULT (datetime('now')),  -- UTC timestamp
    "UpdatedAt"   TEXT     NOT NULL DEFAULT (datetime('now'))   -- UTC timestamp
);

-- ============================================================
-- Indexes
-- ============================================================

-- Composite index: status + priority (common filter/sort combination)
CREATE INDEX IF NOT EXISTS "IX_TodoItems_Status_Priority"
    ON "TodoItems" ("Status", "Priority");

-- Due date filtering and sorting
CREATE INDEX IF NOT EXISTS "IX_TodoItems_DueDate"
    ON "TodoItems" ("DueDate");

-- Category filtering
CREATE INDEX IF NOT EXISTS "IX_TodoItems_Category"
    ON "TodoItems" ("Category");

-- Creation date ordering
CREATE INDEX IF NOT EXISTS "IX_TodoItems_CreatedAt"
    ON "TodoItems" ("CreatedAt");

-- Legacy boolean filter
CREATE INDEX IF NOT EXISTS "IX_TodoItems_IsCompleted"
    ON "TodoItems" ("IsCompleted");
