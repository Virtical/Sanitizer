const CHAT_API_BASE = 'http://localhost:5127/api/chat';

async function apiSendMessage(profileId, message) {
    const resp = await fetch(`${CHAT_API_BASE}/send`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            profileId: profileId,
            message: message
        })
    });

    if (!resp.ok) {
        const errorText = await resp.text();
        throw new Error(`Ошибка отправки сообщения: ${resp.status} ${errorText}`);
    }

    return resp.json();
}