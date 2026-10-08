// Shared fetch wrapper: attaches the JWT, redirects to login on 401 (for logged-in
// requests), and normalizes error handling across all pages.

const Api = (() => {
  const TOKEN_KEY = 'chapp_token';
  const USER_KEY = 'chapp_user';

  function getToken() {
    return localStorage.getItem(TOKEN_KEY);
  }

  function getUser() {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }

  function setSession(token, user) {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  function clearSession() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  function requireAuth() {
    if (!getToken()) {
      window.location.href = '/index.html';
    }
  }

  // Escape user-supplied text before putting it into innerHTML
  function esc(value) {
    return String(value ?? '').replace(/[&<>"']/g, c => ({
      '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[c]));
  }

  async function request(method, path, body, isFormData = false) {
    const headers = {};
    const token = getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;
    if (!isFormData && body !== undefined) headers['Content-Type'] = 'application/json';

    const res = await fetch(path, {
      method,
      headers,
      body: body === undefined ? undefined : (isFormData ? body : JSON.stringify(body))
    });

    // Only treat 401 as "session expired" when we actually sent a token;
    // otherwise (e.g. a failed login) let the real error message through.
    if (res.status === 401 && token) {
      clearSession();
      window.location.href = '/index.html';
      throw new Error('Session expired. Please log in again.');
    }

    if (res.status === 204) return null;

    const contentType = res.headers.get('content-type') || '';
    const data = contentType.includes('json') ? await res.json() : null;

    if (!res.ok) {
      const message = (data && (data.message || (data.errors && data.errors.join(', ')) || data.title))
        || (res.status === 403 ? 'You do not have permission to do that.' : 'Something went wrong.');
      throw new Error(message);
    }

    return data;
  }

  return {
    get: (path) => request('GET', path),
    post: (path, body) => request('POST', path, body),
    put: (path, body) => request('PUT', path, body),
    postForm: (path, formData) => request('POST', path, formData, true),
    getToken, getUser, setSession, clearSession, requireAuth, esc
  };
})();
