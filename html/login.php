<?php
// Устанавливаем время жизни cookie сессии 10 часов
ini_set('session.cookie_lifetime', 36000);
// Устанавливаем время жизни данных на сервере тоже 10 часов
ini_set('session.gc_maxlifetime', 36000);
// Убеждаемся, что сборщик мусора запускается с вероятностью 1% (значения по умолчанию)
ini_set('session.gc_probability', 1);
ini_set('session.gc_divisor', 100);

session_start();
require_once 'config/database.php';

$error = '';
$success = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $login = trim($_POST['login']);
    $password = $_POST['password'];
    
    // Ищем пользователя
    $stmt = $pdo->prepare("SELECT * FROM users WHERE login = ?");
    $stmt->execute([$login]);
    $user = $stmt->fetch();
    
    if ($user && password_verify($password, $user['password_hash'])) {
        $_SESSION['user_id'] = $user['id'];
        $_SESSION['user_name'] = $user['full_name'];
        $_SESSION['permission_group'] = $user['permission_group'];
        $_SESSION['last_activity'] = time();
        
        // Перенаправляем на выбор подразделения
        header('Location: select_division.php');
        exit();
    } else {
        $error = 'Неверный логин или пароль';
    }
}
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Вход - RunaRMK</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">
    <style>
        :root {
            --primary: #4361ee;
            --secondary: #2ecc71;
            --danger: #e74c3c;
            --warning: #f39c12;
            --info: #3498db;
            --light: #f8f9fa;
            --dark: #2c3e50;
            --gray: #95a5a6;
            --light-blue: #e9f2ff;
            --border-radius: 16px;
            --shadow-sm: 0 4px 12px rgba(0,0,0,0.05);
            --shadow-md: 0 8px 24px rgba(0,0,0,0.1);
            --shadow-lg: 0 12px 32px rgba(0,0,0,0.15);
        }
        
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        }
        
        body {
            background: linear-gradient(135deg, #f5f7fa 0%, #e4e8f0 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
            position: relative;
            overflow: hidden;
        }
        
        body::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 100%;
            background: url("data:image/svg+xml,%3Csvg width='100' height='100' viewBox='0 0 100 100' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M11 18c3.866 0 7-3.134 7-7s-3.134-7-7-7-7 3.134-7 7 3.134 7 7 7zm48 25c3.866 0 7-3.134 7-7s-3.134-7-7-7-7 3.134-7 7 3.134 7 7 7zm-43-7c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zm63 31c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zM34 90c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zm56-76c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zM12 86c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm28-65c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm23-11c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zm-6 60c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm29 22c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zM32 63c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zm57-13c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zm-9-21c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zM60 91c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zM35 41c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zM12 60c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2z' fill='%234361ee' fill-opacity='0.03' fill-rule='evenodd'/%3E%3C/svg%3E");
        }
        
        .login-wrapper {
            width: 100%;
            max-width: 480px;
            position: relative;
            z-index: 2;
            animation: slideUp 0.6s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        @keyframes slideUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        .login-container {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            padding: 50px;
            box-shadow: var(--shadow-lg);
            border: 1px solid rgba(0,0,0,0.05);
            position: relative;
            overflow: hidden;
        }
        
        .login-container::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--primary), var(--info));
        }
        
        .login-header {
            text-align: center;
            margin-bottom: 40px;
        }
        
        .login-logo {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 15px;
            margin-bottom: 20px;
        }
        
        .login-logo-icon {
            width: 60px;
            height: 60px;
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            border-radius: 15px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 28px;
            box-shadow: 0 8px 20px rgba(67, 97, 238, 0.3);
        }
        
        .login-logo-text {
            font-size: 28px;
            font-weight: 800;
            color: var(--dark);
            background: linear-gradient(135deg, var(--primary), var(--info));
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }
        
        .login-title {
            font-size: 22px;
            color: #64748b;
            font-weight: 500;
            margin-bottom: 5px;
        }
        
        .login-subtitle {
            font-size: 14px;
            color: #95a5a6;
            font-weight: 400;
        }
        
        .error-message {
            background: linear-gradient(135deg, rgba(231, 76, 60, 0.1) 0%, rgba(231, 76, 60, 0.05) 100%);
            border: 2px solid rgba(231, 76, 60, 0.2);
            border-radius: 12px;
            padding: 15px;
            margin-bottom: 25px;
            display: flex;
            align-items: center;
            gap: 12px;
            color: var(--danger);
            font-weight: 500;
            animation: shake 0.5s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            25% { transform: translateX(-10px); }
            75% { transform: translateX(10px); }
        }
        
        .error-message i {
            font-size: 18px;
        }
        
        .form-group {
            margin-bottom: 25px;
        }
        
        .form-label {
            display: block;
            margin-bottom: 10px;
            font-weight: 600;
            color: var(--dark);
            font-size: 14px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .input-group {
            position: relative;
        }
        
        .input-icon {
            position: absolute;
            left: 18px;
            top: 50%;
            transform: translateY(-50%);
            color: #95a5a6;
            font-size: 18px;
            z-index: 2;
        }
        
        .form-input {
            width: 100%;
            padding: 18px 18px 18px 50px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 16px;
            font-weight: 500;
            color: var(--dark);
            background: white;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            position: relative;
            z-index: 1;
        }
        
        .form-input:focus {
            outline: none;
            border-color: var(--primary);
            box-shadow: 0 0 0 4px rgba(67, 97, 238, 0.1);
            transform: translateY(-2px);
        }
        
        .form-input::placeholder {
            color: #cbd5e0;
        }
        
        .btn-login {
            width: 100%;
            padding: 20px;
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            color: white;
            border: none;
            border-radius: 12px;
            font-size: 16px;
            font-weight: 700;
            cursor: pointer;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            letter-spacing: 0.5px;
            box-shadow: 0 6px 20px rgba(67, 97, 238, 0.3);
            margin-top: 10px;
        }
        
        .btn-login:hover {
            transform: translateY(-3px);
            box-shadow: 0 12px 25px rgba(67, 97, 238, 0.4);
        }
        
        .btn-login:active {
            transform: translateY(-1px);
        }
        
        .demo-section {
            background: linear-gradient(135deg, rgba(52, 152, 219, 0.1) 0%, rgba(52, 152, 219, 0.05) 100%);
            border: 2px solid rgba(52, 152, 219, 0.2);
            border-radius: 12px;
            padding: 25px;
            margin-top: 35px;
            text-align: center;
        }
        
        .demo-title {
            font-size: 14px;
            font-weight: 600;
            color: var(--info);
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
        }
        
        .demo-credentials {
            background: white;
            border-radius: 8px;
            padding: 15px;
            margin-bottom: 15px;
            box-shadow: var(--shadow-sm);
        }
        
        .demo-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 0;
            border-bottom: 1px solid #f1f5f9;
        }
        
        .demo-item:last-child {
            border-bottom: none;
        }
        
        .demo-label {
            color: #64748b;
            font-size: 14px;
            font-weight: 500;
        }
        
        .demo-value {
            color: var(--dark);
            font-weight: 600;
            font-family: 'Courier New', monospace;
            background: #f8fafc;
            padding: 5px 10px;
            border-radius: 6px;
            border: 1px solid #e2e8f0;
        }
        
        .demo-note {
            font-size: 12px;
            color: #94a3b8;
            font-style: italic;
            margin-top: 10px;
        }
        
        .login-footer {
            text-align: center;
            margin-top: 30px;
            color: #94a3b8;
            font-size: 13px;
        }
        
        .particles {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: 1;
        }
        
        .particle {
            position: absolute;
            background: linear-gradient(135deg, var(--primary), var(--info));
            border-radius: 50%;
            opacity: 0.05;
            animation: float 20s infinite linear;
        }
        
        @keyframes float {
            0%, 100% {
                transform: translateY(0) rotate(0deg);
            }
            50% {
                transform: translateY(-20px) rotate(180deg);
            }
        }
        
        @media (max-width: 576px) {
            body {
                padding: 10px;
            }
            
            .login-container {
                padding: 35px 25px;
            }
            
            .login-logo {
                flex-direction: column;
                gap: 10px;
            }
            
            .login-logo-text {
                font-size: 24px;
            }
            
            .form-input {
                padding: 16px 16px 16px 45px;
            }
            
            .input-icon {
                left: 15px;
            }
        }
    </style>
</head>
<body>
    <!-- Анимированные частицы -->
    <div class="particles" id="particles"></div>
    
    <div class="login-wrapper">
        <div class="login-container">
            <div class="login-header">
                <div class="login-logo">
                    <div class="login-logo-icon">
                        <i class="fas fa-cash-register"></i>
                    </div>
                    <div class="login-logo-text">
                        RunaRMK
                    </div>
                </div>
                <h2 class="login-title">Вход в систему</h2>
                <p class="login-subtitle">Для продолжения работы авторизуйтесь</p>
            </div>
            
            <?php if ($error): ?>
                <div class="error-message">
                    <i class="fas fa-exclamation-circle"></i>
                    <?php echo htmlspecialchars($error); ?>
                </div>
            <?php endif; ?>
            
            <form method="POST" action="">
                <div class="form-group">
                    <label class="form-label">Логин</label>
                    <div class="input-group">
                        <div class="input-icon">
                            <i class="fas fa-user"></i>
                        </div>
                        <input type="text" name="login" class="form-input" placeholder="Введите ваш логин" required>
                    </div>
                </div>
                
                <div class="form-group">
                    <label class="form-label">Пароль</label>
                    <div class="input-group">
                        <div class="input-icon">
                            <i class="fas fa-lock"></i>
                        </div>
                        <input type="password" name="password" class="form-input" placeholder="Введите ваш пароль" required>
                    </div>
                </div>
                
                <button type="submit" class="btn-login">
                    <i class="fas fa-sign-in-alt"></i>
                    Войти в систему
                </button>
            </form>
            
            <div class="login-footer">
                © 2026 "ООО Связь 22" 
            </div>
        </div>
    </div>
    
    <script>
        // Создание анимированных частиц
        function createParticles() {
            const particlesContainer = document.getElementById('particles');
            const particleCount = 20;
            
            for (let i = 0; i < particleCount; i++) {
                const particle = document.createElement('div');
                particle.classList.add('particle');
                
                // Случайные размеры
                const size = Math.random() * 100 + 50;
                particle.style.width = `${size}px`;
                particle.style.height = `${size}px`;
                
                // Случайная позиция
                particle.style.left = `${Math.random() * 100}%`;
                particle.style.top = `${Math.random() * 100}%`;
                
                // Случайная прозрачность
                particle.style.opacity = Math.random() * 0.05 + 0.02;
                
                // Случайная задержка анимации
                particle.style.animationDelay = `${Math.random() * 20}s`;
                particle.style.animationDuration = `${Math.random() * 30 + 20}s`;
                
                particlesContainer.appendChild(particle);
            }
        }
        
        // Анимация фокуса для полей ввода
        document.addEventListener('DOMContentLoaded', function() {
            createParticles();
            
            const inputs = document.querySelectorAll('.form-input');
            inputs.forEach(input => {
                input.addEventListener('focus', function() {
                    this.parentElement.querySelector('.input-icon').style.color = 'var(--primary)';
                });
                
                input.addEventListener('blur', function() {
                    this.parentElement.querySelector('.input-icon').style.color = '#95a5a6';
                });
            });
            
            // Автофокус на поле логина
            document.querySelector('input[name="login"]')?.focus();
        });
    </script>
</body>
</html>