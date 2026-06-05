// ==================== СОСТОЯНИЕ ПРИЛОЖЕНИЯ ====================
let currentDialogId = null;
let isDialogsHidden = false;
let dialogs = [];
let sanitizedMessages = [];

let allProfiles = [];
let currentProfile = null
let isProfileCreationVisible = false;

let isDialogsCollapsed = false;

let originalMessageText = '';
let isSanitizeMode = false;

let isMessageSent = false;

async function initProfiles() {
    try {
        allProfiles = await apiGetAllProfiles();
    } catch {
        allProfiles = [];
    }
    currentProfile = allProfiles.length > 0 ? allProfiles[0] : null;
    updateProfileDropdowns();
    updateProfileButtonText();
}
