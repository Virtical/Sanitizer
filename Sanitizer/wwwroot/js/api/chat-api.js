const CHAT_API_BASE = 'http://localhost:5127/api/chat';

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
        headers: { 'Content-Type': 'application/json' },
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

/**
 * Создать новый диалог и отправить первое сообщение (стриминг).
 * @param {string} message
 * @param {function|null} onChunk
 * @returns {Promise<{chatId: string|null, fullText: string}>}
 */
async function apiCreateDialog(message, onChunk) {
    return apiSendStream(`${CHAT_API_BASE}/send`, message, onChunk);
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

async function apiGetDialog() {
    const resp = await fetch(`${CHAT_API_BASE}`, {
        method: 'GET'
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
        headers: { 'Content-Type': 'application/json' },
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
