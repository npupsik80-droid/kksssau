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
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Оприходование SIM-карт - RunaRMK</title>
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
        
        .product-info-section {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            margin-bottom: 30px;
            box-shadow: var(--shadow-md);
            border: 2px solid rgba(67, 97, 238, 0.1);
            position: relative;
            overflow: hidden;
        }
        
        .product-info-section::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--warning), #e67e22);
        }
        
        .product-info-section h2 {
            color: var(--dark);
            font-size: 24px;
            font-weight: 700;
            margin-bottom: 25px;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .form-row {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 25px;
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
            background: linear-gradient(90deg, var(--secondary), #27ae60);
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
        
        .barcodes-table-container {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-md);
            border: 2px solid rgba(67, 97, 238, 0.1);
            margin-bottom: 30px;
            position: relative;
            overflow: hidden;
        }
        
        .barcodes-table-container::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--primary), #3a56d4);
        }
        
        .table-header {
            display: grid;
            grid-template-columns: 1fr 100px;
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
            grid-template-columns: 1fr 100px;
            padding: 20px;
            border-bottom: 1px solid #f1f5f9;
            align-items: center;
            transition: all 0.3s;
        }
        
        .table-row:hover {
            background: #f8fafc;
            transform: translateX(4px);
        }
        
        .no-barcodes {
            text-align: center;
            padding: 60px 30px;
            color: #64748b;
        }
        
        .no-barcodes i {
            font-size: 64px;
            color: #cbd5e1;
            margin-bottom: 25px;
        }
        
        .no-barcodes h3 {
            color: #64748b;
            font-size: 22px;
            font-weight: 700;
            margin-bottom: 15px;
        }
        
        .no-barcodes p {
            color: #94a3b8;
            font-size: 16px;
            margin-bottom: 25px;
            line-height: 1.6;
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
            margin: 0 auto;
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
            background: linear-gradient(90deg, var(--warning), #e67e22);
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
            
            .form-row {
                grid-template-columns: 1fr;
                gap: 20px;
            }
        }
        
        @media (max-width: 768px) {
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
        }
    </style>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">
</head>
<body>
    <!-- Шапка -->
    <div class="header">
        <h1>
            <i class="fas fa-sim-card"></i>
            Оприходование SIM-карт
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
        
        <!-- Информация о продукте -->
        <div class="product-info-section fade-in-up" style="animation-delay: 0.2s;">
            <h2><i class="fas fa-info-circle"></i> Информация о SIM-картах</h2>
            <div class="form-row">
                <div class="form-group">
                    <label><i class="fas fa-tag"></i> Название товара *</label>
                    <input type="text" id="productName" value="" placeholder="Введите название товара">
                </div>
                <div class="form-group">
                    <label><i class="fas fa-money-bill-wave"></i> Цена за единицу (₽) *</label>
                    <input type="number" id="productPrice" value="" step="0.01" min="0" placeholder="0.00">
                </div>
            </div>
        </div>
        
        <!-- Сканер штрих-кодов -->
        <div class="scanner-section fade-in-up" style="animation-delay: 0.3s;">
            <h2><i class="fas fa-barcode"></i> Сканирование штрих-кодов SIM-карт</h2>
            <p class="scanner-help">
                Наведите сканер на штрих-код SIM-карты. Нажмите Enter после сканирования каждого штрих-кода
            </p>
            
            <input type="text" 
                   id="barcodeInput" 
                   class="scanner-input" 
                   placeholder="Отсканируйте штрих-код SIM-карты..."
                   autofocus>
            
            <p class="scanner-help" style="margin-top: 20px;">
                <i class="fas fa-info-circle"></i> Каждый штрих-код будет создан как отдельная номенклатура
            </p>
        </div>
        
        <!-- Сообщения -->
        <div id="errorAlert" class="alert alert-error"></div>
        <div id="successAlert" class="alert alert-success"></div>
        
        <!-- Таблица штрих-кодов -->
        <div class="barcodes-table-container fade-in-up" style="animation-delay: 0.4s;">
            <div class="table-header">
                <div>Штрих-код SIM-карты</div>
                <div>Действие</div>
            </div>
            
            <div id="barcodesContainer">
                <div class="no-barcodes" id="noBarcodesMessage">
                    <i class="fas fa-barcode"></i>
                    <h3>Штрих-коды не добавлены</h3>
                    <p>Отсканируйте первый штрих-код SIM-карты, чтобы добавить его в список</p>
                </div>
            </div>
        </div>
        
        <!-- Итоги -->
        <div class="totals-section fade-in-up" style="animation-delay: 0.5s;">
            <div class="total-items">
                <i class="fas fa-sim-card"></i>
                <span id="totalItemsCount">0</span> SIM-карт на сумму:
            </div>
            <div class="total-amount">
                <i class="fas fa-money-bill-wave"></i>
                <span id="totalAmount">0.00</span> ₽
            </div>
        </div>
        
        <!-- Кнопки действий -->
        <div class="action-buttons fade-in-up" style="animation-delay: 0.6s;">
            <button class="btn btn-secondary" onclick="clearAll()">
                <i class="fas fa-trash"></i> Очистить все
            </button>
            <button class="btn btn-success" onclick="saveSimCards()">
                <i class="fas fa-save"></i> Оприходовать SIM-карты
            </button>
        </div>
    </div>
    
    <script>
        // Глобальные переменные
        let barcodes = [];
        let currentBarcode = '';
        
        // Инициализация
        document.addEventListener('DOMContentLoaded', function() {
            // Фокус на поле сканирования
            focusScanner();
            
            // Обработка сканера (Enter)
            document.getElementById('barcodeInput').addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    const barcode = this.value.trim();
                    
                    if (barcode) {
                        processBarcode(barcode);
                        this.value = '';
                        // После обработки штрих-кода возвращаем фокус
                        setTimeout(focusScanner, 100);
                    }
                }
            });
            
            // Фокус на сканер при клике в любом месте страницы
            document.addEventListener('click', function(e) {
                if (e.target.id !== 'barcodeInput' && 
                    e.target.id !== 'warehouseSelect' &&
                    e.target.id !== 'productName' &&
                    e.target.id !== 'productPrice' &&
                    e.target.tagName !== 'OPTION' &&
                    !e.target.closest('.remove-btn')) {
                    focusScanner();
                }
            });
            
            // Обновление суммы при изменении цены
            document.getElementById('productPrice').addEventListener('input', updateTotals);
        });
        
        // Функция фокусировки на сканере
        function focusScanner() {
            const scannerInput = document.getElementById('barcodeInput');
            if (scannerInput) {
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
        function processBarcode(barcode) {
            currentBarcode = barcode;
            
            // Проверяем, нет ли уже такого штрих-кода
            if (barcodes.includes(barcode)) {
                showError('Этот штрих-код уже добавлен');
                return;
            }
            
            // Добавляем штрих-код в список
            barcodes.push(barcode);
            updateBarcodesDisplay();
        }
        
        // Обновление отображения штрих-кодов
        function updateBarcodesDisplay() {
            const container = document.getElementById('barcodesContainer');
            const price = parseFloat(document.getElementById('productPrice').value) || 0;
            
            if (barcodes.length === 0) {
                container.innerHTML = `
                    <div class="no-barcodes" id="noBarcodesMessage">
                        <i class="fas fa-barcode"></i>
                        <h3>Штрих-коды не добавлены</h3>
                        <p>Отсканируйте первый штрих-код SIM-карты, чтобы добавить его в список</p>
                    </div>
                `;
                
                document.getElementById('totalItemsCount').textContent = 0;
                document.getElementById('totalAmount').textContent = '0.00';
            } else {
                let html = '';
                
                for (let i = 0; i < barcodes.length; i++) {
                    const barcode = barcodes[i];
                    
                    html += `
                        <div class="table-row" data-index="${i}">
                            <div>
                                <strong style="color: var(--dark); font-size: 18px; font-family: monospace;">${barcode}</strong>
                                <div style="font-size: 13px; color: #64748b; margin-top: 5px;">
                                    SIM-карта #${i + 1}
                                </div>
                            </div>
                            <div>
                                <button class="remove-btn" onclick="removeBarcode(${i})" title="Удалить">
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
        
        // Обновление итогов
        function updateTotals() {
            const price = parseFloat(document.getElementById('productPrice').value) || 0;
            const totalItems = barcodes.length;
            const totalAmount = price * totalItems;
            
            document.getElementById('totalItemsCount').textContent = totalItems;
            document.getElementById('totalAmount').textContent = totalAmount.toFixed(2);
        }
        
        // Удаление штрих-кода
        function removeBarcode(index) {
            if (confirm('Удалить этот штрих-код из списка?')) {
                barcodes.splice(index, 1);
                updateBarcodesDisplay();
                focusScanner();
            }
        }
        
        // Очистка всего списка
        function clearAll() {
            if (barcodes.length === 0) {
                return;
            }
            
            if (confirm('Очистить весь список штрих-кодов?')) {
                barcodes = [];
                updateBarcodesDisplay();
                focusScanner();
            }
        }
        
        // Оприходование SIM-карт
        async function saveSimCards() {
            const productName = document.getElementById('productName').value.trim();
            const productPrice = parseFloat(document.getElementById('productPrice').value);
            const warehouseId = document.getElementById('warehouseSelect').value;
            
            // Валидация
            if (!productName) {
                showError('Введите название товара');
                document.getElementById('productName').focus();
                return;
            }
            
            if (isNaN(productPrice) || productPrice <= 0) {
                showError('Введите корректную цену');
                document.getElementById('productPrice').focus();
                return;
            }
            
            if (barcodes.length === 0) {
                showError('Добавьте хотя бы один штрих-код');
                focusScanner();
                return;
            }
            
            // Подготовка данных
            const simCardsData = {
                warehouse_id: warehouseId,
                product_name: productName,
                product_price: productPrice,
                barcodes: barcodes
            };
            
            try {
                const response = await fetch('../../api/receive_sim_cards.php', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(simCardsData)
                });
                
                const result = await response.json();
                
                if (result.success) {
                    showSuccess(`Успешно оприходовано ${barcodes.length} SIM-карт!`);
                    
                    // Очищаем список
                    barcodes = [];
                    updateBarcodesDisplay();
                    
                    // Фокус на поле сканирования
                    focusScanner();
                } else {
                    showError(result.error || 'Ошибка оприходования SIM-карт');
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