<?php
require_once 'includes/auth_check.php';

// Проверяем права доступа
if ($_SESSION['permission_group'] !== 'admin') {
    header('Location: index.php');
    exit();
}

// Обработка смены пароля
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['change_password'])) {
    $current_password = $_POST['current_password'];
    $new_password = $_POST['new_password'];
    $confirm_password = $_POST['confirm_password'];
    
    // Проверки
    if (empty($current_password) || empty($new_password) || empty($confirm_password)) {
        $_SESSION['error'] = 'Заполните все поля';
    } elseif ($new_password !== $confirm_password) {
        $_SESSION['error'] = 'Новые пароли не совпадают';
    } elseif (strlen($new_password) < 6) {
        $_SESSION['error'] = 'Пароль должен быть не менее 6 символов';
    } else {
        // Получаем текущий пароль
        $stmt = $pdo->prepare("SELECT password_hash FROM users WHERE id = ?");
        $stmt->execute([$_SESSION['user_id']]);
        $user = $stmt->fetch();
        
        if ($user && password_verify($current_password, $user['password_hash'])) {
            // Меняем пароль
            $new_hash = password_hash($new_password, PASSWORD_DEFAULT);
            
            $stmt = $pdo->prepare("UPDATE users SET password_hash = ? WHERE id = ?");
            $stmt->execute([$new_hash, $_SESSION['user_id']]);
            
            // Логируем
            $stmt = $pdo->prepare("
                INSERT INTO operation_log (user_id, action, details, ip_address)
                VALUES (?, ?, ?, ?)
            ");
            $stmt->execute([
                $_SESSION['user_id'],
                'password_changed',
                'Пользователь сменил пароль',
                $_SERVER['REMOTE_ADDR']
            ]);
            
            $_SESSION['success'] = 'Пароль успешно изменен!';
        } else {
            $_SESSION['error'] = 'Текущий пароль неверен';
        }
    }
    header('Location: settings.php#security');
    exit();
}

// Обработка добавления пользователя
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['add_user'])) {
    $full_name = trim($_POST['full_name']);
    $login = trim($_POST['login']);
    $password = $_POST['password'];
    $permission_group = $_POST['permission_group'];
    
    // Проверки
    if (empty($full_name) || empty($login) || empty($password)) {
        $_SESSION['error'] = 'Заполните все обязательные поля';
    } elseif (strlen($password) < 6) {
        $_SESSION['error'] = 'Пароль должен быть не менее 6 символов';
    } else {
        // Проверяем, не существует ли уже пользователь с таким логином
        $stmt = $pdo->prepare("SELECT id FROM users WHERE login = ?");
        $stmt->execute([$login]);
        
        if ($stmt->fetch()) {
            $_SESSION['error'] = 'Пользователь с таким логином уже существует';
        } else {
            // Хэшируем пароль
            $password_hash = password_hash($password, PASSWORD_DEFAULT);
            
            // Добавляем пользователя
            $stmt = $pdo->prepare("
                INSERT INTO users (full_name, login, password_hash, permission_group, created_at)
                VALUES (?, ?, ?, ?, NOW())
            ");
            $stmt->execute([$full_name, $login, $password_hash, $permission_group]);
            
            // Логируем
            $stmt = $pdo->prepare("
                INSERT INTO operation_log (user_id, action, details, ip_address)
                VALUES (?, ?, ?, ?)
            ");
            $stmt->execute([
                $_SESSION['user_id'],
                'user_created',
                'Создан новый пользователь: ' . $login,
                $_SERVER['REMOTE_ADDR']
            ]);
            
            $_SESSION['success'] = 'Пользователь успешно добавлен!';
        }
    }
    header('Location: settings.php#users');
    exit();
}

// Обработка удаления пользователя
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['delete_user'])) {
    $user_id = $_POST['user_id'];
    
    // Нельзя удалить самого себя
    if ($user_id == $_SESSION['user_id']) {
        $_SESSION['error'] = 'Нельзя удалить свой собственный аккаунт';
        header('Location: settings.php#users');
        exit();
    }
    
    // Получаем информацию о пользователе для лога
    $stmt = $pdo->prepare("SELECT login FROM users WHERE id = ?");
    $stmt->execute([$user_id]);
    $user = $stmt->fetch();
    
    if ($user) {
        // Удаляем пользователя
        $stmt = $pdo->prepare("DELETE FROM users WHERE id = ?");
        $stmt->execute([$user_id]);
        
        // Логируем
        $stmt = $pdo->prepare("
            INSERT INTO operation_log (user_id, action, details, ip_address)
            VALUES (?, ?, ?, ?)
        ");
        $stmt->execute([
            $_SESSION['user_id'],
            'user_deleted',
            'Удален пользователь: ' . $user['login'],
            $_SERVER['REMOTE_ADDR']
        ]);
        
        $_SESSION['success'] = 'Пользователь успешно удален!';
    } else {
        $_SESSION['error'] = 'Пользователь не найден';
    }
    
    header('Location: settings.php#users');
    exit();
}

// Обработка изменения пароля пользователя
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['reset_user_password'])) {
    $user_id = $_POST['user_id'];
    $new_password = $_POST['new_password'];
    
    if (strlen($new_password) < 6) {
        $_SESSION['error'] = 'Пароль должен быть не менее 6 символов';
    } else {
        // Хэшируем новый пароль
        $password_hash = password_hash($new_password, PASSWORD_DEFAULT);
        
        // Обновляем пароль
        $stmt = $pdo->prepare("UPDATE users SET password_hash = ? WHERE id = ?");
        $stmt->execute([$password_hash, $user_id]);
        
        // Получаем информацию о пользователе для лога
        $stmt = $pdo->prepare("SELECT login FROM users WHERE id = ?");
        $stmt->execute([$user_id]);
        $user = $stmt->fetch();
        
        // Логируем
        $stmt = $pdo->prepare("
            INSERT INTO operation_log (user_id, action, details, ip_address)
            VALUES (?, ?, ?, ?)
        ");
        $stmt->execute([
            $_SESSION['user_id'],
            'user_password_reset',
            'Сброшен пароль пользователю: ' . $user['login'],
            $_SERVER['REMOTE_ADDR']
        ]);
        
        $_SESSION['success'] = 'Пароль пользователя успешно изменен!';
    }
    
    header('Location: settings.php#users');
    exit();
}

// Получаем данные пользователя
$stmt = $pdo->prepare("
    SELECT u.*, 
           (SELECT created_at FROM operation_log WHERE user_id = u.id AND action = 'login' ORDER BY created_at DESC LIMIT 1) as last_login
    FROM users u 
    WHERE u.id = ?
");
$stmt->execute([$_SESSION['user_id']]);
$user_data = $stmt->fetch();

// Получаем всех пользователей для вкладки управления
$stmt = $pdo->prepare("
    SELECT id, full_name, login, permission_group, created_at
    FROM users 
    ORDER BY created_at DESC
");
$stmt->execute();
$all_users = $stmt->fetchAll();
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Настройки - RunaRMK</title>
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
            color: #333;
        }
        
        .header {
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            padding: 25px 35px;
            box-shadow: var(--shadow-lg);
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-bottom: 4px solid rgba(255,255,255,0.15);
            margin-bottom: 35px;
        }
        
        .header h1 {
            color: white;
            font-size: 32px;
            font-weight: 800;
            display: flex;
            align-items: center;
            gap: 15px;
            text-shadow: 0 4px 8px rgba(0,0,0,0.2);
        }
        
        .header h1 i {
            background: rgba(255,255,255,0.15);
            padding: 18px;
            border-radius: 18px;
            backdrop-filter: blur(10px);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn {
            padding: 14px 28px;
            border-radius: 50px;
            font-weight: 700;
            font-size: 15px;
            cursor: pointer;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            border: none;
            letter-spacing: 0.3px;
        }
        
        .btn-primary {
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(67, 97, 238, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-primary:hover {
            transform: translateY(-3px);
            box-shadow: 0 12px 25px rgba(67, 97, 238, 0.4);
        }
        
        .btn-secondary {
            background: linear-gradient(135deg, var(--secondary) 0%, #27ae60 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(46, 204, 113, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-danger {
            background: linear-gradient(135deg, var(--danger) 0%, #c0392b 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(231, 76, 60, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-warning {
            background: linear-gradient(135deg, var(--warning) 0%, #e67e22 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(243, 156, 18, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-info {
            background: linear-gradient(135deg, var(--info) 0%, #2980b9 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(52, 152, 219, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-sm {
            padding: 10px 20px;
            font-size: 14px;
        }
        
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 30px 50px;
        }
        
        .alert {
            padding: 20px 25px;
            border-radius: var(--border-radius);
            margin-bottom: 30px;
            display: flex;
            align-items: center;
            gap: 15px;
            box-shadow: var(--shadow-sm);
            animation: slideDown 0.5s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        @keyframes slideDown {
            from {
                transform: translateY(-20px);
                opacity: 0;
            }
            to {
                transform: translateY(0);
                opacity: 1;
            }
        }
        
        .alert-success {
            background: linear-gradient(135deg, #d4edda 0%, #c3e6cb 100%);
            color: #155724;
            border: 2px solid #28a745;
        }
        
        .alert-danger {
            background: linear-gradient(135deg, #f8d7da 0%, #f5c6cb 100%);
            color: #721c24;
            border: 2px solid #dc3545;
        }
        
        .alert-info {
            background: linear-gradient(135deg, #d1ecf1 0%, #c1e2e9 100%);
            color: #0c5460;
            border: 2px solid #17a2b8;
        }
        
        .settings-grid {
            display: grid;
            grid-template-columns: 300px 1fr;
            gap: 30px;
        }
        
        .settings-sidebar {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
            height: fit-content;
            position: sticky;
            top: 20px;
        }
        
        .user-profile-card {
            text-align: center;
            margin-bottom: 30px;
            padding-bottom: 25px;
            border-bottom: 2px solid #f1f5f9;
        }
        
        .user-avatar {
            width: 100px;
            height: 100px;
            border-radius: 50%;
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            display: flex;
            align-items: center;
            justify-content: center;
            margin: 0 auto 20px;
            color: white;
            font-size: 42px;
            box-shadow: 0 8px 25px rgba(67, 97, 238, 0.3);
            border: 4px solid white;
        }
        
        .user-name {
            color: var(--dark);
            font-size: 22px;
            font-weight: 700;
            margin-bottom: 5px;
        }
        
        .user-role {
            display: inline-block;
            padding: 6px 16px;
            border-radius: 30px;
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.15) 0%, rgba(46, 204, 113, 0.05) 100%);
            color: var(--secondary);
            font-weight: 600;
            font-size: 14px;
            margin-bottom: 15px;
            border: 2px solid rgba(46, 204, 113, 0.2);
        }
        
        .user-division {
            color: #64748b;
            font-size: 14px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
        }
        
        .settings-nav {
            display: flex;
            flex-direction: column;
            gap: 8px;
        }
        
        .nav-item {
            display: flex;
            align-items: center;
            gap: 15px;
            padding: 18px 20px;
            color: var(--dark);
            text-decoration: none;
            border-radius: 12px;
            transition: all 0.3s;
            font-weight: 600;
            font-size: 15px;
            border: 2px solid transparent;
        }
        
        .nav-item:hover {
            background: #f8fafc;
            color: var(--primary);
            border-color: #e2e8f0;
            transform: translateX(5px);
        }
        
        .nav-item.active {
            background: linear-gradient(135deg, rgba(67, 97, 238, 0.1) 0%, rgba(67, 97, 238, 0.05) 100%);
            color: var(--primary);
            border-color: rgba(67, 97, 238, 0.2);
        }
        
        .nav-item i {
            font-size: 20px;
            width: 24px;
            text-align: center;
        }
        
        .settings-content {
            background: white;
            border-radius: var(--border-radius);
            padding: 35px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
            min-height: 500px;
            animation: fadeIn 0.5s ease;
        }
        
        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        .section-title {
            color: var(--dark);
            font-size: 28px;
            font-weight: 800;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .section-subtitle {
            color: #64748b;
            font-size: 15px;
            margin-bottom: 30px;
            line-height: 1.6;
        }
        
        .settings-group {
            margin-bottom: 40px;
        }
        
        .settings-group:last-child {
            margin-bottom: 0;
        }
        
        .group-title {
            color: var(--dark);
            font-size: 18px;
            font-weight: 700;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
            padding-bottom: 12px;
            border-bottom: 2px solid #f1f5f9;
        }
        
        .info-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }
        
        .info-item {
            background: #f8fafc;
            border-radius: 12px;
            padding: 25px;
            border: 2px solid #e2e8f0;
            transition: all 0.3s;
        }
        
        .info-item:hover {
            border-color: var(--primary);
            transform: translateY(-5px);
            box-shadow: var(--shadow-sm);
        }
        
        .info-label {
            color: #64748b;
            font-size: 13px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        
        .info-value {
            color: var(--dark);
            font-size: 20px;
            font-weight: 700;
            font-family: 'Roboto Mono', monospace;
        }
        
        .password-form {
            max-width: 500px;
        }
        
        .form-group {
            margin-bottom: 25px;
        }
        
        .form-label {
            color: var(--dark);
            font-weight: 600;
            font-size: 14px;
            margin-bottom: 10px;
            display: block;
        }
        
        .form-control {
            padding: 16px 20px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 15px;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.3s;
            width: 100%;
        }
        
        .form-control:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .form-row {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
        }
        
        @media (max-width: 768px) {
            .form-row {
                grid-template-columns: 1fr;
            }
        }
        
        .password-strength {
            height: 8px;
            background: #e2e8f0;
            border-radius: 4px;
            margin-top: 10px;
            overflow: hidden;
        }
        
        .strength-bar {
            height: 100%;
            width: 0;
            border-radius: 4px;
            transition: all 0.5s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        .strength-weak { 
            background: linear-gradient(90deg, var(--danger), #c0392b);
        }
        
        .strength-medium { 
            background: linear-gradient(90deg, var(--warning), #e67e22);
        }
        
        .strength-strong { 
            background: linear-gradient(90deg, var(--secondary), #27ae60);
        }
        
        .password-hints {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            margin-top: 15px;
        }
        
        .password-hint {
            display: flex;
            align-items: center;
            gap: 8px;
            font-size: 13px;
            color: #64748b;
        }
        
        .password-hint.valid {
            color: var(--secondary);
        }
        
        .notification-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
            gap: 20px;
        }
        
        .notification-card {
            background: white;
            border-radius: 12px;
            padding: 25px;
            border: 2px solid #e2e8f0;
            transition: all 0.3s;
            display: flex;
            align-items: flex-start;
            gap: 20px;
        }
        
        .notification-card:hover {
            border-color: var(--primary);
            transform: translateY(-5px);
            box-shadow: var(--shadow-sm);
        }
        
        .notification-icon {
            width: 50px;
            height: 50px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 22px;
            background: linear-gradient(135deg, rgba(67, 97, 238, 0.1) 0%, rgba(67, 97, 238, 0.05) 100%);
            color: var(--primary);
            flex-shrink: 0;
        }
        
        .notification-content {
            flex: 1;
        }
        
        .notification-title {
            color: var(--dark);
            font-size: 16px;
            font-weight: 700;
            margin-bottom: 8px;
        }
        
        .notification-description {
            color: #64748b;
            font-size: 14px;
            line-height: 1.5;
            margin-bottom: 15px;
        }
        
        .switch {
            position: relative;
            display: inline-block;
            width: 60px;
            height: 30px;
        }
        
        .switch input {
            opacity: 0;
            width: 0;
            height: 0;
        }
        
        .slider {
            position: absolute;
            cursor: pointer;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: #cbd5e1;
            transition: .4s;
            border-radius: 30px;
        }
        
        .slider:before {
            position: absolute;
            content: "";
            height: 22px;
            width: 22px;
            left: 4px;
            bottom: 4px;
            background: white;
            transition: .4s;
            border-radius: 50%;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        
        input:checked + .slider {
            background: linear-gradient(135deg, var(--secondary), #27ae60);
        }
        
        input:checked + .slider:before {
            transform: translateX(30px);
        }
        
        .danger-zone {
            background: linear-gradient(135deg, #fff5f5 0%, #ffeaea 100%);
            border-radius: var(--border-radius);
            padding: 30px;
            border: 2px solid var(--danger);
            margin-top: 40px;
        }
        
        .danger-title {
            color: var(--danger);
            font-size: 22px;
            font-weight: 700;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .danger-description {
            color: #721c24;
            margin-bottom: 25px;
            line-height: 1.6;
        }
        
        .system-form {
            max-width: 600px;
        }
        
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }
        
        .stat-card {
            background: #f8fafc;
            border-radius: 12px;
            padding: 25px;
            border: 2px solid #e2e8f0;
            text-align: center;
        }
        
        .stat-icon {
            width: 50px;
            height: 50px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 22px;
            background: linear-gradient(135deg, rgba(67, 97, 238, 0.1) 0%, rgba(67, 97, 238, 0.05) 100%);
            color: var(--primary);
            margin: 0 auto 15px;
        }
        
        .stat-value {
            font-size: 28px;
            font-weight: 800;
            color: var(--dark);
            margin-bottom: 5px;
        }
        
        .stat-label {
            color: #64748b;
            font-size: 14px;
            font-weight: 600;
        }
        
        /* Стили для таблицы пользователей */
        .users-table {
            width: 100%;
            background: white;
            border-radius: var(--border-radius);
            overflow: hidden;
            box-shadow: var(--shadow-sm);
            margin-bottom: 30px;
        }
        
        .table-header {
            display: grid;
            grid-template-columns: 50px 1fr 1fr 150px 200px 150px;
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            color: white;
            font-weight: 600;
            font-size: 14px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .table-header-cell {
            padding: 20px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        
        .table-row {
            display: grid;
            grid-template-columns: 50px 1fr 1fr 150px 200px 150px;
            border-bottom: 1px solid #e2e8f0;
            transition: all 0.3s;
        }
        
        .table-row:hover {
            background: #f8fafc;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.05);
        }
        
        .table-row:last-child {
            border-bottom: none;
        }
        
        .table-cell {
            padding: 20px;
            display: flex;
            align-items: center;
            color: var(--dark);
            font-size: 15px;
        }
        
        .user-role-badge {
            padding: 6px 12px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .role-admin {
            background: linear-gradient(135deg, rgba(231, 76, 60, 0.15) 0%, rgba(231, 76, 60, 0.05) 100%);
            color: var(--danger);
            border: 2px solid rgba(231, 76, 60, 0.2);
        }
        
        .role-user {
            background: linear-gradient(135deg, rgba(52, 152, 219, 0.15) 0%, rgba(52, 152, 219, 0.05) 100%);
            color: var(--info);
            border: 2px solid rgba(52, 152, 219, 0.2);
        }
        
        .user-actions {
            display: flex;
            gap: 8px;
        }
        
        .empty-state {
            text-align: center;
            padding: 60px 30px;
            background: white;
            border-radius: var(--border-radius);
            box-shadow: var(--shadow-sm);
            margin-top: 30px;
        }
        
        .empty-state i {
            font-size: 64px;
            color: #cbd5e1;
            margin-bottom: 25px;
        }
        
        .empty-state h3 {
            color: #64748b;
            font-size: 22px;
            font-weight: 700;
            margin-bottom: 15px;
        }
        
        .empty-state p {
            color: #94a3b8;
            font-size: 16px;
            margin-bottom: 25px;
            max-width: 500px;
            margin-left: auto;
            margin-right: auto;
            line-height: 1.6;
        }
        
        .modal-overlay {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0,0,0,0.7);
            display: none;
            justify-content: center;
            align-items: center;
            z-index: 2000;
            padding: 20px;
            backdrop-filter: blur(5px);
        }
        
        .modal-content {
            background: white;
            border-radius: var(--border-radius);
            padding: 35px;
            max-width: 500px;
            width: 100%;
            max-height: 90vh;
            overflow-y: auto;
            box-shadow: var(--shadow-lg);
            border: 1px solid rgba(0,0,0,0.1);
            animation: modalAppear 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        @keyframes modalAppear {
            from {
                opacity: 0;
                transform: translateY(-30px) scale(0.9);
            }
            to {
                opacity: 1;
                transform: translateY(0) scale(1);
            }
        }
        
        .modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 30px;
        }
        
        .modal-header h2 {
            color: var(--dark);
            font-size: 24px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .close-modal {
            background: none;
            border: none;
            font-size: 28px;
            color: #94a3b8;
            cursor: pointer;
            transition: color 0.3s;
            padding: 5px;
            line-height: 1;
        }
        
        .close-modal:hover {
            color: var(--danger);
        }
        
        @media (max-width: 1200px) {
            .table-header,
            .table-row {
                grid-template-columns: 40px 1fr 1fr 120px 180px 130px;
            }
        }
        
        @media (max-width: 992px) {
            .settings-grid {
                grid-template-columns: 1fr;
            }
            
            .settings-sidebar {
                position: static;
                margin-bottom: 30px;
            }
            
            .notification-grid {
                grid-template-columns: 1fr;
            }
            
            .table-header,
            .table-row {
                grid-template-columns: 1fr;
                display: none;
            }
            
            .mobile-user-card {
                display: block;
                background: white;
                border-radius: var(--border-radius);
                padding: 25px;
                margin-bottom: 20px;
                box-shadow: var(--shadow-sm);
                border-left: 4px solid var(--primary);
            }
            
            .mobile-user-header {
                display: flex;
                justify-content: space-between;
                align-items: flex-start;
                margin-bottom: 15px;
            }
            
            .mobile-user-info {
                display: grid;
                grid-template-columns: repeat(2, 1fr);
                gap: 15px;
                margin-bottom: 15px;
            }
            
            .mobile-info-item {
                display: flex;
                flex-direction: column;
                gap: 5px;
            }
            
            .mobile-info-label {
                color: #64748b;
                font-size: 12px;
                font-weight: 600;
                text-transform: uppercase;
            }
            
            .mobile-info-value {
                color: var(--dark);
                font-size: 16px;
                font-weight: 600;
            }
            
            .mobile-user-actions {
                display: flex;
                gap: 10px;
                margin-top: 15px;
            }
        }
        
        @media (max-width: 768px) {
            .container {
                padding: 0 20px 40px;
            }
            
            .header {
                padding: 20px 25px;
                flex-direction: column;
                gap: 20px;
                text-align: center;
            }
            
            .header h1 {
                font-size: 28px;
            }
            
            .info-grid {
                grid-template-columns: 1fr;
            }
            
            .stats-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .mobile-user-info {
                grid-template-columns: 1fr;
            }
        }
        
        @media (max-width: 576px) {
            .container {
                padding: 0 15px 30px;
            }
            
            .stats-grid {
                grid-template-columns: 1fr;
            }
            
            .notification-card {
                flex-direction: column;
                align-items: flex-start;
            }
            
            .mobile-user-actions {
                flex-wrap: wrap;
            }
        }
    </style>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&family=Roboto+Mono:wght@400;500&display=swap" rel="stylesheet">
</head>
<body>
    <!-- Шапка -->
    <div class="header">
        <h1>
            <i class="fas fa-cog"></i>
            Настройки системы
        </h1>
        <a href="index.php" class="btn btn-primary">
            <i class="fas fa-arrow-left"></i> На главную
        </a>
    </div>
    
    <!-- Основной контент -->
    <div class="container">
        <!-- Сообщения -->
        <?php if (isset($_SESSION['success'])): ?>
            <div class="alert alert-success">
                <i class="fas fa-check-circle"></i> <?php echo $_SESSION['success']; ?>
                <?php unset($_SESSION['success']); ?>
            </div>
        <?php endif; ?>
        
        <?php if (isset($_SESSION['error'])): ?>
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-circle"></i> <?php echo $_SESSION['error']; ?>
                <?php unset($_SESSION['error']); ?>
            </div>
        <?php endif; ?>
        
        <div class="settings-grid">
            <!-- Сайдбар -->
            <div class="settings-sidebar fade-in-up">
                <div class="user-profile-card">
                    <div class="user-avatar">
                        <i class="fas fa-user"></i>
                    </div>
                    <div class="user-name"><?php echo htmlspecialchars($_SESSION['user_name']); ?></div>
                    <div class="user-role">
                        <?php echo $_SESSION['permission_group'] === 'admin' ? 'Администратор' : 'Пользователь'; ?>
                    </div>
                    <div class="user-division">
                        <i class="fas fa-store"></i>
                        <?php echo htmlspecialchars($_SESSION['current_division_name'] ?? 'Не выбрано'); ?>
                    </div>
                </div>
                
                <div class="settings-nav">
                    <a href="#profile" class="nav-item active" onclick="showSection('profile')">
                        <i class="fas fa-user"></i>
                        <span>Профиль</span>
                    </a>
                    <a href="#security" class="nav-item" onclick="showSection('security')">
                        <i class="fas fa-shield-alt"></i>
                        <span>Безопасность</span>
                    </a>
                    <a href="#notifications" class="nav-item" onclick="showSection('notifications')">
                        <i class="fas fa-bell"></i>
                        <span>Уведомления</span>
                    </a>
                    <a href="#users" class="nav-item" onclick="showSection('users')">
                        <i class="fas fa-users"></i>
                        <span>Пользователи</span>
                    </a>
                    <a href="#system" class="nav-item" onclick="showSection('system')">
                        <i class="fas fa-server"></i>
                        <span>Система</span>
                    </a>
                </div>
            </div>
            
            <!-- Основной контент -->
            <div class="settings-content" id="settings-content">
                <!-- Профиль -->
                <div id="profile-section">
                    <h2 class="section-title">
                        <i class="fas fa-user"></i> Профиль пользователя
                    </h2>
                    <p class="section-subtitle">Основная информация о вашем аккаунте и активности</p>
                    
                    <div class="settings-group">
                        <div class="group-title">
                            <i class="fas fa-info-circle"></i> Основная информация
                        </div>
                        <div class="info-grid">
                            <div class="info-item">
                                <div class="info-label">
                                    <i class="fas fa-id-card"></i> ID пользователя
                                </div>
                                <div class="info-value">#<?php echo $_SESSION['user_id']; ?></div>
                            </div>
                            
                            <div class="info-item">
                                <div class="info-label">
                                    <i class="fas fa-calendar-plus"></i> Дата регистрации
                                </div>
                                <div class="info-value"><?php echo date('d.m.Y H:i', strtotime($user_data['created_at'])); ?></div>
                            </div>
                            
                            <div class="info-item">
                                <div class="info-label">
                                    <i class="fas fa-sign-in-alt"></i> Последний вход
                                </div>
                                <div class="info-value">
                                    <?php echo $user_data['last_login'] ? date('d.m.Y H:i', strtotime($user_data['last_login'])) : 'Не зарегистрирован'; ?>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <!-- Безопасность -->
                <div id="security-section" style="display: none;">
                    <h2 class="section-title">
                        <i class="fas fa-shield-alt"></i> Безопасность аккаунта
                    </h2>
                    <p class="section-subtitle">Управление паролем и безопасностью вашего аккаунта</p>
                    
                    <div class="settings-group">
                        <div class="group-title">
                            <i class="fas fa-key"></i> Смена пароля
                        </div>
                        <form method="POST" class="password-form">
                            <input type="hidden" name="change_password" value="1">
                            
                            <div class="form-group">
                                <label class="form-label">Текущий пароль</label>
                                <input type="password" name="current_password" class="form-control" required
                                       placeholder="Введите текущий пароль">
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Новый пароль</label>
                                <input type="password" name="new_password" id="newPassword" class="form-control" required
                                       placeholder="Введите новый пароль" oninput="checkPasswordStrength()">
                                <div class="password-strength">
                                    <div class="strength-bar" id="passwordStrength"></div>
                                </div>
                                <div class="password-hints">
                                    <div class="password-hint" id="hint-length">
                                        <i class="fas fa-circle"></i> Не менее 6 символов
                                    </div>
                                    <div class="password-hint" id="hint-case">
                                        <i class="fas fa-circle"></i> Разный регистр
                                    </div>
                                    <div class="password-hint" id="hint-number">
                                        <i class="fas fa-circle"></i> Цифры
                                    </div>
                                    <div class="password-hint" id="hint-special">
                                        <i class="fas fa-circle"></i> Спецсимволы
                                    </div>
                                </div>
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Подтверждение пароля</label>
                                <input type="password" name="confirm_password" class="form-control" required
                                       placeholder="Повторите новый пароль">
                            </div>
                            
                            <button type="submit" class="btn btn-success">
                                <i class="fas fa-save"></i> Изменить пароль
                            </button>
                        </form>
                    </div>
                    
                    <div class="danger-zone">
                        <div class="danger-title">
                            <i class="fas fa-exclamation-triangle"></i> Опасная зона
                        </div>
                        <div class="danger-description">
                            Выход из системы на всех устройствах. Это действие завершит все активные сессии
                            на других устройствах, включая мобильные приложения.
                        </div>
                        <button class="btn btn-danger" onclick="logoutAllDevices()">
                            <i class="fas fa-sign-out-alt"></i> Выйти на всех устройствах
                        </button>
                    </div>
                </div>
                
                <!-- Уведомления -->
                <div id="notifications-section" style="display: none;">
                    <h2 class="section-title">
                        <i class="fas fa-bell"></i> Управление уведомлениями
                    </h2>
                    <p class="section-subtitle">Настройте какие уведомления вы хотите получать</p>
                    
                    <div class="notification-grid">
                        <div class="notification-card">
                            <div class="notification-icon">
                                <i class="fas fa-receipt"></i>
                            </div>
                            <div class="notification-content">
                                <div class="notification-title">Чековые уведомления</div>
                                <div class="notification-description">
                                    Уведомлять о каждом напечатанном чеке в реальном времени
                                </div>
                                <label class="switch">
                                    <input type="checkbox" checked>
                                    <span class="slider"></span>
                                </label>
                            </div>
                        </div>
                        
                        <div class="notification-card">
                            <div class="notification-icon">
                                <i class="fas fa-cash-register"></i>
                            </div>
                            <div class="notification-content">
                                <div class="notification-title">Сменные уведомления</div>
                                <div class="notification-description">
                                    Уведомлять об открытии/закрытии смен на всех кассах
                                </div>
                                <label class="switch">
                                    <input type="checkbox" checked>
                                    <span class="slider"></span>
                                </label>
                            </div>
                        </div>
                        
                        <div class="notification-card">
                            <div class="notification-icon">
                                <i class="fas fa-box"></i>
                            </div>
                            <div class="notification-content">
                                <div class="notification-title">Остатки товаров</div>
                                <div class="notification-description">
                                    Уведомлять при низком количестве товара на складе
                                </div>
                                <label class="switch">
                                    <input type="checkbox">
                                    <span class="slider"></span>
                                </label>
                            </div>
                        </div>
                        
                        <div class="notification-card">
                            <div class="notification-icon">
                                <i class="fas fa-chart-bar"></i>
                            </div>
                            <div class="notification-content">
                                <div class="notification-title">Ежедневные отчеты</div>
                                <div class="notification-description">
                                    Отправлять отчет по итогам дня на email
                                </div>
                                <label class="switch">
                                    <input type="checkbox">
                                    <span class="slider"></span>
                                </label>
                            </div>
                        </div>
                        
                        <div class="notification-card">
                            <div class="notification-icon">
                                <i class="fas fa-exclamation-circle"></i>
                            </div>
                            <div class="notification-content">
                                <div class="notification-title">Системные ошибки</div>
                                <div class="notification-description">
                                    Уведомлять о критических ошибках и сбоях системы
                                </div>
                                <label class="switch">
                                    <input type="checkbox" checked>
                                    <span class="slider"></span>
                                </label>
                            </div>
                        </div>
                        
                        <div class="notification-card">
                            <div class="notification-icon">
                                <i class="fas fa-user-shield"></i>
                            </div>
                            <div class="notification-content">
                                <div class="notification-title">Безопасность</div>
                                <div class="notification-description">
                                    Уведомлять о подозрительной активности в аккаунте
                                </div>
                                <label class="switch">
                                    <input type="checkbox" checked>
                                    <span class="slider"></span>
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
                
                <!-- Управление пользователями -->
                <div id="users-section" style="display: none;">
                    <h2 class="section-title">
                        <i class="fas fa-users"></i> Управление пользователями
                    </h2>
                    <p class="section-subtitle">Добавление, удаление и управление пользователями системы</p>
                    
                    <div class="settings-group">
                        <div class="group-title">
                            <i class="fas fa-user-plus"></i> Добавить нового пользователя
                        </div>
                        <form method="POST" class="password-form">
                            <input type="hidden" name="add_user" value="1">
                            
                            <div class="form-row">
                                <div class="form-group">
                                    <label class="form-label">Полное имя *</label>
                                    <input type="text" name="full_name" class="form-control" required
                                           placeholder="Иван Иванов">
                                </div>
                                
                                <div class="form-group">
                                    <label class="form-label">Логин *</label>
                                    <input type="text" name="login" class="form-control" required
                                           placeholder="user123">
                                </div>
                            </div>
                            
                            <div class="form-row">
                                <div class="form-group">
                                    <label class="form-label">Пароль *</label>
                                    <input type="password" name="password" id="newUserPassword" class="form-control" required
                                           placeholder="Минимум 6 символов" oninput="checkUserPasswordStrength()">
                                    <div class="password-strength">
                                        <div class="strength-bar" id="userPasswordStrength"></div>
                                    </div>
                                </div>
                                
                                <div class="form-group">
                                    <label class="form-label">Роль *</label>
                                    <select name="permission_group" class="form-control" required>
                                        <option value="user">Пользователь</option>
                                        <option value="admin">Администратор</option>
                                    </select>
                                </div>
                            </div>
                            
                            <button type="submit" class="btn btn-success">
                                <i class="fas fa-user-plus"></i> Добавить пользователя
                            </button>
                        </form>
                    </div>
                    
                    <div class="settings-group">
                        <div class="group-title">
                            <i class="fas fa-list"></i> Список пользователей
                        </div>
                        
                        <?php if (count($all_users) > 0): ?>
                            <!-- Десктопная таблица -->
                            <div class="users-table desktop-view">
                                <div class="table-header">
                                    <div class="table-header-cell">ID</div>
                                    <div class="table-header-cell">Имя</div>
                                    <div class="table-header-cell">Логин</div>
                                    <div class="table-header-cell">Роль</div>
                                    <div class="table-header-cell">Дата регистрации</div>
                                    <div class="table-header-cell">Действия</div>
                                </div>
                                
                                <?php foreach ($all_users as $user): ?>
                                <div class="table-row">
                                    <div class="table-cell">
                                        <strong>#<?php echo $user['id']; ?></strong>
                                    </div>
                                    <div class="table-cell">
                                        <?php echo htmlspecialchars($user['full_name']); ?>
                                    </div>
                                    <div class="table-cell">
                                        <?php echo htmlspecialchars($user['login']); ?>
                                    </div>
                                    <div class="table-cell">
                                        <span class="user-role-badge <?php echo $user['permission_group'] === 'admin' ? 'role-admin' : 'role-user'; ?>">
                                            <?php echo $user['permission_group'] === 'admin' ? 'Админ' : 'Пользователь'; ?>
                                        </span>
                                    </div>
                                    <div class="table-cell">
                                        <?php echo date('d.m.Y H:i', strtotime($user['created_at'])); ?>
                                    </div>
                                    <div class="table-cell user-actions">
                                        <?php if ($user['id'] != $_SESSION['user_id']): ?>
                                        <button class="btn btn-warning btn-sm" onclick="resetUserPassword(<?php echo $user['id']; ?>, '<?php echo htmlspecialchars($user['full_name']); ?>')">
                                            <i class="fas fa-key"></i> Сброс пароля
                                        </button>
                                        <button class="btn btn-danger btn-sm" onclick="deleteUser(<?php echo $user['id']; ?>, '<?php echo htmlspecialchars($user['full_name']); ?>')">
                                            <i class="fas fa-trash"></i> Удалить
                                        </button>
                                        <?php else: ?>
                                        <span style="color: #64748b; font-size: 13px;">Это вы</span>
                                        <?php endif; ?>
                                    </div>
                                </div>
                                <?php endforeach; ?>
                            </div>
                            
                            <!-- Мобильные карточки -->
                            <div class="mobile-view">
                                <?php foreach ($all_users as $user): ?>
                                <div class="mobile-user-card">
                                    <div class="mobile-user-header">
                                        <div>
                                            <h3 style="color: var(--dark); margin-bottom: 5px;">
                                                <?php echo htmlspecialchars($user['full_name']); ?>
                                            </h3>
                                            <span class="user-role-badge <?php echo $user['permission_group'] === 'admin' ? 'role-admin' : 'role-user'; ?>">
                                                <?php echo $user['permission_group'] === 'admin' ? 'Администратор' : 'Пользователь'; ?>
                                            </span>
                                        </div>
                                        <div style="text-align: right;">
                                            <div style="color: var(--primary); font-size: 18px; font-weight: 800;">
                                                #<?php echo $user['id']; ?>
                                            </div>
                                            <div style="color: #64748b; font-size: 12px;">
                                                <?php echo date('d.m.Y', strtotime($user['created_at'])); ?>
                                            </div>
                                        </div>
                                    </div>
                                    
                                    <div class="mobile-user-info">
                                        <div class="mobile-info-item">
                                            <div class="mobile-info-label">Логин</div>
                                            <div class="mobile-info-value"><?php echo htmlspecialchars($user['login']); ?></div>
                                        </div>
                                        <div class="mobile-info-item">
                                            <div class="mobile-info-label">Дата создания</div>
                                            <div class="mobile-info-value"><?php echo date('H:i', strtotime($user['created_at'])); ?></div>
                                        </div>
                                    </div>
                                    
                                    <?php if ($user['id'] != $_SESSION['user_id']): ?>
                                    <div class="mobile-user-actions">
                                        <button class="btn btn-warning btn-sm" onclick="resetUserPassword(<?php echo $user['id']; ?>, '<?php echo htmlspecialchars($user['full_name']); ?>')">
                                            <i class="fas fa-key"></i> Сброс пароля
                                        </button>
                                        <button class="btn btn-danger btn-sm" onclick="deleteUser(<?php echo $user['id']; ?>, '<?php echo htmlspecialchars($user['full_name']); ?>')">
                                            <i class="fas fa-trash"></i> Удалить
                                        </button>
                                    </div>
                                    <?php else: ?>
                                    <div style="color: #64748b; font-size: 14px; padding: 10px; text-align: center;">
                                        <i class="fas fa-info-circle"></i> Это ваш аккаунт
                                    </div>
                                    <?php endif; ?>
                                </div>
                                <?php endforeach; ?>
                            </div>
                            
                        <?php else: ?>
                            <div class="empty-state">
                                <i class="fas fa-users"></i>
                                <h3>Пользователи не найдены</h3>
                                <p>
                                    В системе еще нет зарегистрированных пользователей.
                                    Добавьте первого пользователя с помощью формы выше.
                                </p>
                            </div>
                        <?php endif; ?>
                    </div>
                </div>
                
                <!-- Системные настройки -->
                <div id="system-section" style="display: none;">
                    <h2 class="section-title">
                        <i class="fas fa-server"></i> Системные настройки
                    </h2>
                    <p class="section-subtitle">Настройки влияющие на работу всей системы</p>
                    
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle"></i> Изменения в этих настройках применяются ко всем пользователям системы
                    </div>
                    
                    <div class="settings-group">
                        <div class="group-title">
                            <i class="fas fa-sliders-h"></i> Основные параметры
                        </div>
                        <form method="POST" action="api/save_system_settings.php" class="system-form">
                            <div class="form-group">
                                <label class="form-label">Название системы</label>
                                <input type="text" name="system_name" class="form-control" 
                                       value="RunaRMK" placeholder="Введите название вашей CRM">
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Авто-выход (минут)</label>
                                <input type="number" name="session_timeout" class="form-control" 
                                       value="30" min="5" max="480" step="5">
                                <small style="color: #64748b; font-size: 13px;">
                                    Через сколько минут неактивности произойдет автоматический выход из системы
                                </small>
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Печать без ККТ</label>
                                <select name="allow_print_without_kkm" class="form-control">
                                    <option value="1">Разрешено (сохранять чеки без печати)</option>
                                    <option value="0">Запрещено (блокировать без подключения)</option>
                                </select>
                            </div>
                            
                            <div class="form-group">
                                <label class="form-label">Товаров на странице</label>
                                <input type="number" name="items_per_page" class="form-control" 
                                       value="50" min="10" max="500" step="10">
                            </div>
                            
                            <button type="submit" class="btn btn-primary">
                                <i class="fas fa-save"></i> Сохранить настройки
                            </button>
                        </form>
                    </div>
                    
                    <div class="danger-zone">
                        <div class="danger-title">
                            <i class="fas fa-database"></i> Управление базой данных
                        </div>
                        <div class="danger-description">
                            Создание резервных копий базы данных. Рекомендуется регулярно создавать бэкапы
                            для защиты данных от потери.
                        </div>
                        <button class="btn btn-warning" onclick="backupDatabase()">
                            <i class="fas fa-download"></i> Создать резервную копию
                        </button>
                        <button class="btn btn-danger" onclick="clearOldLogs()">
                            <i class="fas fa-trash-alt"></i> Очистить старые логи
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <!-- Модальное окно сброса пароля -->
    <div id="resetPasswordModal" class="modal-overlay">
        <div class="modal-content">
            <div class="modal-header">
                <h2><i class="fas fa-key"></i> Сброс пароля</h2>
                <button class="close-modal" onclick="closeResetPasswordModal()">&times;</button>
            </div>
            <form id="resetPasswordForm" method="POST">
                <input type="hidden" name="reset_user_password" value="1">
                <input type="hidden" name="user_id" id="resetUserId">
                
                <div class="form-group">
                    <label class="form-label">Новый пароль для <span id="userName"></span></label>
                    <input type="password" name="new_password" id="resetNewPassword" class="form-control" required
                           placeholder="Введите новый пароль" oninput="checkResetPasswordStrength()">
                    <div class="password-strength">
                        <div class="strength-bar" id="resetPasswordStrength"></div>
                    </div>
                    <small style="color: #64748b; font-size: 13px;">
                        Минимум 6 символов. Пользователю придется использовать этот пароль для входа.
                    </small>
                </div>
                
                <div class="form-group">
                    <label class="form-label">Подтверждение пароля</label>
                    <input type="password" id="resetConfirmPassword" class="form-control" required
                           placeholder="Повторите новый пароль">
                    <small id="passwordMatchError" style="color: #dc3545; font-size: 13px; display: none;">
                        <i class="fas fa-exclamation-circle"></i> Пароли не совпадают
                    </small>
                </div>
                
                <div style="display: flex; gap: 10px; margin-top: 20px;">
                    <button type="button" class="btn btn-secondary" onclick="closeResetPasswordModal()">
                        <i class="fas fa-times"></i> Отмена
                    </button>
                    <button type="submit" class="btn btn-success">
                        <i class="fas fa-save"></i> Сохранить пароль
                    </button>
                </div>
            </form>
        </div>
    </div>
    
    <script>
        // Переключение разделов
        function showSection(sectionId) {
            // Скрыть все разделы
            ['profile', 'security', 'notifications', 'users', 'system'].forEach(id => {
                const element = document.getElementById(id + '-section');
                if (element) element.style.display = 'none';
            });
            
            // Показать выбранный раздел
            document.getElementById(sectionId + '-section').style.display = 'block';
            
            // Обновить активную ссылку
            document.querySelectorAll('.nav-item').forEach(item => {
                item.classList.remove('active');
            });
            event.currentTarget.classList.add('active');
            
            // Обновить URL без перезагрузки
            history.pushState(null, null, '#' + sectionId);
            
            // Адаптивное отображение таблицы/карточек
            checkViewMode();
        }
        
        // Проверка силы пароля
        function checkPasswordStrength() {
            const password = document.getElementById('newPassword').value;
            const strengthBar = document.getElementById('passwordStrength');
            const hints = {
                length: document.getElementById('hint-length'),
                case: document.getElementById('hint-case'),
                number: document.getElementById('hint-number'),
                special: document.getElementById('hint-special')
            };
            
            let strength = 0;
            let validHints = 0;
            
            // Длина
            if (password.length >= 6) {
                strength += 25;
                hints.length.classList.add('valid');
                validHints++;
            } else {
                hints.length.classList.remove('valid');
            }
            
            // Разный регистр
            if (/[a-z]/.test(password) && /[A-Z]/.test(password)) {
                strength += 25;
                hints.case.classList.add('valid');
                validHints++;
            } else {
                hints.case.classList.remove('valid');
            }
            
            // Цифры
            if (/[0-9]/.test(password)) {
                strength += 25;
                hints.number.classList.add('valid');
                validHints++;
            } else {
                hints.number.classList.remove('valid');
            }
            
            // Спецсимволы
            if (/[^a-zA-Z0-9]/.test(password)) {
                strength += 25;
                hints.special.classList.add('valid');
                validHints++;
            } else {
                hints.special.classList.remove('valid');
            }
            
            // Ограничиваем до 100%
            strength = Math.min(strength, 100);
            
            // Обновляем полоску
            strengthBar.style.width = strength + '%';
            
            // Цвет в зависимости от силы
            if (validHints <= 1) {
                strengthBar.className = 'strength-bar strength-weak';
            } else if (validHints <= 3) {
                strengthBar.className = 'strength-bar strength-medium';
            } else {
                strengthBar.className = 'strength-bar strength-strong';
            }
        }
        
        // Проверка силы пароля для нового пользователя
        function checkUserPasswordStrength() {
            const password = document.getElementById('newUserPassword').value;
            const strengthBar = document.getElementById('userPasswordStrength');
            
            let strength = 0;
            
            // Длина
            if (password.length >= 6) strength += 25;
            if (password.length >= 10) strength += 25;
            
            // Сложность
            if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength += 25;
            if (/[0-9]/.test(password)) strength += 15;
            if (/[^a-zA-Z0-9]/.test(password)) strength += 10;
            
            strength = Math.min(strength, 100);
            strengthBar.style.width = strength + '%';
            
            if (strength < 40) {
                strengthBar.className = 'strength-bar strength-weak';
            } else if (strength < 70) {
                strengthBar.className = 'strength-bar strength-medium';
            } else {
                strengthBar.className = 'strength-bar strength-strong';
            }
        }
        
        // Удаление пользователя
        function deleteUser(userId, userName) {
            if (!confirm(`Вы уверены, что хотите удалить пользователя "${userName}"? Это действие нельзя отменить.`)) {
                return;
            }
            
            // Создаем форму для отправки POST запроса
            const form = document.createElement('form');
            form.method = 'POST';
            form.style.display = 'none';
            
            const userIdInput = document.createElement('input');
            userIdInput.type = 'hidden';
            userIdInput.name = 'user_id';
            userIdInput.value = userId;
            
            const actionInput = document.createElement('input');
            actionInput.type = 'hidden';
            actionInput.name = 'delete_user';
            actionInput.value = '1';
            
            form.appendChild(userIdInput);
            form.appendChild(actionInput);
            document.body.appendChild(form);
            form.submit();
        }
        
        // Сброс пароля пользователя
        function resetUserPassword(userId, userName) {
            document.getElementById('resetUserId').value = userId;
            document.getElementById('userName').textContent = userName;
            document.getElementById('resetPasswordModal').style.display = 'flex';
        }
        
        // Закрыть модальное окно сброса пароля
        function closeResetPasswordModal() {
            document.getElementById('resetPasswordModal').style.display = 'none';
            document.getElementById('resetNewPassword').value = '';
            document.getElementById('resetConfirmPassword').value = '';
            document.getElementById('passwordMatchError').style.display = 'none';
        }
        
        // Проверка силы пароля при сбросе
        function checkResetPasswordStrength() {
            const password = document.getElementById('resetNewPassword').value;
            const strengthBar = document.getElementById('resetPasswordStrength');
            
            let strength = 0;
            if (password.length >= 6) strength += 33;
            if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength += 33;
            if (/[0-9]/.test(password) || /[^a-zA-Z0-9]/.test(password)) strength += 34;
            
            strengthBar.style.width = Math.min(strength, 100) + '%';
            
            if (strength < 40) {
                strengthBar.className = 'strength-bar strength-weak';
            } else if (strength < 70) {
                strengthBar.className = 'strength-bar strength-medium';
            } else {
                strengthBar.className = 'strength-bar strength-strong';
            }
        }
        
        // Проверка совпадения паролей при сбросе
        document.getElementById('resetConfirmPassword').addEventListener('input', function() {
            const newPassword = document.getElementById('resetNewPassword').value;
            const confirmPassword = this.value;
            const errorElement = document.getElementById('passwordMatchError');
            
            if (confirmPassword && newPassword !== confirmPassword) {
                errorElement.style.display = 'block';
            } else {
                errorElement.style.display = 'none';
            }
        });
        
        // Выход на всех устройствах
        function logoutAllDevices() {
            if (!confirm('Вы уверены, что хотите завершить все активные сессии? Это действие нельзя отменить.')) {
                return;
            }
            
            fetch('api/logout_all.php', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ user_id: <?php echo $_SESSION['user_id']; ?> })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Все сессии завершены. Вы будете перенаправлены на страницу входа.');
                    window.location.href = 'logout.php?all=1';
                } else {
                    alert('Ошибка: ' + (data.error || 'Неизвестная ошибка'));
                }
            })
            .catch(error => {
                alert('Ошибка сети: ' + error.message);
            });
        }
        
        // Функции для админа
        function backupDatabase() {
            if (confirm('Создать резервную копию базы данных? Файл будет скачан автоматически.')) {
                window.open('api/backup_database.php', '_blank');
            }
        }
        
        function clearOldLogs() {
            const days = prompt('Удалить логи старше скольки дней?', '30');
            if (days && !isNaN(days)) {
                if (confirm(`Удалить логи старше ${days} дней? Это действие нельзя отменить.`)) {
                    fetch('api/clear_logs.php?days=' + days)
                        .then(response => response.json())
                        .then(data => {
                            alert(data.message || 'Логи успешно очищены');
                        })
                        .catch(error => {
                            alert('Ошибка: ' + error.message);
                        });
                }
            }
        }
        
        // Адаптивное отображение таблицы/карточек
        function checkViewMode() {
            const width = window.innerWidth;
            const desktopViews = document.querySelectorAll('.desktop-view');
            const mobileViews = document.querySelectorAll('.mobile-view');
            
            if (width <= 992) {
                desktopViews.forEach(view => view.style.display = 'none');
                mobileViews.forEach(view => view.style.display = 'block');
            } else {
                desktopViews.forEach(view => view.style.display = 'block');
                mobileViews.forEach(view => view.style.display = 'none');
            }
        }
        
        // Автоматическое переключение по хэшу
        document.addEventListener('DOMContentLoaded', function() {
            const hash = window.location.hash.substring(1);
            if (hash) {
                const link = document.querySelector(`.nav-item[href="#${hash}"]`);
                if (link) {
                    link.click();
                }
            }
            
            // Анимация появления
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.style.opacity = '1';
                        entry.target.style.transform = 'translateY(0)';
                    }
                });
            }, { threshold: 0.1 });
            
            document.querySelectorAll('.settings-sidebar, .settings-content').forEach(el => {
                el.style.opacity = '0';
                el.style.transform = 'translateY(20px)';
                el.style.transition = 'all 0.5s cubic-bezier(0.4, 0, 0.2, 1)';
                observer.observe(el);
            });
            
            // Адаптивное отображение
            checkViewMode();
            window.addEventListener('resize', checkViewMode);
            
            // Автоматическое скрытие сообщений
            setTimeout(() => {
                const alerts = document.querySelectorAll('.alert');
                alerts.forEach(alert => {
                    alert.style.transition = 'all 0.5s ease';
                    alert.style.opacity = '0';
                    alert.style.transform = 'translateY(-20px)';
                    setTimeout(() => {
                        alert.style.display = 'none';
                    }, 500);
                });
            }, 5000);
            
            // Обработка формы сброса пароля
            document.getElementById('resetPasswordForm').addEventListener('submit', function(e) {
                const newPassword = document.getElementById('resetNewPassword').value;
                const confirmPassword = document.getElementById('resetConfirmPassword').value;
                
                if (newPassword.length < 6) {
                    e.preventDefault();
                    alert('Пароль должен быть не менее 6 символов');
                    return;
                }
                
                if (newPassword !== confirmPassword) {
                    e.preventDefault();
                    document.getElementById('passwordMatchError').style.display = 'block';
                    return;
                }
            });
        });
    </script>
</body>
</html>