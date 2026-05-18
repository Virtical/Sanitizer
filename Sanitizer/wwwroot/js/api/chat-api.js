const CHAT_API_BASE = 'http://localhost:5127/api/chat';

async function apiSendMessage(chatId, message) {
    const resp = await fetch(`${CHAT_API_BASE}/send/${chatId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            message: message
        })
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка отправки сообщения: ${resp.status} ${errorText}`);
    }

    return resp.json();
}

async function apiCreateDialog(message) {
    const resp = await fetch(`${CHAT_API_BASE}/send`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            message: message
        })
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка создания чата и отправки сообщения: ${resp.status} ${errorText}`);
    }

    return resp.json();
}

async function apiGetMessages(chatId) {
    const resp = await fetch(`${CHAT_API_BASE}/${chatId}`, {
        method: 'GET'
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка получения истории чата: ${resp.status} ${errorText}`);
    }

    return resp.json();
}