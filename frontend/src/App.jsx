import { useState, useEffect, useCallback } from 'react';
import { todoApi } from './services/todoApi';
import './App.css';

/* ════════════════════════════════════════════════
   TaskFlow — Main Application Component
   Supports two modes:
   - Full: CRUD, filters, search, dashboard stats
   - Simple: Checkbox + text only, minimal UI
   ════════════════════════════════════════════════ */

// ── Constants ──
const PRIORITY_OPTIONS = [
  { value: 0, label: 'Low' },
  { value: 1, label: 'Medium' },
  { value: 2, label: 'High' },
];

const STATUS_OPTIONS = [
  { value: 0, label: 'Pending' },
  { value: 1, label: 'InProgress' },
  { value: 2, label: 'Completed' },
  { value: 3, label: 'Cancelled' },
];

const EMPTY_FORM = {
  title: '',
  description: '',
  priority: 1,
  category: '',
  dueDate: '',
};

// ── Helper: format date for display ──
function formatDate(dateStr) {
  if (!dateStr) return null;
  const d = new Date(dateStr);
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

// ── Helper: format date for input[type=date] ──
function toDateInput(dateStr) {
  if (!dateStr) return '';
  return new Date(dateStr).toISOString().split('T')[0];
}

// ── Helper: is overdue? ──
function isOverdue(todo) {
  if (!todo.dueDate || todo.status === 'Completed' || todo.status === 'Cancelled') return false;
  return new Date(todo.dueDate) < new Date();
}

/* ════════════════════════════════
   Stats Bar Component
   ════════════════════════════════ */
function StatsBar({ summary }) {
  if (!summary) return null;
  return (
    <div className="stats-bar">
      <div className="stat-badge">
        <span>Total</span>
        <span className="stat-count">{summary.totalTasks}</span>
      </div>
      <div className="stat-badge pending">
        <span>Pending</span>
        <span className="stat-count">{summary.pending}</span>
      </div>
      <div className="stat-badge progress">
        <span>In Progress</span>
        <span className="stat-count">{summary.inProgress}</span>
      </div>
      <div className="stat-badge completed">
        <span>Done</span>
        <span className="stat-count">{summary.completed}</span>
      </div>
      {summary.overdue > 0 && (
        <div className="stat-badge overdue">
          <span>Overdue</span>
          <span className="stat-count">{summary.overdue}</span>
        </div>
      )}
    </div>
  );
}

/* ════════════════════════════════
   Create / Edit Form Component
   ════════════════════════════════ */
function TodoForm({ onSubmit, initialData, isEdit, onCancel }) {
  const [form, setForm] = useState(initialData || EMPTY_FORM);

  useEffect(() => {
    if (initialData) setForm(initialData);
  }, [initialData]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(prev => ({ ...prev, [name]: name === 'priority' || name === 'status' ? Number(value) : value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.title.trim()) return;
    onSubmit({
      ...form,
      title: form.title.trim(),
      description: form.description?.trim() || null,
      category: form.category?.trim() || null,
      dueDate: form.dueDate || null,
    });
    if (!isEdit) setForm(EMPTY_FORM);
  };

  return (
    <form className="create-form" onSubmit={handleSubmit}>
      <div className="form-row">
        <div className="form-group full-width">
          <label htmlFor="todo-title">Title *</label>
          <input
            id="todo-title"
            name="title"
            type="text"
            placeholder="What needs to be done?"
            value={form.title}
            onChange={handleChange}
            required
            maxLength={200}
            autoComplete="off"
          />
        </div>
      </div>
      <div className="form-row">
        <div className="form-group full-width">
          <label htmlFor="todo-description">Description</label>
          <textarea
            id="todo-description"
            name="description"
            placeholder="Optional details..."
            value={form.description}
            onChange={handleChange}
            maxLength={1000}
            rows={2}
          />
        </div>
      </div>
      <div className="form-row">
        <div className="form-group">
          <label htmlFor="todo-priority">Priority</label>
          <select id="todo-priority" name="priority" value={form.priority} onChange={handleChange}>
            {PRIORITY_OPTIONS.map(o => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        </div>
        <div className="form-group">
          <label htmlFor="todo-category">Category</label>
          <input
            id="todo-category"
            name="category"
            type="text"
            placeholder="e.g., Work, Personal"
            value={form.category}
            onChange={handleChange}
            maxLength={100}
          />
        </div>
        <div className="form-group">
          <label htmlFor="todo-duedate">Due Date</label>
          <input
            id="todo-duedate"
            name="dueDate"
            type="date"
            value={form.dueDate}
            onChange={handleChange}
          />
        </div>
      </div>
      {isEdit && (
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="todo-status">Status</label>
            <select id="todo-status" name="status" value={form.status} onChange={handleChange}>
              {STATUS_OPTIONS.map(o => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
          </div>
        </div>
      )}
      <div className="form-actions">
        {isEdit && onCancel && (
          <button type="button" className="btn btn-secondary" onClick={onCancel}>Cancel</button>
        )}
        <button type="submit" className="btn btn-primary">
          {isEdit ? '✓ Save Changes' : '+ Add Task'}
        </button>
      </div>
    </form>
  );
}

/* ════════════════════════════════
   Filter Bar Component
   ════════════════════════════════ */
function FilterBar({ filters, onFilterChange, onSearchChange }) {
  return (
    <div className="filter-bar">
      <div className="filter-group">
        <button
          className={`filter-btn ${filters.status === '' ? 'active' : ''}`}
          onClick={() => onFilterChange('status', '')}
        >All</button>
        <button
          className={`filter-btn ${filters.status === '0' ? 'active' : ''}`}
          onClick={() => onFilterChange('status', '0')}
        >Pending</button>
        <button
          className={`filter-btn ${filters.status === '1' ? 'active' : ''}`}
          onClick={() => onFilterChange('status', '1')}
        >In Progress</button>
        <button
          className={`filter-btn ${filters.status === '2' ? 'active' : ''}`}
          onClick={() => onFilterChange('status', '2')}
        >Done</button>
      </div>
      <div className="filter-group">
        <button
          className={`filter-btn ${filters.priority === '' ? 'active' : ''}`}
          onClick={() => onFilterChange('priority', '')}
        >Any Priority</button>
        <button
          className={`filter-btn ${filters.priority === '2' ? 'active' : ''}`}
          onClick={() => onFilterChange('priority', '2')}
        >🔴 High</button>
        <button
          className={`filter-btn ${filters.priority === '1' ? 'active' : ''}`}
          onClick={() => onFilterChange('priority', '1')}
        >🟡 Med</button>
        <button
          className={`filter-btn ${filters.priority === '0' ? 'active' : ''}`}
          onClick={() => onFilterChange('priority', '0')}
        >🟢 Low</button>
      </div>
      <input
        className="search-input"
        type="text"
        placeholder="🔍 Search tasks..."
        value={filters.search}
        onChange={(e) => onSearchChange(e.target.value)}
      />
    </div>
  );
}

/* ════════════════════════════════
   Todo Card Component
   ════════════════════════════════ */
function TodoCard({ todo, onToggle, onEdit, onDelete }) {
  const priorityClass = `priority-${todo.priorityLabel?.toLowerCase() || 'medium'}`;
  const statusClass = `status-${todo.statusLabel?.toLowerCase() || 'pending'}`;
  const completed = todo.status === 'Completed' || todo.status === 2;
  const overdue = isOverdue(todo);

  return (
    <div className={`todo-card ${completed ? 'completed' : ''}`}>
      <button
        className="todo-checkbox"
        onClick={() => onToggle(todo.id)}
        title={completed ? 'Mark as pending' : 'Mark as completed'}
        aria-label={completed ? 'Mark as pending' : 'Mark as completed'}
      >
        {completed ? '✓' : ''}
      </button>

      <div className="todo-content">
        <div className="todo-title">{todo.title}</div>
        {todo.description && (
          <div className="todo-description">{todo.description}</div>
        )}
        <div className="todo-meta">
          <span className={`meta-tag ${priorityClass}`}>
            {todo.priorityLabel || 'Medium'}
          </span>
          <span className={`meta-tag status ${statusClass}`}>
            {todo.statusLabel || 'Pending'}
          </span>
          {todo.category && (
            <span className="meta-tag category">{todo.category}</span>
          )}
          {todo.dueDate && (
            <span className={`meta-tag ${overdue ? 'overdue' : 'due-date'}`}>
              {overdue ? '⚠ ' : '📅 '}{formatDate(todo.dueDate)}
            </span>
          )}
        </div>
      </div>

      <div className="todo-actions">
        <button className="btn-icon" onClick={() => onEdit(todo)} title="Edit" aria-label="Edit task">
          ✏️
        </button>
        <button className="btn-icon" onClick={() => onDelete(todo)} title="Delete" aria-label="Delete task">
          🗑️
        </button>
      </div>
    </div>
  );
}

/* ════════════════════════════════
   Confirm Delete Modal
   ════════════════════════════════ */
function ConfirmModal({ todo, onConfirm, onCancel }) {
  return (
    <div className="modal-overlay" onClick={onCancel}>
      <div className="modal" onClick={e => e.stopPropagation()}>
        <h3>Delete Task</h3>
        <p>Are you sure you want to permanently delete <strong>"{todo.title}"</strong>?</p>
        <div className="modal-actions">
          <button className="btn btn-secondary" onClick={onCancel}>Cancel</button>
          <button className="btn btn-danger" onClick={onConfirm}>Delete</button>
        </div>
      </div>
    </div>
  );
}

/* ════════════════════════════════
   Edit Modal
   ════════════════════════════════ */
function EditModal({ todo, onSave, onCancel }) {
  const editData = {
    title: todo.title,
    description: todo.description || '',
    priority: typeof todo.priority === 'number' ? todo.priority : PRIORITY_OPTIONS.findIndex(o => o.label === todo.priority),
    category: todo.category || '',
    dueDate: toDateInput(todo.dueDate),
    status: typeof todo.status === 'number' ? todo.status : STATUS_OPTIONS.findIndex(o => o.label === todo.status),
  };

  return (
    <div className="modal-overlay edit-modal" onClick={onCancel}>
      <div className="modal" onClick={e => e.stopPropagation()}>
        <h3>Edit Task</h3>
        <TodoForm
          initialData={editData}
          isEdit
          onSubmit={(data) => onSave(todo.id, data)}
          onCancel={onCancel}
        />
      </div>
    </div>
  );
}

/* ════════════════════════════════
   Simple View Component
   Minimal UI: text input + checkboxes + delete
   ════════════════════════════════ */
function SimpleView({ todos, loading, error, onAdd, onToggle, onDelete, onClearError }) {
  const [input, setInput] = useState('');
  const remaining = todos.filter(t => t.status !== 'Completed' && t.status !== 2).length;

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!input.trim()) return;
    onAdd({ title: input.trim() });
    setInput('');
  };

  return (
    <div className="simple-view">
      <form className="simple-input-row" onSubmit={handleSubmit}>
        <input
          id="simple-input"
          className="simple-input"
          type="text"
          placeholder="What needs to be done?"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          autoComplete="off"
          maxLength={200}
        />
        <button type="submit" className="btn btn-primary btn-sm" disabled={!input.trim()}>Add</button>
      </form>

      {error && (
        <div className="error-banner" role="alert">
          <span>⚠️</span>
          <span>{error}</span>
          <button className="btn-icon" onClick={onClearError} style={{ marginLeft: 'auto' }} aria-label="Dismiss error">✕</button>
        </div>
      )}

      {loading ? (
        <div className="loading">
          <div className="loading-dot"></div>
          <div className="loading-dot"></div>
          <div className="loading-dot"></div>
        </div>
      ) : todos.length === 0 ? (
        <div className="simple-empty">No tasks yet. Add one above!</div>
      ) : (
        <ul className="simple-list">
          {todos.map(todo => {
            const done = todo.status === 'Completed' || todo.status === 2;
            return (
              <li key={todo.id} className={`simple-item ${done ? 'done' : ''}`}>
                <button
                  className={`simple-checkbox ${done ? 'checked' : ''}`}
                  onClick={() => onToggle(todo.id)}
                  aria-label={done ? 'Mark as pending' : 'Mark as done'}
                >
                  {done ? '✓' : ''}
                </button>
                <span className="simple-text">{todo.title}</span>
                <button
                  className="simple-delete"
                  onClick={() => onDelete(todo.id)}
                  aria-label="Delete task"
                >✕</button>
              </li>
            );
          })}
        </ul>
      )}

      {todos.length > 0 && (
        <div className="simple-footer">
          {remaining} task{remaining !== 1 ? 's' : ''} left
        </div>
      )}
    </div>
  );
}

/* ════════════════════════════════
   Main App Component
   ════════════════════════════════ */
function App() {
  const [viewMode, setViewMode] = useState(() => localStorage.getItem('taskflow-mode') || 'full');
  const [todos, setTodos] = useState([]);
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({ status: '', priority: '', search: '' });
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [editTarget, setEditTarget] = useState(null);

  // ── Mode toggle ──
  const toggleMode = () => {
    const next = viewMode === 'full' ? 'simple' : 'full';
    setViewMode(next);
    localStorage.setItem('taskflow-mode', next);
  };

  // ── Fetch data ──
  const fetchData = useCallback(async () => {
    try {
      setError(null);
      const filterParams = {};
      if (filters.status !== '') filterParams.status = filters.status;
      if (filters.priority !== '') filterParams.priority = filters.priority;
      if (filters.search) filterParams.search = filters.search;

      const [todosData, summaryData] = await Promise.all([
        todoApi.getAllTodos(filterParams),
        todoApi.getSummary(),
      ]);
      setTodos(todosData);
      setSummary(summaryData);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // ── Debounced search ──
  const [searchTimeout, setSearchTimeout] = useState(null);
  const handleSearchChange = (value) => {
    if (searchTimeout) clearTimeout(searchTimeout);
    const timeout = setTimeout(() => {
      setFilters(prev => ({ ...prev, search: value }));
    }, 300);
    setSearchTimeout(timeout);
    // Update the input immediately for UX
    setFilters(prev => ({ ...prev, search: value }));
  };

  // ── Filter change ──
  const handleFilterChange = (key, value) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  };

  // ── Create ──
  const handleCreate = async (data) => {
    try {
      setError(null);
      await todoApi.createTodo(data);
      await fetchData();
    } catch (err) {
      setError(err.message);
    }
  };

  // ── Toggle ──
  const handleToggle = async (id) => {
    try {
      setError(null);
      await todoApi.toggleTodo(id);
      await fetchData();
    } catch (err) {
      setError(err.message);
    }
  };

  // ── Edit ──
  const handleEdit = async (id, data) => {
    try {
      setError(null);
      await todoApi.updateTodo(id, data);
      setEditTarget(null);
      await fetchData();
    } catch (err) {
      setError(err.message);
    }
  };

  // ── Delete ──
  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      setError(null);
      await todoApi.deleteTodo(deleteTarget.id);
      setDeleteTarget(null);
      await fetchData();
    } catch (err) {
      setError(err.message);
    }
  };

  // ── Simple mode: direct delete (no confirmation) ──
  const handleSimpleDelete = async (id) => {
    try {
      setError(null);
      await todoApi.deleteTodo(id);
      await fetchData();
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <>
      {/* Header */}
      <header className="app-header">
        <div className="header-row">
          <h1>TaskFlow</h1>
          <button
            className="mode-toggle"
            onClick={toggleMode}
            title={`Switch to ${viewMode === 'full' ? 'Simple' : 'Full'} mode`}
            aria-label={`Switch to ${viewMode === 'full' ? 'Simple' : 'Full'} mode`}
          >
            {viewMode === 'full' ? '◉ Full' : '○ Simple'}
          </button>
        </div>
        <p>{viewMode === 'full' ? 'Organize your work, one task at a time' : 'Keep it simple'}</p>
        {viewMode === 'full' && <StatsBar summary={summary} />}
      </header>

      {viewMode === 'simple' ? (
        <SimpleView
          todos={todos}
          loading={loading}
          error={error}
          onAdd={handleCreate}
          onToggle={handleToggle}
          onDelete={handleSimpleDelete}
          onClearError={() => setError(null)}
        />
      ) : (
        <>
          {/* Create Form */}
          <section className="create-section">
            <TodoForm onSubmit={handleCreate} />
          </section>

          {/* Error Banner */}
          {error && (
            <div className="error-banner" role="alert">
              <span>⚠️</span>
              <span>{error}</span>
              <button
                className="btn-icon"
                onClick={() => setError(null)}
                style={{ marginLeft: 'auto' }}
                aria-label="Dismiss error"
              >✕</button>
            </div>
          )}

          {/* Filter Bar */}
          <FilterBar
            filters={filters}
            onFilterChange={handleFilterChange}
            onSearchChange={handleSearchChange}
          />

          {/* Todo List */}
          {loading ? (
            <div className="loading">
              <div className="loading-dot"></div>
              <div className="loading-dot"></div>
              <div className="loading-dot"></div>
            </div>
          ) : todos.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">📋</div>
              <h3>No tasks found</h3>
              <p>
                {filters.status || filters.priority || filters.search
                  ? 'Try adjusting your filters or search query.'
                  : 'Create your first task above to get started!'}
              </p>
            </div>
          ) : (
            <div className="todo-list">
              {todos.map(todo => (
                <TodoCard
                  key={todo.id}
                  todo={todo}
                  onToggle={handleToggle}
                  onEdit={setEditTarget}
                  onDelete={setDeleteTarget}
                />
              ))}
            </div>
          )}

          {/* Delete Confirmation Modal */}
          {deleteTarget && (
            <ConfirmModal
              todo={deleteTarget}
              onConfirm={handleDelete}
              onCancel={() => setDeleteTarget(null)}
            />
          )}

          {/* Edit Modal */}
          {editTarget && (
            <EditModal
              todo={editTarget}
              onSave={handleEdit}
              onCancel={() => setEditTarget(null)}
            />
          )}
        </>
      )}
    </>
  );
}

export default App;
