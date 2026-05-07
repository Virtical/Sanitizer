// ==================== УПРАВЛЕНИЕ ПРОФИЛЯМИ ====================
function updateProfileDropdowns() {
    updateProfileDropdown(document.getElementById('profileDropdown'), document.getElementById('profileChatBtn'));
    updateProfileDropdown(document.getElementById('profileDropdownCreation'), document.getElementById('profileChatBtnCreation'));
}

function updateProfileDropdown(dropdownElement, buttonElement) {
    if (!dropdownElement) return;
    const dropdownContent = dropdownElement;
    dropdownContent.innerHTML = '';

    const selectedDiv = document.createElement('div');
    selectedDiv.className = 'profile-selected-item';
    const selectedName = document.createElement('span');
    selectedName.className = 'profile-selected-name';
    selectedName.textContent = truncateText(currentProfile.name, 25);
    selectedName.title = currentProfile.name;
    selectedDiv.appendChild(selectedName);
    const editIcon = document.createElement('div');
    editIcon.className = 'profile-selected-edit';
    editIcon.innerHTML = '<img src="images/editing.svg" alt="Edit">';
    editIcon.addEventListener('click', (e) => {
        e.stopPropagation();
        console.log('Редактировать профиль:', currentProfile.name);
    });
    selectedDiv.appendChild(editIcon);
    selectedDiv.addEventListener('click', () => {
        dropdownElement.classList.remove('show');
    });
    dropdownContent.appendChild(selectedDiv);

    const otherProfiles = allProfiles.filter(p => p.id !== currentProfile.id);
    if (otherProfiles.length > 0) {
        const profilesList = document.createElement('div');
        profilesList.className = 'profiles-list';
        otherProfiles.forEach(profile => {
            const option = document.createElement('div');
            option.className = 'profile-option-item';
            option.textContent = truncateText(profile.name, 30);
            option.title = profile.name;
            option.addEventListener('click', () => {
                currentProfile = profile;
                dropdownElement.classList.remove('show');
                if (buttonElement) {
                    const profileChatText = buttonElement.querySelector('.profile-chat-text');
                    if (profileChatText) profileChatText.textContent = truncateText(currentProfile.name, 20);
                }
                updateProfileDropdowns();
            });
            profilesList.appendChild(option);
        });
        if (otherProfiles.length >= 8) {
            profilesList.classList.add('has-scroll');
        } else {
            profilesList.classList.remove('has-scroll');
        }
        dropdownContent.appendChild(profilesList);
        if (otherProfiles.length >= 8) {
            setTimeout(() => createProfilesScrollbar(), 10);
        }
    }

    const divider = document.createElement('div');
    divider.className = 'profile-divider-line';
    dropdownContent.appendChild(divider);

    const createBtn = document.createElement('button');
    createBtn.className = 'create-new-profile-btn';
    createBtn.innerHTML = `
        <div class="create-icon-box">
            <span class="create-icon-plus">+</span>
        </div>
        <span class="create-btn-text">Создать новый профиль</span>
    `;
    createBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        dropdownElement.classList.remove('show');
        openProfileCreation();
    });
    dropdownContent.appendChild(createBtn);
}

function toggleProfileDropdown(dropdownElement) {
    if (!dropdownElement) return;
    if (dropdownElement.classList.contains('show')) {
        dropdownElement.classList.remove('show');
    } else {
        dropdownElement.classList.add('show');
    }
}

function openProfileCreation() {
    isProfileCreationVisible = true;
    const chatPanel = document.getElementById('chatPanel');
    const profileCreationPanel = document.getElementById('profileCreationPanel');
    if (chatPanel) chatPanel.style.display = 'none';
    if (profileCreationPanel) profileCreationPanel.style.display = 'flex';
}

function closeProfileCreation() {
    showChatPanel();
    const newProfileName = document.getElementById('newProfileName');
    if (newProfileName) newProfileName.value = '';
}

function showChatPanel() {
    isProfileCreationVisible = false;
    const chatPanel = document.getElementById('chatPanel');
    const profileCreationPanel = document.getElementById('profileCreationPanel');
    if (chatPanel) chatPanel.style.display = 'flex';
    if (profileCreationPanel) profileCreationPanel.style.display = 'none';
}

function saveNewProfile() {
    const newProfileNameInput = document.getElementById('newProfileName');
    const newProfileNameValue = newProfileNameInput ? newProfileNameInput.value.trim() : '';
    let profileName = newProfileNameValue !== '' ? newProfileNameValue : 'Профиль общения';

    const newProfile = {
        id: getNextProfileId(),
        name: profileName,
        dataTypes: [...selectedDataTypes],
        method: selectedMethod ? selectedMethod.value : null
    };

    allProfiles.push(newProfile);
    updateProfileDropdowns();

    const profileChatText = document.querySelector('.profile-chat-text');
    if (profileChatText) profileChatText.textContent = truncateText(currentProfile.name, 20);

    selectedDataTypes = [];
    selectedMethod = null;

    const dataMethodContainer = document.getElementById('dataMethodContainer');
    if (dataMethodContainer) dataMethodContainer.style.display = 'none';
    const addDataTypeBtn = document.getElementById('addDataTypeBtn');
    if (addDataTypeBtn) addDataTypeBtn.style.background = '#037C4E';

    document.querySelectorAll('.data-type-option').forEach(opt => opt.classList.remove('selected'));
    document.querySelectorAll('.method-option').forEach(opt => opt.classList.remove('selected'));

    const dataTypeText = document.querySelector('.data-type-text');
    if (dataTypeText) dataTypeText.textContent = 'Выберите тип данных';
    const methodText = document.querySelector('.method-text');
    if (methodText) methodText.textContent = 'Выберите метод санитизации';

    showChatPanel();
    if (newProfileNameInput) newProfileNameInput.value = '';
}

function updateProfileButtonText() {
    const profileChatText = document.querySelector('.profile-chat-text');
    if (profileChatText) {
        profileChatText.textContent = truncateText(currentProfile.name, 20);
        profileChatText.title = currentProfile.name;
    }
}
