// ==================== СОСТОЯНИЕ ПРИЛОЖЕНИЯ ====================
let currentDialogId = null;
let isDialogsHidden = false;
let dialogs = [];
let messages = {};

let allProfiles = [
    { id: 1, name: 'Профиль общения 1' },
    { id: 2, name: 'Профиль общения 2' },
    { id: 3, name: 'Профиль общения 3' },
    { id: 4, name: 'Профиль общения 4' }
];
let currentProfile = allProfiles[0];
let isProfileCreationVisible = false;
let nextProfileId = 5;

let selectedDataTypes = [];
let selectedMethod = null;

let isDialogsCollapsed = false;
