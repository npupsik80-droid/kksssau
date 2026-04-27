<?php
require_once '../../includes/auth_check.php';

// Проверяем, что есть текущее подразделение
if (!isset($_SESSION['current_division_id'])) {
    header('Location: ../../select_division.php');
    exit();
}

// Получаем текущую открытую смену для подразделения
$stmt = $pdo->prepare("
    SELECT 
        s.*,
        u.full_name as cashier_name,
        d.name as division_name
    FROM shifts s
    LEFT JOIN users u ON s.user_id = u.id
    LEFT JOIN divisions d ON s.division_id = d.id
    WHERE s.division_id = ? 
    AND s.status = 'open'
    AND s.user_id = ?
    ORDER BY s.opened_at DESC 
    LIMIT 1
");
$stmt->execute([$_SESSION['current_division_id'], $_SESSION['user_id']]);
$shift = $stmt->fetch();

if (!$shift) {
    $_SESSION['error'] = 'Нет открытой смены';
    header('Location: ../../../index.php');
    exit();
}

// Получаем чеки для этой смены
$stmt = $pdo->prepare("
    SELECT 
        c.*
    FROM checks c
    WHERE c.shift_id = ?
    ORDER BY c.created_at DESC
");
$stmt->execute([$shift['id']]);
$checks = $stmt->fetchAll();

// Обрабатываем JSON данные о товарах и добавляем дополнительную информацию
$unique_checks = [];
$check_ids_processed = [];

foreach ($checks as $check) {
    // Проверяем, не обрабатывали ли мы уже этот чек (защита от дубликатов)
    if (in_array($check['id'], $check_ids_processed)) {
        continue;
    }
    
    $check['item_names'] = [];
    $check['item_count'] = 0;
    $check['items_array'] = []; // Для мобильного отображения
    
    if (!empty($check['items'])) {
        // Пробуем декодировать JSON
        $items_data = json_decode($check['items'], true);
        
        // Проверяем, удалось ли декодировать
        if (json_last_error() !== JSON_ERROR_NONE) {
            // Если JSON некорректен, пробуем исправить
            $fixed_json = fix_json($check['items']);
            $items_data = json_decode($fixed_json, true);
            
            if (json_last_error() !== JSON_ERROR_NONE) {
                // Если все равно не удается, логируем ошибку
                error_log("Невозможно декодировать JSON для чека ID: " . $check['id'] . ". JSON: " . $check['items']);
                $items_data = [];
            }
        }
        
        // Обрабатываем данные о товарах
        if (is_array($items_data) && !empty($items_data)) {
            // Проверяем структуру данных
            if (isset($items_data['items']) && is_array($items_data['items'])) {
                // Структура: {"items": [...]}
                $items = $items_data['items'];
            } else if (isset($items_data[0]) && is_array($items_data[0])) {
                // Структура: [ {...}, {...} ]
                $items = $items_data;
            } else if (isset($items_data['name']) || isset($items_data['title'])) {
                // Структура: {...} (один товар)
                $items = [$items_data];
            } else {
                // Неизвестная структура
                $items = [];
            }
            
            // Обрабатываем товары
            foreach ($items as $item) {
                if (is_array($item) || is_object($item)) {
                    $item = (array)$item;
                    $name = $item['name'] ?? $item['title'] ?? 'Без названия';
                    $quantity = isset($item['quantity']) ? (int)$item['quantity'] : 1;
                    
                    $check['item_names'][] = $name;
                    $check['item_count'] += $quantity;
                    
                    $check['items_array'][] = [
                        'name' => $name,
                        'quantity' => $quantity,
                        'price' => $item['price'] ?? 0,
                        'total' => $item['total'] ?? 0
                    ];
                }
            }
        }
    }
    
    $unique_checks[] = $check;
    $check_ids_processed[] = $check['id'];
}

// Функция для исправления JSON
function fix_json($json) {
    // Убираем лишние пробелы и символы
    $json = trim($json);
    
    // Пробуем исправить распространенные ошибки
    // 1. Убираем лишние кавычки в начале/конце
    if (substr($json, 0, 1) === '"' && substr($json, -1) === '"') {
        $json = substr($json, 1, -1);
        $json = stripslashes($json);
    }
    
    // 2. Исправляем экранированные кавычки
    $json = str_replace('\"', '"', $json);
    
    // 3. Проверяем, если это строка, начинающаяся с [ или {
    if (substr($json, 0, 1) !== '[' && substr($json, 0, 1) !== '{') {
        $json = '[' . $json . ']';
    }
    
    return $json;
}

$checks = $unique_checks; // Используем уникальные чеки

// Статистика по чекам
$stats_stmt = $pdo->prepare("
    SELECT 
        COUNT(*) as total_checks,
        SUM(total_amount) as total_amount,
        SUM(cash_amount) as cash_amount,
        SUM(card_amount) as card_amount,
        AVG(total_amount) as avg_check
    FROM checks
    WHERE shift_id = ?
");
$stats_stmt->execute([$shift['id']]);
$check_stats = $stats_stmt->fetch();
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Мои чеки - смена #<?php echo $shift['kkm_shift_number']; ?> - RunaRMK</title>
    <!-- Стили остаются без изменений -->
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
            font-size: 28px;
            font-weight: 800;
            display: flex;
            align-items: center;
            gap: 15px;
            text-shadow: 0 4px 8px rgba(0,0,0,0.2);
        }
        
        .header h1 i {
            background: rgba(255,255,255,0.15);
            padding: 15px;
            border-radius: 15px;
            backdrop-filter: blur(10px);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn {
            padding: 12px 24px;
            border-radius: 50px;
            font-weight: 700;
            font-size: 15px;
            cursor: pointer;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
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
            padding: 8px 16px;
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
        
        .shift-info-card {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            margin-bottom: 30px;
            box-shadow: var(--shadow-sm);
            border-left: 6px solid var(--primary);
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
        }
        
        .shift-info-item {
            display: flex;
            flex-direction: column;
            gap: 8px;
        }
        
        .shift-info-label {
            color: #64748b;
            font-size: 13px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        
        .shift-info-value {
            color: var(--dark);
            font-size: 18px;
            font-weight: 700;
        }
        
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 40px;
        }
        
        .stat-card {
            background: white;
            border-radius: var(--border-radius);
            padding: 25px;
            box-shadow: var(--shadow-sm);
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            border: 2px solid transparent;
            position: relative;
            overflow: hidden;
            display: flex;
            flex-direction: column;
            align-items: center;
            text-align: center;
        }
        
        .stat-card:hover {
            transform: translateY(-5px);
            box-shadow: var(--shadow-md);
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
            width: 60px;
            height: 60px;
            border-radius: 15px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 28px;
            margin-bottom: 20px;
            background: linear-gradient(135deg, rgba(67, 97, 238, 0.15) 0%, rgba(67, 97, 238, 0.05) 100%);
            color: var(--primary);
        }
        
        .stat-value {
            font-size: 32px;
            font-weight: 800;
            color: var(--primary);
            margin: 10px 0;
            line-height: 1;
        }
        
        .stat-label {
            color: #64748b;
            font-size: 14px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .filter-bar {
            background: white;
            border-radius: var(--border-radius);
            padding: 20px 25px;
            margin-bottom: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
            display: flex;
            align-items: center;
            justify-content: space-between;
            flex-wrap: wrap;
            gap: 15px;
        }
        
        .search-group {
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
            padding: 12px 18px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 15px;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.3s;
            min-width: 250px;
        }
        
        .form-control:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .checks-table {
            width: 100%;
            background: white;
            border-radius: var(--border-radius);
            overflow: hidden;
            box-shadow: var(--shadow-sm);
            margin-bottom: 30px;
        }
        
        .table-header {
            display: grid;
            grid-template-columns: 100px 100px 2fr 100px 120px 100px 100px 120px;
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
            grid-template-columns: 100px 100px 2fr 100px 120px 100px 100px 120px;
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
        
        .table-cell.items {
            flex-direction: column;
            align-items: flex-start;
            gap: 5px;
            max-height: 100px;
            overflow-y: auto;
        }
        
        .item-name {
            background: #f1f5f9;
            padding: 6px 12px;
            border-radius: 8px;
            font-size: 13px;
            color: #475569;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            max-width: 100%;
        }
        
        .check-type {
            padding: 6px 12px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .type-sale {
            background: rgba(46, 204, 113, 0.15);
            color: var(--secondary);
        }
        
        .type-return {
            background: rgba(231, 76, 60, 0.15);
            color: var(--danger);
        }
        
        .action-buttons {
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
        
        .pagination {
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 10px;
            margin-top: 30px;
        }
        
        .page-btn {
            width: 40px;
            height: 40px;
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: white;
            border: 2px solid #e2e8f0;
            color: var(--dark);
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }
        
        .page-btn:hover {
            border-color: var(--primary);
            color: var(--primary);
            transform: translateY(-2px);
        }
        
        .page-btn.active {
            background: var(--primary);
            color: white;
            border-color: var(--primary);
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
                font-size: 24px;
            }
            
            .table-header,
            .table-row {
                grid-template-columns: 80px 80px 2fr 80px 100px 80px 80px 100px;
            }
        }
        
        @media (max-width: 992px) {
            .header {
                flex-direction: column;
                gap: 20px;
                text-align: center;
                padding: 25px 20px;
            }
            
            .shift-info-card {
                grid-template-columns: 1fr;
            }
            
            .stats-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .filter-bar {
                flex-direction: column;
                align-items: stretch;
            }
            
            .search-group {
                width: 100%;
            }
            
            .form-control {
                width: 100%;
            }
            
            .table-header,
            .table-row {
                grid-template-columns: 1fr;
                display: none;
            }
            
            .mobile-check-card {
                display: block;
                background: white;
                border-radius: var(--border-radius);
                padding: 25px;
                margin-bottom: 20px;
                box-shadow: var(--shadow-sm);
                border-left: 4px solid var(--primary);
            }
            
            .mobile-check-header {
                display: flex;
                justify-content: space-between;
                align-items: flex-start;
                margin-bottom: 15px;
            }
            
            .mobile-check-items {
                background: #f8fafc;
                border-radius: 10px;
                padding: 15px;
                margin-bottom: 15px;
                max-height: 150px;
                overflow-y: auto;
            }
            
            .mobile-item {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 8px 0;
                border-bottom: 1px solid #e2e8f0;
            }
            
            .mobile-item:last-child {
                border-bottom: none;
            }
            
            .mobile-item-name {
                color: var(--dark);
                font-size: 14px;
                flex: 1;
            }
            
            .mobile-item-quantity {
                background: var(--primary);
                color: white;
                font-size: 12px;
                padding: 4px 8px;
                border-radius: 12px;
                font-weight: 600;
                margin-left: 10px;
            }
            
            .mobile-check-info {
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
            
            .mobile-info-time {
                grid-column: span 2;
                text-align: center;
                padding-top: 10px;
                border-top: 1px solid #e2e8f0;
                color: #64748b;
                font-size: 14px;
            }
        }
        
        @media (max-width: 768px) {
            .stats-grid {
                grid-template-columns: 1fr;
            }
            
            .mobile-check-info {
                grid-template-columns: 1fr;
            }
            
            .mobile-info-time {
                grid-column: span 1;
            }
        }
        
        @media (max-width: 576px) {
            .container {
                padding: 0 15px 30px;
            }
            
            .action-buttons {
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
            <i class="fas fa-receipt"></i>
            Мои чеки - смена #<?php echo $shift['kkm_shift_number']; ?>
        </h1>
        <a href="../../index.php" class="btn btn-primary">
            <i class="fas fa-home"></i> На главную
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
        
        <!-- Информация о смене -->
        <div class="shift-info-card fade-in-up">
            <div class="shift-info-item">
                <div class="shift-info-label">
                    <i class="fas fa-building"></i> Подразделение
                </div>
                <div class="shift-info-value"><?php echo htmlspecialchars($shift['division_name']); ?></div>
            </div>
            
            <div class="shift-info-item">
                <div class="shift-info-label">
                    <i class="fas fa-user"></i> Кассир
                </div>
                <div class="shift-info-value"><?php echo htmlspecialchars($shift['cashier_name']); ?></div>
            </div>
            
            <div class="shift-info-item">
                <div class="shift-info-label">
                    <i class="fas fa-clock"></i> Время открытия
                </div>
                <div class="shift-info-value"><?php echo date('d.m.Y H:i:s', strtotime($shift['opened_at'])); ?></div>
            </div>
            
            <?php if ($shift['closed_at']): ?>
            <div class="shift-info-item">
                <div class="shift-info-label">
                    <i class="fas fa-clock"></i> Время закрытия
                </div>
                <div class="shift-info-value"><?php echo date('d.m.Y H:i:s', strtotime($shift['closed_at'])); ?></div>
            </div>
            <?php endif; ?>
            
            <div class="shift-info-item">
                <div class="shift-info-label">
                    <i class="fas fa-cash-register"></i> Статус
                </div>
                <div class="shift-info-value">
                    <span style="color: <?php echo $shift['status'] === 'open' ? 'var(--secondary)' : 'var(--danger)'; ?>; font-weight: 800;">
                        <?php echo $shift['status'] === 'open' ? 'Открыта' : 'Закрыта'; ?>
                    </span>
                </div>
            </div>
        </div>
        
        <!-- Статистика чеков -->
        <div class="stats-grid fade-in-up" style="animation-delay: 0.1s;">
            <div class="stat-card">
                <div class="stat-icon">
                    <i class="fas fa-receipt"></i>
                </div>
                <div class="stat-value"><?php echo $check_stats['total_checks'] ?? 0; ?></div>
                <div class="stat-label">Всего чеков</div>
            </div>
            
            <div class="stat-card">
                <div class="stat-icon">
                    <i class="fas fa-money-bill-wave"></i>
                </div>
                <div class="stat-value"><?php echo number_format($check_stats['total_amount'] ?? 0, 2); ?> ₽</div>
                <div class="stat-label">Общая сумма</div>
            </div>
            
            <div class="stat-card">
                <div class="stat-icon">
                    <i class="fas fa-money-bill"></i>
                </div>
                <div class="stat-value"><?php echo number_format($check_stats['cash_amount'] ?? 0, 2); ?> ₽</div>
                <div class="stat-label">Наличные</div>
            </div>
            
            <div class="stat-card">
                <div class="stat-icon">
                    <i class="fas fa-credit-card"></i>
                </div>
                <div class="stat-value"><?php echo number_format($check_stats['card_amount'] ?? 0, 2); ?> ₽</div>
                <div class="stat-label">Безналичные</div>
            </div>
            
            <div class="stat-card">
                <div class="stat-icon">
                    <i class="fas fa-calculator"></i>
                </div>
                <div class="stat-value"><?php echo number_format($check_stats['avg_check'] ?? 0, 2); ?> ₽</div>
                <div class="stat-label">Средний чек</div>
            </div>
        </div>
        
        <!-- Фильтры и поиск -->
        <div class="filter-bar fade-in-up" style="animation-delay: 0.2s;">
            <div class="search-group">
                <div class="filter-label"><i class="fas fa-search"></i> Поиск:</div>
                <input type="text" id="searchInput" class="form-control" placeholder="По номеру чека, товару...">
            </div>
            
            <div class="search-group">
                <div class="filter-label"><i class="fas fa-filter"></i> Тип чека:</div>
                <select id="typeFilter" class="form-control">
                    <option value="">Все типы</option>
                    <option value="sale">Продажа</option>
                    <option value="return">Возврат</option>
                </select>
            </div>
        </div>
        
        <!-- Список чеков -->
        <div class="fade-in-up" style="animation-delay: 0.3s;">
            <?php if (count($checks) > 0): ?>
                <!-- Десктопная таблица -->
                <div class="checks-table desktop-view">
                    <div class="table-header">
                        <div class="table-header-cell">№</div>
                        <div class="table-header-cell">Тип</div>
                        <div class="table-header-cell">Товары</div>
                        <div class="table-header-cell">Кол-во</div>
                        <div class="table-header-cell">Сумма</div>
                        <div class="table-header-cell">Наличные</div>
                        <div class="table-header-cell">Карта</div>
                        <div class="table-header-cell">Действия</div>
                    </div>
                    
                    <?php foreach ($checks as $index => $check): ?>
                    <div class="table-row">
                        <div class="table-cell">
                            <strong>#<?php echo $check['kkm_check_number']; ?></strong>
                            <div style="font-size: 12px; color: #64748b; margin-top: 5px;">
                                <?php echo date('H:i', strtotime($check['created_at'])); ?>
                            </div>
                        </div>
                        <div class="table-cell">
                            <span class="check-type <?php echo $check['type'] === 'return' ? 'type-return' : 'type-sale'; ?>">
                                <?php echo $check['type'] === 'return' ? 'Возврат' : 'Продажа'; ?>
                            </span>
                        </div>
                        <div class="table-cell items">
                            <?php if (!empty($check['item_names'])): ?>
                                <?php foreach ($check['item_names'] as $itemName): ?>
                                    <div class="item-name" title="<?php echo htmlspecialchars($itemName); ?>">
                                        <?php echo htmlspecialchars(mb_strimwidth($itemName, 0, 40, '...')); ?>
                                    </div>
                                <?php endforeach; ?>
                            <?php else: ?>
                                <div class="item-name">Нет информации о товарах</div>
                            <?php endif; ?>
                        </div>
                        <div class="table-cell">
                            <?php echo $check['item_count']; ?> шт.
                        </div>
                        <div class="table-cell">
                            <strong><?php echo number_format($check['total_amount'], 2); ?> ₽</strong>
                        </div>
                        <div class="table-cell">
                            <?php echo number_format($check['cash_amount'], 2); ?> ₽
                        </div>
                        <div class="table-cell">
                            <?php echo number_format($check['card_amount'], 2); ?> ₽
                        </div>
                        <div class="table-cell action-buttons">
                            <button class="btn btn-primary btn-sm" onclick="viewCheckDetails(<?php echo $check['id']; ?>)">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-info btn-sm" onclick="printCheck(<?php echo $check['id']; ?>)">
                                <i class="fas fa-print"></i>
                            </button>
                        </div>
                    </div>
                    <?php endforeach; ?>
                </div>
                
                <!-- Мобильные карточки -->
                <div class="mobile-view">
                    <?php foreach ($checks as $check): ?>
                    <div class="mobile-check-card">
                        <div class="mobile-check-header">
                            <div>
                                <h3 style="color: var(--dark); margin-bottom: 5px;">
                                    Чек #<?php echo $check['kkm_check_number']; ?>
                                </h3>
                                <span class="check-type <?php echo $check['type'] === 'return' ? 'type-return' : 'type-sale'; ?>">
                                    <?php echo $check['type'] === 'return' ? 'Возврат' : 'Продажа'; ?>
                                </span>
                            </div>
                            <div style="text-align: right;">
                                <div style="color: var(--primary); font-size: 20px; font-weight: 800;">
                                    <?php echo number_format($check['total_amount'], 2); ?> ₽
                                </div>
                                <div style="color: #64748b; font-size: 12px;">
                                    <?php echo date('H:i', strtotime($check['created_at'])); ?>
                                </div>
                            </div>
                        </div>
                        
                        <?php if (!empty($check['items_array'])): ?>
                        <div class="mobile-check-items">
                            <?php foreach ($check['items_array'] as $item): ?>
                            <div class="mobile-item">
                                <div class="mobile-item-name"><?php echo htmlspecialchars(mb_strimwidth($item['name'], 0, 50, '...')); ?></div>
                                <div class="mobile-item-quantity">×<?php echo $item['quantity']; ?></div>
                            </div>
                            <?php endforeach; ?>
                        </div>
                        <?php endif; ?>
                        
                        <div class="mobile-check-info">
                            <div class="mobile-info-item">
                                <div class="mobile-info-label">Товаров</div>
                                <div class="mobile-info-value"><?php echo $check['item_count']; ?> шт.</div>
                            </div>
                            <div class="mobile-info-item">
                                <div class="mobile-info-label">Наличные</div>
                                <div class="mobile-info-value"><?php echo number_format($check['cash_amount'], 2); ?> ₽</div>
                            </div>
                            <div class="mobile-info-item">
                                <div class="mobile-info-label">Карта</div>
                                <div class="mobile-info-value"><?php echo number_format($check['card_amount'], 2); ?> ₽</div>
                            </div>
                            <div class="mobile-info-item mobile-info-time">
                                <div class="mobile-info-label">Время</div>
                                <div class="mobile-info-value"><?php echo date('H:i:s', strtotime($check['created_at'])); ?></div>
                            </div>
                        </div>
                        
                        <div class="action-buttons">
                            <button class="btn btn-primary btn-sm" onclick="viewCheckDetails(<?php echo $check['id']; ?>)">
                                <i class="fas fa-eye"></i> Подробнее
                            </button>
                            <button class="btn btn-info btn-sm" onclick="printCheck(<?php echo $check['id']; ?>)">
                                <i class="fas fa-print"></i> Печать
                            </button>
                        </div>
                    </div>
                    <?php endforeach; ?>
                </div>
                
                <!-- Пагинация -->
                <div class="pagination fade-in-up" style="animation-delay: 0.4s;">
                    <button class="page-btn"><i class="fas fa-chevron-left"></i></button>
                    <button class="page-btn active">1</button>
                    <button class="page-btn">2</button>
                    <button class="page-btn">3</button>
                    <button class="page-btn"><i class="fas fa-chevron-right"></i></button>
                </div>
                
            <?php else: ?>
                <div class="empty-state">
                    <i class="fas fa-receipt"></i>
                    <h3>Чеки не найдены</h3>
                    <p>
                        В этой смене еще не было зарегистрировано ни одного чека.
                    </p>
                </div>
            <?php endif; ?>
        </div>
    </div>
    
    <!-- Модальное окно деталей чека -->
    <div id="checkDetailsModal" class="modal-overlay">
        <div class="modal-content">
            <div class="modal-header">
                <h2><i class="fas fa-receipt"></i> Детали чека</h2>
                <button class="close-modal" onclick="closeCheckDetailsModal()">&times;</button>
            </div>
            <div id="checkDetailsContent" style="max-height: 70vh; overflow-y: auto;">
                <!-- Контент будет загружен динамически -->
            </div>
        </div>
    </div>
    
    <script>
        // Поиск и фильтрация чеков
        document.getElementById('searchInput').addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            filterChecks();
        });
        
        document.getElementById('typeFilter').addEventListener('change', function() {
            filterChecks();
        });
        
        function filterChecks() {
            const searchTerm = document.getElementById('searchInput').value.toLowerCase();
            const typeFilter = document.getElementById('typeFilter').value;
            
            // Для десктопного вида
            document.querySelectorAll('.table-row').forEach(row => {
                const checkNumber = row.querySelector('.table-cell:nth-child(1)').textContent.toLowerCase();
                const checkType = row.querySelector('.check-type').classList.contains('type-return') ? 'return' : 'sale';
                const checkItems = row.querySelector('.table-cell.items').textContent.toLowerCase();
                
                const matchesSearch = checkNumber.includes(searchTerm) || checkItems.includes(searchTerm);
                const matchesType = !typeFilter || checkType === typeFilter;
                
                row.style.display = (matchesSearch && matchesType) ? '' : 'none';
            });
            
            // Для мобильного вида
            document.querySelectorAll('.mobile-check-card').forEach(card => {
                const checkNumber = card.querySelector('h3').textContent.toLowerCase();
                const checkType = card.querySelector('.check-type').classList.contains('type-return') ? 'return' : 'sale';
                const checkItems = card.querySelector('.mobile-check-items')?.textContent.toLowerCase() || '';
                
                const matchesSearch = checkNumber.includes(searchTerm) || checkItems.includes(searchTerm);
                const matchesType = !typeFilter || checkType === typeFilter;
                
                card.style.display = (matchesSearch && matchesType) ? '' : 'none';
            });
        }
        
        // Просмотр деталей чека
        function viewCheckDetails(checkId) {
            const modal = document.getElementById('checkDetailsModal');
            const content = document.getElementById('checkDetailsContent');
            
            content.innerHTML = `
                <div style="text-align: center; padding: 40px;">
                    <i class="fas fa-spinner fa-spin fa-2x"></i>
                    <p>Загрузка деталей чека...</p>
                </div>
            `;
            
            modal.style.display = 'flex';
            
            // Загрузка данных
            fetch(`get_check_details.php?id=${checkId}`)
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
        function closeCheckDetailsModal() {
            document.getElementById('checkDetailsModal').style.display = 'none';
        }
        
        // Печать чека
        function printCheck(checkId) {
            if (confirm('Распечатать копию чека?')) {
                fetch(`print_check.php?id=${checkId}`, {
                    method: 'POST'
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert('Чек отправлен на печать');
                    } else {
                        alert('Ошибка печати: ' + (data.error || 'Неизвестная ошибка'));
                    }
                })
                .catch(error => {
                    alert('Ошибка сети: ' + error.message);
                });
            }
        }
        
        // Анимация появления элементов
        document.addEventListener('DOMContentLoaded', function() {
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('fade-in-up');
                    }
                });
            }, { threshold: 0.1 });
            
            document.querySelectorAll('.shift-info-card, .stats-grid, .filter-bar, .checks-table, .mobile-check-card, .pagination').forEach(el => {
                observer.observe(el);
            });
            
            // Адаптивное отображение таблицы/карточек
            function checkViewMode() {
                const width = window.innerWidth;
                const desktopView = document.querySelector('.desktop-view');
                const mobileView = document.querySelector('.mobile-view');
                
                if (width <= 992) {
                    if (desktopView) desktopView.style.display = 'none';
                    if (mobileView) mobileView.style.display = 'block';
                } else {
                    if (desktopView) desktopView.style.display = 'block';
                    if (mobileView) mobileView.style.display = 'none';
                }
            }
            
            checkViewMode();
            window.addEventListener('resize', checkViewMode);
        });
        
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
    </script>
</body>
</html>