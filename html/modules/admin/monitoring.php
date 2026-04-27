<?php
require_once '../../includes/auth_check.php';

if ($_SESSION['permission_group'] !== 'admin') {
    header('Location: ../../index.php');
    exit();
}

// Получаем все подразделения
$stmt = $pdo->prepare("SELECT * FROM divisions ORDER BY name");
$stmt->execute();
$divisions = $stmt->fetchAll();

// Выбранное подразделение
$selected_division_id = $_GET['division_id'] ?? ($divisions[0]['id'] ?? null);

// Получаем смены выбранного подразделения (исправленный запрос с подзапросом)
$shifts = [];
if ($selected_division_id) {
    $stmt = $pdo->prepare("
        SELECT 
            s.*,
            u.full_name as cashier_name,
            d.name as division_name,
            COALESCE(c.check_count, 0) as check_count,
            COALESCE(c.total_amount, 0) as total_amount,
            COALESCE(c.cash_amount, 0) as cash_amount,
            COALESCE(c.card_amount, 0) as card_amount
        FROM shifts s
        LEFT JOIN users u ON s.user_id = u.id
        LEFT JOIN divisions d ON s.division_id = d.id
        LEFT JOIN (
            SELECT 
                shift_id,
                COUNT(id) as check_count,
                SUM(total_amount) as total_amount,
                SUM(cash_amount) as cash_amount,
                SUM(card_amount) as card_amount
            FROM checks
            GROUP BY shift_id
        ) c ON s.id = c.shift_id
        WHERE s.division_id = ?
        ORDER BY s.opened_at DESC
        LIMIT 50
    ");
    $stmt->execute([$selected_division_id]);
    $shifts = $stmt->fetchAll();
}

// Текущая дата для фильтров
$current_date = date('Y-m-d');
$date_from = $_GET['date_from'] ?? date('Y-m-01'); // Первое число месяца
$date_to = $_GET['date_to'] ?? $current_date;

// Рассчитываем статистику
$today = date('Y-m-d');
$week_ago = date('Y-m-d', strtotime('-7 days'));
$month_ago = date('Y-m-d', strtotime('-30 days'));

// Функция для получения статистики (исправленная)
function getShiftStats($pdo, $division_id, $date_from, $date_to) {
    $stmt = $pdo->prepare("
        SELECT 
            COUNT(*) as shift_count,
            COALESCE(SUM(total_cash), 0) as total_cash,
            COALESCE(SUM(total_card), 0) as total_card,
            COALESCE(SUM(total_checks), 0) as total_checks
        FROM shifts 
        WHERE division_id = ? 
        AND DATE(opened_at) BETWEEN ? AND ?
    ");
    $stmt->execute([$division_id, $date_from, $date_to]);
    return $stmt->fetch();
}

$stats_today = getShiftStats($pdo, $selected_division_id, $today, $today);
$stats_week = getShiftStats($pdo, $selected_division_id, $week_ago, $today);
$stats_month = getShiftStats($pdo, $selected_division_id, $month_ago, $today);
$stats_total = getShiftStats($pdo, $selected_division_id, '2000-01-01', $today);
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Мониторинг смен - RunaRMK</title>
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
        
        .btn-info {
            background: linear-gradient(135deg, var(--info) 0%, #2980b9 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(52, 152, 219, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-warning {
            background: linear-gradient(135deg, var(--warning) 0%, #e67e22 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(243, 156, 18, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-sm {
            padding: 10px 20px;
            font-size: 14px;
        }
        
        .container {
            max-width: 1400px;
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
        }
        
        .stat-card.today::before {
            background: linear-gradient(90deg, var(--secondary), #27ae60);
        }
        
        .stat-card.week::before {
            background: linear-gradient(90deg, var(--warning), #e67e22);
        }
        
        .stat-card.month::before {
            background: linear-gradient(90deg, var(--info), #2980b9);
        }
        
        .stat-card.total::before {
            background: linear-gradient(90deg, var(--primary), #3a56d4);
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
        
        .stat-icon.today {
            background: linear-gradient(135deg, #f0fff4 0%, #dcffe4 100%);
            color: var(--secondary);
        }
        
        .stat-icon.week {
            background: linear-gradient(135deg, #fff8e1 0%, #ffeaa7 100%);
            color: var(--warning);
        }
        
        .stat-icon.month {
            background: linear-gradient(135deg, #e9f2ff 0%, #d4e4ff 100%);
            color: var(--info);
        }
        
        .stat-icon.total {
            background: linear-gradient(135deg, #f0f7ff 0%, #d4e4ff 100%);
            color: var(--primary);
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
        
        .filter-bar {
            background: white;
            border-radius: var(--border-radius);
            padding: 25px 30px;
            margin-bottom: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
            display: flex;
            align-items: center;
            gap: 20px;
            flex-wrap: wrap;
        }
        
        .filter-group {
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .filter-label {
            color: var(--dark);
            font-weight: 600;
            font-size: 14px;
            white-space: nowrap;
        }
        
        .form-control {
            padding: 14px 20px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 15px;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.3s;
            min-width: 200px;
        }
        
        select.form-control {
            appearance: none;
            background-image: url("data:image/svg+xml;charset=UTF-8,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3e%3cpolyline points='6 9 12 15 18 9'%3e%3c/polyline%3e%3c/svg%3e");
            background-repeat: no-repeat;
            background-position: right 15px center;
            background-size: 20px;
            padding-right: 45px;
        }
        
        .form-control:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .export-bar {
            background: white;
            border-radius: var(--border-radius);
            padding: 20px 30px;
            margin-bottom: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
            display: flex;
            justify-content: flex-end;
            gap: 15px;
        }
        
        .shifts-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(600px, 1fr));
            gap: 25px;
        }
        
        .shift-card {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid transparent;
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            position: relative;
            overflow: hidden;
        }
        
        .shift-card:hover {
            transform: translateY(-8px);
            box-shadow: var(--shadow-lg);
            border-color: var(--primary);
        }
        
        .shift-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--primary), var(--info));
        }
        
        .shift-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 25px;
        }
        
        .shift-title {
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .shift-icon {
            width: 60px;
            height: 60px;
            border-radius: 15px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 28px;
            background: linear-gradient(135deg, rgba(67, 97, 238, 0.15) 0%, rgba(67, 97, 238, 0.05) 100%);
            color: var(--primary);
        }
        
        .shift-info h3 {
            color: var(--dark);
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 8px;
            line-height: 1.4;
        }
        
        .shift-badge {
            display: flex;
            align-items: center;
            gap: 8px;
            padding: 8px 18px;
            border-radius: 30px;
            font-weight: 700;
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .badge-open {
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.15) 0%, rgba(46, 204, 113, 0.05) 100%);
            color: var(--secondary);
            border: 2px solid rgba(46, 204, 113, 0.3);
        }
        
        .badge-closed {
            background: linear-gradient(135deg, rgba(231, 76, 60, 0.15) 0%, rgba(231, 76, 60, 0.05) 100%);
            color: var(--danger);
            border: 2px solid rgba(231, 76, 60, 0.3);
        }
        
        .shift-time {
            background: #f8fafc;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 25px;
            border: 2px solid #e2e8f0;
        }
        
        .time-label {
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
        
        .time-value {
            font-family: 'Roboto Mono', monospace;
            font-size: 18px;
            font-weight: 700;
            color: var(--dark);
            display: flex;
            gap: 20px;
        }
        
        .shift-stats {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 15px;
            margin-bottom: 25px;
        }
        
        .stat-item {
            background: white;
            border-radius: 12px;
            padding: 20px;
            border: 1px solid rgba(0,0,0,0.05);
            text-align: center;
        }
        
        .stat-value-small {
            font-size: 24px;
            font-weight: 800;
            color: var(--primary);
            margin-bottom: 5px;
        }
        
        .stat-label {
            color: #64748b;
            font-size: 13px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .cashier-info {
            display: flex;
            gap: 12px;
            margin-bottom: 25px;
            align-items: center;
            padding: 15px;
            background: #f8fafc;
            border-radius: 12px;
        }
        
        .cashier-info i {
            color: var(--primary);
            font-size: 18px;
        }
        
        .cashier-content {
            flex: 1;
        }
        
        .cashier-label {
            color: #64748b;
            font-size: 13px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 5px;
        }
        
        .cashier-name {
            color: var(--dark);
            font-size: 16px;
            font-weight: 600;
        }
        
        .shift-actions {
            display: flex;
            gap: 12px;
        }
        
        .empty-state {
            text-align: center;
            padding: 60px 30px;
            grid-column: 1 / -1;
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
            max-width: 800px;
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
                padding: 0 25px 40px;
            }
            
            .header {
                padding: 20px 25px;
            }
            
            .header h1 {
                font-size: 28px;
            }
            
            .shifts-grid {
                grid-template-columns: repeat(auto-fill, minmax(500px, 1fr));
            }
        }
        
        @media (max-width: 992px) {
            .header {
                flex-direction: column;
                gap: 20px;
                text-align: center;
                padding: 25px 20px;
            }
            
            .shifts-grid {
                grid-template-columns: 1fr;
            }
            
            .stats-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .filter-bar {
                flex-direction: column;
                align-items: stretch;
            }
            
            .filter-group {
                width: 100%;
            }
            
            .form-control {
                width: 100%;
            }
        }
        
        @media (max-width: 768px) {
            .shift-header {
                flex-direction: column;
                align-items: stretch;
                gap: 15px;
            }
            
            .shift-title {
                flex-direction: column;
                align-items: flex-start;
                gap: 10px;
            }
            
            .shift-badge {
                align-self: flex-start;
            }
            
            .shift-actions {
                flex-wrap: wrap;
            }
            
            .stats-grid {
                grid-template-columns: 1fr;
            }
            
            .time-value {
                flex-direction: column;
                gap: 5px;
            }
        }
        
        @media (max-width: 576px) {
            .container {
                padding: 0 15px 30px;
            }
            
            .shift-stats {
                grid-template-columns: 1fr;
            }
            
            .stat-value {
                font-size: 32px;
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
            <i class="fas fa-chart-line"></i>
            Мониторинг смен
        </h1>
        <a href="../../index.php" class="btn btn-primary">
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
        
        <!-- Статистика -->
        <div class="stats-grid">
            <div class="stat-card today fade-in-up" style="animation-delay: 0.1s;">
                <div class="stat-icon today">
                    <i class="fas fa-sun"></i>
                </div>
                <h3>Сегодня</h3>
                <div class="stat-value"><?php echo number_format(($stats_today['total_cash'] ?? 0) + ($stats_today['total_card'] ?? 0), 2); ?> ₽</div>
                <div style="color: #64748b; font-size: 14px;"><?php echo $stats_today['shift_count'] ?? 0; ?> смен</div>
            </div>
            
            <div class="stat-card week fade-in-up" style="animation-delay: 0.2s;">
                <div class="stat-icon week">
                    <i class="fas fa-calendar-week"></i>
                </div>
                <h3>За неделю</h3>
                <div class="stat-value"><?php echo number_format(($stats_week['total_cash'] ?? 0) + ($stats_week['total_card'] ?? 0), 2); ?> ₽</div>
                <div style="color: #64748b; font-size: 14px;"><?php echo $stats_week['shift_count'] ?? 0; ?> смен</div>
            </div>
            
            <div class="stat-card month fade-in-up" style="animation-delay: 0.3s;">
                <div class="stat-icon month">
                    <i class="fas fa-calendar-alt"></i>
                </div>
                <h3>За месяц</h3>
                <div class="stat-value"><?php echo number_format(($stats_month['total_cash'] ?? 0) + ($stats_month['total_card'] ?? 0), 2); ?> ₽</div>
                <div style="color: #64748b; font-size: 14px;"><?php echo $stats_month['shift_count'] ?? 0; ?> смен</div>
            </div>
            
            <div class="stat-card total fade-in-up" style="animation-delay: 0.4s;">
                <div class="stat-icon total">
                    <i class="fas fa-chart-bar"></i>
                </div>
                <h3>Всего</h3>
                <div class="stat-value"><?php echo number_format(($stats_total['total_cash'] ?? 0) + ($stats_total['total_card'] ?? 0), 2); ?> ₽</div>
                <div style="color: #64748b; font-size: 14px;"><?php echo $stats_total['shift_count'] ?? 0; ?> смен</div>
            </div>
        </div>
        
        <!-- Фильтры -->
        <form method="GET" class="filter-bar fade-in-up" style="animation-delay: 0.5s;">
            <div class="filter-group">
                <div class="filter-label"><i class="fas fa-building"></i> Подразделение:</div>
                <select name="division_id" class="form-control" onchange="this.form.submit()">
                    <?php foreach ($divisions as $division): ?>
                    <option value="<?php echo $division['id']; ?>"
                        <?php echo $selected_division_id == $division['id'] ? 'selected' : ''; ?>>
                        <?php echo htmlspecialchars($division['name']); ?>
                    </option>
                    <?php endforeach; ?>
                </select>
            </div>
            
            <div class="filter-group">
                <div class="filter-label"><i class="fas fa-calendar"></i> С:</div>
                <input type="date" name="date_from" class="form-control" 
                       value="<?php echo htmlspecialchars($date_from); ?>">
            </div>
            
            <div class="filter-group">
                <div class="filter-label">По:</div>
                <input type="date" name="date_to" class="form-control" 
                       value="<?php echo htmlspecialchars($date_to); ?>">
            </div>
            
            <button type="submit" class="btn btn-primary">
                <i class="fas fa-filter"></i> Применить фильтры
            </button>
        </form>
        
        <!-- Панель экспорта -->
        
        <!-- Сетка смен -->
        <div class="shifts-grid fade-in-up" style="animation-delay: 0.7s;">
            <?php if (count($shifts) > 0): ?>
                <?php foreach ($shifts as $shift): ?>
                <div class="shift-card" data-id="<?php echo $shift['id']; ?>">
                    <div class="shift-header">
                        <div class="shift-title">
                            <div class="shift-icon">
                                <i class="fas fa-cash-register"></i>
                            </div>
                            <div class="shift-info">
                                <h3>Смена #<?php echo $shift['kkm_shift_number']; ?></h3>
                                <div style="color: #64748b; font-size: 14px;">
                                    <?php echo htmlspecialchars($shift['division_name']); ?>
                                </div>
                            </div>
                        </div>
                        <div class="shift-badge <?php echo $shift['status'] === 'open' ? 'badge-open' : 'badge-closed'; ?>">
                            <i class="fas fa-<?php echo $shift['status'] === 'open' ? 'lock-open' : 'lock'; ?>"></i>
                            <?php echo $shift['status'] === 'open' ? 'Открыта' : 'Закрыта'; ?>
                        </div>
                    </div>
                    
                    <div class="shift-time">
                        <div class="time-label">
                            <i class="fas fa-clock"></i> Время работы
                        </div>
                        <div class="time-value">
                            <span><?php echo date('d.m.Y H:i', strtotime($shift['opened_at'])); ?></span>
                            <?php if ($shift['closed_at']): ?>
                            <span>— <?php echo date('H:i', strtotime($shift['closed_at'])); ?></span>
                            <?php endif; ?>
                        </div>
                    </div>
                    
                    <div class="cashier-info">
                        <i class="fas fa-user"></i>
                        <div class="cashier-content">
                            <div class="cashier-label">Кассир</div>
                            <div class="cashier-name"><?php echo htmlspecialchars($shift['cashier_name']); ?></div>
                        </div>
                    </div>
                    
                    <div class="shift-stats">
                        <div class="stat-item">
                            <div class="stat-value-small"><?php echo $shift['check_count']; ?></div>
                            <div class="stat-label">Чеков</div>
                        </div>
                        <div class="stat-item">
                            <div class="stat-value-small"><?php echo number_format($shift['total_amount'] ?? 0, 2); ?> ₽</div>
                            <div class="stat-label">Общая сумма</div>
                        </div>
                        <div class="stat-item">
                            <div class="stat-value-small"><?php echo number_format($shift['cash_amount'] ?? 0, 2); ?> ₽</div>
                            <div class="stat-label">Наличные</div>
                        </div>
                        <div class="stat-item">
                            <div class="stat-value-small"><?php echo number_format($shift['card_amount'] ?? 0, 2); ?> ₽</div>
                            <div class="stat-label">Карта</div>
                        </div>
                    </div>
                    
                    <div class="shift-actions">
                        <button class="btn btn-primary btn-sm" onclick="viewShiftDetails(<?php echo $shift['id']; ?>)">
                            <i class="fas fa-eye"></i> Детали
                        </button>
                        <button class="btn btn-info btn-sm" onclick="viewShiftChecks(<?php echo $shift['id']; ?>)">
                            <i class="fas fa-receipt"></i> Чеки
                        </button>
                        <?php if ($_SESSION['permission_group'] === 'admin'): ?>
                        <?php endif; ?>
                    </div>
                </div>
                <?php endforeach; ?>
            <?php else: ?>
                <div class="empty-state">
                    <i class="fas fa-history"></i>
                    <h3>Смены не найдены</h3>
                    <p>
                        <?php echo $selected_division_id ? 'На выбранном подразделении еще не было смен' : 'Выберите подразделение для отображения смен'; ?>
                    </p>
                </div>
            <?php endif; ?>
        </div>
    </div>
    
    <!-- Модальное окно деталей смены -->
    <div id="shiftDetailsModal" class="modal-overlay">
        <div class="modal-content">
            <div class="modal-header">
                <h2><i class="fas fa-info-circle"></i> Детали смены</h2>
                <button class="close-modal" onclick="closeShiftDetailsModal()">&times;</button>
            </div>
            <div id="shiftDetailsContent" style="max-height: 70vh; overflow-y: auto;">
                <!-- Контент будет загружен динамически -->
            </div>
        </div>
    </div>
    
    <script>
        // Экспорт смен
        function exportShifts(format) {
            const params = new URLSearchParams(window.location.search);
            params.set('export', format);
            
            window.open('export_shifts.php?' + params.toString(), '_blank');
        }
        
        // Печать отчета
        function printReport() {
            window.print();
        }
        
        // Просмотр деталей смены
        function viewShiftDetails(shiftId) {
            const modal = document.getElementById('shiftDetailsModal');
            const content = document.getElementById('shiftDetailsContent');
            
            content.innerHTML = `
                <div style="text-align: center; padding: 40px;">
                    <i class="fas fa-spinner fa-spin fa-2x"></i>
                    <p>Загрузка деталей смены...</p>
                </div>
            `;
            
            modal.style.display = 'flex';
            
            // Загрузка данных
            fetch(`get_shift_details.php?id=${shiftId}`)
                .then(response => response.text())
                .then(html => {
                    content.innerHTML = html;
                })
                .catch(error => {
                    content.innerHTML = `
                        <div class="alert alert-danger">
                            <i class="fas fa-exclamation-circle"></i> Ошибка загрузки: ${error.message}
                        </div>
                    `;
                });
        }
        
        // Закрыть модальное окно
        function closeShiftDetailsModal() {
            document.getElementById('shiftDetailsModal').style.display = 'none';
        }
        
        // Просмотр чеков смены
        function viewShiftChecks(shiftId) {
            window.open(`shift_checks.php?shift_id=${shiftId}`, '_blank');
        }
        
        // Принудительное закрытие смены
        function forceCloseShift(shiftId) {
            if (!confirm('Принудительно закрыть смену? Это действие нельзя отменить.')) {
                return;
            }
            
            fetch('force_close_shift.php', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ shift_id: shiftId })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Смена принудительно закрыта');
                    location.reload();
                } else {
                    alert('Ошибка: ' + (data.error || 'Неизвестная ошибка'));
                }
            })
            .catch(error => {
                alert('Ошибка сети: ' + error.message);
            });
        }
        
        // Автообновление данных каждые 60 секунд
        setInterval(() => {
            // Обновляем только если страница видима
            if (document.visibilityState === 'visible') {
                // Можно добавить обновление статистики без перезагрузки страницы
                console.log('Автообновление данных мониторинга...');
            }
        }, 60000);
        
        // Анимация появления элементов
        document.addEventListener('DOMContentLoaded', function() {
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('fade-in-up');
                    }
                });
            }, { threshold: 0.1 });
            
            document.querySelectorAll('.stat-card, .filter-bar, .export-bar, .shift-card').forEach(el => {
                observer.observe(el);
            });
        });
        
        // Автообновление страницы каждые 5 минут
        setTimeout(() => {
            if (document.visibilityState === 'visible') {
                window.location.reload();
            }
        }, 300000);
    </script>
</body>
</html>