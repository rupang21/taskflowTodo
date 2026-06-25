/**
 * ToDoApp API Client
 *
 * Centralized API service for communicating with the .NET backend.
 * All endpoints are proxied through Vite dev server: /api/* → localhost:5090/api/*
 */

const API_BASE_URL = '/api/todos';

/**
 * Generic fetch wrapper with error handling.
 * Parses JSON responses and throws descriptive errors on failure.
 */
async function request(url, options = {}) {
  const token = localStorage.getItem('token');
  const headers = {
    'Content-Type': 'application/json',
    ...(token && { Authorization: `Bearer ${token}` }),
    ...options.headers,
  };

  const response = await fetch(url, {
    ...options,
    headers,
  });

  if (response.status === 401) {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.reload();
    throw new Error('Unauthorized');
  }

  if (!response.ok) {
    let errorMessage = `Request failed: ${response.status}`;
    try {
      const errorBody = await response.json();
      errorMessage = errorBody.message || errorBody.title || errorMessage;
    } catch {
      // Response body is not JSON, use default message
    }
    throw new Error(errorMessage);
  }

  // 204 No Content (e.g., DELETE)
  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export const todoApi = {
  /**
   * Retrieve all todos with optional filters.
   * @param {Object} filters - { status, priority, category, search }
   * @returns {Promise<Array>} List of TodoResponseDto
   */
  async getAllTodos(filters = {}) {
    const params = new URLSearchParams();

    if (filters.status !== undefined && filters.status !== null && filters.status !== '')
      params.append('status', filters.status);
    if (filters.priority !== undefined && filters.priority !== null && filters.priority !== '')
      params.append('priority', filters.priority);
    if (filters.category)
      params.append('category', filters.category);
    if (filters.search)
      params.append('search', filters.search);

    const queryString = params.toString();
    const url = queryString ? `${API_BASE_URL}?${queryString}` : API_BASE_URL;

    return request(url);
  },

  /**
   * Retrieve a single todo by ID.
   * @param {number} id
   * @returns {Promise<Object>} TodoResponseDto
   */
  async getTodoById(id) {
    return request(`${API_BASE_URL}/${id}`);
  },

  /**
   * Create a new todo.
   * @param {Object} todoData - { title, description?, priority?, category?, dueDate? }
   * @returns {Promise<Object>} Created TodoResponseDto
   */
  async createTodo(todoData) {
    return request(API_BASE_URL, {
      method: 'POST',
      body: JSON.stringify(todoData),
    });
  },

  /**
   * Full update of an existing todo.
   * @param {number} id
   * @param {Object} todoData - { title, description, status, isCompleted, priority, category, dueDate }
   * @returns {Promise<Object>} Updated TodoResponseDto
   */
  async updateTodo(id, todoData) {
    return request(`${API_BASE_URL}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(todoData),
    });
  },

  /**
   * Delete a todo permanently.
   * @param {number} id
   * @returns {Promise<null>}
   */
  async deleteTodo(id) {
    return request(`${API_BASE_URL}/${id}`, {
      method: 'DELETE',
    });
  },

  /**
   * Toggle the completion status of a todo.
   * @param {number} id
   * @returns {Promise<Object>} Updated TodoResponseDto
   */
  async toggleTodo(id) {
    return request(`${API_BASE_URL}/${id}/toggle`, {
      method: 'PATCH',
    });
  },

  /**
   * Get dashboard summary statistics.
   * @returns {Promise<Object>} TodoSummaryDto
   */
  async getSummary() {
    return request(`${API_BASE_URL}/summary`);
  },
};

export const authApi = {
  async googleLogin(credential) {
    return request('/api/auth/google', {
      method: 'POST',
      body: JSON.stringify({ credential }),
    });
  }
};
