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
    const loginError = document.getElementById('loginError');
    const passwordError = document.getElementById('passwordError');
    loginInput.classList.remove('error');
    passwordInput.classList.remove('error');
    loginError.classList.remove('show');
    passwordError.classList.remove('show');
}

// Функция показа ошибки для логина
function showLoginError(message) {
    const loginInput = document.getElementById('loginInput');
    const loginError = document.getElementById('loginError');
    loginInput.classList.add('error');
    loginError.textContent = message;
    loginError.classList.add('show');
}

// Функция показа ошибки для пароля
function showPasswordError(message) {
    const passwordInput = document.getElementById('passwordInput');
    const passwordError = document.getElementById('passwordError');
    passwordInput.classList.add('error');
    passwordError.textContent = message;
    passwordError.classList.add('show');
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
    document.getElementById('loginError').classList.remove('show');
    checkInputs();
});
passwordInput.addEventListener('input', () => {
    passwordInput.classList.remove('error');
    document.getElementById('passwordError').classList.remove('show');
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
            const result = await apiLogin(login, password);
            if (result && result.token) {
                sessionStorage.setItem('authToken', result.token);
                sessionStorage.setItem('userLogin', result.login);
                window.location.reload();
            } else {
                console.error('Токен не получен:', result);
            }
        } catch (error) {
            console.error('Ошибка:', error);
            if (error.message === "Неверный логин") {
                showLoginError(error.message);
            } else {
                showPasswordError(error.message);
            }
        } finally {
            loginBtn.disabled = false;
            checkInputs();
        }
    }
});

// Проверяем авторизацию при загрузке страницы
checkAuth();
checkInputs();