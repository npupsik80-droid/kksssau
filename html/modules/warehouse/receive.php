<?php
require_once '../../includes/auth_check.php';

// Проверяем выбрано ли подразделение
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

// Если складов нет, создаем основной для текущего подразделения
if (count($warehouses) === 0) {
    $stmt = $pdo->prepare("
        INSERT INTO warehouses (division_id, name) 
        VALUES (?, 'Основной склад')
    ");
    $stmt->execute([$_SESSION['current_division_id']]);
    $warehouse_id = $pdo->lastInsertId();
    
    // Получаем созданный склад с информацией о подразделении
    $stmt = $pdo->prepare("
        SELECT w.*, d.name as division_name 
        FROM warehouses w 
        LEFT JOIN divisions d ON w.division_id = d.id 
        WHERE w.id = ?
    ");
    $stmt->execute([$warehouse_id]);
    $warehouses = $stmt->fetchAll();
    
    $_SESSION['current_warehouse_id'] = $warehouse_id;
}

// Если не выбран склад - выбираем первый
if (!isset($_SESSION['current_warehouse_id']) && count($warehouses) > 0) {
    $_SESSION['current_warehouse_id'] = $warehouses[0]['id'];
}

// Проверяем, что текущий склад существует в списке складов
$current_warehouse_exists = false;
$current_warehouse_id = $_SESSION['current_warehouse_id'] ?? 0;

foreach ($warehouses as $warehouse) {
    if ($warehouse['id'] == $current_warehouse_id) {
        $current_warehouse_exists = true;
        break;
    }
}

// Если текущий склад не существует в списке, выбираем первый
if (!$current_warehouse_exists && count($warehouses) > 0) {
    $_SESSION['current_warehouse_id'] = $warehouses[0]['id'];
}

// Получаем номенклатуры для автодополнения
$stmt = $pdo->prepare("SELECT * FROM nomenclatures ORDER BY name");
$stmt->execute();
$nomenclatures = $stmt->fetchAll();
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Оприходование товара - RunaRMK</title>
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
            background: linear-gradient(135deg, #64748b 0%, #475569 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(100, 116, 139, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-secondary:hover {
            background: linear-gradient(135deg, #475569 0%, #334155 100%);
            transform: translateY(-3px);
            box-shadow: 0 12px 25px rgba(100, 116, 139, 0.4);
        }
        
        .btn-success {
            background: linear-gradient(135deg, var(--secondary) 0%, #27ae60 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(46, 204, 113, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-success:hover {
            background: linear-gradient(135deg, #27ae60 0%, #219653 100%);
            transform: translateY(-3px);
            box-shadow: 0 12px 25px rgba(46, 204, 113, 0.4);
        }
        
        .container {
            max-width: 1400px;
            margin: 0 auto;
            padding: 0 30px 50px;
        }
        
        .fade-in-up {
            animation: fadeInUp 0.6s cubic-bezier(0.4, 0, 0.2, 1);
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
        
        .warehouse-selector {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            margin-bottom: 30px;
            box-shadow: var(--shadow-md);
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
        
        .warehouse-selector h3 {
            color: var(--dark);
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .select-wrapper {
            position: relative;
        }
        
        .select-wrapper i {
            position: absolute;
            left: 20px;
            top: 50%;
            transform: translateY(-50%);
            color: var(--primary);
            z-index: 1;
        }
        
        .warehouse-selector select {
            width: 100%;
            padding: 16px 20px 16px 50px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 16px;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.3s;
            appearance: none;
            background-image: url("data:image/svg+xml;charset=UTF-8,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3e%3cpolyline points='6 9 12 15 18 9'%3e%3c/polyline%3e%3c/svg%3e");
            background-repeat: no-repeat;
            background-position: right 20px center;
            background-size: 20px;
            cursor: pointer;
        }
        
        .warehouse-selector select:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .scanner-section {
            background: white;
            border-radius: var(--border-radius);
            padding: 40px;
            margin-bottom: 30px;
            box-shadow: var(--shadow-md);
            border: 2px solid rgba(67, 97, 238, 0.1);
            text-align: center;
            position: relative;
            overflow: hidden;
        }
        
        .scanner-section::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--warning), #e67e22);
        }
        
        .scanner-section h2 {
            color: var(--dark);
            font-size: 24px;
            font-weight: 700;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
        }
        
        .scanner-help {
            color: #64748b;
            font-size: 15px;
            margin-bottom: 25px;
            line-height: 1.6;
        }
        
        .scanner-input {
            width: 100%;
            max-width: 600px;
            padding: 20px;
            font-size: 20px;
            border: 3px solid var(--primary);
            border-radius: 12px;
            margin: 25px auto;
            text-align: center;
            background: #f8fafc;
            color: var(--dark);
            font-weight: 600;
            transition: all 0.3s;
        }
        
        .scanner-input:focus {
            outline: none;
            border-color: var(--info);
            box-shadow: 0 0 0 4px rgba(52, 152, 219, 0.2);
            background: white;
            transform: translateY(-2px);
        }
        
        .alert {
            padding: 20px 25px;
            border-radius: var(--border-radius);
            margin-bottom: 25px;
            display: none;
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
        
        .alert-error {
            background: linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%);
            color: #dc2626;
            border: 2px solid #ef4444;
        }
        
        .alert-success {
            background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);
            color: #16a34a;
            border: 2px solid #22c55e;
        }
        
        .products-table-container {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-md);
            border: 2px solid rgba(67, 97, 238, 0.1);
            margin-bottom: 30px;
            position: relative;
            overflow: hidden;
        }
        
        .products-table-container::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--secondary), #27ae60);
        }
        
        .table-header {
            display: grid;
            grid-template-columns: 3fr 1fr 1fr 1fr 1fr 70px;
            background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
            padding: 20px;
            border-radius: 12px;
            font-weight: 700;
            color: var(--dark);
            font-size: 14px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 10px;
        }
        
        .table-row {
            display: grid;
            grid-template-columns: 3fr 1fr 1fr 1fr 1fr 70px;
            padding: 20px;
            border-bottom: 1px solid #f1f5f9;
            align-items: center;
            transition: all 0.3s;
        }
        
        .table-row:hover {
            background: #f8fafc;
            transform: translateX(4px);
        }
        
        .no-products {
            text-align: center;
            padding: 60px 30px;
            color: #64748b;
        }
        
        .no-products i {
            font-size: 64px;
            color: #cbd5e1;
            margin-bottom: 25px;
        }
        
        .no-products h3 {
            color: #64748b;
            font-size: 22px;
            font-weight: 700;
            margin-bottom: 15px;
        }
        
        .no-products p {
            color: #94a3b8;
            font-size: 16px;
            margin-bottom: 25px;
            line-height: 1.6;
        }
        
        .quantity-control {
            display: flex;
            align-items: center;
            gap: 12px;
            justify-content: center;
        }
        
        .qty-btn {
            width: 36px;
            height: 36px;
            border-radius: 10px;
            border: 2px solid #e2e8f0;
            background: white;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 800;
            font-size: 18px;
            color: var(--dark);
            transition: all 0.3s;
        }
        
        .qty-btn:hover {
            border-color: var(--primary);
            color: var(--primary);
            transform: scale(1.1);
        }
        
        .qty-value {
            min-width: 40px;
            text-align: center;
            font-weight: 800;
            font-size: 18px;
            color: var(--dark);
        }
        
        .price-input {
            width: 100%;
            padding: 12px 16px;
            border: 2px solid #e2e8f0;
            border-radius: 10px;
            text-align: right;
            font-size: 16px;
            font-weight: 600;
            color: var(--dark);
            background: #f8fafc;
            transition: all 0.3s;
        }
        
        .price-input:focus {
            outline: none;
            border-color: var(--primary);
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .remove-btn {
            width: 40px;
            height: 40px;
            border-radius: 10px;
            border: none;
            background: linear-gradient(135deg, rgba(239, 68, 68, 0.1) 0%, rgba(239, 68, 68, 0.05) 100%);
            color: #ef4444;
            cursor: pointer;
            font-size: 18px;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.3s;
        }
        
        .remove-btn:hover {
            background: #ef4444;
            color: white;
            transform: scale(1.1);
        }
        
        .totals-section {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            margin-bottom: 30px;
            box-shadow: var(--shadow-md);
            border: 2px solid rgba(67, 97, 238, 0.1);
            display: flex;
            justify-content: space-between;
            align-items: center;
            position: relative;
            overflow: hidden;
        }
        
        .totals-section::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--primary), #3a56d4);
        }
        
        .total-items {
            font-size: 18px;
            color: var(--dark);
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .total-items i {
            color: var(--primary);
            font-size: 24px;
        }
        
        .total-amount {
            font-size: 36px;
            font-weight: 800;
            color: var(--secondary);
            text-shadow: 0 4px 8px rgba(46, 204, 113, 0.2);
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .total-amount i {
            font-size: 32px;
        }
        
        .action-buttons {
            display: flex;
            gap: 20px;
            justify-content: flex-end;
            margin-top: 30px;
        }
        
        .action-buttons .btn {
            padding: 16px 32px;
            font-size: 16px;
            min-width: 200px;
        }
        
        .modal-overlay {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0,0,0,0.7);
            backdrop-filter: blur(8px);
            display: none;
            justify-content: center;
            align-items: center;
            z-index: 1000;
            padding: 20px;
            animation: fadeIn 0.3s ease;
        }
        
        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }
        
        .modal-content {
            background: white;
            border-radius: var(--border-radius);
            padding: 40px;
            max-width: 500px;
            width: 100%;
            box-shadow: var(--shadow-lg);
            border: 2px solid rgba(67, 97, 238, 0.2);
            animation: slideUp 0.4s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        @keyframes slideUp {
            from {
                transform: translateY(50px);
                opacity: 0;
            }
            to {
                transform: translateY(0);
                opacity: 1;
            }
        }
        
        .modal-content h2 {
            color: var(--dark);
            font-size: 28px;
            font-weight: 800;
            margin-bottom: 25px;
            display: flex;
            align-items: center;
            gap: 12px;
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
            display: flex;
            align-items: center;
            gap: 8px;
        }
        
        .form-group input {
            width: 100%;
            padding: 16px 20px;
            border: 2px solid #e2e8f0;
            border-radius: 12px;
            font-size: 16px;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.3s;
        }
        
        .form-group input:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
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
        }
        
        @media (max-width: 992px) {
            .header {
                flex-direction: column;
                gap: 20px;
                text-align: center;
                padding: 25px 20px;
            }
            
            .action-buttons {
                flex-direction: column;
            }
            
            .action-buttons .btn {
                width: 100%;
                min-width: unset;
            }
            
            .table-header,
            .table-row {
                grid-template-columns: 2fr 1fr 1fr 1fr;
                font-size: 14px;
            }
            
            .table-header div:nth-child(5),
            .table-row div:nth-child(5),
            .table-header div:nth-child(6),
            .table-row div:nth-child(6) {
                display: none;
            }
        }
        
        @media (max-width: 768px) {
            .table-header,
            .table-row {
                grid-template-columns: 1fr 1fr;
                gap: 10px;
            }
            
            .table-header div:nth-child(3),
            .table-row div:nth-child(3),
            .table-header div:nth-child(4),
            .table-row div:nth-child(4) {
                display: none;
            }
            
            .totals-section {
                flex-direction: column;
                gap: 20px;
                text-align: center;
            }
            
            .scanner-section {
                padding: 30px 20px;
            }
            
            .scanner-input {
                font-size: 18px;
                padding: 16px;
            }
        }
        
        @media (max-width: 576px) {
            .container {
                padding: 0 15px 30px;
            }
            
            .total-amount {
                font-size: 28px;
            }
            
            .modal-content {
                padding: 25px;
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
            <i class="fas fa-boxes"></i>
            Оприходование товара
        </h1>
        <a href="./view.php" class="btn btn-primary">
            <i class="fas fa-arrow-left"></i> Назад к складу
        </a>
    </div>
    
    <div class="container">
        <!-- Выбор склада -->
        <div class="warehouse-selector fade-in-up" style="animation-delay: 0.1s;">
            <h3><i class="fas fa-warehouse"></i> Выбор склада для оприходования</h3>
            <div class="select-wrapper">
                <i class="fas fa-map-marker-alt"></i>
                <select id="warehouseSelect" onchange="changeWarehouse(this.value)">
                    <?php foreach ($warehouses as $warehouse): ?>
                    <option value="<?php echo $warehouse['id']; ?>" 
                            <?php echo (isset($_SESSION['current_warehouse_id']) && $_SESSION['current_warehouse_id'] == $warehouse['id']) ? 'selected' : ''; ?>>
                        <?php echo htmlspecialchars($warehouse['name']); ?> 
                        (<?php echo htmlspecialchars($warehouse['division_name']); ?>)
                    </option>
                    <?php endforeach; ?>
                </select>
            </div>
        </div>
        
        <!-- Сканер штрих-кодов -->
        <div class="scanner-section fade-in-up" style="animation-delay: 0.2s;">
            <h2><i class="fas fa-barcode"></i> Сканирование товаров</h2>
            <p class="scanner-help">
                Наведите сканер на штрих-код или введите вручную. Нажмите Enter после сканирования каждого товара
            </p>
            
            <input type="text" 
                   id="barcodeInput" 
                   class="scanner-input" 
                   placeholder="Отсканируйте штрих-код..."
                   autofocus>
            
            <p class="scanner-help" style="margin-top: 20px;">
                <i class="fas fa-info-circle"></i> Для ручного поиска введите минимум 3 символа
            </p>
        </div>
        
        <!-- Сообщения -->
        <div id="errorAlert" class="alert alert-error"></div>
        <div id="successAlert" class="alert alert-success"></div>
        
        <!-- Таблица товаров -->
        <div class="products-table-container fade-in-up" style="animation-delay: 0.3s;">
            <div class="table-header">
                <div>Наименование товара</div>
                <div>Штрих-код</div>
                <div>Количество</div>
                <div>Цена за шт.</div>
                <div>Сумма</div>
                <div></div>
            </div>
            
            <div id="productsContainer">
                <div class="no-products" id="noProductsMessage">
                    <i class="fas fa-box-open"></i>
                    <h3>Товары не добавлены</h3>
                    <p>Отсканируйте первый товар, чтобы добавить его в список</p>
                </div>
            </div>
        </div>
        
        <!-- Итоги -->
        <div class="totals-section fade-in-up" style="animation-delay: 0.4s;">
            <div class="total-items">
                <i class="fas fa-boxes"></i>
                <span id="totalItemsCount">0</span> товаров на сумму:
            </div>
            <div class="total-amount">
                <i class="fas fa-money-bill-wave"></i>
                <span id="totalAmount">0.00</span> ₽
            </div>
        </div>
        
        <!-- Кнопки действий -->
        <div class="action-buttons fade-in-up" style="animation-delay: 0.5s;">
            <button class="btn btn-secondary" onclick="clearAll()">
                <i class="fas fa-trash"></i> Очистить все
            </button>
            <button class="btn btn-success" onclick="saveReceipt()">
                <i class="fas fa-save"></i> Сохранить оприходование
            </button>
        </div>
    </div>
    
    <!-- Модальное окно создания номенклатуры -->
    <div id="createNomenclatureModal" class="modal-overlay">
        <div class="modal-content">
            <h2><i class="fas fa-plus-circle"></i> Создать номенклатуру</h2>
            
            <div class="form-group">
                <label><i class="fas fa-barcode"></i> Штрих-код *</label>
                <input type="text" id="newBarcode" readonly style="background: #f1f5f9;">
            </div>
            
            <div class="form-group">
                <label><i class="fas fa-tag"></i> Наименование товара *</label>
                <input type="text" id="newProductName" placeholder="Введите название товара" autofocus>
            </div>
            
            <div class="form-group">
                <label><i class="fas fa-align-left"></i> Описание (необязательно)</label>
                <input type="text" id="newProductDescription" placeholder="Описание товара">
            </div>
            
            <div class="action-buttons" style="margin-top: 30px; justify-content: space-between;">
                <button class="btn btn-secondary" onclick="closeCreateModal()" style="min-width: 150px;">
                    <i class="fas fa-times"></i> Отмена
                </button>
                <button class="btn btn-success" onclick="createNomenclature()" style="min-width: 150px;">
                    <i class="fas fa-save"></i> Создать
                </button>
            </div>
        </div>
    </div>
    
<script>
    // Глобальные переменные
    let products = {};
    let currentBarcode = '';
    let nomenclatureCache = <?php echo json_encode($nomenclatures); ?>;
    let isInFormField = false;
    let activePriceInput = null; // Отслеживаем активное поле ввода цены
    
    // Инициализация
    document.addEventListener('DOMContentLoaded', function() {
        // Фокус на поле сканирования при загрузке
        focusScanner();
        
        // Обработка сканера (Enter)
        document.getElementById('barcodeInput').addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                const barcode = this.value.trim();
                
                if (barcode) {
                    processBarcode(barcode);
                    this.value = '';
                    focusScanner();
                }
            }
        });
        
        // Автодополнение при вводе вручную
        let debounceTimer;
        document.getElementById('barcodeInput').addEventListener('input', function() {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                const value = this.value.trim();
                if (value.length >= 3) {
                    searchNomenclature(value);
                }
            }, 500);
        });
        
        // Отслеживаем фокус на полях формы
        document.addEventListener('focusin', function(e) {
            const target = e.target;
            
            // Проверяем, является ли элемент полем формы, которое НЕ должно терять фокус
            if (target.matches('.price-input, .qty-value, #warehouseSelect, #newProductName, #newProductDescription')) {
                isInFormField = true;
                if (target.classList.contains('price-input')) {
                    activePriceInput = target; // Запоминаем активное поле ввода цены
                }
            } else if (target.id === 'barcodeInput') {
                // Это поле сканера - тоже форма, но не мешаем ему
                isInFormField = true;
                activePriceInput = null; // Сбрасываем активное поле цены
            }
        });
        
        document.addEventListener('focusout', function(e) {
            if (e.target.classList.contains('price-input')) {
                // Обновляем цену при потере фокуса с поля ввода
                const productId = e.target.closest('.table-row').getAttribute('data-id');
                updatePrice(productId, e.target.value);
            }
        });
        
        // Автофокус только при клике на пустые области
        document.addEventListener('click', function(e) {
            // Определяем, кликнули ли мы на элемент, который НЕ должен триггерить автофокус
            const noFocusElements = [
                '.price-input',
                '.qty-btn',
                '.qty-value',
                '.remove-btn',
                '.btn',
                '.warehouse-selector',
                'select',
                'option',
                '.table-row',
                '.table-header',
                '.scanner-section',
                '.totals-section',
                '.action-buttons',
                '.products-table-container',
                '.alert',
                '.form-group',
                '.modal-content',
                '.quantity-control',
                '#createNomenclatureModal',
                '#errorAlert',
                '#successAlert'
            ];
            
            let shouldNotFocus = false;
            
            // Проверяем все элементы, которые не должны вызывать автофокус
            for (let selector of noFocusElements) {
                if (e.target.matches(selector) || e.target.closest(selector)) {
                    shouldNotFocus = true;
                    break;
                }
            }
            
            // Также проверяем по ID
            if (['warehouseSelect', 'newProductName', 'newProductDescription'].includes(e.target.id)) {
                shouldNotFocus = true;
            }
            
            // Если кликнули на self - тоже не фокусируем
            if (e.target.id === 'barcodeInput') {
                shouldNotFocus = true;
            }
            
            // Если клик был на не-интерактивном элементе - фокусируем сканер
            if (!shouldNotFocus) {
                // Но только если не находимся в поле формы
                if (!isInFormField && document.activeElement.id !== 'barcodeInput') {
                    // Небольшая задержка, чтобы дать браузеру обработать клик
                    setTimeout(focusScanner, 50);
                }
            }
        });
        
        // Делегирование событий для динамически созданных полей ввода цены
        document.addEventListener('input', function(e) {
            if (e.target.classList.contains('price-input')) {
                const productId = e.target.closest('.table-row').getAttribute('data-id');
                // Обновляем промежуточную цену в объекте, но не перерисовываем весь список
                updatePriceInMemory(productId, e.target.value);
            }
        });
    });
    
    // Функция фокусировки на сканере
    function focusScanner() {
        // Проверяем, не находимся ли мы в модальном окне
        const modal = document.getElementById('createNomenclatureModal');
        if (modal && modal.style.display === 'flex') {
            return; // Не фокусируем сканер, если открыто модальное окно
        }
        
        // Проверяем, не находится ли фокус на других важных элементах
        const activeElement = document.activeElement;
        if (activeElement && (
            activeElement.matches('.price-input') ||
            activeElement.matches('select') ||
            activeElement.matches('#newProductName') ||
            activeElement.matches('#newProductDescription')
        )) {
            return; // Не перехватываем фокус, если пользователь работает с формой
        }
        
        const scannerInput = document.getElementById('barcodeInput');
        if (scannerInput && scannerInput !== activeElement) {
            scannerInput.focus();
        }
    }
    
    // Смена склада
    function changeWarehouse(warehouseId) {
        fetch('../../api/change_warehouse.php', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ warehouse_id: warehouseId })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateWarehouseSession(warehouseId);
            }
        });
    }
    
    // Функция для обновления склада в сессии без перезагрузки страницы
    function updateWarehouseSession(warehouseId) {
        fetch('../../api/update_current_warehouse.php', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ warehouse_id: warehouseId })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('Склад обновлен в сессии:', warehouseId);
            }
        });
    }
    
    // Обработка штрих-кода
    async function processBarcode(barcode) {
        currentBarcode = barcode;
        
        // Ищем в кэше
        const nomenclature = nomenclatureCache.find(n => n.barcode === barcode);
        
        if (nomenclature) {
            addProductToList(nomenclature);
        } else {
            showCreateNomenclatureModal(barcode);
        }
    }
    
    // Поиск номенклатуры по названию
    function searchNomenclature(searchText) {
        const results = nomenclatureCache.filter(n => 
            n.name.toLowerCase().includes(searchText.toLowerCase()) ||
            n.barcode.includes(searchText)
        );
        
        if (results.length > 0) {
            console.log('Найдено:', results);
        }
    }
    
    // Добавление товара в список
    function addProductToList(nomenclature) {
        const productId = nomenclature.id;
        
        if (products[productId]) {
            products[productId].quantity++;
        } else {
            products[productId] = {
                id: productId,
                name: nomenclature.name,
                barcode: nomenclature.barcode,
                quantity: 1,
                price: "",
                total: 0
            };
        }
        
        updateProductsDisplay();
    }
    
    // Обновление отображения товаров
    function updateProductsDisplay() {
        const container = document.getElementById('productsContainer');
        
        if (Object.keys(products).length === 0) {
            container.innerHTML = `
                <div class="no-products" id="noProductsMessage">
                    <i class="fas fa-box-open"></i>
                    <h3>Товары не добавлены</h3>
                    <p>Отсканируйте первый товар, чтобы добавить его в список</p>
                </div>
            `;
            
            document.getElementById('totalItemsCount').textContent = 0;
            document.getElementById('totalAmount').textContent = '0.00';
        } else {
            let html = '';
            
            for (const productId in products) {
                const product = products[productId];
                const productPrice = parseFloat(product.price) || 0;
                const productTotal = productPrice * product.quantity;
                
                html += `
                    <div class="table-row" data-id="${productId}">
                        <div>
                            <strong style="color: var(--dark); font-size: 16px;">${product.name}</strong>
                            <div style="font-size: 13px; color: #64748b; margin-top: 5px;">${product.barcode}</div>
                        </div>
                        <div style="font-weight: 600; color: var(--dark);">${product.barcode}</div>
                        <div>
                            <div class="quantity-control">
                                <button class="qty-btn" onclick="changeQuantity('${productId}', -1)">-</button>
                                <span class="qty-value">${product.quantity}</span>
                                <button class="qty-btn" onclick="changeQuantity('${productId}', 1)">+</button>
                            </div>
                        </div>
                        <div>
                            <input type="number" 
                                   class="price-input" 
                                   value="${product.price || ''}"
                                   step="0.01" 
                                   min="0"
                                   onfocus="isInFormField = true; activePriceInput = this;">
                        </div>
                        <div style="font-weight: 800; color: var(--dark); font-size: 16px;">
                            <span class="product-total" data-id="${productId}">${productTotal.toFixed(2)}</span> ₽
                        </div>
                        <div>
                            <button class="remove-btn" onclick="removeProduct('${productId}')" title="Удалить">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                    </div>
                `;
            }
            
            container.innerHTML = html;
            updateTotals();
        }
    }
    
    // Обновление промежуточной цены в памяти (без перерисовки)
    function updatePriceInMemory(productId, price) {
        products[productId].price = price;
        // Обновляем сумму только для этого товара
        updateProductTotal(productId);
        updateTotals();
    }
    
    // Обновление цены (окончательное)
    function updatePrice(productId, price) {
        products[productId].price = price;
        updateProductTotal(productId);
        updateTotals();
    }
    
    // Обновление суммы для конкретного товара
    function updateProductTotal(productId) {
        const product = products[productId];
        const productPrice = parseFloat(product.price) || 0;
        const productTotal = productPrice * product.quantity;
        
        // Обновляем только элемент с суммой для этого товара
        const totalElement = document.querySelector(`.product-total[data-id="${productId}"]`);
        if (totalElement) {
            totalElement.textContent = productTotal.toFixed(2);
        }
    }
    
    // Обновление общих итогов
    function updateTotals() {
        let totalItems = 0;
        let totalAmount = 0;
        
        for (const productId in products) {
            const product = products[productId];
            const productPrice = parseFloat(product.price) || 0;
            totalItems += product.quantity;
            totalAmount += productPrice * product.quantity;
        }
        
        document.getElementById('totalItemsCount').textContent = totalItems;
        document.getElementById('totalAmount').textContent = totalAmount.toFixed(2);
    }
    
    // Изменение количества
    function changeQuantity(productId, delta) {
        const newQuantity = products[productId].quantity + delta;
        
        if (newQuantity < 1) {
            removeProduct(productId);
        } else {
            products[productId].quantity = newQuantity;
            // Обновляем отображение количества
            const qtyElement = document.querySelector(`.table-row[data-id="${productId}"] .qty-value`);
            if (qtyElement) {
                qtyElement.textContent = newQuantity;
            }
            
            // Обновляем сумму для этого товара и общие итоги
            updateProductTotal(productId);
            updateTotals();
        }
    }
    
    // Удаление товара
    function removeProduct(productId) {
        if (confirm('Удалить товар из списка?')) {
            delete products[productId];
            updateProductsDisplay();
            focusScanner();
        }
    }
    
    // Очистка всего списка
    function clearAll() {
        if (Object.keys(products).length === 0) {
            return;
        }
        
        if (confirm('Очистить весь список товаров?')) {
            products = {};
            updateProductsDisplay();
            focusScanner();
        }
    }
    
    // Показать модальное окно создания номенклатуры
    function showCreateNomenclatureModal(barcode) {
        document.getElementById('newBarcode').value = barcode;
        document.getElementById('newProductName').value = '';
        document.getElementById('newProductDescription').value = '';
        document.getElementById('createNomenclatureModal').style.display = 'flex';
        document.getElementById('newProductName').focus();
    }
    
    // Закрыть модальное окно
    function closeCreateModal() {
        document.getElementById('createNomenclatureModal').style.display = 'none';
        focusScanner();
    }
    
    // Создание номенклатуры
    async function createNomenclature() {
        const barcode = document.getElementById('newBarcode').value;
        const name = document.getElementById('newProductName').value;
        const description = document.getElementById('newProductDescription').value;
        
        if (!name.trim()) {
            showError('Введите название товара');
            document.getElementById('newProductName').focus();
            return;
        }
        
        try {
            const response = await fetch('../../api/create_nomenclature.php', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    barcode: barcode,
                    name: name,
                    description: description
                })
            });
            
            const result = await response.json();
            
            if (result.success) {
                nomenclatureCache.push({
                    id: result.nomenclature_id,
                    barcode: barcode,
                    name: name,
                    description: description
                });
                
                addProductToList({
                    id: result.nomenclature_id,
                    barcode: barcode,
                    name: name
                });
                
                closeCreateModal();
                showSuccess('Номенклатура создана');
            } else {
                showError(result.error || 'Ошибка создания номенклатуры');
            }
        } catch (error) {
            showError('Ошибка сети: ' + error.message);
        }
    }
    
    // Сохранение оприходования
    async function saveReceipt() {
        if (Object.keys(products).length === 0) {
            showError('Добавьте хотя бы один товар');
            return;
        }
        
        // Проверяем, что у всех товаров указана цена
        for (const productId in products) {
            const price = parseFloat(products[productId].price);
            if (isNaN(price) || price <= 0) {
                showError('Укажите цену для всех товаров');
                return;
            }
        }
        
        const warehouseId = document.getElementById('warehouseSelect').value;
        const receiptData = {
            warehouse_id: warehouseId,
            products: Object.values(products).map(p => ({
                nomenclature_id: p.id,
                quantity: p.quantity,
                price: parseFloat(p.price)
            }))
        };
        
        try {
            const response = await fetch('../../api/receive_goods.php', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(receiptData)
            });
            
            const result = await response.json();
            
            if (result.success) {
                showSuccess('Товар успешно оприходован!');
                
                // Очищаем список
                products = {};
                updateProductsDisplay();
                
                // Фокус на поле сканирования
                focusScanner();
            } else {
                showError(result.error || 'Ошибка сохранения');
            }
        } catch (error) {
            showError('Ошибка сети: ' + error.message);
        }
    }
    
    // Показать сообщение об ошибке
    function showError(message) {
        const alert = document.getElementById('errorAlert');
        alert.innerHTML = `<i class="fas fa-exclamation-circle"></i> ${message}`;
        alert.style.display = 'flex';
        
        setTimeout(() => {
            alert.style.display = 'none';
        }, 5000);
    }
    
    // Показать сообщение об успехе
    function showSuccess(message) {
        const alert = document.getElementById('successAlert');
        alert.innerHTML = `<i class="fas fa-check-circle"></i> ${message}`;
        alert.style.display = 'flex';
        
        setTimeout(() => {
            alert.style.display = 'none';
        }, 5000);
    }
</script>
</body>
</html>