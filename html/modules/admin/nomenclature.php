<?php
error_reporting(E_ALL);
ini_set('display_errors', 1);
require_once '../../includes/auth_check.php';

if ($_SESSION['permission_group'] !== 'admin') {
    header('Location: ../../index.php');
    exit();
}

// Поиск и фильтрация
$search = $_GET['search'] ?? '';
$page = max(1, intval($_GET['page'] ?? 1));
$limit = 50;
$offset = ($page - 1) * $limit;

// Получаем номенклатуры
$where = '';
$params = [];

if ($search) {
    // Используем LOWER для регистронезависимого поиска
    $where = "WHERE LOWER(n.name) LIKE LOWER(?) OR LOWER(n.barcode) LIKE LOWER(?)";
    $params = ["%{$search}%", "%{$search}%"];
}

// Исправленный запрос с регистронезависимым поиском
$query = "
    SELECT n.*, COUNT(wi.id) as usage_count
    FROM nomenclatures n
    LEFT JOIN warehouse_items wi ON n.id = wi.nomenclature_id
    {$where}
    GROUP BY n.id
    ORDER BY n.created_at DESC
    LIMIT ? OFFSET ?
";

// Подготавливаем запрос
$stmt = $pdo->prepare($query);

// Привязываем параметры поиска, если есть
if ($search) {
    $stmt->bindValue(1, $params[0], PDO::PARAM_STR);
    $stmt->bindValue(2, $params[1], PDO::PARAM_STR);
    $stmt->bindValue(3, $limit, PDO::PARAM_INT);
    $stmt->bindValue(4, $offset, PDO::PARAM_INT);
} else {
    // Без поиска
    $stmt->bindValue(1, $limit, PDO::PARAM_INT);
    $stmt->bindValue(2, $offset, PDO::PARAM_INT);
}

$stmt->execute();
$nomenclatures = $stmt->fetchAll();

// Общее количество для пагинации
$count_query = "SELECT COUNT(*) as total FROM nomenclatures n {$where}";
$stmt = $pdo->prepare($count_query);
if ($search) {
    $stmt->execute($params);
} else {
    $stmt->execute();
}
$total = $stmt->fetch()['total'];
$totalPages = ceil($total / $limit);

// Обработка действий
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    if (isset($_POST['action'])) {
        switch ($_POST['action']) {
            case 'create':
                createNomenclature();
                break;
            case 'update':
                updateNomenclature();
                break;
            case 'delete':
                deleteNomenclature();
                break;
            case 'import':
                importNomenclature();
                break;
        }
    }
}

function createNomenclature() {
    global $pdo;
    
    $name = trim($_POST['name']);
    $barcode = trim($_POST['barcode']);
    $description = trim($_POST['description'] ?? '');
    
    if (empty($name) || empty($barcode)) {
        $_SESSION['error'] = 'Заполните обязательные поля';
        return;
    }
    
    // Проверяем уникальность штрих-кода (регистронезависимо)
    $stmt = $pdo->prepare("SELECT id FROM nomenclatures WHERE LOWER(barcode) = LOWER(?)");
    $stmt->execute([$barcode]);
    
    if ($stmt->fetch()) {
        $_SESSION['error'] = 'Штрих-код уже используется';
        return;
    }
    
    try {
        $stmt = $pdo->prepare("
            INSERT INTO nomenclatures (name, barcode, description)
            VALUES (?, ?, ?)
        ");
        $stmt->execute([$name, $barcode, $description]);
        
        $_SESSION['success'] = 'Номенклатура создана';
        
    } catch (Exception $e) {
        $_SESSION['error'] = 'Ошибка: ' . $e->getMessage();
    }
}

function updateNomenclature() {
    global $pdo;
    
    $id = intval($_POST['id']);
    $name = trim($_POST['name']);
    $barcode = trim($_POST['barcode']);
    $description = trim($_POST['description'] ?? '');
    
    // Проверяем уникальность штрих-кода (регистронезависимо, кроме текущей записи)
    $stmt = $pdo->prepare("SELECT id FROM nomenclatures WHERE LOWER(barcode) = LOWER(?) AND id != ?");
    $stmt->execute([$barcode, $id]);
    
    if ($stmt->fetch()) {
        $_SESSION['error'] = 'Штрих-код уже используется';
        return;
    }
    
    try {
        $stmt = $pdo->prepare("
            UPDATE nomenclatures 
            SET name = ?, barcode = ?, description = ?
            WHERE id = ?
        ");
        $stmt->execute([$name, $barcode, $description, $id]);
        
        $_SESSION['success'] = 'Номенклатура обновлена';
        
    } catch (Exception $e) {
        $_SESSION['error'] = 'Ошибка: ' . $e->getMessage();
    }
}

function deleteNomenclature() {
    global $pdo;
    
    $id = intval($_POST['id']);
    
    // Проверяем использование
    $stmt = $pdo->prepare("
        SELECT COUNT(*) as count 
        FROM warehouse_items 
        WHERE nomenclature_id = ?
    ");
    $stmt->execute([$id]);
    $usage = $stmt->fetch();
    
    if ($usage['count'] > 0) {
        $_SESSION['error'] = 'Номенклатура используется на складах';
        return;
    }
    
    try {
        $stmt = $pdo->prepare("DELETE FROM nomenclatures WHERE id = ?");
        $stmt->execute([$id]);
        
        $_SESSION['success'] = 'Номенклатура удалена';
        
    } catch (Exception $e) {
        $_SESSION['error'] = 'Ошибка: ' . $e->getMessage();
    }
}

// Статистика для отображения
$stmt = $pdo->prepare("SELECT COUNT(DISTINCT nomenclature_id) as count FROM warehouse_items");
$stmt->execute();
$used_count = $stmt->fetch()['count'];
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Номенклатура - RunaRMK</title>
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
        
        .stat-card:nth-child(1)::before {
            background: linear-gradient(90deg, var(--primary), var(--info));
        }
        
        .stat-card:nth-child(2)::before {
            background: linear-gradient(90deg, var(--secondary), #27ae60);
        }
        
        .stat-card:nth-child(3)::before {
            background: linear-gradient(90deg, var(--warning), #e67e22);
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
        
        .stat-icon.primary {
            background: linear-gradient(135deg, #f0f7ff 0%, #d4e4ff 100%);
            color: var(--primary);
        }
        
        .stat-icon.secondary {
            background: linear-gradient(135deg, #f0fff4 0%, #dcffe4 100%);
            color: var(--secondary);
        }
        
        .stat-icon.warning {
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
        
        .search-bar {
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
        
        .search-input-wrapper {
            flex: 1;
            min-width: 300px;
            position: relative;
        }
        
        .search-input {
            width: 100%;
            padding: 18px 25px 18px 55px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 16px;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.3s;
        }
        
        .search-input:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .search-icon {
            position: absolute;
            left: 25px;
            top: 50%;
            transform: translateY(-50%);
            color: #64748b;
            font-size: 18px;
        }
        
        .nomenclature-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(500px, 1fr));
            gap: 25px;
        }
        
        .nomenclature-card {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid transparent;
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            position: relative;
            overflow: hidden;
        }
        
        .nomenclature-card:hover {
            transform: translateY(-8px);
            box-shadow: var(--shadow-lg);
            border-color: var(--primary);
        }
        
        .nomenclature-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--primary), var(--info));
        }
        
        .nomenclature-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 25px;
        }
        
        .nomenclature-title {
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .nomenclature-icon {
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
        
        .nomenclature-info h3 {
            color: var(--dark);
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 8px;
            line-height: 1.4;
        }
        
        .nomenclature-badge {
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
        
        .badge-usage {
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.15) 0%, rgba(46, 204, 113, 0.05) 100%);
            color: var(--secondary);
            border: 2px solid rgba(46, 204, 113, 0.3);
        }
        
        .badge-unused {
            background: linear-gradient(135deg, rgba(149, 165, 166, 0.15) 0%, rgba(149, 165, 166, 0.05) 100%);
            color: var(--gray);
            border: 2px solid rgba(149, 165, 166, 0.3);
        }
        
        .barcode-display {
            background: #f8fafc;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 25px;
            border: 2px solid #e2e8f0;
        }
        
        .barcode-label {
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
        
        .barcode-value {
            font-family: 'Roboto Mono', monospace;
            font-size: 20px;
            font-weight: 700;
            color: var(--dark);
            letter-spacing: 2px;
        }
        
        .nomenclature-details {
            background: white;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 25px;
            border: 1px solid rgba(0,0,0,0.05);
        }
        
        .detail-item {
            display: flex;
            gap: 12px;
            margin-bottom: 15px;
        }
        
        .detail-item:last-child {
            margin-bottom: 0;
        }
        
        .detail-item i {
            color: var(--primary);
            width: 24px;
            text-align: center;
            font-size: 16px;
        }
        
        .detail-content {
            flex: 1;
        }
        
        .detail-label {
            color: #64748b;
            font-size: 13px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 5px;
        }
        
        .detail-text {
            color: var(--dark);
            font-size: 15px;
            line-height: 1.5;
        }
        
        .nomenclature-actions {
            display: flex;
            gap: 12px;
            margin-top: 20px;
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
        
        .pagination {
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 10px;
            margin-top: 40px;
        }
        
        .page-link {
            padding: 12px 20px;
            border: 2px solid #e2e8f0;
            border-radius: 10px;
            text-decoration: none;
            color: var(--primary);
            font-weight: 600;
            font-size: 15px;
            transition: all 0.3s;
            min-width: 50px;
            text-align: center;
        }
        
        .page-link:hover {
            background: var(--light-blue);
            border-color: var(--primary);
            transform: translateY(-2px);
        }
        
        .page-link.active {
            background: var(--primary);
            color: white;
            border-color: var(--primary);
        }
        
        .page-link.disabled {
            color: #cbd5e1;
            border-color: #e2e8f0;
            cursor: not-allowed;
            transform: none;
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
        
        .form-group {
            margin-bottom: 25px;
        }
        
        .form-group label {
            display: block;
            color: var(--dark);
            font-weight: 600;
            margin-bottom: 10px;
            font-size: 15px;
        }
        
        .form-control {
            width: 100%;
            padding: 16px 20px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 16px;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.3s;
        }
        
        .form-control:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        textarea.form-control {
            min-height: 120px;
            resize: vertical;
        }
        
        .modal-actions {
            display: flex;
            gap: 12px;
            margin-top: 30px;
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
            
            .nomenclature-grid {
                grid-template-columns: repeat(auto-fill, minmax(450px, 1fr));
            }
        }
        
        @media (max-width: 992px) {
            .header {
                flex-direction: column;
                gap: 20px;
                text-align: center;
                padding: 25px 20px;
            }
            
            .nomenclature-grid {
                grid-template-columns: 1fr;
            }
            
            .stats-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .search-bar {
                flex-direction: column;
                align-items: stretch;
            }
            
            .search-input-wrapper {
                min-width: auto;
            }
        }
        
        @media (max-width: 768px) {
            .nomenclature-header {
                flex-direction: column;
                align-items: stretch;
                gap: 15px;
            }
            
            .nomenclature-title {
                flex-direction: column;
                align-items: flex-start;
                gap: 10px;
            }
            
            .nomenclature-badge {
                align-self: flex-start;
            }
            
            .nomenclature-actions {
                flex-wrap: wrap;
            }
            
            .stats-grid {
                grid-template-columns: 1fr;
            }
            
            .modal-actions {
                flex-direction: column;
            }
        }
        
        @media (max-width: 576px) {
            .container {
                padding: 0 15px 30px;
            }
            
            .page-link {
                padding: 10px 15px;
                min-width: 40px;
            }
            
            .barcode-value {
                font-size: 16px;
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
            <i class="fas fa-list-alt"></i>
            Управление номенклатурой
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
            <div class="stat-card fade-in-up" style="animation-delay: 0.1s;">
                <div class="stat-icon primary">
                    <i class="fas fa-box"></i>
                </div>
                <h3>Всего номенклатур</h3>
                <div class="stat-value"><?php echo $total; ?></div>
                <div style="color: #64748b; font-size: 14px;">Создано в системе</div>
            </div>
            
            <div class="stat-card fade-in-up" style="animation-delay: 0.2s;">
                <div class="stat-icon secondary">
                    <i class="fas fa-warehouse"></i>
                </div>
                <h3>Используется на складах</h3>
                <div class="stat-value"><?php echo $used_count; ?></div>
                <div style="color: #64748b; font-size: 14px;">Активные позиции</div>
            </div>
            
        </div>
        
        <!-- Поиск и панель управления -->
        <div class="search-bar fade-in-up" style="animation-delay: 0.4s;">
            <form method="GET" style="flex: 1; min-width: 300px;">
                <div class="search-input-wrapper">
                    <i class="fas fa-search search-icon"></i>
                    <input type="text" 
                           name="search" 
                           class="search-input" 
                           placeholder="Поиск по названию или штрих-коду..."
                           value="<?php echo htmlspecialchars($search); ?>">
                </div>
            </form>
            
            <div style="display: flex; gap: 15px;">
                <button class="btn btn-primary" onclick="openCreateModal()" id="createBtn">
                    <i class="fas fa-plus"></i> Добавить
                </button>
                <?php if ($search): ?>
                <a href="?" class="btn btn-info">
                    <i class="fas fa-times"></i> Сбросить
                </a>
                <?php endif; ?>
            </div>
        </div>
        
        <!-- Сетка номенклатур -->
        <div class="nomenclature-grid fade-in-up" style="animation-delay: 0.5s;">
            <?php if (count($nomenclatures) > 0): ?>
                <?php foreach ($nomenclatures as $item): ?>
                <div class="nomenclature-card" data-id="<?php echo $item['id']; ?>">
                    <div class="nomenclature-header">
                        <div class="nomenclature-title">
                            <div class="nomenclature-icon">
                                <i class="fas fa-box"></i>
                            </div>
                            <div class="nomenclature-info">
                                <h3><?php echo htmlspecialchars($item['name']); ?></h3>
                                <div style="color: #64748b; font-size: 14px;">
                                    ID: <?php echo $item['id']; ?>
                                </div>
                            </div>
                        </div>
                        <div class="nomenclature-badge <?php echo $item['usage_count'] > 0 ? 'badge-usage' : 'badge-unused'; ?>">
                            <i class="fas fa-<?php echo $item['usage_count'] > 0 ? 'check-circle' : 'times-circle'; ?>"></i>
                            <?php echo $item['usage_count'] > 0 ? 'Используется' : 'Не используется'; ?>
                        </div>
                    </div>
                    
                    <div class="barcode-display">
                        <div class="barcode-label">
                            <i class="fas fa-barcode"></i> Штрих-код
                        </div>
                        <div class="barcode-value"><?php echo htmlspecialchars($item['barcode']); ?></div>
                    </div>
                    
                    <?php if (!empty($item['description'])): ?>
                    <div class="nomenclature-details">
                        <div class="detail-item">
                            <i class="fas fa-align-left"></i>
                            <div class="detail-content">
                                <div class="detail-label">Описание</div>
                                <div class="detail-text"><?php echo htmlspecialchars($item['description']); ?></div>
                            </div>
                        </div>
                    </div>
                    <?php endif; ?>
                    
                    <div class="detail-item">
                        <i class="fas fa-history"></i>
                        <div class="detail-content">
                            <div class="detail-label">Дата создания</div>
                            <div class="detail-text"><?php echo date('d.m.Y H:i', strtotime($item['created_at'])); ?></div>
                        </div>
                    </div>
                    
                    <div class="detail-item">
                        <i class="fas fa-warehouse"></i>
                        <div class="detail-content">
                            <div class="detail-label">Использование</div>
                            <div class="detail-text">
                                <?php echo $item['usage_count']; ?> раз на складах
                            </div>
                        </div>
                    </div>
                    
                    <div class="nomenclature-actions">
                        <button class="btn btn-primary btn-sm" onclick="editItem(<?php echo $item['id']; ?>, '<?php echo addslashes($item['name']); ?>', '<?php echo addslashes($item['barcode']); ?>', '<?php echo addslashes($item['description'] ?? ''); ?>')">
                            <i class="fas fa-edit"></i> Редактировать
                        </button>
                        <button class="btn btn-info btn-sm" onclick="viewItemDetails(<?php echo $item['id']; ?>)">
                            <i class="fas fa-info-circle"></i> Подробнее
                        </button>
                        <button class="btn btn-danger btn-sm" onclick="deleteItem(<?php echo $item['id']; ?>, '<?php echo addslashes($item['name']); ?>')">
                            <i class="fas fa-trash"></i> Удалить
                        </button>
                    </div>
                </div>
                <?php endforeach; ?>
            <?php else: ?>
                <div class="empty-state">
                    <i class="fas fa-box-open"></i>
                    <h3>Номенклатура не найдена</h3>
                    <p>
                        <?php echo $search ? 'Попробуйте изменить параметры поиска' : 'Добавьте первую номенклатуру для начала работы с товарами'; ?>
                    </p>
                    <button class="btn btn-secondary" onclick="openCreateModal()">
                        <i class="fas fa-plus"></i> Добавить номенклатуру
                    </button>
                </div>
            <?php endif; ?>
        </div>
        
        <!-- Пагинация -->
        <?php if ($totalPages > 1): ?>
        <div class="pagination fade-in-up" style="animation-delay: 0.6s;">
            <?php if ($page > 1): ?>
            <a href="?page=<?php echo $page-1; ?>&search=<?php echo urlencode($search); ?>" class="page-link">
                <i class="fas fa-chevron-left"></i>
            </a>
            <?php else: ?>
            <span class="page-link disabled">
                <i class="fas fa-chevron-left"></i>
            </span>
            <?php endif; ?>
            
            <?php 
                $start = max(1, $page - 2);
                $end = min($totalPages, $page + 2);
                
                if ($start > 1) {
                    echo '<a href="?page=1&search=' . urlencode($search) . '" class="page-link">1</a>';
                    if ($start > 2) echo '<span class="page-link disabled">...</span>';
                }
                
                for ($i = $start; $i <= $end; $i++):
            ?>
                <?php if ($i == $page): ?>
                <span class="page-link active"><?php echo $i; ?></span>
                <?php else: ?>
                <a href="?page=<?php echo $i; ?>&search=<?php echo urlencode($search); ?>" class="page-link">
                    <?php echo $i; ?>
                </a>
                <?php endif; ?>
            <?php endfor; ?>
            
            <?php if ($end < $totalPages): ?>
                <?php if ($end < $totalPages - 1) echo '<span class="page-link disabled">...</span>'; ?>
                <a href="?page=<?php echo $totalPages; ?>&search=<?php echo urlencode($search); ?>" class="page-link">
                    <?php echo $totalPages; ?>
                </a>
            <?php endif; ?>
            
            <?php if ($page < $totalPages): ?>
            <a href="?page=<?php echo $page+1; ?>&search=<?php echo urlencode($search); ?>" class="page-link">
                <i class="fas fa-chevron-right"></i>
            </a>
            <?php else: ?>
            <span class="page-link disabled">
                <i class="fas fa-chevron-right"></i>
            </span>
            <?php endif; ?>
        </div>
        <?php endif; ?>
    </div>
    
    <!-- Модальное окно создания -->
    <div id="createModal" class="modal-overlay">
        <div class="modal-content">
            <div class="modal-header">
                <h2><i class="fas fa-plus-circle"></i> Создать номенклатуру</h2>
                <button class="close-modal" onclick="closeCreateModal()">&times;</button>
            </div>
            
            <form method="POST" id="createForm">
                <input type="hidden" name="action" value="create">
                
                <div class="form-group">
                    <label><i class="fas fa-signature"></i> Название номенклатуры *</label>
                    <input type="text" name="name" class="form-control" required 
                           placeholder="Например: Кока-Кола 0.5л">
                </div>
                
                <div class="form-group">
                    <label><i class="fas fa-barcode"></i> Штрих-код *</label>
                    <input type="text" name="barcode" class="form-control" required 
                           placeholder="Штрих-код товара">
                </div>
                
                <div class="form-group">
                    <label><i class="fas fa-align-left"></i> Описание (необязательно)</label>
                    <textarea name="description" class="form-control" 
                              placeholder="Дополнительное описание товара"></textarea>
                </div>
                
                <div class="modal-actions">
                    <button type="button" class="btn btn-primary" onclick="closeCreateModal()">
                        Отмена
                    </button>
                    <button type="submit" class="btn btn-secondary">
                        <i class="fas fa-save"></i> Создать
                    </button>
                </div>
            </form>
        </div>
    </div>
    
    <!-- Модальное окно редактирования -->
    <div id="editModal" class="modal-overlay">
        <div class="modal-content">
            <div class="modal-header">
                <h2><i class="fas fa-edit"></i> Редактировать номенклатуру</h2>
                <button class="close-modal" onclick="closeEditModal()">&times;</button>
            </div>
            
            <form method="POST" id="editForm">
                <input type="hidden" name="action" value="update">
                <input type="hidden" name="id" id="editId">
                
                <div class="form-group">
                    <label>Название номенклатуры *</label>
                    <input type="text" name="name" id="editName" class="form-control" required>
                </div>
                
                <div class="form-group">
                    <label>Штрих-код *</label>
                    <input type="text" name="barcode" id="editBarcode" class="form-control" required>
                </div>
                
                <div class="form-group">
                    <label>Описание</label>
                    <textarea name="description" id="editDescription" class="form-control"></textarea>
                </div>
                
                <div class="modal-actions">
                    <button type="button" class="btn btn-primary" onclick="closeEditModal()">
                        Отмена
                    </button>
                    <button type="submit" class="btn btn-secondary">
                        <i class="fas fa-save"></i> Сохранить
                    </button>
                </div>
            </form>
        </div>
    </div>
    
    <script>
        // Модальные окна
        function openCreateModal() {
            document.getElementById('createModal').style.display = 'flex';
            document.querySelector('#createForm input[name="name"]').focus();
            
            const createBtn = document.getElementById('createBtn');
            createBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Открытие...';
            createBtn.disabled = true;
            
            setTimeout(() => {
                createBtn.innerHTML = '<i class="fas fa-plus"></i> Добавить';
                createBtn.disabled = false;
            }, 500);
        }
        
        function closeCreateModal() {
            document.getElementById('createModal').style.display = 'none';
            document.getElementById('createForm').reset();
        }
        
        function openEditModal() {
            document.getElementById('editModal').style.display = 'flex';
            document.querySelector('#editForm input[name="name"]').focus();
        }
        
        function closeEditModal() {
            document.getElementById('editModal').style.display = 'none';
        }
        
        // Редактирование
        function editItem(id, name, barcode, description) {
            document.getElementById('editId').value = id;
            document.getElementById('editName').value = name;
            document.getElementById('editBarcode').value = barcode;
            document.getElementById('editDescription').value = description || '';
            openEditModal();
        }
        
        // Просмотр деталей
        function viewItemDetails(id) {
            // В реальном приложении здесь можно сделать AJAX запрос для получения полной информации
            alert('Функция просмотра деталей номенклатуры. ID: ' + id + '\n\nВ реальной системе здесь будет отображена полная статистика использования товара.');
        }
        
        // Удаление
        function deleteItem(id, name) {
            if (!confirm(`Удалить номенклатуру "${name}"?\n\nВнимание: если товар используется на складах, удаление будет невозможно.`)) {
                return;
            }
            
            const form = document.createElement('form');
            form.method = 'POST';
            form.style.display = 'none';
            
            const actionInput = document.createElement('input');
            actionInput.type = 'hidden';
            actionInput.name = 'action';
            actionInput.value = 'delete';
            form.appendChild(actionInput);
            
            const idInput = document.createElement('input');
            idInput.type = 'hidden';
            idInput.name = 'id';
            idInput.value = id;
            form.appendChild(idInput);
            
            document.body.appendChild(form);
            form.submit();
        }
        
        // Закрытие по клику вне
        document.querySelectorAll('.modal-overlay').forEach(modal => {
            modal.addEventListener('click', function(e) {
                if (e.target === this) {
                    if (this.id === 'createModal') {
                        closeCreateModal();
                    } else if (this.id === 'editModal') {
                        closeEditModal();
                    }
                }
            });
        });
        
        // Автопоиск при вводе
        let searchTimeout;
        document.querySelector('input[name="search"]')?.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                this.form.submit();
            }, 500);
        });
        
        // Анимация появления элементов
        document.addEventListener('DOMContentLoaded', function() {
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('fade-in-up');
                    }
                });
            }, { threshold: 0.1 });
            
            document.querySelectorAll('.stat-card, .search-bar, .nomenclature-card').forEach(el => {
                observer.observe(el);
            });
            
            // Обработка отправки формы создания
            document.getElementById('createForm').addEventListener('submit', function() {
                const submitBtn = this.querySelector('button[type="submit"]');
                const originalHTML = submitBtn.innerHTML;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Создание...';
                submitBtn.disabled = true;
            });
            
            // Обработка отправки формы редактирования
            document.getElementById('editForm').addEventListener('submit', function() {
                const submitBtn = this.querySelector('button[type="submit"]');
                const originalHTML = submitBtn.innerHTML;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Сохранение...';
                submitBtn.disabled = true;
            });
        });
    </script>
</body>
</html>