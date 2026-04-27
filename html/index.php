<?php
require_once 'includes/auth_check.php';

// Получаем выбранное подразделение
$division_id = $_SESSION['current_division_id'] ?? null;
$division_name = 'Не выбрано';

if ($division_id) {
    $stmt = $pdo->prepare("SELECT name FROM divisions WHERE id = ?");
    $stmt->execute([$division_id]);
    $division = $stmt->fetch();
    $division_name = $division['name'] ?? 'Не выбрано';
}

// Статистика за сегодня
$today = date('Y-m-d');
$stmt = $pdo->prepare("
    SELECT 
        COUNT(*) as check_count,
        SUM(total_amount) as total_revenue
    FROM checks 
    WHERE DATE(created_at) = ? 
    AND division_id = ?
    AND type = 'sale'
");
$stmt->execute([$today, $division_id]);
$stats = $stmt->fetch();

// Текущая смена
$stmt = $pdo->prepare("
    SELECT * FROM shifts 
    WHERE division_id = ? 
    AND status = 'open'
    ORDER BY opened_at DESC 
    LIMIT 1
");
$stmt->execute([$division_id]);
$current_shift = $stmt->fetch();

// Статистика по товарам
$stmt = $pdo->prepare("
    SELECT 
        COUNT(*) as product_count,
        SUM(quantity) as total_stock
    FROM warehouse_items wi
    JOIN warehouses w ON wi.warehouse_id = w.id
    WHERE w.division_id = ?
    AND wi.quantity > 0
");
$stmt->execute([$division_id]);
$product_stats = $stmt->fetch();
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>RunaRMK - Главная</title>
    <script>var KkmServerAddIn = {};</script>
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
            padding-bottom: 40px;
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
            position: relative;
            overflow: hidden;
        }
        
        .header::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 100%;
            background: url("data:image/svg+xml,%3Csvg width='100' height='100' viewBox='0 0 100 100' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M11 18c3.866 0 7-3.134 7-7s-3.134-7-7-7-7 3.134-7 7 3.134 7 7 7zm48 25c3.866 0 7-3.134 7-7s-3.134-7-7-7-7 3.134-7 7 3.134 7 7 7zm-43-7c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zm63 31c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zM34 90c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zm56-76c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zM12 86c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm28-65c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm23-11c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zm-6 60c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm29 22c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zM32 63c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zm57-13c2.76 0 5-2.24 5-5s-2.24-5-5-5-5 2.24-5 5 2.24 5 5 5zm-9-21c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zM60 91c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zM35 41c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zM12 60c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2z' fill='%23ffffff' fill-opacity='0.03' fill-rule='evenodd'/%3E%3C/svg%3E");
        }
        
        .header h1 {
            color: white;
            font-size: 32px;
            font-weight: 800;
            display: flex;
            align-items: center;
            gap: 15px;
            text-shadow: 0 4px 8px rgba(0,0,0,0.2);
            position: relative;
            z-index: 2;
        }
        
        .header h1 i {
            background: rgba(255,255,255,0.15);
            padding: 18px;
            border-radius: 18px;
            backdrop-filter: blur(10px);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .user-panel {
            display: flex;
            align-items: center;
            gap: 25px;
            background: rgba(255,255,255,0.15);
            padding: 15px 30px;
            border-radius: 50px;
            backdrop-filter: blur(10px);
            border: 2px solid rgba(255,255,255,0.2);
            position: relative;
            z-index: 2;
            box-shadow: 0 8px 20px rgba(0,0,0,0.1);
        }
        
        .user-panel span {
            color: white;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 10px;
            font-size: 15px;
        }
        
        .user-panel a {
            color: white;
            text-decoration: none;
            font-weight: 600;
            padding: 10px 20px;
            background: rgba(255,255,255,0.2);
            border-radius: 30px;
            transition: all 0.3s;
            display: flex;
            align-items: center;
            gap: 8px;
            border: 1px solid rgba(255,255,255,0.3);
        }
        
        .user-panel a:hover {
            background: rgba(255,255,255,0.3);
            transform: translateY(-2px);
            box-shadow: 0 6px 15px rgba(0,0,0,0.15);
        }
        
        .connection-status {
            position: fixed;
            bottom: 30px;
            right: 30px;
            padding: 16px 30px;
            border-radius: 50px;
            font-weight: 700;
            z-index: 1000;
            box-shadow: var(--shadow-lg);
            display: flex;
            align-items: center;
            gap: 12px;
            font-size: 15px;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            backdrop-filter: blur(10px);
            border: 2px solid rgba(255,255,255,0.3);
            animation: slideIn 0.5s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        @keyframes slideIn {
            from {
                transform: translateX(100%);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }
        
        .connected {
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.95) 0%, rgba(39, 174, 96, 0.95) 100%);
            color: white;
        }
        
        .disconnected {
            background: linear-gradient(135deg, rgba(231, 76, 60, 0.95) 0%, rgba(192, 57, 43, 0.95) 100%);
            color: white;
        }
        
        .container {
            max-width: 1400px;
            margin: 0 auto;
            padding: 0 30px;
        }
        
        .welcome-section {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            padding: 40px;
            margin-bottom: 40px;
            box-shadow: var(--shadow-md);
            border: 1px solid rgba(0,0,0,0.05);
            position: relative;
            overflow: hidden;
        }
        
        .welcome-section::before {
            content: '';
            position: absolute;
            top: 0;
            right: 0;
            width: 300px;
            height: 300px;
            background: linear-gradient(135deg, var(--primary) 0%, var(--info) 100%);
            opacity: 0.05;
            border-radius: 50%;
            transform: translate(100px, -100px);
        }
        
        .welcome-title {
            color: var(--dark);
            font-size: 32px;
            font-weight: 800;
            margin-bottom: 15px;
            position: relative;
            z-index: 1;
        }
        
        .welcome-subtitle {
            color: #64748b;
            font-size: 18px;
            line-height: 1.6;
            max-width: 800px;
            position: relative;
            z-index: 1;
        }
        
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 25px;
            margin-bottom: 40px;
        }
        
        .stat-card {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            border: 2px solid transparent;
            position: relative;
            overflow: hidden;
            display: flex;
            flex-direction: column;
        }
        
        .stat-card:hover {
            transform: translateY(-10px);
            box-shadow: var(--shadow-lg);
            border-color: var(--primary);
        }
        
        .stat-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--primary), var(--info));
        }
        
        .stat-icon {
            width: 70px;
            height: 70px;
            border-radius: 18px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 32px;
            margin-bottom: 25px;
            position: relative;
            z-index: 1;
        }
        
        .stat-icon.revenue {
            background: linear-gradient(135deg, #f0f7ff 0%, #d4e4ff 100%);
            color: var(--primary);
        }
        
        .stat-icon.checks {
            background: linear-gradient(135deg, #f0fff4 0%, #dcffe4 100%);
            color: var(--secondary);
        }
        
        .stat-icon.products {
            background: linear-gradient(135deg, #fff5f5 0%, #ffe5e5 100%);
            color: var(--danger);
        }
        
        .stat-icon.shift {
            background: linear-gradient(135deg, #fff8e1 0%, #ffeaa7 100%);
            color: var(--warning);
        }
        
        .stat-card h3 {
            color: var(--dark);
            font-size: 16px;
            font-weight: 600;
            margin-bottom: 12px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .stat-value {
            font-size: 42px;
            font-weight: 800;
            color: var(--dark);
            margin: 15px 0;
            line-height: 1;
        }
        
        .stat-change {
            display: flex;
            align-items: center;
            gap: 8px;
            font-size: 14px;
            font-weight: 600;
            margin-top: auto;
        }
        
        .stat-change.positive {
            color: var(--secondary);
        }
        
        .stat-change.negative {
            color: var(--danger);
        }
        
        .shift-info-card {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            padding: 35px;
            margin-bottom: 40px;
            box-shadow: var(--shadow-md);
            border: 2px solid rgba(67, 97, 238, 0.1);
            position: relative;
            overflow: hidden;
        }
        
        .shift-info-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--secondary), var(--warning));
        }
        
        .shift-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 30px;
        }
        
        .shift-header h2 {
            color: var(--dark);
            font-size: 24px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .shift-status {
            padding: 12px 28px;
            border-radius: 30px;
            font-weight: 700;
            font-size: 15px;
            text-transform: uppercase;
            letter-spacing: 1px;
        }
        
        .shift-status.open {
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.15) 0%, rgba(46, 204, 113, 0.05) 100%);
            color: var(--secondary);
            border: 2px solid rgba(46, 204, 113, 0.3);
        }
        
        .shift-status.closed {
            background: linear-gradient(135deg, rgba(231, 76, 60, 0.15) 0%, rgba(231, 76, 60, 0.05) 100%);
            color: var(--danger);
            border: 2px solid rgba(231, 76, 60, 0.3);
        }
        
        .shift-stats {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 25px;
        }
        
        .shift-stat {
            text-align: center;
            padding: 25px;
            background: white;
            border-radius: 12px;
            box-shadow: var(--shadow-sm);
            border: 1px solid rgba(0,0,0,0.05);
            transition: all 0.3s;
        }
        
        .shift-stat:hover {
            transform: translateY(-5px);
            box-shadow: var(--shadow-md);
        }
        
        .shift-stat h4 {
            color: #64748b;
            font-size: 14px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 12px;
        }
        
        .shift-stat .value {
            font-size: 36px;
            font-weight: 800;
            color: var(--primary);
        }
        
        .quick-actions {
            margin-bottom: 50px;
        }
        
        .quick-actions h2 {
            color: var(--dark);
            font-size: 28px;
            font-weight: 700;
            margin-bottom: 30px;
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .actions-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 25px;
        }
        
        .action-card {
            background: white;
            border-radius: var(--border-radius);
            padding: 35px;
            box-shadow: var(--shadow-sm);
            border: 2px solid transparent;
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            position: relative;
            overflow: hidden;
            text-decoration: none;
            color: inherit;
            display: block;
        }
        
        .action-card:hover {
            transform: translateY(-10px) scale(1.02);
            box-shadow: var(--shadow-lg);
            border-color: var(--primary);
        }
        
        .action-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--primary), var(--info));
        }
        
        .action-icon {
            width: 80px;
            height: 80px;
            border-radius: 20px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 36px;
            margin-bottom: 25px;
            position: relative;
            z-index: 1;
            background: linear-gradient(135deg, var(--light-blue) 0%, #dbeafe 100%);
            color: var(--primary);
        }
        
        .action-card h3 {
            color: var(--dark);
            font-size: 22px;
            font-weight: 700;
            margin-bottom: 15px;
        }
        
        .action-card p {
            color: #64748b;
            font-size: 15px;
            line-height: 1.6;
            margin-bottom: 20px;
        }
        
        .action-arrow {
            color: var(--primary);
            font-size: 20px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 8px;
            transition: all 0.3s;
        }
        
        .action-card:hover .action-arrow {
            gap: 15px;
            color: var(--info);
        }
        
        .admin-section {
            margin-top: 60px;
            padding-top: 50px;
            border-top: 2px solid rgba(0,0,0,0.05);
        }
        
        .admin-section h2 {
            color: var(--dark);
            font-size: 28px;
            font-weight: 700;
            margin-bottom: 35px;
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .admin-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 25px;
        }
        
        .admin-card {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid transparent;
            transition: all 0.3s;
            text-decoration: none;
            color: inherit;
            display: block;
        }
        
        .admin-card:hover {
            transform: translateY(-8px);
            box-shadow: var(--shadow-lg);
            border-color: var(--warning);
        }
        
        .admin-card h3 {
            color: var(--dark);
            font-size: 18px;
            font-weight: 700;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .admin-card p {
            color: #64748b;
            font-size: 14px;
            line-height: 1.5;
        }
        
        .no-shift {
            background: linear-gradient(135deg, #fff8e1 0%, #ffeaa7 100%);
            border: 2px solid #ffc107;
            border-radius: var(--border-radius);
            padding: 30px;
            text-align: center;
            margin-bottom: 40px;
        }
        
        .no-shift h3 {
            color: #856404;
            font-size: 22px;
            font-weight: 700;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 15px;
        }
        
        .no-shift p {
            color: #856404;
            margin-bottom: 25px;
            font-size: 16px;
        }
        
        .btn {
            padding: 18px 35px;
            border-radius: 50px;
            font-weight: 700;
            font-size: 16px;
            cursor: pointer;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            border: none;
            letter-spacing: 0.5px;
        }
        
        .btn-primary {
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(67, 97, 238, 0.3);
        }
        
        .btn-primary:hover {
            transform: translateY(-3px);
            box-shadow: 0 12px 25px rgba(67, 97, 238, 0.4);
        }
        
        .btn-secondary {
            background: linear-gradient(135deg, var(--secondary) 0%, #27ae60 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(46, 204, 113, 0.3);
        }
        
        .btn-secondary:hover {
            transform: translateY(-3px);
            box-shadow: 0 12px 25px rgba(46, 204, 113, 0.4);
        }
        
        .btn-warning {
            background: linear-gradient(135deg, var(--warning) 0%, #e67e22 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(243, 156, 18, 0.3);
        }
        
        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        .fade-in-up {
            animation: fadeInUp 0.6s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        @media (max-width: 1200px) {
            .container {
                padding: 0 25px;
            }
            
            .header {
                padding: 20px 25px;
            }
            
            .header h1 {
                font-size: 28px;
            }
        }
        
        @media (max-width: 992px) {
            .header {
                flex-direction: column;
                gap: 20px;
                text-align: center;
                padding: 25px 20px;
            }
            
            .user-panel {
                flex-wrap: wrap;
                justify-content: center;
            }
            
            .stats-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .actions-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }
        
        @media (max-width: 768px) {
            .stats-grid,
            .actions-grid {
                grid-template-columns: 1fr;
            }
            
            .shift-stats {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .welcome-section {
                padding: 30px 25px;
            }
            
            .welcome-title {
                font-size: 26px;
            }
            
            .stat-card {
                padding: 25px;
            }
            
            .stat-value {
                font-size: 36px;
            }
        }
        
        @media (max-width: 576px) {
            .container {
                padding: 0 15px;
            }
            
            .shift-stats {
                grid-template-columns: 1fr;
            }
            
            .user-panel {
                flex-direction: column;
                width: 100%;
                border-radius: var(--border-radius);
            }
            
            .connection-status {
                bottom: 20px;
                right: 20px;
                left: 20px;
                text-align: center;
                justify-content: center;
            }
        }
    </style>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">
</head>
<body>
    <!-- Статус соединения -->
    <div id="connectionStatus" class="connection-status" style="display: none;">
        <i class="fas fa-sync fa-spin"></i> Проверка связи с ККТ...
    </div>
    
    <!-- Шапка -->
    <div class="header">
        <h1>
            <i class="fas fa-cash-register"></i>
            RunaRMK
        </h1>
        <div class="user-panel">
            <span><i class="fas fa-user-circle"></i> <?php echo htmlspecialchars($_SESSION['user_name']); ?></span>
            <span><i class="fas fa-store-alt"></i> <?php echo htmlspecialchars($division_name); ?></span>
            <a href="logout.php">
                <i class="fas fa-sign-out-alt"></i> Выход
            </a>
        </div>
    </div>
    
    <!-- Основной контент -->
    <div class="container">
        <!-- Приветственная секция -->
        <div class="welcome-section fade-in-up">
            <h1 class="welcome-title">Добро пожаловать <?php echo htmlspecialchars($_SESSION['user_name']); ?>!</h1>
            <p class="welcome-subtitle">
                Удачной смены и хороших продаж!
            </p>
        </div>
        
        <!-- Статистика -->
        <div class="stats-grid">
            <div class="stat-card fade-in-up" style="animation-delay: 0.1s;">
                <div class="stat-icon revenue">
                    <i class="fas fa-chart-line"></i>
                </div>
                <h3>Выручка сегодня</h3>
                <div class="stat-value"><?php echo number_format($stats['total_revenue'] ?? 0, 2); ?> ₽</div>
            </div>
            
            <div class="stat-card fade-in-up" style="animation-delay: 0.2s;">
                <div class="stat-icon checks">
                    <i class="fas fa-receipt"></i>
                </div>
                <h3>Чеков сегодня</h3>
                <div class="stat-value"><?php echo $stats['check_count'] ?? 0; ?></div>
            </div>
            
            <div class="stat-card fade-in-up" style="animation-delay: 0.3s;">
                <div class="stat-icon products">
                    <i class="fas fa-boxes"></i>
                </div>
                <h3>Товаров на складе</h3>
                <div class="stat-value"><?php echo $product_stats['product_count'] ?? 0; ?></div>
            </div>
            
            <div class="stat-card fade-in-up" style="animation-delay: 0.4s;">
                <div class="stat-icon shift">
                    <i class="fas fa-clock"></i>
                </div>
                <h3>Статус смены</h3>
                <div class="stat-value">
                    <?php echo $current_shift ? 'Открыта' : 'Закрыта'; ?>
                </div>
                <div class="stat-change positive">
                    <i class="fas fa-check-circle"></i>
                    <?php if ($current_shift): ?>
                    <span>Смена #<?php echo $current_shift['kkm_shift_number']; ?></span>
                    <?php else: ?>
                    <span>Требуется открыть</span>
                    <?php endif; ?>
                </div>
            </div>
        </div>
        
        <!-- Информация о смене -->
        <?php if ($current_shift): ?>
        <div class="shift-info-card fade-in-up" style="animation-delay: 0.5s;">
            <div class="shift-header">
                <h2><i class="fas fa-clock"></i> Текущая смена</h2>
                <div class="shift-status open">
                    Открыта
                </div>
            </div>
            <div class="shift-stats">
                <div class="shift-stat">
                    <h4>Наличные</h4>
                    <div class="value"><?php echo number_format($current_shift['total_cash'], 2); ?> ₽</div>
                </div>
                <div class="shift-stat">
                    <h4>Безналичные</h4>
                    <div class="value"><?php echo number_format($current_shift['total_card'], 2); ?> ₽</div>
                </div>
                <div class="shift-stat">
                    <h4>Всего чеков</h4>
                    <div class="value"><?php echo $current_shift['total_checks']; ?></div>
                </div>
                <div class="shift-stat">
                    <h4>Средний чек</h4>
                    <div class="value">
                        <?php 
                        $avg_check = $current_shift['total_checks'] > 0 
                            ? ($current_shift['total_cash'] + $current_shift['total_card']) / $current_shift['total_checks'] 
                            : 0;
                        echo number_format($avg_check, 2); 
                        ?> ₽
                    </div>
                </div>
            </div>
        </div>
        <?php else: ?>
        <div class="no-shift fade-in-up" style="animation-delay: 0.5s;">
            <h3><i class="fas fa-exclamation-triangle"></i> Смена не открыта</h3>
            <p>Для начала работы с чеками необходимо открыть смену.</p>
            <a href="modules/shifts/" class="btn btn-secondary">
                <i class="fas fa-door-open"></i> Открыть смену
            </a>
        </div>
        <?php endif; ?>
        
        <!-- Быстрые действия -->
        <div class="quick-actions fade-in-up" style="animation-delay: 0.6s;">
            <h2><i class="fas fa-bolt"></i> Быстрые действия</h2>
            <div class="actions-grid">
                <a href="modules/checks/new_check.php" class="action-card">
                    <div class="action-icon">
                        <i class="fas fa-receipt"></i>
                    </div>
                    <h3>Новый чек</h3>
                    <p>Оформление продажи, возврата или коррекции. Быстрый доступ к товарам и печать чека.</p>
                    <div class="action-arrow">
                        <span>Перейти</span>
                        <i class="fas fa-arrow-right"></i>
                    </div>
                </a>
                <a href="modules/shifts/" class="action-card">
                    <div class="action-icon">
                        <i class="fas fa-clock"></i>
                    </div>
                    <h3>Управление сменами</h3>
                    <p>Открытие, закрытие смен, просмотр отчетов и контроль кассовой дисциплины.</p>
                    <div class="action-arrow">
                        <span>Перейти</span>
                        <i class="fas fa-arrow-right"></i>
                    </div>
                </a>
                
                <a href="modules/history/" class="action-card">
                    <div class="action-icon">
                        <i class="fas fa-history"></i>
                    </div>
                    <h3>Журнал операций</h3>
                    <p>Полная история всех операций, фильтры по датам, поиск и детальная информация.</p>
                    <div class="action-arrow">
                        <span>Перейти</span>
                        <i class="fas fa-arrow-right"></i>
                    </div>
                </a>
                <a href="./123.php" class="action-card">
                    <div class="action-icon">
                        <i class="fa fa-tags"></i>
                    </div>
                    <h3>Печать ценников 50x50</h3>
                    <p>Печать ценников на товары.</p>
                    <div class="action-arrow">
                        <span>Перейти</span>
                        <i class="fas fa-arrow-right"></i>
                    </div>
                </a>
                <a href="./zx.php" class="action-card">
                    <div class="action-icon">
                        <i class="fa fa-tags"></i>
                    </div>
                    <h3>Печать ценников 120x45</h3>
                    <p>Печать ценников на товары.</p>
                    <div class="action-arrow">
                        <span>Перейти</span>
                        <i class="fas fa-arrow-right"></i>
                    </div>
                </a>
            </div>
        </div>
        <div class="quick-actions fade-in-up" style="animation-delay: 0.6s;">
            <h2><i class="fas fa-game"></i> Игры</h2>
            <div class="actions-grid">
                <a href="./zmey.php" class="action-card">
                    <div class="action-icon">
                        <i class="fas fa-snake"></i>
                    </div>
                    <h3>Змейка</h3>
                    <p>Змейка игра играц.</p>
                    <div class="action-arrow">
                        <span>Перейти</span>
                        <i class="fas fa-arrow-right"></i>
                    </div>
                </a>
            </div>
        </div>
        <!-- Административные функции -->
        <?php if ($_SESSION['permission_group'] === 'admin'): ?>
        <div class="admin-section fade-in-up" style="animation-delay: 0.7s;">
            <h2><i class="fas fa-cogs"></i> Административные функции</h2>
            <div class="admin-grid">
                <a href="modules/admin/devices.php" class="admin-card">
                    <h3><i class="fas fa-cash-register"></i> Устройства ККТ</h3>
                    <p>Настройка и управление кассовыми аппаратами</p>
                </a>
                
                <a href="modules/warehouse/view.php" class="admin-card">
                    <h3><i class="fas fa-warehouse"></i> Склад</h3>
                    <p>Управление товарами, остатками и номенклатурой</p>
                </a>
                <a href="modules/warehouse/inventory.php" class="admin-card">
                    <h3><i class="fas fa-warehouse"></i> Инвенторизация</h3>
                    <p>Инвенторизация емае</p>
                </a>
                <a href="modules/admin/divisions.php" class="admin-card">
                    <h3><i class="fas fa-store-alt"></i> Подразделения</h3>
                    <p>Настройка магазинов, складов и точек продаж</p>
                </a>
                
                <a href="modules/admin/nomenclature.php" class="admin-card">
                    <h3><i class="fas fa-list-alt"></i> Номенклатуры</h3>
                    <p>Справочник товаров, категорий и цен</p>
                </a>
                
                <a href="modules/admin/monitoring.php" class="admin-card">
                    <h3><i class="fas fa-desktop"></i> Мониторинг</h3>
                    <p>Контроль работы системы в реальном времени</p>
                </a>
                
                <a href="settings.php" class="admin-card">
                    <h3><i class="fas fa-sliders-h"></i> Настройки</h3>
                    <p>Общие настройки</p>
                </a>
                <a href="modules/admin/pma/index.php" class="admin-card">
                    <h3><i class="fas fa-database"></i> phpMyAdmin</h3>
                    <p>База дынных</p>
                </a>
            </div>
        </div>
        <?php endif; ?>
    </div>
    
    <script>
        // Проверка связи с ККТ
        function checkKKTConnection() {
            const statusElement = document.getElementById('connectionStatus');
            
            if (typeof KkmServer === 'undefined') {
                isKKTConnected = false;
                statusElement.innerHTML = '<i class="fas fa-wifi-slash"></i> Расширение ККТ не найдено';
                statusElement.className = 'connection-status disconnected';
                statusElement.style.display = 'block';
                return;
            }
            
            const testData = {
                Command: "GetDataKKT",
                NumDevice: 0,
                IdCommand: generateGuid()
            };
            
            try {
                KkmServer.Execute(function(result) {
                    if (result && result.Status !== undefined) {
                        statusElement.innerHTML = '<i class="fas fa-wifi"></i> Соединение с ККТ установлено';
                        statusElement.className = 'connection-status connected';
                        statusElement.style.display = 'block';
                    } else {
                        statusElement.innerHTML = '<i class="fas fa-wifi-slash"></i> Нет ответа от ККТ';
                        statusElement.className = 'connection-status disconnected';
                        statusElement.style.display = 'block';
                    }
                }, testData);
            } catch (error) {
                statusElement.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Ошибка связи с ККТ';
                statusElement.className = 'connection-status disconnected';
                statusElement.style.display = 'block';
            }
        }
        
        // Генерация GUID
        function generateGuid() {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
                const r = Math.random() * 16 | 0;
                const v = c === 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        }
        
        // Запускаем проверку связи при загрузке
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(checkKKTConnection, 1000);
            setInterval(checkKKTConnection, 10000);
            
            // Анимация появления элементов
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('fade-in-up');
                    }
                });
            }, { threshold: 0.1 });
            
            document.querySelectorAll('.stat-card, .action-card, .admin-card').forEach(card => {
                observer.observe(card);
            });
        });
    </script>
</body>
</html>