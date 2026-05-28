const DIALOGS_API_BASE = 'http://localhost:5127/api/chat';

async function apiGetDialog() {
    const resp = await fetch(`${DIALOGS_API_BASE}`, {
        method: 'GET',
        headers: {
            'X-Auth-Token': AUTH_TOKEN
        }
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка получения диалогов: ${resp.status} ${errorText}`);
    }

    return await resp.json();
}

async function apiCreateDialog(name) {
    const resp = await fetch(`${DIALOGS_API_BASE}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Auth-Token': AUTH_TOKEN
        },
        body: JSON.stringify({
            name: name
        })
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка создания диалога: ${resp.status} ${errorText}`);
    }

    return await resp.text();
}