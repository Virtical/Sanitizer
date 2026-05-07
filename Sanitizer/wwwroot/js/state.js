// ==================== СОСТОЯНИЕ ПРИЛОЖЕНИЯ ====================
let currentDialogId = null;
let isDialogsHidden = false;
let dialogs = [];
let messages = {};

let allProfiles = [];
let currentProfile = { id: null, name: 'Нет профилей' };
let isProfileCreationVisible = false;

let selectedDataTypes = [];
let selectedMethod = null;

let isDialogsCollapsed = false;

async function initProfiles() {
    try {
        allProfiles = await apiGetAllProfiles();
    } catch {
        allProfiles = [];
    }
    currentProfile = allProfiles.length > 0 ? allProfiles[0] : { id: null, name: 'Нет профилей' };
    updateProfileDropdowns();
    updateProfileButtonText();
}
