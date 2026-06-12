// Функция проверки авторизации при загрузке
function checkAuth() {
    const token = sessionStorage.getItem('authToken');
    if (token) {
        document.getElementById('authenticatedContent').style.display = 'flex';
        document.getElementById('loginContent').style.display = 'none';
    } else {
        document.getElementById('authenticatedContent').style.display = 'none';
        document.getElementById('loginContent').style.display = 'flex';
    }
}

// Функция очистки ошибок
function clearErrors() {
    const loginInput = document.getElementById('loginInput');
    const passwordInput = document.getElementById('passwordInput');
    const generalError = document.getElementById('generalError');
    loginInput.classList.remove('error');
    passwordInput.classList.remove('error');
    generalError.classList.remove('show');
}

// Функция показа ошибки для обоих полей
function showAuthError() {
    const loginInput = document.getElementById('loginInput');
    const passwordInput = document.getElementById('passwordInput');
    const generalError = document.getElementById('generalError');
    loginInput.classList.add('error');
    passwordInput.classList.add('error');
    generalError.classList.add('show');
}

// Получаем элементы формы
const loginInput = document.getElementById('loginInput');
const passwordInput = document.getElementById('passwordInput');
const loginBtn = document.querySelector('.login-btn');
const togglePassword = document.getElementById('togglePassword');

// Функция проверки заполненности полей
function checkInputs() {
    const loginValue = loginInput.value.trim();
    const passwordValue = passwordInput.value.trim();
    if (loginValue !== '' && passwordValue !== '') {
        loginBtn.style.background = 'var(--accent-primary)';
        loginBtn.disabled = false;
    } else {
        loginBtn.style.background = 'var(--btn-login-disabled)';
        loginBtn.disabled = true;
    }
}

// Слушаем события ввода и очищаем ошибки при вводе
loginInput.addEventListener('input', () => {
    loginInput.classList.remove('error');
    document.getElementById('generalError').classList.remove('show');
    checkInputs();
});
passwordInput.addEventListener('input', () => {
    passwordInput.classList.remove('error');
    document.getElementById('generalError').classList.remove('show');
    checkInputs();
});

// Переключение видимости пароля
togglePassword.addEventListener('click', function() {
    const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
    passwordInput.setAttribute('type', type);
    this.classList.toggle('bi-eye');
    this.classList.toggle('bi-eye-slash');
});

// Обработка отправки формы
const loginForm = document.getElementById('loginForm');
loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const login = loginInput.value.trim();
    const password = passwordInput.value.trim();
    if (login && password) {
        try {
            clearErrors();
            loginBtn.disabled = true;
            const token = await apiLogin(login, password);
            if (token) {
                sessionStorage.setItem('authToken', token);
                window.location.reload();
            } else {
                console.error('Токен не получен:', token);
            }
        } catch (error) {
            console.error('Ошибка:', error);
            showAuthError();
        } finally {
            loginBtn.disabled = false;
            checkInputs();
        }
    }
});

// Проверяем авторизацию при загрузке страницы
checkAuth();
checkInputs();