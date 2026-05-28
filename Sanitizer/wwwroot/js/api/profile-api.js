const PROFILES_API_BASE = 'http://localhost:5127/api/profiles';

async function apiGetAllProfiles() {
    const resp = await fetch(PROFILES_API_BASE, {
        headers: {
            'X-Auth-Token': AUTH_TOKEN
        }
    });
    if (!resp.ok) throw new Error('Ошибка загрузки профилей');
    return resp.json();
}

async function apiCreateProfile(profile) {
    const resp = await fetch(PROFILES_API_BASE, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Auth-Token': AUTH_TOKEN
        },
        body: JSON.stringify(profile)
    });
    if (!resp.ok) throw new Error('Ошибка создания профиля');
    return resp.json();
}

async function apiUpdateProfile(profileId, profile) {
    const resp = await fetch(`${PROFILES_API_BASE}/${profileId}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'X-Auth-Token': AUTH_TOKEN
        },
        body: JSON.stringify(profile)
    });
    if (!resp.ok) throw new Error('Ошибка обновления профиля');
    return resp.json();
}