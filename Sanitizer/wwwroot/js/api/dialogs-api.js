const DIALOGS_API_BASE = 'http://localhost:5127/api/chat';

async function apiSaveDialog(dialogName) {
    const resp = await fetch(`${DIALOGS_API_BASE}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            name: dialogName
        })
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

async function apiUpdateDialogName(chatId, newName) {
    const resp = await fetch(`${DIALOGS_API_BASE}/${chatId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            name: newName
        })
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка обновления названия диалога: ${resp.status} ${errorText}`);
    }

    return await resp.json();
}