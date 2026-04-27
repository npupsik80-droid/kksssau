<?php
session_start();

// Проверяем выбрано ли подразделение
if (!isset($_SESSION['current_division_id'])) {
    header('Location: ../../select_division.php');
    exit();
}

// Получаем информацию о текущем подразделении
$currentDivisionId = $_SESSION['current_division_id'];
$currentDivisionName = $_SESSION['current_division_name'];
$currentWarehouseId = $_SESSION['current_warehouse_id'];
$currentWarehouseName = $_SESSION['current_warehouse_name'];

// Подключение к базе данных
require_once 'config/database.php';

// Очищаем список товаров для печати при каждом новом входе на страницу
if (!isset($_SESSION['print_session_started'])) {
    $_SESSION['print_items'] = [];
    $_SESSION['print_session_started'] = true;
}

// Получаем товары текущего склада с группировкой по номенклатуре
$items = [];
try {
    $stmt = $pdo->prepare("
        SELECT 
            n.id as nomenclature_id,
            n.name as nomenclature_name,
            n.barcode,
            n.description,
            SUM(wi.quantity) as total_quantity,
            AVG(wi.price) as avg_price,
            MAX(wi.created_at) as last_received
        FROM warehouse_items wi
        JOIN nomenclatures n ON wi.nomenclature_id = n.id
        WHERE wi.warehouse_id = :warehouse_id AND wi.quantity > 0
        GROUP BY n.id
        ORDER BY n.name
    ");
    $stmt->execute(['warehouse_id' => $currentWarehouseId]);
    $items = $stmt->fetchAll(PDO::FETCH_ASSOC);
} catch (PDOException $e) {
    $error = "Ошибка загрузки товаров: " . $e->getMessage();
}

// Получаем все склады текущего подразделения для возможного переключения
$allWarehouses = [];
try {
    $stmt = $pdo->prepare("
        SELECT w.*, d.name as division_name 
        FROM warehouses w 
        LEFT JOIN divisions d ON w.division_id = d.id 
        WHERE w.division_id = :division_id
        ORDER BY w.name
    ");
    $stmt->execute(['division_id' => $currentDivisionId]);
    $allWarehouses = $stmt->fetchAll(PDO::FETCH_ASSOC);
} catch (PDOException $e) {
    // Продолжаем работу даже если ошибка
}

// Обработка добавления товара
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    // Смена склада
    if (isset($_POST['change_warehouse'])) {
        $newWarehouseId = intval($_POST['warehouse_id']);
        
        // Находим выбранный склад
        foreach ($allWarehouses as $warehouse) {
            if ($warehouse['id'] == $newWarehouseId) {
                $_SESSION['current_warehouse_id'] = $warehouse['id'];
                $_SESSION['current_warehouse_name'] = $warehouse['name'];
                $currentWarehouseId = $warehouse['id'];
                $currentWarehouseName = $warehouse['name'];
                
                // Перезагружаем товары нового склада
                $stmt = $pdo->prepare("
                    SELECT 
                        n.id as nomenclature_id,
                        n.name as nomenclature_name,
                        n.barcode,
                        n.description,
                        SUM(wi.quantity) as total_quantity,
                        AVG(wi.price) as avg_price,
                        MAX(wi.created_at) as last_received
                    FROM warehouse_items wi
                    JOIN nomenclatures n ON wi.nomenclature_id = n.id
                    WHERE wi.warehouse_id = :warehouse_id AND wi.quantity > 0
                    GROUP BY n.id
                    ORDER BY n.name
                ");
                $stmt->execute(['warehouse_id' => $currentWarehouseId]);
                $items = $stmt->fetchAll(PDO::FETCH_ASSOC);
                
                $success = "Склад изменен на: " . $warehouse['name'];
                break;
            }
        }
    }
    
    // Добавление по штрихкоду
    if (isset($_POST['add_by_barcode'])) {
        $barcode = trim($_POST['barcode']);
        $quantity = intval($_POST['quantity']);
        
        if (!empty($barcode) && $quantity > 0) {
            try {
                $stmt = $pdo->prepare("
                    SELECT 
                        n.id as nomenclature_id,
                        n.name as nomenclature_name,
                        n.barcode,
                        n.description,
                        SUM(wi.quantity) as total_quantity,
                        AVG(wi.price) as avg_price
                    FROM warehouse_items wi
                    JOIN nomenclatures n ON wi.nomenclature_id = n.id
                    WHERE wi.warehouse_id = :warehouse_id 
                    AND n.barcode = :barcode
                    AND wi.quantity > 0
                    GROUP BY n.id
                    LIMIT 1
                ");
                $stmt->execute([
                    'warehouse_id' => $currentWarehouseId,
                    'barcode' => $barcode
                ]);
                $item = $stmt->fetch(PDO::FETCH_ASSOC);
                
                if ($item) {
                    if (!isset($_SESSION['print_items'])) {
                        $_SESSION['print_items'] = [];
                    }
                    
                    $product_key = $item['nomenclature_id'] . '_' . $item['barcode'];
                    
                    if (isset($_SESSION['print_items'][$product_key])) {
                        $_SESSION['print_items'][$product_key]['quantity'] += $quantity;
                    } else {
                        $_SESSION['print_items'][$product_key] = [
                            'id' => $item['nomenclature_id'],
                            'name' => $item['nomenclature_name'],
                            'barcode' => $item['barcode'],
                            'price' => $item['avg_price'],
                            'quantity' => $quantity,
                            'warehouse' => $currentWarehouseName
                        ];
                    }
                    
                    $success = "Товар '{$item['nomenclature_name']}' добавлен ({$quantity} шт.)";
                    $_POST['barcode'] = '';
                } else {
                    $error = "Товар со штрихкодом '{$barcode}' не найден на складе '{$currentWarehouseName}'";
                }
            } catch (PDOException $e) {
                $error = "Ошибка поиска товара: " . $e->getMessage();
            }
        }
    }
    
    // Добавление товара из выпадающего списка
    if (isset($_POST['add_from_list'])) {
        $item_id = intval($_POST['item_id']);
        $quantity = intval($_POST['list_quantity']);
        
        if ($item_id > 0 && $quantity > 0) {
            foreach ($items as $item) {
                if ($item['nomenclature_id'] == $item_id) {
                    if (!isset($_SESSION['print_items'])) {
                        $_SESSION['print_items'] = [];
                    }
                    
                    $product_key = $item['nomenclature_id'] . '_' . $item['barcode'];
                    
                    if (isset($_SESSION['print_items'][$product_key])) {
                        $_SESSION['print_items'][$product_key]['quantity'] += $quantity;
                    } else {
                        $_SESSION['print_items'][$product_key] = [
                            'id' => $item['nomenclature_id'],
                            'name' => $item['nomenclature_name'],
                            'barcode' => $item['barcode'],
                            'price' => $item['avg_price'],
                            'quantity' => $quantity,
                            'warehouse' => $currentWarehouseName
                        ];
                    }
                    
                    $success = "Товар '{$item['nomenclature_name']}' добавлен ({$quantity} шт.)";
                    break;
                }
            }
        }
    }
    
    // Очистка списка
    if (isset($_POST['clear_list'])) {
        $_SESSION['print_items'] = [];
        $success = "Список товаров очищен";
    }
    
    // Удаление одного товара
    if (isset($_POST['remove_item'])) {
        $item_key = $_POST['item_key'];
        if (isset($_SESSION['print_items'][$item_key])) {
            $item_name = $_SESSION['print_items'][$item_key]['name'];
            unset($_SESSION['print_items'][$item_key]);
            $success = "Товар '{$item_name}' удален из списка";
        }
    }
}

// Получаем список товаров для печати из сессии
$print_items = isset($_SESSION['print_items']) ? $_SESSION['print_items'] : [];

// Считаем общее количество ценников и стоимость
$total_tags = 0;
$total_value = 0;
foreach ($print_items as $item) {
    $total_tags += $item['quantity'];
    $total_value += $item['price'] * $item['quantity'];
}
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Формирование ценников</title>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
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
            --border-radius: 12px;
            --shadow-sm: 0 2px 8px rgba(0,0,0,0.05);
            --shadow-md: 0 4px 12px rgba(0,0,0,0.1);
            --shadow-lg: 0 8px 24px rgba(0,0,0,0.15);
        }
        
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        
        body {
            background: #f5f7fa;
            min-height: 100vh;
            color: #333;
        }
        
        .header {
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            padding: 20px 30px;
            box-shadow: var(--shadow-md);
            display: flex;
            justify-content: space-between;
            align-items: center;
            position: sticky;
            top: 0;
            z-index: 1000;
            border-bottom: 4px solid rgba(255,255,255,0.1);
        }
        
        .header h1 {
            color: white;
            font-size: 24px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 12px;
            text-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        
        .user-info {
            display: flex;
            gap: 24px;
            align-items: center;
            background: rgba(255,255,255,0.15);
            padding: 10px 20px;
            border-radius: 50px;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255,255,255,0.2);
        }
        
        .user-info span {
            color: white;
            font-weight: 500;
            display: flex;
            align-items: center;
            gap: 8px;
            font-size: 14px;
        }
        

        .header {
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            padding: 15px 20px;
            box-shadow: var(--shadow-md);
            border-bottom: 3px solid rgba(255,255,255,0.15);
            margin-bottom: 20px;
        }
        
        .header-info {
            color: white;
        }
        
        .header h1 {
            color: white;
            font-size: 24px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 10px;
            margin-bottom: 10px;
        }
        
        .current-location {
            display: flex;
            gap: 15px;
            flex-wrap: wrap;
        }
        
        .location-item {
            display: flex;
            align-items: center;
            gap: 6px;
            background: rgba(255,255,255,0.15);
            padding: 6px 12px;
            border-radius: 30px;
            border: 1px solid rgba(255,255,255,0.2);
            font-size: 13px;
            font-weight: 600;
        }
        
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 15px 30px;
        }
        
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin-bottom: 20px;
        }
        
        .stat-card {
            background: white;
            border-radius: var(--border-radius);
            padding: 20px;
            box-shadow: var(--shadow-sm);
            transition: all 0.3s;
            border: 1px solid #e0e6ed;
        }
        
        .stat-card:hover {
            transform: translateY(-3px);
            box-shadow: var(--shadow-md);
        }
        
        .stat-icon {
            width: 50px;
            height: 50px;
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 24px;
            margin-bottom: 15px;
        }
        
        .stat-icon.products {
            background: #f0f7ff;
            color: var(--primary);
        }
        
        .stat-icon.tags {
            background: #f0fff4;
            color: var(--secondary);
        }
        
        .stat-icon.warehouse {
            background: #e9f7fe;
            color: var(--info);
        }
        
        .stat-card h3 {
            color: #64748b;
            font-size: 13px;
            font-weight: 600;
            margin-bottom: 8px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .stat-value {
            font-size: 28px;
            font-weight: 800;
            color: var(--dark);
            margin: 8px 0;
            line-height: 1;
        }
        
        .stat-desc {
            color: #94a3b8;
            font-size: 12px;
            line-height: 1.5;
        }
        
        .form-panel {
            background: white;
            border-radius: var(--border-radius);
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: var(--shadow-md);
            border: 1px solid #e0e6ed;
        }
        
        .form-panel h2 {
            color: var(--dark);
            font-size: 18px;
            font-weight: 700;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .form-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin-bottom: 15px;
        }
        
        .form-group {
            margin-bottom: 0;
        }
        
        .form-group label {
            display: block;
            color: var(--dark);
            font-weight: 600;
            margin-bottom: 8px;
            font-size: 13px;
            display: flex;
            align-items: center;
            gap: 6px;
        }
        
        .form-control {
            width: 100%;
            padding: 10px 14px;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            font-size: 14px;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.2s;
        }
        
        select.form-control {
            height: 42px;
        }
        
        .form-control:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 2px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .btn {
            padding: 10px 20px;
            border-radius: 8px;
            font-weight: 600;
            font-size: 14px;
            cursor: pointer;
            transition: all 0.2s;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            border: none;
        }
        
        .btn-primary {
            background: var(--primary);
            color: white;
        }
        
        .btn-primary:hover {
            background: #3a56d4;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(67, 97, 238, 0.3);
        }
        
        .btn-secondary {
            background: var(--secondary);
            color: white;
        }
        
        .btn-secondary:hover {
            background: #27ae60;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(46, 204, 113, 0.3);
        }
        
        .btn-danger {
            background: var(--danger);
            color: white;
        }
        
        .btn-danger:hover {
            background: #c0392b;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(231, 76, 60, 0.3);
        }
        
        .btn-info {
            background: var(--info);
            color: white;
        }
        
        .btn-info:hover {
            background: #2980b9;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(52, 152, 219, 0.3);
        }
        
        .btn-sm {
            padding: 6px 12px;
            font-size: 12px;
        }
        
        .action-buttons {
            display: flex;
            gap: 10px;
            margin-top: 15px;
            flex-wrap: wrap;
        }
        
        .products-list {
            background: white;
            border-radius: var(--border-radius);
            padding: 20px;
            box-shadow: var(--shadow-md);
            border: 1px solid #e0e6ed;
            margin-bottom: 20px;
        }
        
        .products-list h2 {
            color: var(--dark);
            font-size: 18px;
            font-weight: 700;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .product-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 12px;
            border-bottom: 1px solid #e2e8f0;
            transition: all 0.2s;
        }
        
        .product-item:hover {
            background: #f8fafc;
        }
        
        .product-item:last-child {
            border-bottom: none;
        }
        
        .product-info {
            flex: 1;
        }
        
        .product-name {
            font-weight: 600;
            color: var(--dark);
            font-size: 14px;
            margin-bottom: 4px;
        }
        
        .product-details {
            display: flex;
            gap: 12px;
            color: #64748b;
            font-size: 12px;
        }
        
        .product-price {
            color: var(--secondary);
            font-weight: 700;
            font-size: 14px;
        }
        
        .product-quantity {
            color: var(--primary);
            font-weight: 700;
        }
        
        .empty-list {
            text-align: center;
            padding: 40px 20px;
            color: #94a3b8;
        }
        
        .empty-list i {
            font-size: 36px;
            margin-bottom: 10px;
            color: #cbd5e1;
        }
        
        .empty-list h3 {
            font-size: 15px;
            margin-bottom: 8px;
        }
        
        .message {
            padding: 10px 15px;
            border-radius: 8px;
            margin-bottom: 15px;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 8px;
            font-size: 14px;
        }
        
        .message-success {
            background: #d4edda;
            color: #155724;
            border-left: 4px solid var(--secondary);
        }
        
        .message-error {
            background: #f8d7da;
            color: #721c24;
            border-left: 4px solid var(--danger);
        }
        
        .message-info {
            background: #d1ecf1;
            color: #0c5460;
            border-left: 4px solid var(--info);
        }
        
        .print-container {
            display: none;
        }
        
        .print-price-tag {
            width: 48mm;
            height: 48mm;
            border: 1px solid #000;
            page-break-inside: avoid;
            float: left;
            margin: 0;
            box-sizing: border-box;
            display: flex;
            flex-direction: column;
            font-family: Arial, sans-serif;
        }
        
        .print-price-tag-name {
            height: 9mm;
            font-size: 8pt;
            text-align: left;
            display: flex;
            align-items: center;
            word-break: break-word;
            padding-left: 2mm;
        }
        
        .print-price-tag-price {
            height: 26mm; /* уменьшено с 28 до 26 для сохранения общей высоты 48 мм */
            font-size: 20pt;
            font-weight: bold;
            text-align: left;
            display: flex;
            align-items: center;
            padding-left: 2mm;
        }
        
        .print-price-tag-footer {
            height: 4mm;
            font-size: 6pt;
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 0 1mm;
        }
        
        .warehouse-selector {
            background: white;
            border-radius: var(--border-radius);
            padding: 15px;
            margin-bottom: 20px;
            box-shadow: var(--shadow-sm);
            border: 1px solid #e0e6ed;
        }
        
        .warehouse-selector h3 {
            color: var(--dark);
            font-size: 16px;
            font-weight: 600;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        
        .warehouse-options {
            display: flex;
            gap: 10px;
            align-items: center;
        }
        
        @media print {
            body * {
                visibility: hidden;
            }
            
            .print-container, .print-container * {
                visibility: visible;
            }
            
            .print-container {
                position: absolute;
                left: 5mm;
                top: 5mm;
                width: 192mm; /* 4 * 48mm */
                display: block !important;
            }
            
            .print-price-tag {
                width: 48mm !important;
                height: 48mm !important;
                border: 1px solid #000 !important;
                page-break-inside: avoid;
                float: left !important;
                margin: 0 !important;
                border-right: none;
                border-bottom: none;
            }
            
            .print-price-tag:nth-child(4n) {
                border-right: 1px solid #000 !important;
            }
            
            .print-price-tag:last-child,
            .print-price-tag:nth-last-child(-n+4) {
                border-bottom: 1px solid #000 !important;
            }
            
            @page {
                margin: 0;
                size: A4;
            }
            
            body {
                margin: 0;
            }
        }
        
        @media (max-width: 768px) {
            .container {
                padding: 0 10px 20px;
            }
            
            .form-row {
                grid-template-columns: 1fr;
            }
            
            .stats-grid {
                grid-template-columns: 1fr;
            }
            
            .action-buttons {
                flex-direction: column;
            }
            
            .btn {
                width: 100%;
                justify-content: center;
            }
            
            .product-item {
                flex-direction: column;
                align-items: flex-start;
                gap: 8px;
            }
            
            .product-details {
                flex-direction: column;
                gap: 4px;
            }
            
            .warehouse-options {
                flex-direction: column;
                align-items: flex-start;
            }
        }
    </style>
</head>
<body>
    <div class="header">
        <h1><i class="fas fa-tags"></i> Печать ценников</h1>
        <div class="user-info">
            <span><i class="fas fa-user-circle"></i> <?php echo htmlspecialchars($_SESSION['user_name']); ?></span>
            <span><i class="fas fa-store-alt"></i> <?php echo htmlspecialchars($_SESSION['current_division_name']); ?></span>
            <a href="../../index.php" style="color: white; text-decoration: none; display: flex; align-items: center; gap: 8px;">
                <i class="fas fa-arrow-left"></i> На главную
            </a>
        </div>
    </div>
    
    <div class="container">
        <?php if (isset($success)): ?>
            <div class="message message-success">
                <i class="fas fa-check-circle"></i>
                <?php echo $success; ?>
            </div>
        <?php endif; ?>
        
        <?php if (isset($error)): ?>
            <div class="message message-error">
                <i class="fas fa-exclamation-circle"></i>
                <?php echo $error; ?>
            </div>
        <?php endif; ?>
        
        <?php if (count($allWarehouses) > 1): ?>
        <div class="warehouse-selector">
            <h3><i class="fas fa-exchange-alt"></i> Сменить склад</h3>
            <form method="POST" class="warehouse-options">
                <select name="warehouse_id" class="form-control" style="width: 300px; max-width: 100%;">
                    <?php foreach ($allWarehouses as $warehouse): ?>
                        <option value="<?php echo $warehouse['id']; ?>" 
                            <?php echo $warehouse['id'] == $currentWarehouseId ? 'selected' : ''; ?>>
                            <?php echo htmlspecialchars($warehouse['name']); ?>
                        </option>
                    <?php endforeach; ?>
                </select>
                <button type="submit" name="change_warehouse" class="btn btn-info btn-sm">
                    <i class="fas fa-sync-alt"></i> Сменить
                </button>
            </form>
        </div>
        <?php endif; ?>
        
        <div class="stats-grid">
            <div class="stat-card products">
                <div class="stat-icon products">
                    <i class="fas fa-box"></i>
                </div>
                <h3>Товаров в списке</h3>
                <div class="stat-value"><?php echo count($print_items); ?></div>
                <div class="stat-desc">Добавлено товаров для печати</div>
            </div>
            
            <div class="stat-card tags">
                <div class="stat-icon tags">
                    <i class="fas fa-tag"></i>
                </div>
                <h3>Всего ценников</h3>
                <div class="stat-value"><?php echo $total_tags; ?></div>
                <div class="stat-desc">Будет сформировано для печати</div>
            </div>
            
            <div class="stat-card warehouse">
                <div class="stat-icon warehouse">
                    <i class="fas fa-boxes"></i>
                </div>
                <h3>Товаров на складе</h3>
                <div class="stat-value"><?php echo count($items); ?></div>
                <div class="stat-desc">Позиций на текущем складе</div>
            </div>
        </div>
        
        <form method="POST" class="form-panel" id="addProductForm">
            <h2><i class="fas fa-plus-circle"></i> ДОБАВЛЕНИЕ ТОВАРА</h2>
            
            <div class="form-row">
                <div class="form-group">
                    <label for="item_id"><i class="fas fa-list"></i> Выберите товар из списка</label>
                    <select id="item_id" name="item_id" class="form-control">
                        <option value="">-- Выберите товар --</option>
                        <?php foreach ($items as $item): ?>
                            <option value="<?php echo $item['nomenclature_id']; ?>">
                                <?php 
                                echo htmlspecialchars(
                                    $item['nomenclature_name'] . 
                                    ' (штрихкод: ' . $item['barcode'] . 
                                    ', цена: ' . number_format($item['avg_price'], 2) . ' ₽' .
                                    ', остаток: ' . intval($item['total_quantity']) . ' шт.)'
                                ); 
                                ?>
                            </option>
                        <?php endforeach; ?>
                    </select>
                </div>
                
                <div class="form-group">
                    <label for="list_quantity"><i class="fas fa-copy"></i> Количество ценников</label>
                    <input type="number" id="list_quantity" name="list_quantity" class="form-control" value="1" min="1">
                </div>
            </div>
            
            <div class="action-buttons">
                <button type="submit" name="add_from_list" class="btn btn-primary">
                    <i class="fas fa-plus"></i> ДОБАВИТЬ ВЫБРАННЫЙ ТОВАР
                </button>
            </div>
            
            <hr style="margin: 20px 0; border-color: #e2e8f0;">
            
            <div class="form-row">
                <div class="form-group">
                    <label for="barcode"><i class="fas fa-barcode"></i> Или введите штрихкод</label>
                    <input type="text" id="barcode" name="barcode" class="form-control" 
                           placeholder="Введите штрихкод товара" 
                           value="<?php echo isset($_POST['barcode']) ? htmlspecialchars($_POST['barcode']) : ''; ?>">
                </div>
                
                <div class="form-group">
                    <label for="quantity"><i class="fas fa-copy"></i> Количество ценников</label>
                    <input type="number" id="quantity" name="quantity" class="form-control" value="1" min="1">
                </div>
            </div>
            
            <div class="action-buttons">
                <button type="submit" name="add_by_barcode" class="btn btn-secondary" id="addByBarcodeBtn">
                    <i class="fas fa-barcode"></i> ДОБАВИТЬ ПО ШТРИХКОДУ
                </button>
            </div>
        </form>
        
        <div class="products-list">
            <h2><i class="fas fa-list"></i> СПИСОК ТОВАРОВ ДЛЯ ПЕЧАТИ</h2>
            
            <?php if (empty($print_items)): ?>
                <div class="empty-list">
                    <i class="fas fa-clipboard-list"></i>
                    <h3>Список товаров пуст</h3>
                    <p>Добавьте первый товар для формирования ценников</p>
                </div>
            <?php else: ?>
                <?php foreach ($print_items as $key => $item): ?>
                    <div class="product-item">
                        <div class="product-info">
                            <div class="product-name"><?php echo htmlspecialchars($item['name']); ?></div>
                            <div class="product-details">
                                <span class="product-price"><?php echo number_format($item['price'], 2); ?> ₽</span>
                                <span class="product-quantity"><?php echo $item['quantity']; ?> шт.</span>
                                <span style="color: #64748b;">штрихкод: <?php echo htmlspecialchars($item['barcode']); ?></span>
                            </div>
                        </div>
                        <form method="POST" style="display: inline;">
                            <input type="hidden" name="item_key" value="<?php echo $key; ?>">
                            <button type="submit" name="remove_item" class="btn btn-danger btn-sm">
                                <i class="fas fa-trash-alt"></i> Удалить
                            </button>
                        </form>
                    </div>
                <?php endforeach; ?>
            <?php endif; ?>
            
            <div class="action-buttons">
                <button class="btn btn-secondary" onclick="generatePDF()">
                    <i class="fas fa-file-pdf"></i> СФОРМИРОВАТЬ PDF
                </button>
                <button class="btn btn-primary" onclick="printDirectly()">
                    <i class="fas fa-print"></i> ПЕЧАТАТЬ СРАЗУ
                </button>
                
                <form method="POST" style="display: inline;">
                    <button type="submit" name="clear_list" class="btn btn-danger" <?php echo empty($print_items) ? 'disabled' : ''; ?>>
                        <i class="fas fa-trash-alt"></i> ОЧИСТИТЬ ВЕСЬ СПИСОК
                    </button>
                </form>
            </div>
        </div>
    </div>
    
    <!-- Скрытый контейнер для печати -->
    <div class="print-container" id="printContainer"></div>
    
    <script>
        // Глобальные переменные для PDF
        let pdfBlob = null;
        
        // Прямая печать
        function printDirectly() {
            <?php if (empty($print_items)): ?>
                alert('Добавьте товары для печати');
                return;
            <?php endif; ?>
            
            // Создаем HTML для печати с отступами 5мм
            const printContainer = document.getElementById('printContainer');
            printContainer.innerHTML = '';
            
            const currentDate = new Date().toLocaleDateString('ru-RU');
            const currentTime = new Date().toLocaleTimeString('ru-RU', {hour: '2-digit', minute:'2-digit'});
            
            let html = '';
            <?php foreach ($print_items as $item): ?>
                <?php for ($i = 0; $i < $item['quantity']; $i++): ?>
                    html += `
                        <div class="print-price-tag">
                            <div style="height: 3mm;"></div>
                            <div class="print-price-tag-name"><?php echo addslashes($item['name']); ?></div>
                            <div style="height: 3mm;"></div>
                            <div class="print-price-tag-price"><?php echo number_format($item['price'], 2); ?> ₽</div>
                            <div style="height: 3mm;"></div>
                            <div class="print-price-tag-footer">
                                <div>ООО "Связь 22"</div>
                                <div>${currentDate} ${currentTime}</div>
                            </div>
                        </div>
                    `;
                <?php endfor; ?>
            <?php endforeach; ?>
            
            printContainer.innerHTML = html;
            
            // Показываем контейнер для печати
            printContainer.style.display = 'block';
            
            // Запускаем печать
            setTimeout(() => {
                window.print();
                // После печати скрываем контейнер
                setTimeout(() => {
                    printContainer.style.display = 'none';
                    printContainer.innerHTML = '';
                }, 100);
            }, 100);
        }
        
        // Генерация PDF с разбивкой на страницы
        async function generatePDF() {
            <?php if (empty($print_items)): ?>
                alert('Добавьте товары для формирования ценников');
                return;
            <?php endif; ?>
            
            alert('Формирование PDF...');
            
            // Создаем массив товаров из PHP сессии
            const printItems = [
                <?php foreach ($print_items as $key => $item): ?>
                    {
                        name: '<?php echo addslashes($item['name']); ?>',
                        price: '<?php echo number_format($item['price'], 2); ?>',
                        quantity: <?php echo $item['quantity']; ?>,
                        warehouse: '<?php echo addslashes($currentWarehouseName); ?>',
                        division: '<?php echo addslashes($currentDivisionName); ?>'
                    },
                <?php endforeach; ?>
            ];
            
            // Разворачиваем quantity в отдельные элементы (каждый ценник отдельно)
            let tags = [];
            printItems.forEach(item => {
                for (let i = 0; i < item.quantity; i++) {
                    tags.push({
                        name: item.name,
                        price: item.price
                    });
                }
            });
            
            // Параметры страницы
            const tagSize = 48; // мм (было 50)
            const leftMargin = 5; // мм
            const topMargin = 5; // мм
            const pageWidth = 210; // мм
            const pageHeight = 297; // мм
            
            const tagsPerRow = Math.floor((pageWidth - leftMargin) / tagSize); // 4 (48*4=192 <205)
            const rowsPerPage = Math.floor((pageHeight - topMargin) / tagSize); // 6 (48*6=288 <292)
            const tagsPerPage = tagsPerRow * rowsPerPage; // 24
            
            // Разбиваем на страницы
            const pages = [];
            for (let i = 0; i < tags.length; i += tagsPerPage) {
                pages.push(tags.slice(i, i + tagsPerPage));
            }
            
            // Создаём PDF
            const { jsPDF } = window.jspdf;
            const pdf = new jsPDF({
                orientation: 'portrait',
                unit: 'mm',
                format: 'a4'
            });
            
            const currentDate = new Date().toLocaleDateString('ru-RU');
            const currentTime = new Date().toLocaleTimeString('ru-RU', {hour: '2-digit', minute:'2-digit'});
            
            // Обрабатываем каждую страницу
            for (let pageIndex = 0; pageIndex < pages.length; pageIndex++) {
                const pageTags = pages[pageIndex];
                const rowsOnPage = Math.ceil(pageTags.length / tagsPerRow);
                const containerHeight = rowsOnPage * tagSize + topMargin; // мм
                
                // Создаём временный контейнер для этой страницы
                const tempContainer = document.createElement('div');
                tempContainer.style.position = 'absolute';
                tempContainer.style.left = '-9999px';
                tempContainer.style.top = '0';
                tempContainer.style.width = pageWidth + 'mm';
                tempContainer.style.height = containerHeight + 'mm';
                tempContainer.style.backgroundColor = 'white';
                tempContainer.style.padding = topMargin + 'mm 0 0 ' + leftMargin + 'mm';
                tempContainer.style.display = 'flex';
                tempContainer.style.flexWrap = 'wrap';
                tempContainer.style.justifyContent = 'flex-start';
                tempContainer.style.alignItems = 'flex-start';
                tempContainer.style.boxSizing = 'border-box';
                document.body.appendChild(tempContainer);
                
                // Заполняем ценниками
                pageTags.forEach(tag => {
                    const priceTagDiv = document.createElement('div');
                    priceTagDiv.style.cssText = `
                        width: ${tagSize}mm;
                        height: ${tagSize}mm;
                        border: 1px solid #000;
                        box-sizing: border-box;
                        display: flex;
                        flex-direction: column;
                        font-family: Arial, sans-serif;
                        margin: 0;
                    `;
                    
                    priceTagDiv.innerHTML = `
                        <div style="height: 3mm;"></div>
                        <div style="height: 9mm; font-size: 8pt; text-align: left; display: flex; align-items: center; word-break: break-word; padding-left: 2mm;">
                            ${tag.name}
                        </div>
                        <div style="height: 3mm;"></div>
                        <div style="height: 26mm; font-size: 20pt; font-weight: bold; text-align: left; display: flex; align-items: center; padding-left: 2mm;">
                            ${tag.price} ₽
                        </div>
                        <div style="height: 3mm;"></div>
                        <div style="height: 4mm; font-size: 6pt; display: flex; justify-content: space-between; align-items: center; padding: 0 1mm;">
                            <div>ООО "Связь 22"</div>
                            <div>${currentDate} ${currentTime}</div>
                        </div>
                    `;
                    
                    tempContainer.appendChild(priceTagDiv);
                });
                
                // Конвертируем в canvas
                try {
                    const canvas = await html2canvas(tempContainer, {
                        scale: 2,
                        backgroundColor: '#ffffff',
                        useCORS: true,
                        logging: false,
                        width: pageWidth * 3.78,
                        windowWidth: pageWidth * 3.78,
                        height: containerHeight * 3.78,
                        windowHeight: containerHeight * 3.78
                    });
                    
                    // Удаляем контейнер
                    document.body.removeChild(tempContainer);
                    
                    // Конвертируем canvas в изображение
                    const imgData = canvas.toDataURL('image/jpeg', 1.0);
                    
                    // Если это не первая страница, добавляем новую
                    if (pageIndex > 0) {
                        pdf.addPage();
                    }
                    
                    // Вставляем изображение на страницу
                    pdf.addImage(imgData, 'JPEG', 0, 0, pageWidth, containerHeight);
                    
                } catch (error) {
                    console.error('Ошибка при создании canvas для страницы ' + (pageIndex + 1) + ':', error);
                    alert('Ошибка при создании PDF: ' + error.message);
                    return;
                }
            }
            
            // Сохраняем PDF
            const currentDateStr = new Date().toLocaleDateString('ru-RU').replace(/\./g, '-');
            pdf.save(`Ценники_<?php echo addslashes($currentWarehouseName); ?>_${currentDateStr}.pdf`);
        }

        // Скачивание PDF (если сохранен blob)
        function downloadPDF() {
            if (!pdfBlob) {
                alert('Сначала сформируйте PDF');
                return;
            }
            
            const currentDate = new Date().toLocaleDateString('ru-RU').replace(/\./g, '-');
            const url = URL.createObjectURL(pdfBlob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `Ценники_<?php echo addslashes($currentWarehouseName); ?>_${currentDate}.pdf`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        }
        
        // Автофокус на штрихкоде при загрузке и обработка Enter
        document.addEventListener('DOMContentLoaded', function() {
            // Фокус на поле штрихкода при загрузке
            const barcodeField = document.getElementById('barcode');
            if (barcodeField) {
                barcodeField.focus();
                
                // Обработка Enter в поле штрихкода - отправка формы
                barcodeField.addEventListener('keypress', function(e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        if (this.value.trim() !== '') {
                            document.getElementById('addByBarcodeBtn').click();
                        }
                    }
                });
            }
            
            // Обработка Ctrl+B для фокуса на штрихкоде
            document.addEventListener('keydown', function(e) {
                if ((e.ctrlKey || e.metaKey) && e.key === 'b') {
                    e.preventDefault();
                    const barcodeField = document.getElementById('barcode');
                    if (barcodeField) {
                        barcodeField.focus();
                        barcodeField.select();
                    }
                }
            });
            
            // Возврат фокуса на поле штрихкода после любых действий
            document.addEventListener('click', function(e) {
                // Если клик был не по форме добавления товара и не по кнопкам в списке
                if (!e.target.closest('#addProductForm') && 
                    !e.target.closest('.product-item form') && 
                    !e.target.closest('.action-buttons form')) {
                    setTimeout(() => {
                        const barcodeField = document.getElementById('barcode');
                        if (barcodeField && document.activeElement.tagName !== 'INPUT' && 
                            document.activeElement.tagName !== 'SELECT' && 
                            document.activeElement.tagName !== 'TEXTAREA') {
                            barcodeField.focus();
                        }
                    }, 100);
                }
            });
            
            // Убедимся, что функции определены глобально
            window.printDirectly = printDirectly;
            window.generatePDF = generatePDF;
            window.downloadPDF = downloadPDF;
        });
        
        // При закрытии вкладки/обновлении страницы очищаем флаг сессии
        window.addEventListener('beforeunload', function() {
            sessionStorage.removeItem('print_session_started');
        });
    </script>
</body>
</html>