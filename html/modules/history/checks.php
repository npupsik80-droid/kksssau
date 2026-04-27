<?php
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);
require_once '../../includes/auth_check.php';

// Фильтры
$filter_division_id = $_GET['division_id'] ?? $_SESSION['current_division_id'];
$filter_type = $_GET['type'] ?? 'all';
$filter_cashier_id = $_GET['cashier_id'] ?? '';
$date_from = $_GET['date_from'] ?? date('Y-m-d', strtotime('-7 days'));
$date_to = $_GET['date_to'] ?? date('Y-m-d');
$search = $_GET['search'] ?? ''; // Поиск по номеру чека или товару

// Пагинация
$page = max(1, intval($_GET['page'] ?? 1));
$limit = 50;
$offset = ($page - 1) * $limit;

// Проверка корректности дат
if (!strtotime($date_from)) {
    $date_from = date('Y-m-d', strtotime('-7 days'));
}
if (!strtotime($date_to)) {
    $date_to = date('Y-m-d');
}

// Получаем все подразделения (для администратора)
$divisions = [];
if ($_SESSION['permission_group'] === 'admin') {
    $stmt = $pdo->prepare("SELECT * FROM divisions ORDER BY name");
    $stmt->execute();
    $divisions = $stmt->fetchAll();
}

// Получаем всех кассиров
$cashiers = [];
$cashierStmt = $pdo->prepare("
    SELECT DISTINCT u.id, u.full_name, u.login
    FROM users u
    JOIN checks c ON u.id = c.user_id
    ORDER BY u.full_name
");
$cashierStmt->execute();
$cashiers = $cashierStmt->fetchAll();

// Условия для запроса
$where = "WHERE DATE(c.created_at) BETWEEN ? AND ?";
$params = [$date_from, $date_to];

if ($filter_division_id && $filter_division_id !== 'all') {
    $where .= " AND c.division_id = ?";
    $params[] = $filter_division_id;
} elseif ($_SESSION['permission_group'] !== 'admin') {
    // Обычный пользователь видит только свои подразделения
    $where .= " AND c.division_id = ?";
    $params[] = $_SESSION['current_division_id'];
}

if ($filter_type && $filter_type !== 'all') {
    $where .= " AND c.type = ?";
    $params[] = $filter_type;
}

if ($filter_cashier_id && $filter_cashier_id !== 'all') {
    $where .= " AND c.user_id = ?";
    $params[] = $filter_cashier_id;
}

if ($search) {
    $where .= " AND (c.kkm_check_number LIKE ? OR c.items LIKE ?)";
    $params[] = "%{$search}%";
    $params[] = "%{$search}%";
}

// Преобразуем лимит и офсет в числа для безопасности
$limit_int = (int)$limit;
$offset_int = (int)$offset;

// Получаем чеки
$stmt = $pdo->prepare("
    SELECT 
        c.*,
        u.full_name as cashier_name,
        u.login as cashier_login,
        d.name as division_name,
        s.kkm_shift_number,
        s.opened_at as shift_opened
    FROM checks c
    LEFT JOIN users u ON c.user_id = u.id
    LEFT JOIN divisions d ON c.division_id = d.id
    LEFT JOIN shifts s ON c.shift_id = s.id
    {$where}
    ORDER BY c.created_at DESC
    LIMIT {$limit_int} OFFSET {$offset_int}
");

$stmt->execute($params);
$checks = $stmt->fetchAll();

// Общее количество
$countStmt = $pdo->prepare("
    SELECT COUNT(*) as total 
    FROM checks c
    {$where}
");
$countStmt->execute($params);
$total = $countStmt->fetch()['total'];
$totalPages = ceil($total / $limit);

// Статистика
$statsStmt = $pdo->prepare("
    SELECT 
        COUNT(*) as total_checks,
        SUM(c.total_amount) as total_amount,
        SUM(c.cash_amount) as total_cash,
        SUM(c.card_amount) as total_card,
        COUNT(CASE WHEN c.type = 'sale' THEN 1 END) as sale_count,
        COUNT(CASE WHEN c.type = 'return' THEN 1 END) as return_count,
        COUNT(CASE WHEN c.type = 'correction' THEN 1 END) as correction_count
    FROM checks c
    {$where}
");
$statsStmt->execute($params);
$stats = $statsStmt->fetch();
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>История чеков - RunaRMK</title>
    <link rel="stylesheet" href="../../css/style.css">
    <style>
        .checks-container {
            max-width: 1400px;
            margin: 0 auto;
        }
        
        .check-card {
            background: white;
            border-radius: 10px;
            padding: 25px;
            margin-bottom: 20px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            border-left: 4px solid #3498db;
        }
        
        .check-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 20px;
            padding-bottom: 15px;
            border-bottom: 2px solid #f8f9fa;
        }
        
        .check-number {
            font-size: 24px;
            font-weight: bold;
            color: #2c3e50;
        }
        
        .check-type {
            display: inline-block;
            padding: 6px 12px;
            border-radius: 20px;
            font-weight: bold;
            font-size: 14px;
            margin-right: 10px;
        }
        
        .type-sale { background: #d4edda; color: #155724; }
        .type-return { background: #f8d7da; color: #721c24; }
        .type-correction { background: #fff3cd; color: #856404; }
        
        .check-meta {
            display: flex;
            gap: 20px;
            color: #666;
            font-size: 14px;
            margin-top: 10px;
        }
        
        .check-items {
            margin: 20px 0;
        }
        
        .item-row {
            display: grid;
            grid-template-columns: 3fr 1fr 1fr 1fr 1fr;
            padding: 10px 0;
            border-bottom: 1px solid #f0f0f0;
        }
        
        .item-row:last-child {
            border-bottom: none;
        }
        
        .item-header {
            font-weight: bold;
            color: #495057;
            background: #f8f9fa;
            padding: 12px 0;
            border-radius: 5px;
        }
        
        .check-totals {
            background: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            margin-top: 20px;
        }
        
        .total-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
        }
        
        .total-item {
            text-align: center;
        }
        
        .total-item .label {
            font-size: 14px;
            color: #666;
            margin-bottom: 5px;
        }
        
        .total-item .value {
            font-size: 20px;
            font-weight: bold;
            color: #2c3e50;
        }
        
        .check-actions {
            display: flex;
            gap: 10px;
            margin-top: 20px;
            padding-top: 20px;
            border-top: 1px solid #f0f0f0;
        }
        
        .fiscal-info {
            background: #e3f2fd;
            padding: 15px;
            border-radius: 8px;
            margin-top: 15px;
            font-family: monospace;
            font-size: 12px;
            word-break: break-all;
        }
        
        .stats-overview {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 15px;
            margin-bottom: 20px;
        }
        
        .stat-card {
            background: white;
            padding: 20px;
            border-radius: 10px;
            text-align: center;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
        }
        
        .stat-card .number {
            font-size: 28px;
            font-weight: bold;
            margin: 10px 0;
        }
        
        .stat-card.total-checks .number { color: #3498db; }
        .stat-card.total-amount .number { color: #2ecc71; }
        .stat-card.cash-amount .number { color: #f39c12; }
        .stat-card.card-amount .number { color: #9b59b6; }
        
        .advanced-filters {
            background: white;
            padding: 20px;
            border-radius: 10px;
            margin-bottom: 20px;
        }
        
        .filter-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
        }
        
        .empty-state {
            text-align: center;
            padding: 50px;
            color: #666;
        }
        
        .check-collapse {
            cursor: pointer;
            color: #3498db;
            font-size: 14px;
            margin-top: 10px;
            display: inline-flex;
            align-items: center;
            gap: 5px;
        }
        
        .collapsed-content {
            max-height: 0;
            overflow: hidden;
            transition: max-height 0.3s ease;
        }
        
        .collapsed-content.expanded {
            max-height: 1000px;
        }
        
        @media print {
            .filters-panel, .check-actions, .header, .export-buttons {
                display: none !important;
            }
            
            .check-card {
                page-break-inside: avoid;
                border: 1px solid #ddd;
                box-shadow: none;
            }
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1><i class="fas fa-receipt"></i> История чеков</h1>
            <a href="../../index.php" class="btn btn-primary">
                <i class="fas fa-arrow-left"></i> На главную
            </a>
        </div>
        
        <!-- Общая статистика -->
        <div class="stats-overview">
            <div class="stat-card total-checks">
                <div class="title">Всего чеков</div>
                <div class="number"><?php echo $stats['total_checks'] ?? 0; ?></div>
                <div class="details">
                    <?php echo ($stats['sale_count'] ?? 0); ?> продаж, 
                    <?php echo ($stats['return_count'] ?? 0); ?> возвратов
                </div>
            </div>
            
            <div class="stat-card total-amount">
                <div class="title">Общая сумма</div>
                <div class="number"><?php echo number_format($stats['total_amount'] ?? 0, 2); ?> ₽</div>
                <div class="details">За выбранный период</div>
            </div>
            
            <div class="stat-card cash-amount">
                <div class="title">Наличными</div>
                <div class="number"><?php echo number_format($stats['total_cash'] ?? 0, 2); ?> ₽</div>
                <div class="details">
                    <?php echo $stats['total_cash'] > 0 ? round(($stats['total_cash'] / $stats['total_amount']) * 100, 1) : 0; ?>%
                </div>
            </div>
            
            <div class="stat-card card-amount">
                <div class="title">Безналичными</div>
                <div class="number"><?php echo number_format($stats['total_card'] ?? 0, 2); ?> ₽</div>
                <div class="details">
                    <?php echo $stats['total_card'] > 0 ? round(($stats['total_card'] / $stats['total_amount']) * 100, 1) : 0; ?>%
                </div>
            </div>
        </div>
        
        <!-- Фильтры -->
        <div class="advanced-filters">
            <form method="GET" id="filterForm">
                <div class="filter-grid">
                    <?php if ($_SESSION['permission_group'] === 'admin'): ?>
                    <div>
                        <label>Подразделение</label>
                        <select name="division_id" class="form-control">
                            <option value="all">Все подразделения</option>
                            <?php foreach ($divisions as $division): ?>
                            <option value="<?php echo $division['id']; ?>"
                                <?php echo $filter_division_id == $division['id'] ? 'selected' : ''; ?>>
                                <?php echo htmlspecialchars($division['name']); ?>
                            </option>
                            <?php endforeach; ?>
                        </select>
                    </div>
                    <?php endif; ?>
                    
                    <div>
                        <label>Тип чека</label>
                        <select name="type" class="form-control">
                            <option value="all">Все типы</option>
                            <option value="sale" <?php echo $filter_type == 'sale' ? 'selected' : ''; ?>>Продажа</option>
                            <option value="return" <?php echo $filter_type == 'return' ? 'selected' : ''; ?>>Возврат</option>
                            <option value="correction" <?php echo $filter_type == 'correction' ? 'selected' : ''; ?>>Коррекция</option>
                        </select>
                    </div>
                    
                    <div>
                        <label>Кассир</label>
                        <select name="cashier_id" class="form-control">
                            <option value="all">Все кассиры</option>
                            <?php foreach ($cashiers as $cashier): ?>
                            <option value="<?php echo $cashier['id']; ?>"
                                <?php echo $filter_cashier_id == $cashier['id'] ? 'selected' : ''; ?>>
                                <?php echo htmlspecialchars($cashier['full_name']); ?>
                            </option>
                            <?php endforeach; ?>
                        </select>
                    </div>
                    
                    <div>
                        <label>С даты</label>
                        <input type="date" name="date_from" class="form-control" 
                               value="<?php echo htmlspecialchars($date_from); ?>">
                    </div>
                    
                    <div>
                        <label>По дату</label>
                        <input type="date" name="date_to" class="form-control" 
                               value="<?php echo htmlspecialchars($date_to); ?>">
                    </div>
                    
                    <div>
                        <label>Поиск (номер или товар)</label>
                        <input type="text" name="search" class="form-control" 
                               value="<?php echo htmlspecialchars($search); ?>"
                               placeholder="Номер чека или название товара">
                    </div>
                </div>
                
                <div style="display: flex; gap: 10px; justify-content: flex-end; margin-top: 15px;">
                    <button type="submit" class="btn btn-primary">
                        <i class="fas fa-filter"></i> Применить фильтры
                    </button>
                    <a href="?" class="btn btn-secondary">Сбросить</a>
                    
                    <button type="button" class="btn btn-success" onclick="setToday()">
                        Сегодня
                    </button>
                    <button type="button" class="btn btn-warning" onclick="setThisMonth()">
                        Этот месяц
                    </button>
                </div>
            </form>
        </div>
        
        <!-- Экспорт -->
        <div class="export-buttons" style="text-align: right; margin-bottom: 20px;">
            <button class="btn btn-success" onclick="exportChecks('excel')">
                <i class="fas fa-file-excel"></i> Экспорт в Excel
            </button>
            <button class="btn btn-primary" onclick="exportChecks('pdf')">
                <i class="fas fa-file-pdf"></i> Экспорт в PDF
            </button>
            <button class="btn btn-secondary" onclick="window.print()">
                <i class="fas fa-print"></i> Печать отчета
            </button>
        </div>
        
        <!-- Список чеков -->
        <div id="checksList">
            <?php if (count($checks) > 0): ?>
                <?php foreach ($checks as $check): 
                    $items = json_decode($check['items'], true) ?? [];
                    $fiscal_data = json_decode($check['fiscal_data'] ?? '{}', true);
                ?>
                <div class="check-card">
                    <div class="check-header">
                        <div>
                            <div class="check-number">
                                Чек #<?php echo $check['kkm_check_number'] ?? $check['id']; ?>
                                <span class="check-type type-<?php echo $check['type']; ?>">
                                    <?php 
                                    echo match($check['type']) {
                                        'sale' => 'Продажа',
                                        'return' => 'Возврат',
                                        'correction' => 'Коррекция',
                                        default => $check['type']
                                    };
                                    ?>
                                </span>
                            </div>
                            
                            <div class="check-meta">
                                <span><i class="fas fa-calendar"></i> <?php echo date('d.m.Y H:i', strtotime($check['created_at'])); ?></span>
                                <span><i class="fas fa-user"></i> <?php echo htmlspecialchars($check['cashier_name']); ?></span>
                                <span><i class="fas fa-store"></i> <?php echo htmlspecialchars($check['division_name']); ?></span>
                                <span><i class="fas fa-door-open"></i> Смена #<?php echo $check['kkm_shift_number']; ?></span>
                            </div>
                        </div>
                        
                        <div style="text-align: right;">
                            <div style="font-size: 28px; font-weight: bold; color: #2ecc71;">
                                <?php echo number_format($check['total_amount'], 2); ?> ₽
                            </div>
                            <div style="color: #666; font-size: 14px; margin-top: 5px;">
                                <?php if ($check['cash_amount'] > 0): ?>
                                <span>Нал: <?php echo number_format($check['cash_amount'], 2); ?> ₽</span>
                                <?php endif; ?>
                                <?php if ($check['card_amount'] > 0): ?>
                                <span>Безнал: <?php echo number_format($check['card_amount'], 2); ?> ₽</span>
                                <?php endif; ?>
                            </div>
                        </div>
                    </div>
                    
                    <div class="check-collapse" onclick="toggleCheckDetails(<?php echo $check['id']; ?>)">
                        <i class="fas fa-chevron-down" id="chevron-<?php echo $check['id']; ?>"></i>
                        Показать детали чека
                    </div>
                    
                    <div class="collapsed-content" id="details-<?php echo $check['id']; ?>">
                        <!-- Товары -->
                        <?php if (count($items) > 0): ?>
                        <div class="check-items">
                            <div class="item-row item-header">
                                <div>Наименование</div>
                                <div>Количество</div>
                                <div>Цена</div>
                                <div>Сумма</div>
                                <div>НДС</div>
                            </div>
                            
                            <?php foreach ($items as $item): ?>
                            <div class="item-row">
                                <div><?php echo htmlspecialchars($item['name'] ?? ''); ?></div>
                                <div><?php echo $item['quantity'] ?? 0; ?> шт.</div>
                                <div><?php echo number_format($item['price'] ?? 0, 2); ?> ₽</div>
                                <div><?php echo number_format(($item['quantity'] ?? 0) * ($item['price'] ?? 0), 2); ?> ₽</div>
                                <div><?php echo $item['tax'] ?? 20; ?>%</div>
                            </div>
                            <?php endforeach; ?>
                        </div>
                        <?php endif; ?>
                        
                        <!-- Итоги -->
                        <div class="check-totals">
                            <div class="total-grid">
                                <div class="total-item">
                                    <div class="label">Общая сумма</div>
                                    <div class="value"><?php echo number_format($check['total_amount'], 2); ?> ₽</div>
                                </div>
                                
                                <?php if ($check['cash_amount'] > 0): ?>
                                <div class="total-item">
                                    <div class="label">Наличные</div>
                                    <div class="value"><?php echo number_format($check['cash_amount'], 2); ?> ₽</div>
                                </div>
                                <?php endif; ?>
                                
                                <?php if ($check['card_amount'] > 0): ?>
                                <div class="total-item">
                                    <div class="label">Безналичные</div>
                                    <div class="value"><?php echo number_format($check['card_amount'], 2); ?> ₽</div>
                                </div>
                                <?php endif; ?>
                                
                                <div class="total-item">
                                    <div class="label">Количество товаров</div>
                                    <div class="value">
                                        <?php 
                                        $totalItems = 0;
                                        foreach ($items as $item) {
                                            $totalItems += $item['quantity'] ?? 0;
                                        }
                                        echo $totalItems;
                                        ?> шт.
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Фискальные данные -->
                        <?php if (!empty($fiscal_data) || !empty($check['fiscal_data'])): ?>
                        <div class="fiscal-info">
                            <div style="font-weight: bold; margin-bottom: 10px; color: #1976d2;">
                                <i class="fas fa-qrcode"></i> Фискальные данные
                            </div>
                            
                            <?php if (isset($fiscal_data['QRCode'])): ?>
                            <div style="margin-bottom: 10px;">
                                <strong>QR-код:</strong> <?php echo htmlspecialchars($fiscal_data['QRCode']); ?>
                            </div>
                            <?php endif; ?>
                            
                            <?php if (isset($fiscal_data['URL'])): ?>
                            <div style="margin-bottom: 10px;">
                                <strong>Ссылка на чек:</strong> 
                                <a href="<?php echo htmlspecialchars($fiscal_data['URL']); ?>" target="_blank">
                                    <?php echo htmlspecialchars($fiscal_data['URL']); ?>
                                </a>
                            </div>
                            <?php endif; ?>
                            
                            <div style="font-size: 11px; color: #666; margin-top: 10px;">
                                Фискальный признак документа: 
                                <?php echo $fiscal_data['FiscalSign'] ?? 'не указан'; ?>
                            </div>
                        </div>
                        <?php endif; ?>
                    </div>
                    
                    <!-- Действия -->
                    <div class="check-actions">
                        <button class="btn btn-sm btn-primary" onclick="reprintCheck(<?php echo $check['id']; ?>)">
                            <i class="fas fa-print"></i> Повторная печать
                        </button>
                        
                        <?php if ($check['type'] === 'sale'): ?>
                        <button class="btn btn-sm btn-danger" onclick="createReturn(<?php echo $check['id']; ?>)">
                            <i class="fas fa-undo"></i> Создать возврат
                        </button>
                        <?php endif; ?>
                        
                        <button class="btn btn-sm btn-secondary" onclick="viewCheckDetails(<?php echo $check['id']; ?>)">
                            <i class="fas fa-eye"></i> Полная информация
                        </button>
                        
                        <?php if ($_SESSION['permission_group'] === 'admin'): ?>
                        <button class="btn btn-sm btn-warning" onclick="voidCheck(<?php echo $check['id']; ?>)">
                            <i class="fas fa-ban"></i> Аннулировать
                        </button>
                        <?php endif; ?>
                    </div>
                </div>
                <?php endforeach; ?>
            <?php else: ?>
                <div class="empty-state">
                    <i class="fas fa-receipt fa-3x" style="margin-bottom: 20px; color: #ddd;"></i>
                    <h3>Чеки не найдены</h3>
                    <p>Попробуйте изменить параметры фильтрации</p>
                </div>
            <?php endif; ?>
        </div>
        
        <!-- Пагинация -->
        <?php if ($totalPages > 1): ?>
        <div class="pagination" style="display: flex; justify-content: center; gap: 10px; margin: 30px 0;">
            <?php if ($page > 1): ?>
            <a href="?<?php echo http_build_query(array_merge($_GET, ['page' => $page - 1])); ?>" 
               class="btn btn-sm btn-primary">
                <i class="fas fa-chevron-left"></i> Назад
            </a>
            <?php endif; ?>
            
            <span style="padding: 8px 12px; background: #f8f9fa; border-radius: 4px;">
                Страница <?php echo $page; ?> из <?php echo $totalPages; ?>
            </span>
            
            <?php if ($page < $totalPages): ?>
            <a href="?<?php echo http_build_query(array_merge($_GET, ['page' => $page + 1])); ?>" 
               class="btn btn-sm btn-primary">
                Вперед <i class="fas fa-chevron-right"></i>
            </a>
            <?php endif; ?>
        </div>
        
        <div style="text-align: center; color: #666; margin-bottom: 30px;">
            Показано <?php echo count($checks); ?> из <?php echo $total; ?> чеков
        </div>
        <?php endif; ?>
    </div>
    
    <script>
        // Переключение отображения деталей чека
        function toggleCheckDetails(checkId) {
            const content = document.getElementById('details-' + checkId);
            const chevron = document.getElementById('chevron-' + checkId);
            
            if (content.classList.contains('expanded')) {
                content.classList.remove('expanded');
                chevron.className = 'fas fa-chevron-down';
                content.previousElementSibling.innerHTML = '<i class="fas fa-chevron-down"></i> Показать детали чека';
            } else {
                content.classList.add('expanded');
                chevron.className = 'fas fa-chevron-up';
                content.previousElementSibling.innerHTML = '<i class="fas fa-chevron-up"></i> Скрыть детали чека';
            }
        }
        
        // Экспорт чеков
        function exportChecks(format) {
            const params = new URLSearchParams(window.location.search);
            params.set('export', format);
            
            window.open('export_checks.php?' + params.toString(), '_blank');
        }
        
        // Фильтр "сегодня"
        function setToday() {
            const today = new Date().toISOString().split('T')[0];
            document.querySelector('input[name="date_from"]').value = today;
            document.querySelector('input[name="date_to"]').value = today;
            document.getElementById('filterForm').submit();
        }
        
        // Фильтр "этот месяц"
        function setThisMonth() {
            const today = new Date();
            const firstDay = new Date(today.getFullYear(), today.getMonth(), 1)
                .toISOString().split('T')[0];
            
            document.querySelector('input[name="date_from"]').value = firstDay;
            document.querySelector('input[name="date_to"]').value = today.toISOString().split('T')[0];
            document.getElementById('filterForm').submit();
        }
        
        // Действия с чеком
        function reprintCheck(checkId) {
            if (confirm('Повторно распечатать чек?')) {
                // В реальности здесь будет запрос к API ККТ
                alert('Команда на повторную печать отправлена на ККТ');
            }
        }
        
        function createReturn(checkId) {
            if (confirm('Создать возврат по этому чеку?')) {
                window.open(`../checks/return.php?check_id=${checkId}`, '_blank');
            }
        }
        
        function viewCheckDetails(checkId) {
            window.open(`check_details.php?id=${checkId}`, '_blank');
        }
        
        function voidCheck(checkId) {
            if (confirm('Аннулировать чек? Это действие нельзя отменить.')) {
                fetch('void_check.php', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ check_id: checkId })
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert('Чек аннулирован');
                        location.reload();
                    } else {
                        alert('Ошибка: ' + (data.error || 'Неизвестная ошибка'));
                    }
                })
                .catch(error => {
                    alert('Ошибка сети: ' + error.message);
                });
            }
        }
        
        // Автоматически разворачиваем первый чек
        document.addEventListener('DOMContentLoaded', function() {
            const firstCheck = document.querySelector('.check-card');
            if (firstCheck) {
                const checkId = firstCheck.querySelector('.check-collapse').getAttribute('onclick').match(/\d+/)[0];
                toggleCheckDetails(checkId);
            }
        });
        
        // Поиск с задержкой
        let searchTimer;
        const searchInput = document.querySelector('input[name="search"]');
        if (searchInput) {
            searchInput.addEventListener('input', function() {
                clearTimeout(searchTimer);
                searchTimer = setTimeout(() => {
                    this.form.submit();
                }, 500);
            });
        }
    </script>
</body>
</html>