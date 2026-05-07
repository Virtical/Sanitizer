const PROFILES_API_BASE = 'http://localhost:5000/api/profiles'; // указать актуальный порт Sanitizer.Api

async function apiGetAllProfiles() {
    const resp = await fetch(PROFILES_API_BASE);
    if (!resp.ok) throw new Error('Ошибка загрузки профилей');
    return resp.json();
}

async function apiCreateProfile(profile) {
    const resp = await fetch(PROFILES_API_BASE, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(profile)
    });
    if (!resp.ok) throw new Error('Ошибка создания профиля');
    return resp.json();
}

async function apiUpdateProfile(id, profile) {
    const resp = await fetch(`${PROFILES_API_BASE}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(profile)
    });
    if (!resp.ok) throw new Error('Ошибка обновления профиля');
    return resp.json();
}

async function apiDeleteProfile(id) {
    const resp = await fetch(`${PROFILES_API_BASE}/${id}`, { method: 'DELETE' });
    return resp.ok;
}
