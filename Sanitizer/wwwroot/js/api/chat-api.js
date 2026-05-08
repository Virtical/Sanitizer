const CHAT_API_BASE = 'http://localhost:5127/api/chat';

async function apiSendMessage(chatId, message) {
    const resp = await fetch(`${CHAT_API_BASE}/send`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            chatId: chatId,
            message: message
        })
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка отправки сообщения: ${resp.status} ${errorText}`);
    }

    return resp.json();
}