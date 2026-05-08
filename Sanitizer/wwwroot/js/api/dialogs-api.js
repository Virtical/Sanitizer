const DIALOGS_API_BASE = 'http://localhost:5127/api/chat';

async function apiSaveDialog(dialogName) {
    const resp = await fetch(`${DIALOGS_API_BASE}/save`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dialogName)
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка сохранения диалога: ${resp.status} ${errorText}`);
    }

    return await resp.json();
}

async function apiGetDialog() {
    const resp = await fetch(`${DIALOGS_API_BASE}`, {
        method: 'GET',
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка получения диалогов: ${resp.status} ${errorText}`);
    }
    
    return await resp.json();
}