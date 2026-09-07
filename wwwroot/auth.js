/**
 * KutubxonaAPI - Auth utilities
 * Access token + Refresh token bilan avtomatik yangilash
 */

const AUTH_KEYS = {
    ACCESS: 'auth_token',           // Backward compat — access token
    REFRESH: 'auth_refresh_token',
    USER: 'auth_user'
};

function getAccessToken() {
    return localStorage.getItem(AUTH_KEYS.ACCESS);
}

function getRefreshToken() {
    return localStorage.getItem(AUTH_KEYS.REFRESH);
}

function getCurrentUser() {
    const s = localStorage.getItem(AUTH_KEYS.USER);
    return s ? JSON.parse(s) : null;
}

function saveAuth(data) {
    if (data.accessToken || data.token) {
        localStorage.setItem(AUTH_KEYS.ACCESS, data.accessToken || data.token);
    }
    if (data.refreshToken) {
        localStorage.setItem(AUTH_KEYS.REFRESH, data.refreshToken);
    }
    if (data.user) {
        localStorage.setItem(AUTH_KEYS.USER, JSON.stringify(data.user));
    }
}

function clearAuth() {
    localStorage.removeItem(AUTH_KEYS.ACCESS);
    localStorage.removeItem(AUTH_KEYS.REFRESH);
    localStorage.removeItem(AUTH_KEYS.USER);
}

/**
 * Access token'ni refresh qilish
 */
let refreshPromise = null;

async function refreshAccessToken() {
    // Concurrent so'rovlar bir vaqtda refresh urinmasin
    if (refreshPromise) return refreshPromise;

    const refresh = getRefreshToken();
    if (!refresh) {
        return null;
    }

    refreshPromise = (async () => {
        try {
            const res = await fetch('/api/auth/refresh', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ refreshToken: refresh })
            });

            if (!res.ok) {
                clearAuth();
                return null;
            }

            const data = await res.json();
            saveAuth(data);
            return data.accessToken;
        } catch (err) {
            console.error('Refresh xato:', err);
            return null;
        } finally {
            refreshPromise = null;
        }
    })();

    return refreshPromise;
}

/**
 * Fetch wrapper — 401 bo'lsa avtomatik refresh + qayta uring
 */
async function apiFetch(url, options = {}) {
    const doFetch = async (token) => {
        const headers = { ...(options.headers || {}) };
        if (token) headers['Authorization'] = `Bearer ${token}`;
        return fetch(url, { ...options, headers });
    };

    let token = getAccessToken();
    let response = await doFetch(token);

    // 401 bo'lsa refresh qilib qayta uring
    if (response.status === 401 && getRefreshToken()) {
        const newToken = await refreshAccessToken();
        if (newToken) {
            response = await doFetch(newToken);
        } else {
            // Refresh ham amal qilmaydi — logout
            clearAuth();
            // Auth kerak bo'lgan sahifada ekan — login'ga yo'naltiring
            const requiresAuth = ['/admin.html', '/my-orders.html'];
            if (requiresAuth.some(p => window.location.pathname.startsWith(p))) {
                window.location.href = '/login.html';
            }
        }
    }

    return response;
}

/**
 * Logout — refresh tokenni serverda bekor qilib, local'dan tozalaydi
 */
async function serverLogout() {
    const refresh = getRefreshToken();
    if (refresh) {
        try {
            await fetch('/api/auth/logout', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ refreshToken: refresh })
            });
        } catch (err) {
            console.warn('Logout xato:', err);
        }
    }
    clearAuth();
}
