<?php
require_once '../../includes/auth_check.php';

// Проверяем выбрано ли подразделение
if (!isset($_SESSION['current_division_id'])) {
    header('Location: ../../select_division.php');
    exit();
}

// Получаем товары со склада текущего подразделения
$stmt = $pdo->prepare("
    SELECT 
        wi.id as warehouse_item_id,
        wi.quantity,
        wi.price,
        n.name,
        n.barcode,
        w.name as warehouse_name
    FROM warehouse_items wi
    JOIN nomenclatures n ON wi.nomenclature_id = n.id
    JOIN warehouses w ON wi.warehouse_id = w.id
    WHERE wi.warehouse_id = ?
    AND wi.quantity > 0
    ORDER BY n.name
");
$stmt->execute([$_SESSION['current_warehouse_id']]);
$products = $stmt->fetchAll();

// Получаем текущую открытую смену
$stmt = $pdo->prepare("
    SELECT * FROM shifts 
    WHERE division_id = ? 
    AND status = 'open'
    ORDER BY opened_at DESC 
    LIMIT 1
");
$stmt->execute([$_SESSION['current_division_id']]);
$current_shift = $stmt->fetch();
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Новый чек - RunaRMK</title>
    <script>var KkmServerAddIn = {};</script>
    <style>
        /* Оставляем ВСЕ оригинальные стили без изменений */
        :root {
            --primary: #4361ee;
            --secondary: #2ecc71;
            --danger: #e74c3c;
            --warning: #f39c12;
            --light: #f8f9fa;
            --dark: #2c3e50;
            --gray: #95a5a6;
            --light-blue: #e9f2ff;
            --border-radius: 12px;
            --shadow-sm: 0 2px 8px rgba(0,0,0,0.08);
            --shadow-md: 0 4px 12px rgba(0,0,0,0.12);
            --shadow-lg: 0 8px 24px rgba(0,0,0,0.15);
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
        
        .container {
            display: grid;
            grid-template-columns: 1fr 420px;
            gap: 25px;
            padding: 0 30px 30px;
            min-height: calc(100vh - 140px);
        }
        
        .check-section {
            background: white;
            border-radius: var(--border-radius);
            padding: 25px;
            box-shadow: var(--shadow-md);
            display: flex;
            flex-direction: column;
            border: 1px solid rgba(0,0,0,0.05);
            position: relative;
        }
        
        .section-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 25px;
            padding-bottom: 18px;
            border-bottom: 2px solid var(--light);
        }
        
        .section-header h2 {
            color: var(--dark);
            font-size: 20px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .search-box {
            width: 100%;
            padding: 16px 20px;
            border: 2px solid #e0e6ff;
            border-radius: 10px;
            font-size: 16px;
            margin-bottom: 25px;
            transition: all 0.3s;
            background: var(--light-blue);
            color: var(--dark);
        }
        
        .search-box:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .search-box::placeholder {
            color: #94a3b8;
        }
        
        /* Стили для таблицы выбранных товаров (перемещаем в левую колонку) */
        .selected-items-container {
            flex: 1;
            overflow-y: auto;
            margin-bottom: 20px;
        }
        
        .table-header {
            display: grid;
            grid-template-columns: 3fr 1fr 1fr 1fr 80px;
            background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
            padding: 15px 20px;
            border-radius: 10px;
            font-weight: 600;
            color: var(--dark);
            font-size: 14px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 10px;
            text-align: center;
        }
        
        .table-header div:first-child {
            text-align: left;
        }
        
        .table-row {
            display: grid;
            grid-template-columns: 3fr 1fr 1fr 1fr 80px;
            padding: 18px 20px;
            border-bottom: 1px solid #f1f5f9;
            align-items: center;
            transition: all 0.3s;
            text-align: center;
        }
        
        .table-row:hover {
            background: #f8fafc;
        }
        
        .table-row div:first-child {
            text-align: left;
        }
        
        .no-items {
            text-align: center;
            padding: 60px 20px;
            color: var(--gray);
        }
        
        .no-items i {
            font-size: 64px;
            color: #cbd5e1;
            margin-bottom: 25px;
        }
        
        .no-items h3 {
            color: #64748b;
            font-size: 22px;
            font-weight: 700;
            margin-bottom: 15px;
        }
        
        .no-items p {
            color: #94a3b8;
            font-size: 16px;
            margin-bottom: 25px;
            line-height: 1.6;
        }
        
        .quantity-control {
            display: flex;
            align-items: center;
            gap: 10px;
            justify-content: center;
        }
        
        .qty-btn {
            width: 30px;
            height: 30px;
            border-radius: 8px;
            border: 2px solid #e2e8f0;
            background: white;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 800;
            font-size: 16px;
            color: var(--dark);
            transition: all 0.3s;
        }
        
        .qty-btn:hover {
            border-color: var(--primary);
            color: var(--primary);
            transform: scale(1.1);
        }
        
        .qty-value {
            min-width: 30px;
            text-align: center;
            font-weight: 700;
            font-size: 16px;
            color: var(--dark);
        }
        
        .remove-btn {
            width: 36px;
            height: 36px;
            border-radius: 8px;
            border: none;
            background: linear-gradient(135deg, rgba(239, 68, 68, 0.1) 0%, rgba(239, 68, 68, 0.05) 100%);
            color: #ef4444;
            cursor: pointer;
            font-size: 16px;
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
        
        /* Стили для правой колонки (оплата) */
        .totals-section {
            margin-top: 25px;
            padding-top: 25px;
            border-top: 2px solid #e2e8f0;
            background: #f8fafc;
            border-radius: 10px;
            padding: 20px;
        }
        
        .total-row {
            display: flex;
            justify-content: space-between;
            padding: 12px 0;
            color: var(--dark);
            font-size: 16px;
        }
        
        .total-row.grand-total {
            font-size: 28px;
            font-weight: 800;
            color: var(--secondary);
            padding: 20px 0;
            border-top: 2px solid #e2e8f0;
            margin-top: 15px;
        }
        
        /* Стили для раздельной оплаты */
        .split-payment-section {
            margin-top: 25px;
            padding-top: 25px;
            border-top: 2px solid #e2e8f0;
        }
        
        .split-payment-toggle {
            display: flex;
            align-items: center;
            gap: 12px;
            margin-bottom: 20px;
            padding: 15px;
            background: #f8fafc;
            border-radius: 10px;
            cursor: pointer;
            transition: all 0.3s;
        }
        
        .split-payment-toggle:hover {
            background: #f1f5f9;
        }
        
        .split-payment-toggle input[type="checkbox"] {
            width: 20px;
            height: 20px;
            cursor: pointer;
        }
        
        .split-payment-toggle label {
            font-weight: 600;
            color: var(--dark);
            cursor: pointer;
            font-size: 16px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        
        .payment-method {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
            margin-bottom: 20px;
            width: 100%;
        }
        
        .payment-input-group {
            display: flex;
            flex-direction: column;
            width: 100%;
        }
        
        .payment-input-group label {
            display: flex;
            align-items: center;
            gap: 8px;
            margin-bottom: 8px;
            color: var(--dark);
            font-weight: 600;
            font-size: 14px;
        }
        
        .payment-input-wrapper {
            position: relative;
            width: 100%;
        }
        
        .payment-input {
            width: 100%;
            padding: 16px 20px;
            border: 2px solid #e2e8f0;
            border-radius: 10px;
            font-size: 18px;
            font-weight: 700;
            text-align: right;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.3s;
        }
        
        .payment-input:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .payment-input:disabled {
            background: #f1f5f9;
            color: #94a3b8;
            cursor: not-allowed;
        }
        
        .payment-input::placeholder {
            color: #94a3b8;
            font-weight: 400;
        }
        
        .payment-currency {
            position: absolute;
            right: 20px;
            top: 50%;
            transform: translateY(-50%);
            color: #64748b;
            font-weight: 600;
            font-size: 18px;
            pointer-events: none;
        }
        
        .remaining-section {
            background: linear-gradient(135deg, #fff5f5 0%, #ffe5e5 100%);
            border-radius: 10px;
            padding: 18px 20px;
            margin-top: 20px;
            border: 2px solid #fed7d7;
        }
        
        .remaining-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-size: 16px;
        }
        
        .remaining-row span:first-child {
            color: #7c2d12;
            font-weight: 600;
        }
        
        .remaining-row span:last-child {
            color: var(--danger);
            font-weight: 800;
            font-size: 20px;
        }
        
        /* Изменяем стили для кнопок оплаты */
        .payment-buttons {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
            margin-top: 30px;
        }
        
        .order-button {
            margin-top: 15px;
        }
        
        .clear-button {
            margin-top: 15px;
        }
        
        .btn {
            padding: 18px;
            border: none;
            border-radius: var(--border-radius);
            font-size: 16px;
            font-weight: 700;
            cursor: pointer;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            letter-spacing: 0.5px;
        }
        
        .btn-cash {
            background: linear-gradient(135deg, var(--secondary) 0%, #27ae60 100%);
            color: white;
        }
        
        .btn-cash:hover {
            transform: translateY(-3px);
            box-shadow: var(--shadow-lg);
        }
        
        .btn-card {
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            color: white;
        }
        
        .btn-card:hover {
            transform: translateY(-3px);
            box-shadow: var(--shadow-lg);
        }
        
        .btn-warning {
            background: linear-gradient(135deg, var(--warning) 0%, #e67e22 100%);
            color: white;
        }
        
        .btn-warning:hover {
            transform: translateY(-3px);
            box-shadow: var(--shadow-lg);
        }
        
        .btn-danger {
            background: linear-gradient(135deg, var(--danger) 0%, #c0392b 100%);
            color: white;
        }
        
        .btn-danger:hover {
            transform: translateY(-3px);
            box-shadow: var(--shadow-lg);
        }
        
        .btn:disabled {
            background: linear-gradient(135deg, #cbd5e1 0%, #94a3b8 100%);
            color: #64748b;
            cursor: not-allowed;
            transform: none !important;
            box-shadow: none !important;
        }
        
        /* Статус соединения */
        .connection-status {
            position: fixed;
            bottom: 25px;
            right: 25px;
            padding: 12px 24px;
            border-radius: 50px;
            font-weight: 700;
            z-index: 1001;
            box-shadow: var(--shadow-md);
            display: flex;
            align-items: center;
            gap: 10px;
            font-size: 14px;
            transition: all 0.3s;
            backdrop-filter: blur(10px);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .connected {
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.95) 0%, rgba(39, 174, 96, 0.95) 100%);
            color: white;
        }
        
        .disconnected {
            background: linear-gradient(135deg, rgba(231, 76, 60, 0.95) 0%, rgba(192, 57, 43, 0.95) 100%);
            color: white;
        }
        
        /* Модальные окна */
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
        }
        
        /* Стили для модального окна выдачи заказа */
        .issue-order-modal .form-group {
            margin-bottom: 20px;
        }
        
        .issue-order-modal .form-group label {
            display: block;
            margin-bottom: 8px;
            font-weight: 600;
            color: var(--dark);
            display: flex;
            align-items: center;
            gap: 8px;
        }
        
        .issue-order-modal .form-group input {
            width: 100%;
            padding: 16px 20px;
            border: 2px solid #e2e8f0;
            border-radius: 10px;
            font-size: 16px;
            background: #f8fafc;
            color: var(--dark);
            transition: all 0.3s;
        }
        
        .issue-order-modal .form-group input:focus {
            border-color: var(--primary);
            outline: none;
            box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.1);
            background: white;
        }
        
        .issue-order-modal .form-group input:disabled {
            background: #f1f5f9;
            color: #64748b;
        }
        
        /* Предупреждение о смене */
        .shift-warning {
            background: linear-gradient(135deg, #fff3cd 0%, #ffeaa7 100%);
            border: 2px solid #ffc107;
            color: #856404;
            padding: 20px 25px;
            border-radius: var(--border-radius);
            margin: 20px 30px;
            display: flex;
            align-items: center;
            gap: 15px;
            box-shadow: var(--shadow-sm);
        }
        
        /* Адаптивность */
        @media (max-width: 1200px) {
            .container {
                grid-template-columns: 1fr;
                gap: 20px;
            }
        }
        
        @media (max-width: 768px) {
            .header {
                flex-direction: column;
                gap: 15px;
                padding: 15px;
            }
            
            .user-info {
                width: 100%;
                justify-content: center;
                flex-wrap: wrap;
                gap: 15px;
                padding: 8px 15px;
            }
            
            .container {
                padding: 15px;
            }
            
            .table-header,
            .table-row {
                grid-template-columns: 2fr 1fr 1fr 1fr 80px;
                font-size: 14px;
                padding: 12px 15px;
            }
            
            .payment-method {
                grid-template-columns: 1fr;
                gap: 12px;
            }
            
            .payment-buttons {
                grid-template-columns: 1fr;
                gap: 12px;
            }
        }
        
        @media (max-width: 576px) {
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
        }
    </style>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet">
</head>
<body>
    <!-- Статус соединения -->
    <div id="connectionStatus" class="connection-status" style="display: none;">
        <i class="fas fa-sync fa-spin"></i> Проверка связи...
    </div>
    
    <!-- Шапка -->
    <div class="header">
        <h1><i class="fas fa-receipt"></i> Новый чек</h1>
        <div class="user-info">
            <span><i class="fas fa-user-circle"></i> <?php echo htmlspecialchars($_SESSION['user_name']); ?></span>
            <span><i class="fas fa-store-alt"></i> <?php echo htmlspecialchars($_SESSION['current_division_name']); ?></span>
            <a href="../../index.php" style="color: white; text-decoration: none; display: flex; align-items: center; gap: 8px;">
                <i class="fas fa-arrow-left"></i> На главную
            </a>
        </div>
    </div>
    
    <?php if (!$current_shift): ?>
    <div class="shift-warning" style="margin: 20px 30px;">
        <i class="fas fa-exclamation-triangle"></i>
        <div>
            <strong>Смена не открыта!</strong> Для работы с чеками необходимо открыть смену.
            <br>
            <a href="../shifts/" style="color: #856404; font-weight: bold; text-decoration: underline;">Открыть смену</a>
        </div>
    </div>
    <?php endif; ?>
    
    <!-- Основной контент -->
    <div class="container">
        <!-- Левая колонка: Товары в чеке -->
        <div class="check-section">
            <div class="section-header">
                <h2><i class="fas fa-shopping-cart"></i> Товары в чеке</h2>
                <span id="itemsCount" style="background: var(--primary); color: white; padding: 6px 15px; border-radius: 20px; font-weight: 700;">0 товаров</span>
            </div>
            
            <input type="text" 
                   id="productSearch" 
                   class="search-box" 
                   placeholder="🔍 Сканируйте или введите штрих-код товара..."
                   autofocus>
            
            <div class="selected-items-container">
                <div class="table-header">
                    <div>Наименование товара</div>
                    <div>Цена</div>
                    <div>Количество</div>
                    <div>Сумма</div>
                    <div></div>
                </div>
                
                <div id="checkItems">
                    <!-- Товары будут добавляться сюда -->
                    <div class="no-items" id="noItemsMessage">
                        <i class="fas fa-shopping-cart"></i>
                        <h3>Товары не добавлены</h3>
                        <p>Отсканируйте или введите штрих-код товара</p>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- Правая колонка: Оплата -->
        <div class="check-section">
            <div class="section-header">
                <h2><i class="fas fa-money-bill-wave"></i> Оплата</h2>
                <span id="totalQuantity" style="background: var(--secondary); color: white; padding: 6px 15px; border-radius: 20px; font-weight: 700;">0 шт</span>
            </div>
            
            <div class="totals-section">
                <div class="total-row">
                    <span>Сумма:</span>
                    <span id="subtotal" style="font-weight: 700;">0.00 ₽</span>
                </div>
                <div class="total-row grand-total">
                    <span>К ОПЛАТЕ:</span>
                    <span id="grandTotal" style="text-shadow: 0 2px 4px rgba(0,0,0,0.1);">0.00 ₽</span>
                </div>
            </div>
            
            <div class="split-payment-section">
                <div class="split-payment-toggle" onclick="toggleSplitPayment()">
                    <input type="checkbox" id="splitPaymentCheckbox">
                    <label for="splitPaymentCheckbox">
                        <i class="fas fa-money-check-alt"></i> Раздельная оплата
                    </label>
                </div>
                
                <div class="payment-method">
                    <div class="payment-input-group">
                        <label><i class="fas fa-money-bill"></i> Наличные:</label>
                        <div class="payment-input-wrapper">
                            <input type="number" 
                                   id="cashPayment" 
                                   class="payment-input" 
                                   min="0" 
                                   step="0.01" 
                                   onchange="updatePayments()"
                                   disabled>
                            <span class="payment-currency">₽</span>
                        </div>
                    </div>
                    <div class="payment-input-group">
                        <label><i class="fas fa-credit-card"></i> Карта:</label>
                        <div class="payment-input-wrapper">
                            <input type="number" 
                                   id="cardPayment" 
                                   class="payment-input" 
                                   min="0" 
                                   step="0.01" 
                                   onchange="updatePayments()"
                                   disabled>
                            <span class="payment-currency">₽</span>
                        </div>
                    </div>
                </div>
                
                <div class="remaining-section" id="remainingSection" style="display: none;">
                    <div class="remaining-row">
                        <span>Остаток к оплате:</span>
                        <span id="remainingAmount">0.00 ₽</span>
                    </div>
                </div>
            </div>
            
            <!-- Кнопки оплаты -->
            <div class="payment-buttons">
                <button class="btn btn-cash" id="cashButton" onclick="processCashPayment()" disabled>
                    <i class="fas fa-money-bill-wave"></i> Оплата наличными
                </button>
                <button class="btn btn-card" id="cardButton" onclick="processCardPayment()" disabled>
                    <i class="fas fa-credit-card"></i> Оплата по карте
                </button>
            </div>
            
            <!-- Кнопка выдачи заказа -->
            <div class="order-button">
                <button class="btn btn-warning" onclick="openIssueOrderModal()">
                    <i class="fas fa-sim-card"></i> Выдать заказ
                </button>
            </div>
            
            <!-- Кнопка очистки чека -->
            <div class="clear-button">
                <button class="btn btn-danger" onclick="clearCheck()">
                    <i class="fas fa-trash-alt"></i> Очистить чек
                </button>
            </div>
        </div>
    </div>
    
    <!-- Модальное окно выдачи заказа -->
    <div id="issueOrderModal" class="modal-overlay">
        <div class="modal-content issue-order-modal">
            <h2 style="margin-bottom: 20px; color: var(--dark); display: flex; align-items: center; gap: 12px; justify-content: center;">
                <i class="fas fa-sim-card"></i> Выдача SIM-карты
            </h2>
            
            <div class="form-group">
                <label><i class="fas fa-tag"></i> Название товара</label>
                <input type="text" id="issueProductName" value="Sim Карта" disabled>
            </div>
            
            <div class="form-group">
                <label><i class="fas fa-barcode"></i> Штрих-код *</label>
                <input type="text" id="issueBarcode" placeholder="Введите или отсканируйте штрих-код" autofocus>
            </div>
            
            <div class="form-group">
                <label><i class="fas fa-money-bill-wave"></i> Цена (₽) *</label>
                <input type="number" id="issuePrice" placeholder="0.00" step="0.01" min="0">
            </div>
            
            <div style="display: flex; gap: 12px; margin-top: 30px;">
                <button class="btn btn-danger" onclick="closeIssueOrderModal()" style="flex: 1; padding: 16px;">
                    <i class="fas fa-times"></i> Отмена
                </button>
                <button class="btn btn-success" onclick="createAndAddSimCard()" style="flex: 1; padding: 16px;">
                    <i class="fas fa-plus-circle"></i> Создать и добавить в чек
                </button>
            </div>
        </div>
    </div>
    
    <!-- Модальное окно оплаты по карте -->
    <div id="cardPaymentModal" class="modal-overlay">
        <div class="modal-content card-payment-modal">
            <h2 style="margin-bottom: 20px; color: var(--dark); display: flex; align-items: center; gap: 12px; justify-content: center;">
                <i class="fas fa-credit-card"></i> Оплата по карте
            </h2>
            <div id="cardPaymentStatus" class="card-payment-status card-payment-processing">
                <i class="fas fa-spinner fa-spin fa-2x" style="margin-bottom: 15px;"></i>
                <div style="font-size: 18px; margin-bottom: 8px;">Ожидание оплаты...</div>
                <p id="cardPaymentMessage" style="color: #666; font-size: 14px;">Пожалуйста, приложите карту к терминалу</p>
            </div>
            <div style="display: flex; gap: 12px; margin-top: 30px;">
                <button class="btn btn-danger" onclick="cancelCardPayment()" style="flex: 1; padding: 16px;">
                    <i class="fas fa-times"></i> Отменить
                </button>
            </div>
        </div>
    </div>
    
    <!-- Модальное окно результата -->
    <div id="resultModal" class="modal-overlay">
        <div class="modal-content">
            <h2 style="margin-bottom: 20px; color: var(--dark); display: flex; align-items: center; gap: 12px; justify-content: center;">
                <i class="fas fa-check-circle"></i> Чек успешно создан
            </h2>
            <div id="modalContent" style="text-align: center;"></div>
            <div style="display: flex; gap: 12px; margin-top: 30px;">
                <button class="btn btn-primary" onclick="closeModal()" style="flex: 1; padding: 16px;">
                    <i class="fas fa-times"></i> Закрыть
                </button>
                <button class="btn btn-success" onclick="printAnother()" style="flex: 1; padding: 16px;">
                    <i class="fas fa-receipt"></i> Новый чек
                </button>
            </div>
        </div>
    </div>
    
    <script>
        // Используем оригинальную JavaScript логику, но адаптируем ее под новый интерфейс
        
        // Глобальные переменные (как в оригинале)
        let checkType = 'sale';
        let checkItems = {};
        let currentShiftId = <?php echo $current_shift ? $current_shift['id'] : 'null'; ?>;
        let isKKTConnected = false;
        let splitPaymentEnabled = false;
        let lastCardTransaction = null;
        
        // Массив всех товаров со склада для поиска
        let allProducts = <?php echo json_encode($products); ?>;
        
        // Инициализация при загрузке (используем оригинальную логику)
        document.addEventListener('DOMContentLoaded', function() {
            // Запускаем проверку связи с задержкой
            setTimeout(checkKKTConnection, 1000);
            
            // Обработка сканирования штрих-кода (Enter)
            document.getElementById('productSearch').addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    const barcode = this.value.trim();
                    
                    if (barcode) {
                        findProductByBarcode(barcode);
                        this.value = '';
                    }
                }
            });
            
            // Обработка Enter в модальном окне выдачи заказа
            document.getElementById('issueBarcode').addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    document.getElementById('issuePrice').focus();
                }
            });
            
            document.getElementById('issuePrice').addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    createAndAddSimCard();
                }
            });
            
            // Автоматически распределяем оплату
            document.getElementById('cashPayment').addEventListener('change', function() {
                distributePayment();
            });
            
            document.getElementById('cardPayment').addEventListener('change', function() {
                distributePayment();
            });
            
            // Проверяем есть ли открытая смена
            if (!currentShiftId) {
                disableCheckOperations();
            }
            
            // Периодически проверяем связь
            setInterval(checkKKTConnection, 5000);
            
            // Фокус на поле поиска
            document.getElementById('productSearch').focus();
        });
        function canSetFocus() {
		// Проверяем, открыто ли любое модальное окно
		const isIssueOrderModalOpen = document.getElementById('issueOrderModal').style.display === 'flex';
		const isCardPaymentModalOpen = document.getElementById('cardPaymentModal').style.display === 'flex';
		const isResultModalOpen = document.getElementById('resultModal').style.display === 'flex';
    
		// Если любое модальное окно открыто - не ставим фокус
		return !(isIssueOrderModalOpen || isCardPaymentModalOpen || isResultModalOpen);
}
        // Общая функция для вызова API через расширение (как в документации)
        function ExecuteCommand(Data, FunSuccess, FunError, timeout) {
            try {
                if (typeof KkmServer !== 'undefined') {
                    // Если данные - строка JSON конвертируем в объект
                    if (typeof (Data) == "string") Data = JSON.parse(Data);
                    // Выполняем команду через расширение
                    KkmServer.Execute(FunSuccess || ExecuteSuccess, Data);
                    return;
                };
            } catch { };
            
            // Если нет расширения
            if (FunError) FunError('Нет соединения с ККТ');
        }
        
        // Функция вызываемая после обработки команды
        function ExecuteSuccess(Result) {
            console.log('Результат от ККТ:', Result);
        }
        
        // Функция вызываемая при ошибке передачи данных
        function ErrorSuccess(TextError) {
            console.error('Ошибка:', TextError);
        }
        
        // Функция генерации GUID (из оригинала)
        function guid() {
            function S4() {
                return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
            }
            return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
        }
        
        // Поиск товара по штрих-коду
function findProductByBarcode(barcode) {
    // Ищем товар в массиве allProducts
    const product = allProducts.find(p => p.barcode === barcode);
    
    if (product) {
        addToCheck(product);
        if (canSetFocus()) {
            document.getElementById('productSearch').focus();
        }
    } else {
        alert('Товар с штрих-кодом ' + barcode + ' не найден на складе');
        if (canSetFocus()) {
            document.getElementById('productSearch').focus();
        }
    }
}
        
        // Оригинальная функция проверки связи с ККТ
        function checkKKTConnection() {
            const statusElement = document.getElementById('connectionStatus');
            
            if (typeof KkmServer === 'undefined') {
                isKKTConnected = false;
                statusElement.innerHTML = '<i class="fas fa-wifi-slash"></i> Ожидание расширения...';
                statusElement.className = 'connection-status disconnected';
                statusElement.style.display = 'block';
                disableCheckOperations();
                return;
            }
            
            const testData = {
                Command: "GetDataKKT",
                NumDevice: 0,
                IdCommand: guid()
            };
            
            try {
                KkmServer.Execute(function(result) {
                    if (result && result.Status !== undefined) {
                        isKKTConnected = true;
                        statusElement.innerHTML = '<i class="fas fa-wifi"></i> Соединение установлено';
                        statusElement.className = 'connection-status connected';
                        statusElement.style.display = 'block';
                        
                        if (currentShiftId) {
                            enableCheckOperations();
                        }
                    } else {
                        isKKTConnected = false;
                        statusElement.innerHTML = '<i class="fas fa-wifi-slash"></i> Нет ответа от ККТ';
                        statusElement.className = 'connection-status disconnected';
                        statusElement.style.display = 'block';
                        disableCheckOperations();
                    }
                }, testData);
            } catch (error) {
                isKKTConnected = false;
                statusElement.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Ошибка связи';
                statusElement.className = 'connection-status disconnected';
                statusElement.style.display = 'block';
                disableCheckOperations();
            }
        }
        
function enableCheckOperations() {
    // Активируем поле поиска
    document.getElementById('productSearch').disabled = false;
    document.getElementById('productSearch').placeholder = "🔍 Сканируйте или введите штрих-код товара...";
    
    // Ставим фокус только если нет открытых модальных окон
    if (canSetFocus()) {
        document.getElementById('productSearch').focus();
    }
    
    // Активируем кнопки оплаты если есть товары
    updatePaymentButtons();
}
        
        // Обновление состояния кнопок оплаты
        function updatePaymentButtons() {
            const cashBtn = document.getElementById('cashButton');
            const cardBtn = document.getElementById('cardButton');
            const hasItems = Object.keys(checkItems).length > 0;
            const canPay = hasItems && isKKTConnected && currentShiftId;
            
            cashBtn.disabled = !canPay;
            cardBtn.disabled = !canPay;
            
            if (!canPay) {
                cashBtn.innerHTML = '<i class="fas fa-ban"></i> Недоступно';
                cardBtn.innerHTML = '<i class="fas fa-ban"></i> Недоступно';
            } else {
                cashBtn.innerHTML = '<i class="fas fa-money-bill-wave"></i> Оплата наличными';
                cardBtn.innerHTML = '<i class="fas fa-credit-card"></i> Оплата по карте';
            }
        }
        
        // Оригинальная функция отключения операций с чеком
        function disableCheckOperations() {
            // Деактивируем поле поиска
            document.getElementById('productSearch').disabled = true;
            document.getElementById('productSearch').placeholder = "❌ Смена не открыта";
            
            // Деактивируем кнопки оплаты
            document.getElementById('cashButton').disabled = true;
            document.getElementById('cardButton').disabled = true;
            document.getElementById('cashButton').innerHTML = '<i class="fas fa-ban"></i> Недоступно';
            document.getElementById('cardButton').innerHTML = '<i class="fas fa-ban"></i> Недоступно';
        }
        
        // Включение/выключение раздельной оплаты
        function toggleSplitPayment() {
            const checkbox = document.getElementById('splitPaymentCheckbox');
            const cashInput = document.getElementById('cashPayment');
            const cardInput = document.getElementById('cardPayment');
            
            splitPaymentEnabled = checkbox.checked;
            
            if (splitPaymentEnabled) {
                // Включаем поля ввода
                cashInput.disabled = false;
                cardInput.disabled = false;
                
                // Автоматически заполняем общую сумму
                const subtotal = parseFloat(document.getElementById('subtotal').textContent) || 0;
                cashInput.value = subtotal.toFixed(2);
                cardInput.value = '0';
                
                // Обновляем отображение
                updatePayments();
            } else {
                // Выключаем поля ввода
                cashInput.disabled = true;
                cardInput.disabled = true;
                
                // Сбрасываем значения
                cashInput.value = '0';
                cardInput.value = '0';
                
                // Скрываем блок остатка
                document.getElementById('remainingSection').style.display = 'none';
                
                // Обновляем итоговую сумму
                const subtotal = parseFloat(document.getElementById('subtotal').textContent) || 0;
                document.getElementById('grandTotal').textContent = subtotal.toFixed(2) + ' ₽';
            }
        }
        
        // Обновление полей оплаты
        function updatePayments() {
            if (!splitPaymentEnabled) return;
            
            const subtotal = parseFloat(document.getElementById('subtotal').textContent) || 0;
            const cash = parseFloat(document.getElementById('cashPayment').value) || 0;
            const card = parseFloat(document.getElementById('cardPayment').value) || 0;
            const paid = cash + card;
            const remaining = subtotal - paid;
            
            updateRemainingDisplay(remaining);
        }
        
        // Обновление отображения остатка
        function updateRemainingDisplay(remaining) {
            const remainingElement = document.getElementById('remainingAmount');
            const remainingSection = document.getElementById('remainingSection');
            
            if (remaining > 0) {
                remainingElement.textContent = remaining.toFixed(2) + ' ₽';
                remainingElement.style.color = 'var(--danger)';
                remainingSection.style.display = 'block';
            } else if (remaining < 0) {
                remainingElement.textContent = Math.abs(remaining).toFixed(2) + ' ₽';
                remainingElement.style.color = 'var(--warning)';
                remainingSection.style.display = 'block';
            } else {
                remainingSection.style.display = 'none';
            }
        }
        
        // Автоматическое распределение оплаты (из оригинала)
        function distributePayment() {
            const total = parseFloat(document.getElementById('grandTotal').textContent) || 0;
            const cashInput = document.getElementById('cashPayment');
            const cardInput = document.getElementById('cardPayment');
            
            const cash = parseFloat(cashInput.value) || 0;
            const card = parseFloat(cardInput.value) || 0;
            const paid = cash + card;
            
            if (paid > total) {
                // Переплата - распределяем обратно пропорционально
                const excess = paid - total;
                if (cash > 0 && card > 0) {
                    const cashRatio = cash / paid;
                    cashInput.value = (cash - excess * cashRatio).toFixed(2);
                    cardInput.value = (card - excess * (1 - cashRatio)).toFixed(2);
                } else if (cash > 0) {
                    cashInput.value = (cash - excess).toFixed(2);
                } else if (card > 0) {
                    cardInput.value = (card - excess).toFixed(2);
                }
            }
            
            updatePayments();
        }
        
        // Добавление товара в чек
        function addToCheck(product) {
            const id = product.warehouse_item_id;
            const name = product.name;
            const price = parseFloat(product.price);
            const barcode = product.barcode;
            const stock = parseInt(product.quantity);
            
            if (checkItems[id]) {
                // Увеличиваем количество, если есть остаток
                if (checkItems[id].quantity < stock) {
                    checkItems[id].quantity++;
                } else {
                    alert('Недостаточно товара на складе!');
                    return;
                }
            } else {
                // Добавляем новый товар
                checkItems[id] = {
                    id: id,
                    name: name,
                    price: price,
                    barcode: barcode,
                    quantity: 1,
                    stock: stock
                };
            }
            
            updateCheckDisplay();
        }
        
        // Обновление отображения чека
        function updateCheckDisplay() {
            const checkItemsElement = document.getElementById('checkItems');
            let totalQuantity = 0;
            let subtotal = 0;
            
            if (Object.keys(checkItems).length === 0) {
                checkItemsElement.innerHTML = `
                    <div class="no-items" id="noItemsMessage">
                        <i class="fas fa-shopping-cart"></i>
                        <h3>Товары не добавлены</h3>
                        <p>Отсканируйте или введите штрих-код товара</p>
                    </div>
                `;
            } else {
                let html = '';
                
                for (const id in checkItems) {
                    const item = checkItems[id];
                    const itemTotal = item.price * item.quantity;
                    
                    totalQuantity += item.quantity;
                    subtotal += itemTotal;
                    
                    html += `
                        <div class="table-row" data-id="${id}">
                            <div>
                                <strong style="color: var(--dark); font-size: 15px;">${item.name}</strong>
                                <div style="font-size: 13px; color: #64748b; margin-top: 5px;">${item.barcode}</div>
                            </div>
                            <div style="font-weight: 700; color: var(--dark);">${item.price.toFixed(2)} ₽</div>
                            <div>
                                <div class="quantity-control">
                                    <button class="qty-btn" onclick="changeQuantity('${id}', -1)">-</button>
                                    <span class="qty-value">${item.quantity}</span>
                                    <button class="qty-btn" onclick="changeQuantity('${id}', 1)">+</button>
                                </div>
                            </div>
                            <div style="font-weight: 800; color: var(--primary);">${itemTotal.toFixed(2)} ₽</div>
                            <div>
                                <button class="remove-btn" onclick="removeItem('${id}')" title="Удалить">
                                    <i class="fas fa-times"></i>
                                </button>
                            </div>
                        </div>
                    `;
                }
                
                checkItemsElement.innerHTML = html;
            }
            
            // Обновляем статистику
            document.getElementById('itemsCount').textContent = Object.keys(checkItems).length + ' товаров';
            document.getElementById('totalQuantity').textContent = totalQuantity + ' шт';
            document.getElementById('subtotal').textContent = subtotal.toFixed(2) + ' ₽';
            
            // Обновляем итоговую сумму
            if (splitPaymentEnabled) {
                const cash = parseFloat(document.getElementById('cashPayment').value) || 0;
                const card = parseFloat(document.getElementById('cardPayment').value) || 0;
                const paid = cash + card;
                const remaining = subtotal - paid;
                
                document.getElementById('grandTotal').textContent = subtotal.toFixed(2) + ' ₽';
                updateRemainingDisplay(remaining);
            } else {
                document.getElementById('grandTotal').textContent = subtotal.toFixed(2) + ' ₽';
                document.getElementById('remainingSection').style.display = 'none';
            }
            
            // Обновляем состояние кнопок оплаты
            updatePaymentButtons();
        }
        
        // Изменение количества товара
        function changeQuantity(itemId, delta) {
            const item = checkItems[itemId];
            const newQuantity = item.quantity + delta;
            
            if (newQuantity < 1) {
                removeItem(itemId);
                return;
            }
            
            if (newQuantity > item.stock) {
                alert('Недостаточно товара на складе!');
                return;
            }
            
            item.quantity = newQuantity;
            updateCheckDisplay();
        }
        
        // Удаление товара из чека
        function removeItem(itemId) {
            if (confirm('Удалить товар из чека?')) {
                delete checkItems[itemId];
                updateCheckDisplay();
            }
        }
        
        // Очистка чека
function clearCheck() {
    if (Object.keys(checkItems).length === 0) return;
    
    if (confirm('Очистить весь чек?')) {
        checkItems = {};
        updateCheckDisplay();
        
        // Сбрасываем оплату
        document.getElementById('cashPayment').value = '0';
        document.getElementById('cardPayment').value = '0';
        document.getElementById('splitPaymentCheckbox').checked = false;
        splitPaymentEnabled = false;
        toggleSplitPayment();
        
        // Фокус на поле поиска
        if (canSetFocus()) {
            document.getElementById('productSearch').focus();
        }
    }
}
        
        // Открыть модальное окно выдачи заказа
function openIssueOrderModal() {
    document.getElementById('issueOrderModal').style.display = 'flex';
    document.getElementById('issueBarcode').focus();
    
    // Гарантированно убираем фокус с поля поиска
    document.getElementById('productSearch').blur();
    
    // Добавим обработчик закрытия модального окна
    document.getElementById('issueOrderModal').addEventListener('click', function(e) {
        if (e.target === this) {
            closeIssueOrderModal();
        }
    });
}
        
function closeIssueOrderModal() {
    document.getElementById('issueOrderModal').style.display = 'none';
    
    // Возвращаем фокус на поле поиска только если нет других открытых модальных окон
    setTimeout(() => {
        if (canSetFocus()) {
            document.getElementById('productSearch').focus();
        }
    }, 50); // Небольшая задержка для гарантии
}
        
        // Создать и добавить SIM-карту
        async function createAndAddSimCard() {
            const barcode = document.getElementById('issueBarcode').value.trim();
            const price = parseFloat(document.getElementById('issuePrice').value);
            const productName = document.getElementById('issueProductName').value;
            
            if (!barcode) {
                alert('Введите штрих-код SIM-карты');
                document.getElementById('issueBarcode').focus();
                return;
            }
            
            if (isNaN(price) || price <= 0) {
                alert('Введите корректную цену');
                document.getElementById('issuePrice').focus();
                return;
            }
            
            // Проверяем, нет ли уже такой SIM-карты в чеке (по штрих-коду)
            const existingInCheck = Object.values(checkItems).find(item => item.barcode === barcode);
            if (existingInCheck) {
                alert('Эта SIM-карта уже добавлена в чек');
                return;
            }
            
            try {
                // Создаем SIM-карту через API
                const response = await fetch('../../api/receive_sim_cards.php', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        warehouse_id: <?php echo $_SESSION['current_warehouse_id']; ?>,
                        product_name: productName,
                        product_price: price,
                        barcodes: [barcode]
                    })
                });
                
                const result = await response.json();
                
                if (result.success && result.products && result.products.length > 0) {
                    // Добавляем все созданные товары в массив allProducts
                    result.products.forEach(product => {
                        // Проверяем, нет ли уже такого товара в allProducts (по штрих-коду)
                        const existingIndex = allProducts.findIndex(p => p.barcode === product.barcode);
                        if (existingIndex >= 0) {
                            // Если уже есть, обновляем (заменяем)
                            allProducts[existingIndex] = product;
                        } else {
                            // Иначе добавляем
                            allProducts.push(product);
                        }
                    });
                    
                    // Берем первый товар из массива и добавляем в чек
                    // (в нашем случае массив будет содержать один элемент)
                    addToCheck(result.products[0]);
                    
                    // Сначала показываем сообщение
                    alert('SIM-карта успешно создана и добавлена в чек');
                    
                    // Затем закрываем модальное окно и очищаем поля
                    document.getElementById('issueBarcode').value = '';
                    document.getElementById('issuePrice').value = '';
                    
                    // Закрываем модальное окно
                    document.getElementById('issueOrderModal').style.display = 'none';
                    
                    // Возвращаем фокус на основное поле поиска
                    setTimeout(() => {
    if (canSetFocus()) {
        document.getElementById('productSearch').focus();
    }
}, 50);
                } else {
                    alert('Ошибка создания SIM-карты: ' + (result.error || 'Неизвестная ошибка'));
                }
            } catch (error) {
                alert('Ошибка сети: ' + error.message);
            }
        }
        
        // Обновить список товаров со склада
        async function refreshProductsList() {
            try {
                const response = await fetch(`../../api/get_products.php?warehouse_id=<?php echo $_SESSION['current_warehouse_id']; ?>`);
                const result = await response.json();
                
                if (result.success) {
                    allProducts = result.products;
                }
            } catch (error) {
                console.error('Ошибка обновления списка товаров:', error);
            }
        }
        
        // Оплата наличными
        async function processCashPayment() {
            if (!currentShiftId) {
                alert('Сначала откройте смену!');
                return;
            }
            
            if (!isKKTConnected) {
                alert('Нет соединения с ККТ! Проверьте подключение расширения.');
                return;
            }
            
            const subtotal = parseFloat(document.getElementById('subtotal').textContent) || 0;
            let cashAmount = 0;
            let cardAmount = 0;
            
            if (splitPaymentEnabled) {
                // При раздельной оплате берем сумму из полей
                cashAmount = parseFloat(document.getElementById('cashPayment').value) || 0;
                cardAmount = parseFloat(document.getElementById('cardPayment').value) || 0;
                
                if (cashAmount <= 0 && cardAmount <= 0) {
                    alert('Введите сумму для оплаты!');
                    return;
                }
                
                // Проверяем, что сумма оплаты не меньше общей суммы
                if (cashAmount + cardAmount < subtotal) {
                    alert('Сумма оплаты меньше суммы чека!');
                    return;
                }
            } else {
                // Без раздельной оплаты - вся сумма наличными
                cashAmount = subtotal;
                cardAmount = 0;
            }
            
            await saveAndPrintCheck(cashAmount, cardAmount, 'cash');
        }
        
        // Оплата по карте
        async function processCardPayment() {
            if (!currentShiftId) {
                alert('Сначала откройте смену!');
                return;
            }
            
            if (!isKKTConnected) {
                alert('Нет соединения с ККТ! Проверьте подключение расширения.');
                return;
            }
            
            const subtotal = parseFloat(document.getElementById('subtotal').textContent) || 0;
            let cashAmount = 0;
            let cardAmount = 0;
            
            if (splitPaymentEnabled) {
                // При раздельной оплате берем сумму из полей
                cashAmount = parseFloat(document.getElementById('cashPayment').value) || 0;
                cardAmount = parseFloat(document.getElementById('cardPayment').value) || 0;
                
                if (cashAmount <= 0 && cardAmount <= 0) {
                    alert('Введите сумму для оплаты!');
                    return;
                }
                
                // Проверяем, что сумма оплаты не меньше общей суммы
                if (cashAmount + cardAmount < subtotal) {
                    alert('Сумма оплаты меньше суммы чека!');
                    return;
                }
                
                // Если есть оплата по карте, проводим ее
                if (cardAmount > 0) {
                    try {
                        await processCardPaymentKKT(cardAmount);
                    } catch (cardError) {
                        alert('Ошибка оплаты по карте: ' + cardError.message);
                        return;
                    }
                }
                
                await saveAndPrintCheck(cashAmount, cardAmount, 'card');
            } else {
                // Без раздельной оплаты - вся сумма картой
                cardAmount = subtotal;
                
                try {
                    await processCardPaymentKKT(cardAmount);
                    await saveAndPrintCheck(0, cardAmount, 'card');
                } catch (cardError) {
                    alert('Ошибка оплаты по карте: ' + cardError.message);
                    return;
                }
            }
        }
        
        // Оригинальная функция оплаты по карте через эквайринг
        function processCardPaymentKKT(amount) {
            return new Promise((resolve, reject) => {
                // Проверяем доступно ли расширение ККТ
                if (!isKKTConnected) {
                    reject(new Error('Нет соединения с ККТ для проведения оплаты'));
                    return;
                }
                
                // Показываем модальное окно оплаты
                const modal = document.getElementById('cardPaymentModal');
                const statusElement = document.getElementById('cardPaymentStatus');
                const messageElement = document.getElementById('cardPaymentMessage');
                
                statusElement.className = 'card-payment-status card-payment-processing';
                statusElement.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Ожидание оплаты...';
                messageElement.textContent = 'Пожалуйста, приложите карту к терминалу';
                
                modal.style.display = 'flex';
                
                // Подготавливаем данные для оплаты по карте (из документации)
                const paymentData = {
                    Command: "PayByPaymentCard",
                    NumDevice: 0, // первое доступное устройство
                    Amount: amount,
                    IdCommand: guid(),
                    Timeout: 120 // 2 минуты
                };
                
                // Вызываем команду оплаты по карте через расширение
                ExecuteCommand(paymentData, 
                    function(result) {
                        if (result.Status === 0) {
                            // Успешная оплата
                            statusElement.className = 'card-payment-status card-payment-success';
                            statusElement.innerHTML = '<i class="fas fa-check-circle"></i> Оплата успешно проведена';
                            messageElement.textContent = 'Оплата по карте прошла успешно';
                            
                            // Сохраняем информацию о транзакции
                            lastCardTransaction = {
                                universalID: result.UniversalID,
                                amount: amount,
                                timestamp: new Date().toISOString()
                            };
                            
                            // Закрываем модальное окно через 2 секунды
                            setTimeout(() => {
                                modal.style.display = 'none';
                                resolve(result);
                            }, 2000);
                            
                        } else {
                            // Ошибка оплаты
                            statusElement.className = 'card-payment-status card-payment-error';
                            statusElement.innerHTML = '<i class="fas fa-times-circle"></i> Ошибка оплаты';
                            messageElement.textContent = result.Error || 'Неизвестная ошибка';
                            
                            // Показываем кнопку для повторной попытки
                            const nextBtn = document.createElement('button');
                            nextBtn.className = 'btn btn-success';
                            nextBtn.innerHTML = '<i class="fas fa-redo"></i> Повторить';
                            nextBtn.style.marginTop = '20px';
                            nextBtn.onclick = function() {
                                modal.style.display = 'none';
                                processCardPaymentKKT(amount).then(resolve).catch(reject);
                            };
                            
                            statusElement.appendChild(nextBtn);
                            
                            reject(new Error(result.Error || 'Ошибка оплаты по карте'));
                        }
                    },
                    function(error) {
                        statusElement.className = 'card-payment-status card-payment-error';
                        statusElement.innerHTML = '<i class="fas fa-times-circle"></i> Ошибка связи';
                        messageElement.textContent = error;
                        reject(new Error(error));
                    }
                );
            });
        }
        
        // Отмена оплаты по карте
        function cancelCardPayment() {
            document.getElementById('cardPaymentModal').style.display = 'none';
            lastCardTransaction = null;
        }
        
        // Сохранить и распечатать чек
        async function saveAndPrintCheck(cashAmount, cardAmount, paymentMethod) {
            const subtotal = parseFloat(document.getElementById('subtotal').textContent) || 0;
            
            // Подготавливаем данные
            const checkData = {
                shift_id: currentShiftId,
                type: 'sale',
                items: Object.values(checkItems).map(item => ({
                    warehouse_item_id: item.id,
                    name: item.name,
                    price: item.price,
                    quantity: item.quantity,
                    total: item.price * item.quantity
                })),
                total_amount: subtotal,
                cash_amount: cashAmount,
                card_amount: cardAmount,
                cashier_name: '<?php echo $_SESSION['user_name']; ?>',
                payment_method: paymentMethod
            };
            
            try {
                // Сначала печатаем чек на ККТ
                await printCheckToKKT(checkData);
                
                // Только если печать успешна, сохраняем в базу данных
                const response = await fetch('../../api/save_check.php', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(checkData)
                });
                
                const result = await response.json();
                
                if (result.success) {
                    showSuccessModal(result.check_number || 'Чек #' + result.check_id, true, paymentMethod, cashAmount, cardAmount);
                } else {
                    // Отменяем транзакцию по карте, если чек не сохранился
                    if (cardAmount > 0 && lastCardTransaction) {
                        try {
                            await cancelCardTransaction(lastCardTransaction);
                        } catch (cancelError) {
                            console.error('Ошибка отмены транзакции:', cancelError);
                        }
                    }
                    throw new Error(result.error || 'Ошибка сохранения чека');
                }
            } catch (error) {
                // Отменяем транзакцию по карте, если была ошибка
                if (cardAmount > 0 && lastCardTransaction) {
                    try {
                        await cancelCardTransaction(lastCardTransaction);
                    } catch (cancelError) {
                        console.error('Ошибка отмены транзакции:', cancelError);
                    }
                }
                alert('Ошибка: ' + error.message);
            }
        }
        
        // Отмена транзакции по карте
        function cancelCardTransaction(transaction) {
            return new Promise((resolve, reject) => {
                if (!transaction || !transaction.universalID) {
                    reject(new Error('Нет данных транзакции для отмены'));
                    return;
                }
                
                const cancelData = {
                    Command: "CancelPaymentByPaymentCard",
                    NumDevice: 0,
                    Amount: transaction.amount,
                    UniversalID: transaction.universalID,
                    IdCommand: guid(),
                    Timeout: 120
                };
                
                ExecuteCommand(cancelData, 
                    function(result) {
                        if (result.Status === 0) {
                            resolve(result);
                        } else {
                            reject(new Error(result.Error || 'Ошибка отмены транзакции'));
                        }
                    },
                    function(error) {
                        reject(new Error(error));
                    }
                );
            });
        }
        
        // Печать чека на ККТ
        function printCheckToKKT(checkData) {
            return new Promise((resolve, reject) => {
                // Проверяем подключение к ККТ
                if (!isKKTConnected) {
                    reject(new Error('Нет соединения с ККТ! Проверьте подключение расширения.'));
                    return;
                }
                
                // Определяем тип чека для ККТ
                let typeCheck;
                switch (checkData.type) {
                    case 'sale': typeCheck = 0; break; // Продажа
                    case 'return': typeCheck = 1; break; // Возврат
                    case 'correction': typeCheck = 2; break; // Коррекция
                    default: typeCheck = 0;
                }
                
                // Подготавливаем данные для ККТ (упрощенная версия)
                const kkmData = {
                    Command: "RegisterCheck",
                    NumDevice: 0,
                    TypeCheck: typeCheck,
                    IsFiscalCheck: true,
                    NotPrint: false,
                    Timeout: 30,
                    IdCommand: guid(),
                    CashierName: checkData.cashier_name,
                    CheckStrings: checkData.items.map(item => ({
                        Register: {
                            Name: item.name.substring(0, 64),
                            Quantity: item.quantity,
                            Price: item.price,
                            Amount: item.total,
                            Tax: -1,
                            SignMethodCalculation: 4,
                            SignCalculationObject: 1
                        }
                    })),
                    Cash: checkData.cash_amount,
                    ElectronicPayment: checkData.card_amount,
                    // Если оплата по карте уже была проведена, отключаем автоматический эквайринг
                    PayByProcessing: false
                };
                
                // Вызываем команду через расширение
                ExecuteCommand(kkmData, 
                    function(result) {
                        if (result.Status === 0) {
                            // Успешная печать
                            resolve(result);
                        } else {
                            reject(new Error(result.Error || 'Ошибка печати чека на ККТ'));
                        }
                    },
                    function(error) {
                        reject(new Error('Ошибка связи с ККТ: ' + error));
                    }
                );
            });
        }
        
        // Показать окно успешного создания чека
        function showSuccessModal(checkNumber, printed, paymentMethod, cashAmount, cardAmount) {
            const modal = document.getElementById('resultModal');
            const content = document.getElementById('modalContent');
            
            let html = `
                <div style="text-align: center; padding: 20px 0;">
                    <i class="fas fa-check-circle" style="font-size: 60px; color: var(--secondary); margin-bottom: 20px;"></i>
                    <h3 style="color: var(--secondary); margin-bottom: 10px;">Чек успешно создан</h3>
                    <p><strong>Номер чека:</strong> ${checkNumber}</p>
                    <p><strong>Тип:</strong> Продажа</p>
            `;
            
            if (printed) {
                html += `
                    <p style="color: var(--secondary); margin-top: 15px;">
                        <i class="fas fa-print"></i> Чек распечатан на ККТ
                    </p>
                `;
            }
            
            if (cashAmount > 0 && cardAmount > 0) {
                html += `
                    <p style="color: var(--warning); margin-top: 10px;">
                        <i class="fas fa-money-check-alt"></i> Раздельная оплата: ${cashAmount.toFixed(2)} ₽ наличными + ${cardAmount.toFixed(2)} ₽ картой
                    </p>
                `;
            } else if (cashAmount > 0) {
                html += `
                    <p style="color: var(--secondary); margin-top: 10px;">
                        <i class="fas fa-money-bill-wave"></i> Оплата наличными: ${cashAmount.toFixed(2)} ₽
                    </p>
                `;
            } else if (cardAmount > 0) {
                html += `
                    <p style="color: var(--primary); margin-top: 10px;">
                        <i class="fas fa-credit-card"></i> Оплата по карте: ${cardAmount.toFixed(2)} ₽
                    </p>
                `;
            }
            
            html += `</div>`;
            content.innerHTML = html;
            modal.style.display = 'flex';
            
            // Очищаем чек после успешного сохранения
            setTimeout(() => {
                clearCheck();
            }, 100);
        }
        
        // Закрыть модальное окно
        function closeModal() {
            document.getElementById('resultModal').style.display = 'none';
        }
        
        // Печатать новый чек
        function printAnother() {
            closeModal();
            clearCheck();
        }
    </script>
</body>
</html>