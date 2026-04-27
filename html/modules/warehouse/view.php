<?php
require_once '../../includes/auth_check.php';

if ($_SESSION['permission_group'] !== 'admin') {
    header('Location: ../../index.php');
    exit();
}

// Сначала получаем ВСЕ склады (даже пустые)
$stmt = $pdo->prepare("
    SELECT w.*, d.name as division_name 
    FROM warehouses w 
    LEFT JOIN divisions d ON w.division_id = d.id 
    ORDER BY w.name
");
$stmt->execute();
$allWarehouses = $stmt->fetchAll();

// Создаем структуру для хранения складов
$warehouses = [];
foreach ($allWarehouses as $warehouse) {
    $warehouses[$warehouse['id']] = [
        'id' => $warehouse['id'],
        'name' => $warehouse['name'],
        'division_name' => $warehouse['division_name'],
        'items' => [],
        'total_quantity' => 0,
        'total_value' => 0,
        'item_count' => 0
    ];
}

// Теперь получаем товары на складах с группировкой по номенклатуре
$stmt = $pdo->prepare("
    SELECT 
        w.id as warehouse_id,
        w.name as warehouse_name,
        d.name as division_name,
        n.id as nomenclature_id,
        n.name as nomenclature_name,
        n.barcode,
        n.description,
        SUM(wi.quantity) as total_quantity,
        AVG(wi.price) as avg_price,
        MAX(wi.created_at) as last_received
    FROM warehouses w
    LEFT JOIN divisions d ON w.division_id = d.id
    LEFT JOIN warehouse_items wi ON w.id = wi.warehouse_id
    LEFT JOIN nomenclatures n ON wi.nomenclature_id = n.id
    WHERE wi.quantity > 0
    GROUP BY w.id, n.id
    ORDER BY w.name, n.name
");
$stmt->execute();
$warehouseItems = $stmt->fetchAll();

// Заполняем товары по складам и подсчитываем статистику
$totalItems = 0;
$totalValue = 0;

foreach ($warehouseItems as $item) {
    $warehouseId = $item['warehouse_id'];
    
    if (isset($warehouses[$warehouseId])) {
        $warehouses[$warehouseId]['items'][] = $item;
        $warehouses[$warehouseId]['total_quantity'] += $item['total_quantity'];
        $warehouses[$warehouseId]['total_value'] += $item['total_quantity'] * $item['avg_price'];
        $warehouses[$warehouseId]['item_count']++;
        
        $totalItems += $item['total_quantity'];
        $totalValue += $item['total_quantity'] * $item['avg_price'];
    }
}

// Получаем список всех номенклатур для фильтрации
$stmt = $pdo->query("SELECT id, name, barcode FROM nomenclatures ORDER BY name");
$allNomenclatures = $stmt->fetchAll();

// Фильтрация
$warehouseFilter = $_GET['warehouse_id'] ?? '';
$nomenclatureFilter = $_GET['nomenclature_id'] ?? '';
$searchTerm = $_GET['search'] ?? '';

// Подготавливаем данные для отображения
$displayWarehouses = $warehouses;
$filteredItems = [];

if ($warehouseFilter || $nomenclatureFilter || $searchTerm) {
    foreach ($warehouses as $warehouseId => $warehouse) {
        if ($warehouseFilter && $warehouseId != $warehouseFilter) {
            unset($displayWarehouses[$warehouseId]);
            continue;
        }
        
        // Фильтруем товары в складе
        $filteredItemsInWarehouse = [];
        foreach ($warehouse['items'] as $item) {
            $match = true;
            
            if ($nomenclatureFilter && $item['nomenclature_id'] != $nomenclatureFilter) {
                $match = false;
            }
            
            if ($searchTerm) {
                $searchLower = mb_strtolower($searchTerm, 'UTF-8');
                $itemName = mb_strtolower($item['nomenclature_name'], 'UTF-8');
                $itemBarcode = mb_strtolower($item['barcode'], 'UTF-8');
                $itemDescription = mb_strtolower($item['description'], 'UTF-8');
                
                if (strpos($itemName, $searchLower) === false && 
                    strpos($itemBarcode, $searchLower) === false &&
                    strpos($itemDescription, $searchLower) === false) {
                    $match = false;
                }
            }
            
            if ($match) {
                $filteredItemsInWarehouse[] = $item;
                $filteredItems[] = $item;
            }
        }
        
        $displayWarehouses[$warehouseId]['items'] = $filteredItemsInWarehouse;
    }
} else {
    // Собираем все товары для отображения
    foreach ($warehouses as $warehouse) {
        foreach ($warehouse['items'] as $item) {
            $filteredItems[] = $item;
        }
    }
}

// Подсчитываем количество складов с товарами
$warehousesWithItems = array_filter($warehouses, function($w) {
    return $w['item_count'] > 0;
});
?>

<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Управление складом - RunaRMK</title>
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
        
        .alert-info {
            background: linear-gradient(135deg, #d1ecf1 0%, #bee5eb 100%);
            color: #0c5460;
            border: 2px solid #17a2b8;
        }
        
        .alert-warning {
            background: linear-gradient(135deg, #fff3cd 0%, #ffeaa7 100%);
            color: #856404;
            border: 2px solid #ffc107;
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
        
        .stat-card.items::before {
            background: linear-gradient(90deg, var(--primary), #3a56d4);
        }
        
        .stat-card.value::before {
            background: linear-gradient(90deg, var(--secondary), #27ae60);
        }
        
        .stat-card.warehouses::before {
            background: linear-gradient(90deg, var(--warning), #e67e22);
        }
        
        .stat-card.active::before {
            background: linear-gradient(90deg, var(--danger), #c0392b);
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
        
        .stat-icon.items {
            background: linear-gradient(135deg, #f0f7ff 0%, #d4e4ff 100%);
            color: var(--primary);
        }
        
        .stat-icon.value {
            background: linear-gradient(135deg, #f0fff4 0%, #dcffe4 100%);
            color: var(--secondary);
        }
        
        .stat-icon.warehouses {
            background: linear-gradient(135deg, #fff8e1 0%, #ffeaa7 100%);
            color: var(--warning);
        }
        
        .stat-icon.active {
            background: linear-gradient(135deg, #ffeaea 0%, #ffd4d4 100%);
            color: var(--danger);
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
        
        .action-panel {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            margin-bottom: 40px;
            box-shadow: var(--shadow-md);
            border: 2px solid rgba(67, 97, 238, 0.1);
            position: relative;
            overflow: hidden;
        }
        
        .action-panel::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--secondary), var(--primary));
        }
        
        .action-panel h2 {
            color: var(--dark);
            font-size: 24px;
            font-weight: 700;
            margin-bottom: 25px;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .action-buttons {
            display: flex;
            gap: 20px;
            flex-wrap: wrap;
        }
        
        .filter-panel {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            margin-bottom: 40px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
            position: relative;
            overflow: hidden;
        }
        
        .filter-panel::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--info), #2980b9);
        }
        
        .filter-panel h3 {
            color: var(--dark);
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 25px;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .filter-form {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 20px;
            align-items: end;
        }
        
        .form-group {
            margin-bottom: 0;
        }
        
        .form-group label {
            display: block;
            color: var(--dark);
            font-weight: 600;
            margin-bottom: 10px;
            font-size: 15px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        
        .form-control {
            width: 100%;
            padding: 14px 20px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 15px;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.3s;
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
        
        .warehouses-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
            gap: 25px;
            margin-bottom: 40px;
        }
        
        .warehouse-card {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid transparent;
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            position: relative;
            overflow: hidden;
        }
        
        .warehouse-card:hover {
            transform: translateY(-8px);
            box-shadow: var(--shadow-lg);
            border-color: var(--primary);
        }
        
        .warehouse-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
        }
        
        .warehouse-card.has-items::before {
            background: linear-gradient(90deg, var(--primary), #3a56d4);
        }
        
        .warehouse-card.empty::before {
            background: linear-gradient(90deg, #95a5a6, #7f8c8d);
        }
        
        .warehouse-header {
            display: flex;
            align-items: center;
            gap: 15px;
            margin-bottom: 20px;
        }
        
        .warehouse-icon {
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
        
        .warehouse-info h3 {
            color: var(--dark);
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 8px;
            line-height: 1.4;
        }
        
        .warehouse-location {
            color: #64748b;
            font-size: 14px;
            display: flex;
            align-items: center;
            gap: 6px;
        }
        
        .warehouse-stats {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 15px;
            margin: 25px 0;
            padding: 20px;
            background: #f8fafc;
            border-radius: 12px;
        }
        
        .warehouse-stat {
            text-align: center;
        }
        
        .warehouse-stat-value {
            font-size: 24px;
            font-weight: 800;
            color: var(--primary);
            margin-bottom: 5px;
        }
        
        .warehouse-stat-label {
            color: #64748b;
            font-size: 13px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .warehouse-actions {
            display: flex;
            gap: 12px;
            margin-top: 20px;
        }
        
        .btn-view {
            background: linear-gradient(135deg, rgba(67, 97, 238, 0.1) 0%, rgba(67, 97, 238, 0.05) 100%);
            color: var(--primary);
            border: 1px solid rgba(67, 97, 238, 0.3);
            padding: 8px 16px;
            font-size: 14px;
        }
        
        .btn-view:hover {
            background: var(--primary);
            color: white;
        }
        
        .products-table-container {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
            margin-bottom: 40px;
            overflow-x: auto;
            position: relative;
        }
        
        .products-table-container::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--warning), #e67e22);
        }
        
        .table-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 25px;
        }
        
        .table-header h2 {
            color: var(--dark);
            font-size: 24px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .table-count {
            background: var(--light-blue);
            color: var(--primary);
            padding: 8px 16px;
            border-radius: 30px;
            font-weight: 700;
            font-size: 14px;
            display: flex;
            align-items: center;
            gap: 6px;
        }
        
        .products-table {
            width: 100%;
            border-collapse: collapse;
        }
        
        .products-table thead {
            background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
        }
        
        .products-table th {
            padding: 20px;
            text-align: left;
            font-weight: 700;
            color: var(--dark);
            font-size: 14px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            border-bottom: 2px solid #e2e8f0;
        }
        
        .products-table tbody tr {
            transition: all 0.3s;
            border-bottom: 1px solid #f1f5f9;
        }
        
        .products-table tbody tr:hover {
            background: #f8fafc;
        }
        
        .products-table td {
            padding: 20px;
            color: #334155;
        }
        
        .product-name {
            font-weight: 700;
            color: var(--dark);
            font-size: 16px;
        }
        
        .product-description {
            color: #64748b;
            font-size: 14px;
            margin-top: 5px;
            line-height: 1.4;
        }
        
        .badge {
            display: inline-flex;
            align-items: center;
            gap: 6px;
            padding: 8px 16px;
            border-radius: 30px;
            font-weight: 700;
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .badge-success {
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.15) 0%, rgba(46, 204, 113, 0.05) 100%);
            color: var(--secondary);
            border: 2px solid rgba(46, 204, 113, 0.3);
        }
        
        .badge-warning {
            background: linear-gradient(135deg, rgba(243, 156, 18, 0.15) 0%, rgba(243, 156, 18, 0.05) 100%);
            color: var(--warning);
            border: 2px solid rgba(243, 156, 18, 0.3);
        }
        
        .badge-info {
            background: linear-gradient(135deg, rgba(52, 152, 219, 0.15) 0%, rgba(52, 152, 219, 0.05) 100%);
            color: var(--info);
            border: 2px solid rgba(52, 152, 219, 0.3);
        }
        
        .product-actions {
            display: flex;
            gap: 8px;
        }
        
        .action-btn {
            width: 36px;
            height: 36px;
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            border: none;
            cursor: pointer;
            transition: all 0.3s;
            font-size: 14px;
        }
        
        .action-btn.receive {
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.1) 0%, rgba(46, 204, 113, 0.05) 100%);
            color: var(--secondary);
            border: 1px solid rgba(46, 204, 113, 0.3);
        }
        
        .action-btn.receive:hover {
            background: var(--secondary);
            color: white;
        }
        
        .action-btn.delete {
            background: linear-gradient(135deg, rgba(231, 76, 60, 0.1) 0%, rgba(231, 76, 60, 0.05) 100%);
            color: var(--danger);
            border: 1px solid rgba(231, 76, 60, 0.3);
        }
        
        .action-btn.delete:hover {
            background: var(--danger);
            color: white;
        }
        
        .action-btn.edit {
            background: linear-gradient(135deg, rgba(67, 97, 238, 0.1) 0%, rgba(67, 97, 238, 0.05) 100%);
            color: var(--primary);
            border: 1px solid rgba(67, 97, 238, 0.3);
        }
        
        .action-btn.edit:hover {
            background: var(--primary);
            color: white;
        }
        
        .table-footer {
            background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
            padding: 20px;
            border-radius: 0 0 12px 12px;
            margin-top: -1px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-weight: 700;
            color: var(--dark);
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
            
            .warehouses-grid {
                grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
            }
            
            .filter-form {
                grid-template-columns: repeat(2, 1fr);
            }
        }
        
        @media (max-width: 992px) {
            .header {
                flex-direction: column;
                gap: 20px;
                text-align: center;
                padding: 25px 20px;
            }
            
            .warehouses-grid {
                grid-template-columns: 1fr;
            }
            
            .stats-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .filter-form {
                grid-template-columns: 1fr;
            }
            
            .action-buttons {
                flex-direction: column;
            }
        }
        
        @media (max-width: 768px) {
            .table-header {
                flex-direction: column;
                align-items: stretch;
                gap: 15px;
            }
            
            .products-table {
                display: block;
                overflow-x: auto;
            }
            
            .warehouse-stats {
                grid-template-columns: 1fr;
                gap: 10px;
            }
            
            .stats-grid {
                grid-template-columns: 1fr;
            }
            
            .product-actions {
                flex-wrap: wrap;
                justify-content: center;
            }
        }
        
        @media (max-width: 576px) {
            .container {
                padding: 0 15px 30px;
            }
            
            .stat-value {
                font-size: 32px;
            }
            
            .warehouse-stat-value {
                font-size: 20px;
            }
            
            .products-table th,
            .products-table td {
                padding: 12px;
                font-size: 13px;
            }
        }
    </style>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">
</head>
<body>
    <!-- Шапка -->
    <div class="header">
        <h1>
            <i class="fas fa-warehouse"></i>
            Управление складом
        </h1>
        <a href="../../index.php" class="btn btn-primary">
            <i class="fas fa-arrow-left"></i> На главную
        </a>
    </div>
    
    <!-- Основной контент -->
    <div class="container">
        <!-- Панель действий -->
        <div class="action-panel fade-in-up" style="animation-delay: 0.1s;">
            <h2><i class="fas fa-bolt"></i> Быстрые действия</h2>
            <div class="action-buttons">
                <a href="receive.php" class="btn btn-secondary">
                    <i class="fas fa-plus-circle"></i> Оприходование товара
                </a>
                <a href="receive_sim.php" class="btn btn-secondary">
                    <i class="fas fa-plus-circle"></i> Оприходование Sim Карт
                </a>
                <a href="delete.php" class="btn btn-danger">
                    <i class="fas fa-minus-circle"></i> Списание товара
                </a>
                <button class="btn btn-info" onclick="window.print()">
                    <i class="fas fa-print"></i> Печать отчета
                </button>
                <button class="btn btn-warning" onclick="exportToExcel()">
                    <i class="fas fa-file-excel"></i> Экспорт в Excel
                </button>
            </div>
        </div>
        
        <!-- Статистика -->
        <div class="stats-grid fade-in-up" style="animation-delay: 0.2s;">
            <div class="stat-card items">
                <div class="stat-icon items">
                    <i class="fas fa-boxes"></i>
                </div>
                <h3>Всего единиц товара</h3>
                <div class="stat-value"><?php echo number_format($totalItems, 0, '.', ' '); ?></div>
                <div style="color: #64748b; font-size: 14px;">На всех складах</div>
            </div>
            
            <div class="stat-card value">
                <div class="stat-icon value">
                    <i class="fas fa-money-bill-wave"></i>
                </div>
                <h3>Общая стоимость</h3>
                <div class="stat-value"><?php echo number_format($totalValue, 2, '.', ' '); ?> ₽</div>
                <div style="color: #64748b; font-size: 14px;">Рыночная стоимость</div>
            </div>
            
            <div class="stat-card warehouses">
                <div class="stat-icon warehouses">
                    <i class="fas fa-warehouse"></i>
                </div>
                <h3>Всего складов</h3>
                <div class="stat-value"><?php echo count($allWarehouses); ?></div>
                <div style="color: #64748b; font-size: 14px;">В системе</div>
            </div>
            
            <div class="stat-card active">
                <div class="stat-icon active">
                    <i class="fas fa-box-open"></i>
                </div>
                <h3>Складов с товарами</h3>
                <div class="stat-value"><?php echo count($warehousesWithItems); ?></div>
                <div style="color: #64748b; font-size: 14px;">Не пустые склады</div>
            </div>
        </div>
        
        <!-- Фильтры -->
        <div class="filter-panel fade-in-up" style="animation-delay: 0.3s;">
            <h3><i class="fas fa-filter"></i> Фильтры поиска</h3>
            <form method="GET" class="filter-form">
                <div class="form-group">
                    <label for="warehouse_id"><i class="fas fa-warehouse"></i> Склад</label>
                    <select name="warehouse_id" id="warehouse_id" class="form-control">
                        <option value="">Все склады</option>
                        <?php foreach ($allWarehouses as $wh): ?>
                        <option value="<?php echo $wh['id']; ?>" <?php echo $warehouseFilter == $wh['id'] ? 'selected' : ''; ?>>
                            <?php echo htmlspecialchars($wh['name']); ?> (<?php echo htmlspecialchars($wh['division_name']); ?>)
                        </option>
                        <?php endforeach; ?>
                    </select>
                </div>
                
                <div class="form-group">
                    <label for="nomenclature_id"><i class="fas fa-barcode"></i> Номенклатура</label>
                    <select name="nomenclature_id" id="nomenclature_id" class="form-control">
                        <option value="">Вся номенклатура</option>
                        <?php foreach ($allNomenclatures as $nom): ?>
                        <option value="<?php echo $nom['id']; ?>" <?php echo $nomenclatureFilter == $nom['id'] ? 'selected' : ''; ?>>
                            <?php echo htmlspecialchars($nom['name']); ?> (<?php echo htmlspecialchars($nom['barcode']); ?>)
                        </option>
                        <?php endforeach; ?>
                    </select>
                </div>
                
                <div class="form-group">
                    <label for="search"><i class="fas fa-search"></i> Поиск</label>
                    <input type="text" name="search" id="search" class="form-control" 
                           placeholder="Название, штрих-код, описание..." 
                           value="<?php echo htmlspecialchars($searchTerm); ?>">
                </div>
                
                <div class="form-group">
                    <button type="submit" class="btn btn-primary" style="width: 100%;">
                        <i class="fas fa-filter"></i> Применить фильтры
                    </button>
                    <?php if ($warehouseFilter || $nomenclatureFilter || $searchTerm): ?>
                    <a href="view.php" class="btn" style="width: 100%; margin-top: 10px; background: #f8fafc; color: #64748b;">
                        <i class="fas fa-times"></i> Сбросить фильтры
                    </a>
                    <?php endif; ?>
                </div>
            </form>
        </div>
        
        <!-- Карточки складов -->
        <?php if (count($allWarehouses) > 0): ?>
        <div class="fade-in-up" style="animation-delay: 0.4s;">
            <h2 style="margin-bottom: 25px; color: var(--dark); display: flex; align-items: center; gap: 12px;">
                <i class="fas fa-warehouse"></i>
                Склады (<?php echo count($allWarehouses); ?>)
            </h2>
            <div class="warehouses-grid">
                <?php foreach ($allWarehouses as $warehouse): 
                    $whData = $warehouses[$warehouse['id']] ?? ['total_quantity' => 0, 'item_count' => 0, 'total_value' => 0];
                    $isEmpty = $whData['item_count'] == 0;
                ?>
                <div class="warehouse-card <?php echo $isEmpty ? 'empty' : 'has-items'; ?>">
                    <div class="warehouse-header">
                        <div class="warehouse-icon">
                            <i class="fas fa-warehouse"></i>
                        </div>
                        <div class="warehouse-info">
                            <h3><?php echo htmlspecialchars($warehouse['name']); ?></h3>
                            <div class="warehouse-location">
                                <i class="fas fa-map-marker-alt"></i>
                                <?php echo htmlspecialchars($warehouse['division_name']); ?>
                            </div>
                        </div>
                    </div>
                    
                    <div class="warehouse-stats">
                        <div class="warehouse-stat">
                            <div class="warehouse-stat-value"><?php echo $whData['item_count']; ?></div>
                            <div class="warehouse-stat-label">Позиций</div>
                        </div>
                        <div class="warehouse-stat">
                            <div class="warehouse-stat-value"><?php echo number_format($whData['total_quantity'], 0, '.', ' '); ?></div>
                            <div class="warehouse-stat-label">Единиц</div>
                        </div>
                        <div class="warehouse-stat">
                            <div class="warehouse-stat-value"><?php echo number_format($whData['total_value'], 0, '.', ' '); ?> ₽</div>
                            <div class="warehouse-stat-label">Стоимость</div>
                        </div>
                    </div>
                    
                    <div class="warehouse-actions">
                        <a href="?warehouse_id=<?php echo $warehouse['id']; ?>" class="btn btn-view">
                            <i class="fas fa-eye"></i> Показать товары
                        </a>
                    </div>
                </div>
                <?php endforeach; ?>
            </div>
        </div>
        <?php endif; ?>
        
        <!-- Таблица товаров -->
        <div class="products-table-container fade-in-up" style="animation-delay: 0.5s;">
            <div class="table-header">
                <h2><i class="fas fa-boxes"></i> Товары на складах</h2>
                <div class="table-count">
                    <i class="fas fa-box"></i>
                    <?php echo count($filteredItems); ?> позиций
                </div>
            </div>
            
            <?php if (count($filteredItems) > 0): ?>
            <div style="overflow-x: auto;">
                <table class="products-table">
                    <thead>
                        <tr>
                            <th>Товар</th>
                            <th>Штрих-код</th>
                            <th>Склад</th>
                            <th>Количество</th>
                            <th>Ср. цена</th>
                            <th>Стоимость</th>
                            <th>Посл. поступление</th>
                            <th>Действия</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($filteredItems as $item): 
                        $totalCost = $item['total_quantity'] * $item['avg_price'];
                        $lastReceived = $item['last_received'] ? date('d.m.Y', strtotime($item['last_received'])) : '—';
                        ?>
                        <tr>
                            <td>
                                <div class="product-name"><?php echo htmlspecialchars($item['nomenclature_name']); ?></div>
                                <?php if ($item['description']): ?>
                                <div class="product-description">
                                    <?php echo htmlspecialchars(substr($item['description'], 0, 100)); ?>
                                    <?php if (strlen($item['description']) > 100): ?>...<?php endif; ?>
                                </div>
                                <?php endif; ?>
                            </td>
                            <td>
                                <span class="badge badge-info">
                                    <i class="fas fa-barcode"></i> <?php echo htmlspecialchars($item['barcode']); ?>
                                </span>
                            </td>
                            <td>
                                <div style="font-weight: 600; color: var(--dark);"><?php echo htmlspecialchars($item['warehouse_name']); ?></div>
                                <div style="font-size: 13px; color: #64748b;"><?php echo htmlspecialchars($item['division_name']); ?></div>
                            </td>
                            <td>
                                <span class="badge <?php echo $item['total_quantity'] > 10 ? 'badge-success' : 'badge-warning'; ?>">
                                    <?php echo number_format($item['total_quantity'], 0, '.', ' '); ?> шт.
                                </span>
                            </td>
                            <td>
                                <div style="font-weight: 700; color: var(--dark);"><?php echo number_format($item['avg_price'], 2, '.', ' '); ?> ₽</div>
                            </td>
                            <td>
                                <div style="font-weight: 700; color: var(--secondary);"><?php echo number_format($totalCost, 2, '.', ' '); ?> ₽</div>
                            </td>
                            <td>
                                <div style="color: #64748b; font-size: 14px;"><?php echo $lastReceived; ?></div>
                            </td>
                            <td>
                                <div class="product-actions">
                                    <a href="receive.php?nomenclature_id=<?php echo $item['nomenclature_id']; ?>&warehouse_id=<?php echo $item['warehouse_id']; ?>" 
                                       class="action-btn receive" title="Оприходовать">
                                        <i class="fas fa-plus"></i>
                                    </a>
                                    <a href="delete.php?nomenclature_id=<?php echo $item['nomenclature_id']; ?>&warehouse_id=<?php echo $item['warehouse_id']; ?>" 
                                       class="action-btn delete" title="Списать">
                                        <i class="fas fa-minus"></i>
                                    </a>
                                    <a href="../nomenclatures/edit.php?id=<?php echo $item['nomenclature_id']; ?>" 
                                       class="action-btn edit" title="Редактировать">
                                        <i class="fas fa-edit"></i>
                                    </a>
                                </div>
                            </td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
            </div>
            
            <div class="table-footer">
                <div>Итого по фильтру:</div>
                <div>
                    <?php 
                    $filteredTotalQty = array_sum(array_column($filteredItems, 'total_quantity'));
                    echo number_format($filteredTotalQty, 0, '.', ' ');
                    ?> шт.
                    | 
                    <?php 
                    $filteredTotalCost = 0;
                    foreach ($filteredItems as $item) {
                        $filteredTotalCost += $item['total_quantity'] * $item['avg_price'];
                    }
                    echo number_format($filteredTotalCost, 2, '.', ' ');
                    ?> ₽
                </div>
            </div>
            
            <?php else: ?>
            <div class="empty-state">
                <i class="fas fa-box-open"></i>
                <h3>Товары не найдены</h3>
                <p>
                    <?php if ($warehouseFilter || $nomenclatureFilter || $searchTerm): ?>
                    Попробуйте изменить параметры поиска или
                    <a href="view.php" style="color: var(--primary); font-weight: 600;">показать все товары</a>
                    <?php else: ?>
                    На складах пока нет товаров. Начните с оприходования товаров.
                    <?php endif; ?>
                </p>
                
                <?php if (!($warehouseFilter || $nomenclatureFilter || $searchTerm)): ?>
                <a href="receive.php" class="btn btn-secondary" style="margin-top: 15px;">
                    <i class="fas fa-plus-circle"></i> Оприходовать первый товар
                </a>
                <?php endif; ?>
            </div>
            <?php endif; ?>
        </div>
        
        <!-- Информационный блок -->
        <div class="alert alert-info fade-in-up" style="animation-delay: 0.6s;">
            <i class="fas fa-info-circle fa-2x"></i>
            <div>
                <strong style="font-size: 18px; color: var(--dark);">Управление складом</strong>
                <div style="margin-top: 10px; line-height: 1.6;">
                    <p><strong>Оприходование</strong> — добавление товара на склад при поступлении от поставщика</p>
                    <p><strong>Списание</strong> — удаление товара со склада при продаже, порче или ином выбытии</p>
                    <p><strong>Инвентаризация</strong> — сверка фактического наличия товара с учетными данными</p>
                </div>
            </div>
        </div>
    </div>
    
    <script>
        // Экспорт в Excel
        function exportToExcel() {
            const table = document.querySelector('.products-table');
            if (!table) return;
            
            let csv = [];
            // Заголовки
            const headers = [];
            table.querySelectorAll('th').forEach(th => {
                headers.push(th.textContent.trim());
            });
            csv.push(headers.join(','));
            
            // Данные
            table.querySelectorAll('tbody tr').forEach(row => {
                const rowData = [];
                row.querySelectorAll('td').forEach((td, index) => {
                    if (index !== 7) { // Пропускаем колонку действий
                        let text = td.textContent.trim();
                        // Убираем лишние пробелы и символы
                        text = text.replace(/\s+/g, ' ').replace(/[^\w\s.,₽-]/g, '');
                        rowData.push(`"${text}"`);
                    }
                });
                csv.push(rowData.join(','));
            });
            
            // Создаем и скачиваем файл
            const csvContent = "data:text/csv;charset=utf-8," + csv.join('\n');
            const encodedUri = encodeURI(csvContent);
            const link = document.createElement("a");
            link.setAttribute("href", encodedUri);
            link.setAttribute("download", `склад_${new Date().toISOString().slice(0,10)}.csv`);
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
        
        // Автоматическое обновление каждые 5 минут
        setInterval(() => {
            console.log('Автоматическое обновление данных склада...');
            // Можно добавить AJAX обновление данных
        }, 300000);
        
        // Анимация появления элементов
        document.addEventListener('DOMContentLoaded', function() {
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('fade-in-up');
                    }
                });
            }, { threshold: 0.1 });
            
            document.querySelectorAll('.stat-card, .action-panel, .filter-panel, .warehouse-card, .products-table-container, .alert').forEach(el => {
                observer.observe(el);
            });
            
            // Быстрый поиск по таблице
            const searchInput = document.getElementById('search');
            if (searchInput) {
                searchInput.addEventListener('input', function() {
                    const searchTerm = this.value.toLowerCase();
                    const rows = document.querySelectorAll('.products-table tbody tr');
                    
                    rows.forEach(row => {
                        const rowText = row.textContent.toLowerCase();
                        row.style.display = rowText.includes(searchTerm) ? '' : 'none';
                    });
                });
            }
        });
        
        // Печать отчета
        function printReport() {
            const printContent = document.querySelector('.container').innerHTML;
            const originalContent = document.body.innerHTML;
            
            document.body.innerHTML = `
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Отчет по складу - ${new Date().toLocaleDateString()}</title>
                    <style>
                        body { font-family: Arial, sans-serif; margin: 20px; }
                        h1, h2, h3 { color: #333; }
                        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
                        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                        th { background-color: #f2f2f2; }
                        .no-print { display: none; }
                        @media print {
                            .no-print { display: none; }
                            .print-only { display: block; }
                        }
                    </style>
                </head>
                <body>
                    <h1>Отчет по складу</h1>
                    <p>Дата формирования: ${new Date().toLocaleDateString()} ${new Date().toLocaleTimeString()}</p>
                    ${printContent}
                </body>
                </html>
            `;
            
            window.print();
            document.body.innerHTML = originalContent;
            window.location.reload();
        }
        
        // Заменяем стандартный window.print на нашу функцию
        window.print = printReport;
    </script>
</body>
</html>