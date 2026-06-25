import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import App from './App';

// Mock the GoogleLogin component to avoid real OAuth flow during tests
vi.mock('@react-oauth/google', () => ({
  GoogleLogin: () => <div data-testid="google-login-mock">Google Login Mock</div>
}));

describe('App Component', () => {
  it('renders login view when not authenticated', () => {
    // Clear localStorage to ensure user is not authenticated
    localStorage.clear();
    
    render(<App />);
    
    // Check if the login view elements are present
    expect(screen.getByText('Welcome to ToDo')).toBeInTheDocument();
    expect(screen.getByText('Please sign in to manage your tasks')).toBeInTheDocument();
    expect(screen.getByTestId('google-login-mock')).toBeInTheDocument();
  });
});
