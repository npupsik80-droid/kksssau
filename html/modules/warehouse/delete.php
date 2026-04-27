<?php
require_once '../../includes/auth_check.php';

if (!isset($_SESSION['current_division_id'])) {
    header('Location: ../../select_division.php');
    exit();
}

// Получаем ВСЕ склады ВСЕХ подразделений
$stmt = $pdo->prepare("
    SELECT w.*, d.name as division_name 
    FROM warehouses w 
    LEFT JOIN divisions d ON w.division_id = d.id 
    ORDER BY d.name, w.name
");
$stmt->execute();
$warehouses = $stmt->fetchAll();

// Выбранный склад
$selected_warehouse_id = $_GET['warehouse_id'] ?? ($_SESSION['current_warehouse_id'] ?? ($warehouses[0]['id'] ?? null));

// Получаем товары на выбранном складе
$items = [];
if ($selected_warehouse_id) {
    $stmt = $pdo->prepare("
        SELECT 
            wi.*,
            n.name,
            n.barcode,
            wi.quantity as current_quantity
        FROM warehouse_items wi
        JOIN nomenclatures n ON wi.nomenclature_id = n.id
        WHERE wi.warehouse_id = ?
        AND wi.quantity > 0
        ORDER BY n.name
    ");
    $stmt->execute([$selected_warehouse_id]);
    $items = $stmt->fetchAll();
}

// Обработка списания
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['writeoff'])) {
    $item_id = intval($_POST['item_id']);
    $quantity = floatval($_POST['quantity']);
    $reason = trim($_POST['reason']);
    $writeoff_type = $_POST['writeoff_type'];
    
    if ($quantity <= 0) {
        $error = 'Укажите количество для списания';
    } else {
        // Проверяем доступное количество
        $stmt = $pdo->prepare("SELECT quantity FROM warehouse_items WHERE id = ?");
        $stmt->execute([$item_id]);
        $item = $stmt->fetch();
        
        if (!$item) {
            $error = 'Товар не найден';
        } elseif ($item['quantity'] < $quantity) {
            $error = 'Недостаточно товара на складе. Доступно: ' . $item['quantity'];
        } else {
            try {
                $pdo->beginTransaction();
                
                // Списание товара
                $stmt = $pdo->prepare("
                    UPDATE warehouse_items 
                    SET quantity = quantity - ? 
                    WHERE id = ?
                ");
                $stmt->execute([$quantity, $item_id]);
                
                // Запись в историю списаний
                $stmt = $pdo->prepare("
                    INSERT INTO writeoffs 
                    (warehouse_item_id, quantity, reason, type, user_id, created_at)
                    VALUES (?, ?, ?, ?, ?, NOW())
                ");
                $stmt->execute([
                    $item_id, 
                    $quantity, 
                    $reason, 
                    $writeoff_type,
                    $_SESSION['user_id']
                ]);
                
                // Логируем действие
                $stmt = $pdo->prepare("
                    INSERT INTO operation_log 
                    (user_id, action, details, ip_address)
                    VALUES (?, ?, ?, ?)
                ");
                $stmt->execute([
                    $_SESSION['user_id'],
                    'goods_writeoff',
                    "Списано товара: {$quantity} шт. Причина: {$reason}",
                    $_SERVER['REMOTE_ADDR']
                ]);
                
                $pdo->commit();
                $success = 'Товар успешно списан';
                
                // Обновляем список товаров
                $items = array_filter($items, fn($item) => $item['id'] != $item_id || $item['quantity'] > $quantity);
                
            } catch (Exception $e) {
                $pdo->rollBack();
                $error = 'Ошибка списания: ' . $e->getMessage();
            }
        }
    }
}
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Списание товара - RunaRMK</title>
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
        
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
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
            border-radius: var(--border-radius);
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
        
        .alert {
            padding: 20px 25px;
            border-radius: var(--border-radius);
            margin-bottom: 30px;
            display: flex;
            align-items: center;
            gap: 15px;
            box-shadow: var(--shadow-sm);
        }
        
        .alert-danger {
            background: linear-gradient(135deg, #fee2e2 0%, #fecaca 100%);
            color: #dc2626;
            border: 2px solid #ef4444;
        }
        
        .alert-success {
            background: linear-gradient(135deg, #dcfce7 0%, #bbf7d0 100%);
            color: #16a34a;
            border: 2px solid #22c55e;
        }
        
        .warehouse-selector {
            background: white;
            padding: 20px;
            border-radius: var(--border-radius);
            box-shadow: var(--shadow-md);
            margin-bottom: 30px;
            border: 2px solid rgba(67, 97, 238, 0.1);
            position: relative;
            overflow: hidden;
        }
        
        .warehouse-selector::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--info), #2980b9);
        }
        
        .warehouse-selector form {
            display: flex;
            gap: 15px;
            align-items: center;
        }
        
        .warehouse-selector .form-control {
            padding: 12px 16px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 16px;
            background: #f8fafc;
            color: var(--dark);
            min-width: 300px;
            transition: all 0.3s;
        }
        
        .warehouse-selector .form-control:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .writeoff-container {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 30px;
            margin-bottom: 40px;
        }
        
        .items-list {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            max-height: 600px;
            overflow-y: auto;
            box-shadow: var(--shadow-md);
            border: 2px solid rgba(67, 97, 238, 0.1);
        }
        
        .items-list h3 {
            font-size: 24px;
            font-weight: 700;
            color: var(--dark);
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .item-card {
            padding: 20px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            margin-bottom: 15px;
            cursor: pointer;
            transition: all 0.3s;
        }
        
        .item-card:hover {
            border-color: var(--primary);
            background: #f8fafc;
            transform: translateX(4px);
        }
        
        .item-card.selected {
            border-color: #28a745;
            background: #d4edda;
        }
        
        .item-card .item-info-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        
        .item-card .item-name {
            font-weight: 700;
            color: var(--dark);
            font-size: 16px;
        }
        
        .item-card .item-stats .item-quantity {
            font-size: 24px;
            font-weight: 800;
            color: var(--danger);
        }
        
        .item-card .item-stats .item-price {
            font-size: 14px;
            color: #64748b;
        }
        
        .item-card .item-barcode {
            font-size: 13px;
            color: #64748b;
            margin-top: 8px;
            display: flex;
            align-items: center;
            gap: 6px;
        }
        
        .writeoff-form {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-md);
            border: 2px solid rgba(239, 68, 68, 0.2);
            position: sticky;
            top: 20px;
        }
        
        .writeoff-form h3 {
            font-size: 24px;
            font-weight: 700;
            color: var(--dark);
            margin-bottom: 25px;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .writeoff-form .form-group {
            margin-bottom: 25px;
        }
        
        .writeoff-form .form-group label {
            display: block;
            color: var(--dark);
            font-weight: 600;
            margin-bottom: 10px;
            font-size: 15px;
        }
        
        .writeoff-form .form-control {
            width: 100%;
            padding: 12px 16px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 16px;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.3s;
        }
        
        .writeoff-form .form-control:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .writeoff-form .form-control[readonly] {
            background: #f1f5f9;
            color: #64748b;
            cursor: not-allowed;
        }
        
        .writeoff-form textarea.form-control {
            resize: vertical;
            min-height: 100px;
        }
        
        .writeoff-form .quantity-control {
            display: flex;
            align-items: center;
            gap: 10px;
            margin: 15px 0;
        }
        
        .writeoff-form .qty-btn {
            width: 44px;
            height: 44px;
            border-radius: 12px;
            border: 2px solid #e2e8f0;
            background: white;
            font-size: 20px;
            font-weight: 800;
            color: var(--dark);
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.3s;
        }
        
        .writeoff-form .qty-btn:hover {
            border-color: var(--primary);
            color: var(--primary);
            transform: scale(1.1);
        }
        
        .writeoff-form .qty-input {
            width: 100px;
            padding: 12px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 18px;
            font-weight: 700;
            text-align: center;
            background: #f8fafc;
            color: var(--dark);
        }
        
        .writeoff-form .btn-secondary {
            background: linear-gradient(135deg, #64748b 0%, #475569 100%);
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 12px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s;
        }
        
        .writeoff-form .btn-secondary:hover {
            background: linear-gradient(135deg, #475569 0%, #334155 100%);
            transform: translateY(-2px);
        }
        
        .writeoff-form .btn-danger {
            background: linear-gradient(135deg, var(--danger) 0%, #c0392b 100%);
            color: white;
            border: none;
            padding: 14px 28px;
            border-radius: 12px;
            font-weight: 700;
            cursor: pointer;
            transition: all 0.3s;
        }
        
        .writeoff-form .btn-danger:hover {
            background: linear-gradient(135deg, #c0392b 0%, #a93226 100%);
            transform: translateY(-2px);
        }
        
        .writeoff-form .available-quantity {
            color: #64748b;
            font-size: 14px;
            margin-top: 8px;
        }
        
        .writeoff-form .reason-types {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
            gap: 10px;
            margin: 15px 0;
        }
        
        .writeoff-form .reason-type {
            padding: 16px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            text-align: center;
            cursor: pointer;
            transition: all 0.3s;
        }
        
        .writeoff-form .reason-type:hover {
            border-color: #3498db;
        }
        
        .writeoff-form .reason-type.selected {
            border-color: #28a745;
            background: #d4edda;
        }
        
        .history-section {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            margin-top: 30px;
            box-shadow: var(--shadow-md);
            border: 2px solid rgba(67, 97, 238, 0.1);
        }
        
        .history-section h3 {
            font-size: 24px;
            font-weight: 700;
            color: var(--dark);
            margin-bottom: 25px;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .history-section .history-item {
            padding: 20px;
            border-bottom: 2px solid #f1f5f9;
            font-size: 14px;
        }
        
        .history-section .history-item:last-child {
            border-bottom: none;
        }
        
        .history-section .history-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 10px;
        }
        
        .history-section .history-product {
            font-weight: 700;
            color: var(--dark);
            font-size: 16px;
        }
        
        .history-section .history-quantity {
            font-weight: 800;
            color: var(--danger);
            font-size: 20px;
        }
        
        .history-section .history-details {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-top: 10px;
            font-size: 13px;
            color: #64748b;
        }
        
        .history-section .writeoff-reason {
            display: flex;
            align-items: center;
            gap: 8px;
        }
        
        .history-section .history-meta {
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .history-section .badge {
            display: inline-flex;
            align-items: center;
            gap: 6px;
            padding: 6px 12px;
            border-radius: 30px;
            font-weight: 700;
            font-size: 12px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .history-section .badge-danger {
            background: linear-gradient(135deg, rgba(239, 68, 68, 0.15) 0%, rgba(239, 68, 68, 0.05) 100%);
            color: var(--danger);
            border: 2px solid rgba(239, 68, 68, 0.3);
        }
        
        .history-section .badge-warning {
            background: linear-gradient(135deg, rgba(245, 158, 11, 0.15) 0%, rgba(245, 158, 11, 0.05) 100%);
            color: #f59e0b;
            border: 2px solid rgba(245, 158, 11, 0.3);
        }
        
        .history-section .badge-purple {
            background: linear-gradient(135deg, rgba(124, 58, 237, 0.15) 0%, rgba(124, 58, 237, 0.05) 100%);
            color: #7c3aed;
            border: 2px solid rgba(124, 58, 237, 0.3);
        }
        
        .history-section .badge-gray {
            background: linear-gradient(135deg, rgba(100, 116, 139, 0.15) 0%, rgba(100, 116, 139, 0.05) 100%);
            color: #64748b;
            border: 2px solid rgba(100, 116, 139, 0.3);
        }
        
        .empty-state {
            text-align: center;
            padding: 60px 30px;
            color: #64748b;
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
            line-height: 1.6;
        }
        
        @media (max-width: 992px) {
            .writeoff-container {
                grid-template-columns: 1fr;
            }
            
            .writeoff-form {
                position: static;
            }
            
            .warehouse-selector form {
                flex-direction: column;
                align-items: stretch;
            }
            
            .warehouse-selector .form-control {
                min-width: unset;
                width: 100%;
            }
        }
        
        @media (max-width: 768px) {
            .container {
                padding: 15px;
            }
            
            .header {
                flex-direction: column;
                gap: 15px;
                text-align: center;
                padding: 20px;
            }
            
            .header h1 {
                font-size: 28px;
            }
            
            .history-details {
                flex-direction: column;
                align-items: flex-start;
                gap: 10px;
            }
            
            .history-meta {
                width: 100%;
                justify-content: space-between;
            }
        }
    </style>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">
</head>
<body>
    <div class="container">
        <div class="header">
            <h1><i class="fas fa-minus-circle"></i> Списание товара</h1>
            <a href="./view.php" class="btn btn-primary">
                <i class="fas fa-arrow-left"></i> На главную
            </a>
        </div>
        
        <?php if (isset($success)): ?>
            <div class="alert alert-success">
                <i class="fas fa-check-circle"></i> <?php echo $success; ?>
            </div>
        <?php endif; ?>
        
        <?php if (isset($error)): ?>
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-circle"></i> <?php echo $error; ?>
            </div>
        <?php endif; ?>
        
        <!-- Выбор склада -->
        <div class="warehouse-selector">
            <form method="GET" style="display: flex; gap: 15px; align-items: center;">
                <div style="min-width: 300px;">
                    <select name="warehouse_id" class="form-control" onchange="this.form.submit()">
                        <?php foreach ($warehouses as $warehouse): ?>
                        <option value="<?php echo $warehouse['id']; ?>"
                            <?php echo $selected_warehouse_id == $warehouse['id'] ? 'selected' : ''; ?>>
                            <?php echo htmlspecialchars($warehouse['name']); ?>
                            (<?php echo htmlspecialchars($warehouse['division_name']); ?>)
                        </option>
                        <?php endforeach; ?>
                    </select>
                </div>
                
                <div style="color: #666;">
                    <i class="fas fa-warehouse"></i> 
                    <?php 
                    if ($selected_warehouse_id) {
                        $warehouseIndex = array_search($selected_warehouse_id, array_column($warehouses, 'id'));
                        if ($warehouseIndex !== false) {
                            echo htmlspecialchars($warehouses[$warehouseIndex]['name'] ?? 'Неизвестный склад');
                            echo ' (';
                            echo htmlspecialchars($warehouses[$warehouseIndex]['division_name'] ?? '');
                            echo ')';
                        } else {
                            echo 'Неизвестный склад';
                        }
                    }
                    ?>
                </div>
            </form>
        </div>
        
        <div class="writeoff-container">
            <!-- Список товаров -->
            <div class="items-list">
                <h3><i class="fas fa-boxes"></i> Товары на складе</h3>
                <p style="color: #666; margin-bottom: 15px;">Выберите товар для списания</p>
                
                <?php if (count($items) > 0): ?>
                    <?php foreach ($items as $item): ?>
                    <div class="item-card" 
                         onclick="selectItem(<?php echo $item['id']; ?>, '<?php echo addslashes($item['name']); ?>', <?php echo $item['current_quantity']; ?>, '<?php echo addslashes($item['barcode']); ?>', <?php echo $item['price']; ?>)"
                         id="item-<?php echo $item['id']; ?>">
                        <div style="display: flex; justify-content: space-between; align-items: center;">
                            <div>
                                <strong><?php echo htmlspecialchars($item['name']); ?></strong>
                                <div style="font-size: 12px; color: #666; margin-top: 5px;">
                                    ШК: <?php echo htmlspecialchars($item['barcode']); ?>
                                </div>
                            </div>
                            <div style="text-align: right;">
                                <div style="font-size: 18px; font-weight: bold; color: #e74c3c;">
                                    <?php echo $item['current_quantity']; ?> шт.
                                </div>
                                <div style="font-size: 14px; color: #666;">
                                    <?php echo number_format($item['price'], 2); ?> ₽/шт.
                                </div>
                            </div>
                        </div>
                    </div>
                    <?php endforeach; ?>
                <?php else: ?>
                    <div style="text-align: center; padding: 40px; color: #666;">
                        <i class="fas fa-box-open fa-3x" style="margin-bottom: 20px; color: #ddd;"></i>
                        <h3>Товары не найдены</h3>
                        <p>На выбранном складе нет товаров для списания</p>
                    </div>
                <?php endif; ?>
            </div>
            
            <!-- Форма списания -->
            <div class="writeoff-form">
                <h3><i class="fas fa-file-export"></i> Списание товара</h3>
                
                <form method="POST" id="writeoffForm">
                    <input type="hidden" name="writeoff" value="1">
                    <input type="hidden" name="item_id" id="selectedItemId">
                    
                    <div class="form-group">
                        <label>Выбранный товар</label>
                        <input type="text" id="selectedItemName" class="form-control" readonly 
                               placeholder="Выберите товар из списка слева">
                    </div>
                    
                    <div class="form-group">
                        <label>Доступное количество</label>
                        <div style="display: flex; align-items: center; gap: 10px;">
                            <input type="text" id="availableQuantity" class="form-control" readonly 
                                   style="width: 120px;">
                            <span>штук</span>
                        </div>
                    </div>
                    
                    <div class="form-group">
                        <label>Количество для списания *</label>
                        <div class="quantity-control">
                            <button type="button" class="qty-btn" onclick="changeQuantity(-1)">-</button>
                            <input type="number" name="quantity" id="writeoffQuantity" 
                                   class="qty-input" value="1" min="1" step="1" required>
                            <button type="button" class="qty-btn" onclick="changeQuantity(1)">+</button>
                            <button type="button" class="btn btn-sm btn-secondary" onclick="setMaxQuantity()">
                                Все
                            </button>
                        </div>
                        <div class="available-quantity" id="quantityInfo">
                            Введите количество для списания
                        </div>
                    </div>
                    
                    <div class="form-group">
                        <label>Тип списания *</label>
                        <div class="reason-types">
                            <div class="reason-type" data-type="damage" onclick="selectReasonType('damage')">
                                <i class="fas fa-times-circle"></i>
                                <div>Порча</div>
                            </div>
                            <div class="reason-type" data-type="theft" onclick="selectReasonType('theft')">
                                <i class="fas fa-user-secret"></i>
                                <div>Недосдача</div>
                            </div>
                            <div class="reason-type" data-type="other" onclick="selectReasonType('other')">
                                <i class="fas fa-question-circle"></i>
                                <div>Другое</div>
                            </div>
                        </div>
                        <input type="hidden" name="writeoff_type" id="writeoffType" required>
                    </div>
                    
                    <div class="form-group">
                        <label>Причина списания *</label>
                        <textarea name="reason" id="writeoffReason" class="form-control" 
                                  rows="3" placeholder="Укажите причину списания..." required></textarea>
                    </div>
                    
                    <div class="form-group">
                        <label>Штрих-код товара</label>
                        <input type="text" id="itemBarcode" class="form-control" readonly>
                    </div>
                    
                    <div class="form-group">
                        <label>Стоимость списываемого товара</label>
                        <input type="text" id="writeoffValue" class="form-control" readonly 
                               value="0.00 ₽">
                    </div>
                    
                    <div class="modal-actions" style="margin-top: 25px;">
                        <button type="button" class="btn btn-secondary" onclick="resetForm()">
                            <i class="fas fa-redo"></i> Сбросить
                        </button>
                        <button type="submit" class="btn btn-danger" id="submitBtn" disabled>
                            <i class="fas fa-minus-circle"></i> Списать товар
                        </button>
                    </div>
                </form>
            </div>
        </div>
        
        <!-- История списаний -->
        <div class="history-section">
            <h3><i class="fas fa-history"></i> Последние списания</h3>
            
            <?php
            $historyStmt = $pdo->prepare("
                SELECT 
                    w.*,
                    n.name as item_name,
                    n.barcode,
                    u.full_name as user_name
                FROM writeoffs w
                JOIN warehouse_items wi ON w.warehouse_item_id = wi.id
                JOIN nomenclatures n ON wi.nomenclature_id = n.id
                JOIN users u ON w.user_id = u.id
                WHERE wi.warehouse_id = ?
                ORDER BY w.created_at DESC
                LIMIT 10
            ");
            $historyStmt->execute([$selected_warehouse_id]);
            $writeoffHistory = $historyStmt->fetchAll();
            ?>
            
            <?php if (count($writeoffHistory) > 0): ?>
                <?php foreach ($writeoffHistory as $record): ?>
                <div class="history-item">
                    <div style="display: flex; justify-content: space-between;">
                        <div>
                            <strong><?php echo htmlspecialchars($record['item_name']); ?></strong>
                            <span style="color: #666; font-size: 12px; margin-left: 10px;">
                                <?php echo $record['barcode']; ?>
                            </span>
                        </div>
                        <div style="color: #e74c3c; font-weight: bold;">
                            -<?php echo $record['quantity']; ?> шт.
                        </div>
                    </div>
                    <div style="display: flex; justify-content: space-between; margin-top: 5px; font-size: 12px;">
                        <div class="writeoff-reason">
                            <i class="fas fa-comment"></i> <?php echo htmlspecialchars($record['reason']); ?>
                            <span style="color: #666; margin-left: 10px;">
                                (<?php echo $record['type']; ?>)
                            </span>
                        </div>
                        <div style="color: #666;">
                            <?php echo date('d.m.Y H:i', strtotime($record['created_at'])); ?>
                            <span style="margin-left: 10px;">
                                <i class="fas fa-user"></i> <?php echo htmlspecialchars($record['user_name']); ?>
                            </span>
                        </div>
                    </div>
                </div>
                <?php endforeach; ?>
            <?php else: ?>
                <div style="text-align: center; padding: 20px; color: #666;">
                    <i class="fas fa-history"></i> История списаний пуста
                </div>
            <?php endif; ?>
        </div>
    </div>
    
    <script>
        let selectedItemId = null;
        let selectedItemPrice = 0;
        let maxQuantity = 0;
        
        // Выбор товара
        function selectItem(itemId, itemName, quantity, barcode, price) {
            // Снимаем выделение с предыдущего
            if (selectedItemId) {
                document.getElementById('item-' + selectedItemId).classList.remove('selected');
            }
            
            // Выделяем текущий
            selectedItemId = itemId;
            document.getElementById('item-' + itemId).classList.add('selected');
            
            // Заполняем форму
            document.getElementById('selectedItemId').value = itemId;
            document.getElementById('selectedItemName').value = itemName;
            document.getElementById('availableQuantity').value = quantity;
            document.getElementById('itemBarcode').value = barcode;
            
            // Сохраняем данные для расчетов
            maxQuantity = quantity;
            selectedItemPrice = parseFloat(price) || 0;
            
            console.log('Выбран товар:', {
                id: itemId,
                name: itemName,
                quantity: quantity,
                price: selectedItemPrice,
                barcode: barcode
            });
            
            // Обновляем информацию о количестве
            updateQuantityInfo();
            
            // Включаем кнопку отправки
            document.getElementById('submitBtn').disabled = false;
        }
        
        // Изменение количества
        function changeQuantity(delta) {
            const input = document.getElementById('writeoffQuantity');
            let newValue = parseInt(input.value) + delta;
            
            if (newValue < 1) newValue = 1;
            if (maxQuantity && newValue > maxQuantity) newValue = maxQuantity;
            
            input.value = newValue;
            updateQuantityInfo();
        }
        
        // Установить максимальное количество
        function setMaxQuantity() {
            if (maxQuantity > 0) {
                document.getElementById('writeoffQuantity').value = maxQuantity;
                updateQuantityInfo();
            }
        }
        
        // Обновление информации о количестве
        function updateQuantityInfo() {
            const quantity = parseInt(document.getElementById('writeoffQuantity').value) || 0;
            const infoElement = document.getElementById('quantityInfo');
            
            if (maxQuantity > 0 && selectedItemPrice > 0) {
                const percent = (quantity / maxQuantity * 100).toFixed(1);
                const totalValue = quantity * selectedItemPrice;
                
                infoElement.innerHTML = `
                    Списание: <strong>${quantity}</strong> из <strong>${maxQuantity}</strong> шт. (${percent}%)
                `;
                
                // Расчет стоимости
                document.getElementById('writeoffValue').value = totalValue.toFixed(2) + ' ₽';
                
                console.log('Расчет стоимости:', {
                    quantity: quantity,
                    price: selectedItemPrice,
                    total: totalValue
                });
                
                // Цветовое кодирование
                if (quantity > maxQuantity) {
                    infoElement.style.color = '#dc3545';
                } else if (quantity === maxQuantity) {
                    infoElement.style.color = '#e74c3c';
                } else if (quantity > maxQuantity * 0.5) {
                    infoElement.style.color = '#f39c12';
                } else {
                    infoElement.style.color = '#28a745';
                }
            } else if (maxQuantity > 0) {
                infoElement.innerHTML = `
                    Списание: <strong>${quantity}</strong> из <strong>${maxQuantity}</strong> шт.
                    <div style="color: #ff6b6b; font-size: 12px; margin-top: 5px;">
                        <i class="fas fa-exclamation-triangle"></i> Цена товара не определена
                    </div>
                `;
                document.getElementById('writeoffValue').value = '0.00 ₽';
            } else {
                infoElement.innerHTML = 'Введите количество для списания';
                document.getElementById('writeoffValue').value = '0.00 ₽';
            }
        }
        
        // Выбор типа причины
        function selectReasonType(type) {
            // Снимаем выделение
            document.querySelectorAll('.reason-type').forEach(el => {
                el.classList.remove('selected');
            });
            
            // Выделяем выбранный
            document.querySelector(`.reason-type[data-type="${type}"]`).classList.add('selected');
            document.getElementById('writeoffType').value = type;
            
            // Автозаполнение причины по типу
            const reasons = {
                'damage': 'Испорчен товарный вид',
                'theft': 'Недостача при инвентаризации',
                'other': ''
            };
            
            document.getElementById('writeoffReason').value = reasons[type] || '';
        }
        
        // Сброс формы
        function resetForm() {
            // Снимаем выделение с товара
            if (selectedItemId) {
                document.getElementById('item-' + selectedItemId).classList.remove('selected');
            }
            
            // Сбрасываем форму
            document.getElementById('writeoffForm').reset();
            document.getElementById('selectedItemId').value = '';
            document.getElementById('selectedItemName').value = '';
            document.getElementById('availableQuantity').value = '';
            document.getElementById('itemBarcode').value = '';
            document.getElementById('writeoffValue').value = '0.00 ₽';
            document.getElementById('quantityInfo').innerHTML = 'Введите количество для списания';
            document.getElementById('quantityInfo').style.color = '';
            
            // Снимаем выделение с типа причины
            document.querySelectorAll('.reason-type').forEach(el => {
                el.classList.remove('selected');
            });
            
            // Отключаем кнопку отправки
            document.getElementById('submitBtn').disabled = true;
            
            selectedItemId = null;
            selectedItemPrice = 0;
            maxQuantity = 0;
        }
        
        // Валидация формы при отправке
        document.getElementById('writeoffForm').addEventListener('submit', function(e) {
            const quantity = parseInt(document.getElementById('writeoffQuantity').value) || 0;
            const reasonType = document.getElementById('writeoffType').value;
            const reason = document.getElementById('writeoffReason').value.trim();
            
            if (!selectedItemId) {
                e.preventDefault();
                alert('Выберите товар для списания');
                return;
            }
            
            if (quantity <= 0) {
                e.preventDefault();
                alert('Укажите количество для списания');
                return;
            }
            
            if (quantity > maxQuantity) {
                e.preventDefault();
                alert(`Нельзя списать больше ${maxQuantity} шт. (доступное количество)`);
                return;
            }
            
            if (!reasonType) {
                e.preventDefault();
                alert('Выберите тип списания');
                return;
            }
            
            if (!reason) {
                e.preventDefault();
                alert('Укажите причину списания');
                return;
            }
            
            // Расчет стоимости для подтверждения
            const totalCost = (quantity * selectedItemPrice).toFixed(2);
            const confirmationMessage = `Списать ${quantity} шт. выбранного товара?\n` +
                                      `Стоимость списания: ${totalCost} ₽\n` +
                                      `Причина: ${reason}\n\n` +
                                      `Это действие нельзя отменить.`;
            
            if (!confirm(confirmationMessage)) {
                e.preventDefault();
            }
        });
        
        // Автоматический расчет при изменении количества
        document.getElementById('writeoffQuantity').addEventListener('input', updateQuantityInfo);
        
        // Автовыбор типа "Другое" при вводе причины
        document.getElementById('writeoffReason').addEventListener('input', function() {
            if (this.value.trim() && !document.getElementById('writeoffType').value) {
                selectReasonType('other');
            }
        });
    </script>
</body>
</html>