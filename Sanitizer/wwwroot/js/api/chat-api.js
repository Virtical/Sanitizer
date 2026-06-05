const CHAT_API_BASE = 'http://localhost:5127/api/chat';
const AUTH_TOKEN = 'C6F0DC04-333C-4969-A2D2-E06DDB389604';

/**
 * Общая вспомогательная функция для стримингового POST-запроса.
 * @param {string} url - URL запроса
 * @param {string} message - текст сообщения
 * @param {function|null} onChunk - колбэк (chunk, fullText), вызывается на каждый чанк
 * @returns {Promise<{chatId: string|null, fullText: string}>}
 */
async function apiSendStream(url, message, onChunk) {
    const resp = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'X-Auth-Token': AUTH_TOKEN },
        body: JSON.stringify({ message })
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка отправки сообщения: ${resp.status} ${errorText}`);
    }

    const chatId = resp.headers.get('X-Chat-Id');
    const reader = resp.body.getReader();
    const decoder = new TextDecoder('utf-8');
    let fullText = '';

    while (true) {
        const { value, done } = await reader.read();
        if (done) break;
        const chunk = decoder.decode(value, { stream: true });
        if (chunk) {
            fullText += chunk;
            if (onChunk) onChunk(chunk, fullText);
        }
    }

    return { chatId, fullText };
}

/**
 * Отправить сообщение в существующий чат (стриминг).
 * @param {string} chatId
 * @param {string} message
 * @param {function|null} onChunk
 * @returns {Promise<{chatId: string|null, fullText: string}>}
 */
async function apiSendMessage(chatId, message, onChunk) {
    return apiSendStream(`${CHAT_API_BASE}/send/${chatId}`, message, onChunk);
}

async function apiGetMessages(chatId) {
    const resp = await fetch(`${CHAT_API_BASE}/${chatId}`, {
        method: 'GET',
        headers: { 'X-Auth-Token': AUTH_TOKEN }
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка получения истории чата: ${resp.status} ${errorText}`);
    }

    return resp.json();
}

async function apiGetDialog() {
    const resp = await fetch(`${CHAT_API_BASE}`, {
        method: 'GET',
        headers: { 'X-Auth-Token': AUTH_TOKEN }
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка получения диалогов: ${resp.status} ${errorText}`);
    }

    return resp.json();
}

async function apiUpdateDialogProfile(dialogId, profileId) {
    const resp = await fetch(`${CHAT_API_BASE}/${dialogId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', 'X-Auth-Token': AUTH_TOKEN },
        body: JSON.stringify({
            profileId: profileId
        })
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка привязки профиля: ${resp.status} ${errorText}`);
    }

    return resp.json();
}

async function apiSanitizeMessage(chatId, message) {
    const resp = await fetch(`${CHAT_API_BASE}/sanitized/${chatId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Auth-Token': AUTH_TOKEN
        },
        body: JSON.stringify({ message })
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка санитизации сообщения: ${resp.status} ${errorText}`);
    }

    const sanitizedText = await resp.text();
    return { sanitizedMessage: sanitizedText };
}

async function apiDeleteDialog(dialogId) {
    const resp = await fetch(`${CHAT_API_BASE}/${dialogId}`, {
        method: 'DELETE',
        headers: {
            'X-Auth-Token': AUTH_TOKEN
        }
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка удаления диалога: ${resp.status} ${errorText}`);
    }

    const result = await resp.text();
    return { deletedId: result };
}