const AUTH_API_BASE = '/api/login';

async function apiLogin(login, password) {
    const response = await fetch(AUTH_API_BASE, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            login: login,
            password: password
        })
    });

    const responseText = await response.text();

    if (response.ok) {
        return { token: responseText.trim(), login: login };
    } else {
        throw new Error("Неверный логин или пароль");
    }
}