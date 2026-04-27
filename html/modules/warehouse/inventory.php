<?php
require_once '../../includes/auth_check.php';

if ($_SESSION['permission_group'] !== 'admin') {
    header('Location: ../../index.php');
    exit();
}

// Инициализация сессии для инвентаризации
if (!isset($_SESSION['inventory'])) {
    $_SESSION['inventory'] = [];
}

$warehouse_id = isset($_GET['warehouse_id']) ? (int)$_GET['warehouse_id'] : 0;
$action = $_POST['action'] ?? '';

// Получаем список всех складов
$stmt = $pdo->query("SELECT w.*, d.name as division_name FROM warehouses w LEFT JOIN divisions d ON w.division_id = d.id ORDER BY w.name");
$allWarehouses = $stmt->fetchAll();

// Если склад не выбран, пытаемся взять из сессии
if (!$warehouse_id && isset($_SESSION['current_inventory_warehouse'])) {
    $warehouse_id = $_SESSION['current_inventory_warehouse'];
} elseif ($warehouse_id) {
    $_SESSION['current_inventory_warehouse'] = $warehouse_id;
}

// Инициализация данных инвентаризации для выбранного склада
if ($warehouse_id && !isset($_SESSION['inventory'][$warehouse_id])) {
    $_SESSION['inventory'][$warehouse_id] = [
        'scanned' => [], // nomenclature_id => scanned_qty
        'unknown' => []  // barcode => scanned_qty (для неизвестных штрих-кодов)
    ];
}

// Обработка действий
if ($_SERVER['REQUEST_METHOD'] === 'POST' && $warehouse_id) {
    if ($action === 'scan') {
        $barcode = trim($_POST['barcode'] ?? '');
        if ($barcode !== '') {
            // Ищем номенклатуру по штрих-коду
            $stmt = $pdo->prepare("SELECT id FROM nomenclatures WHERE barcode = ?");
            $stmt->execute([$barcode]);
            $nomenclature = $stmt->fetch();

            if ($nomenclature) {
                $nomenclature_id = $nomenclature['id'];
                // Увеличиваем количество для этой номенклатуры
                if (!isset($_SESSION['inventory'][$warehouse_id]['scanned'][$nomenclature_id])) {
                    $_SESSION['inventory'][$warehouse_id]['scanned'][$nomenclature_id] = 0;
                }
                $_SESSION['inventory'][$warehouse_id]['scanned'][$nomenclature_id] += 1;
            } else {
                // Неизвестный штрих-код
                if (!isset($_SESSION['inventory'][$warehouse_id]['unknown'][$barcode])) {
                    $_SESSION['inventory'][$warehouse_id]['unknown'][$barcode] = 0;
                }
                $_SESSION['inventory'][$warehouse_id]['unknown'][$barcode] += 1;
            }
        }
    } elseif ($action === 'reset') {
        // Сброс инвентаризации для этого склада
        unset($_SESSION['inventory'][$warehouse_id]);
        $_SESSION['inventory'][$warehouse_id] = [
            'scanned' => [],
            'unknown' => []
        ];
    } elseif ($action === 'complete') {
        // Здесь можно сохранить результаты инвентаризации в БД (опционально)
        // Пока просто покажем отчет
    }
    // Редирект, чтобы избежать повторной отправки формы
    header("Location: inventory.php?warehouse_id=$warehouse_id");
    exit();
}

// Получаем ожидаемые товары для выбранного склада
$expectedItems = [];
$warehouseInfo = null;
if ($warehouse_id) {
    $stmt = $pdo->prepare("
        SELECT n.id, n.name, n.barcode, n.description, SUM(wi.quantity) as expected_quantity
        FROM warehouse_items wi
        JOIN nomenclatures n ON wi.nomenclature_id = n.id
        WHERE wi.warehouse_id = ? AND wi.quantity > 0
        GROUP BY n.id
        ORDER BY n.name
    ");
    $stmt->execute([$warehouse_id]);
    $expectedItems = $stmt->fetchAll();

    // Информация о складе
    $stmt = $pdo->prepare("SELECT w.*, d.name as division_name FROM warehouses w LEFT JOIN divisions d ON w.division_id = d.id WHERE w.id = ?");
    $stmt->execute([$warehouse_id]);
    $warehouseInfo = $stmt->fetch();
}

// Подготовка данных для отображения
$scanned = $_SESSION['inventory'][$warehouse_id]['scanned'] ?? [];
$unknown = $_SESSION['inventory'][$warehouse_id]['unknown'] ?? [];

// Собираем полную информацию по отсканированным товарам (известным)
$scannedItems = [];
if (!empty($scanned)) {
    $ids = array_keys($scanned);
    $placeholders = implode(',', array_fill(0, count($ids), '?'));
    $stmt = $pdo->prepare("SELECT id, name, barcode, description FROM nomenclatures WHERE id IN ($placeholders)");
    $stmt->execute($ids);
    $nomenclatures = $stmt->fetchAll();
    foreach ($nomenclatures as $nom) {
        $nom['scanned_quantity'] = $scanned[$nom['id']];
        $scannedItems[] = $nom;
    }
}

// Формируем списки расхождений
$missing = []; // товары, которые есть в ожидании, но не отсканированы или отсканировано меньше
$excess = [];  // товары, которые отсканированы, но их нет в ожидании или отсканировано больше

if ($warehouse_id) {
    // Недостающие
    foreach ($expectedItems as $expected) {
        $scannedQty = $scanned[$expected['id']] ?? 0;
        if ($scannedQty < $expected['expected_quantity']) {
            $missing[] = [
                'name' => $expected['name'],
                'barcode' => $expected['barcode'],
                'expected' => $expected['expected_quantity'],
                'scanned' => $scannedQty,
                'difference' => $expected['expected_quantity'] - $scannedQty
            ];
        }
    }

    // Излишки среди известных товаров
    foreach ($scannedItems as $scanned) {
        $expectedQty = 0;
        foreach ($expectedItems as $exp) {
            if ($exp['id'] == $scanned['id']) {
                $expectedQty = $exp['expected_quantity'];
                break;
            }
        }
        if ($scanned['scanned_quantity'] > $expectedQty) {
            $excess[] = [
                'name' => $scanned['name'],
                'barcode' => $scanned['barcode'],
                'expected' => $expectedQty,
                'scanned' => $scanned['scanned_quantity'],
                'difference' => $scanned['scanned_quantity'] - $expectedQty
            ];
        }
    }

    // Излишки неизвестных товаров
    foreach ($unknown as $barcode => $qty) {
        $excess[] = [
            'name' => 'Неизвестный товар',
            'barcode' => $barcode,
            'expected' => 0,
            'scanned' => $qty,
            'difference' => $qty
        ];
    }
}
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Инвентаризация склада - RunaRMK</title>
    <style>
        /* Стили аналогичны view.php, можно скопировать оттуда, но для краткости оставим основные */
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
        .btn-warning {
            background: linear-gradient(135deg, var(--warning) 0%, #e67e22 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(243, 156, 18, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        .btn-info {
            background: linear-gradient(135deg, var(--info) 0%, #2980b9 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(52, 152, 219, 0.3);
        }
        .container {
            max-width: 1400px;
            margin: 0 auto;
            padding: 0 30px 50px;
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
        .filter-panel {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            margin-bottom: 40px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
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
        .form-group {
            margin-bottom: 20px;
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
        .scan-panel {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            margin-bottom: 40px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
        }
        .scan-form {
            display: flex;
            gap: 20px;
            align-items: flex-end;
        }
        .scan-form .form-group {
            flex: 1;
            margin-bottom: 0;
        }
        .products-table-container {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
            margin-bottom: 40px;
            overflow-x: auto;
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
        .badge-danger {
            background: linear-gradient(135deg, rgba(231, 76, 60, 0.15) 0%, rgba(231, 76, 60, 0.05) 100%);
            color: var(--danger);
            border: 2px solid rgba(231, 76, 60, 0.3);
        }
        .badge-info {
            background: linear-gradient(135deg, rgba(52, 152, 219, 0.15) 0%, rgba(52, 152, 219, 0.05) 100%);
            color: var(--info);
            border: 2px solid rgba(52, 152, 219, 0.3);
        }
        .empty-state {
            text-align: center;
            padding: 60px 30px;
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
        .action-buttons {
            display: flex;
            gap: 20px;
            flex-wrap: wrap;
            margin-top: 20px;
        }
    </style>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">
</head>
<body>
    <div class="header">
        <h1>
            <i class="fas fa-clipboard-list"></i>
            Инвентаризация склада
        </h1>
        <a href="view.php" class="btn btn-primary">
            <i class="fas fa-arrow-left"></i> Вернуться к складам
        </a>
    </div>

    <div class="container">
        <!-- Панель выбора склада -->
        <div class="filter-panel fade-in-up">
            <h3><i class="fas fa-warehouse"></i> Выберите склад для инвентаризации</h3>
            <form method="GET" class="filter-form">
                <div class="form-group">
                    <label for="warehouse_id"><i class="fas fa-warehouse"></i> Склад</label>
                    <select name="warehouse_id" id="warehouse_id" class="form-control" onchange="this.form.submit()">
                        <option value="">-- Выберите склад --</option>
                        <?php foreach ($allWarehouses as $wh): ?>
                        <option value="<?php echo $wh['id']; ?>" <?php echo $warehouse_id == $wh['id'] ? 'selected' : ''; ?>>
                            <?php echo htmlspecialchars($wh['name']); ?> (<?php echo htmlspecialchars($wh['division_name']); ?>)
                        </option>
                        <?php endforeach; ?>
                    </select>
                </div>
            </form>
        </div>

        <?php if ($warehouse_id && $warehouseInfo): ?>
            <!-- Панель сканирования -->
            <div class="scan-panel fade-in-up">
                <h2><i class="fas fa-qrcode"></i> Сканирование штрих-кодов</h2>
                <form method="POST" class="scan-form">
                    <input type="hidden" name="action" value="scan">
                    <div class="form-group" style="flex: 1;">
                        <label for="barcode"><i class="fas fa-barcode"></i> Штрих-код</label>
                        <input type="text" name="barcode" id="barcode" class="form-control" placeholder="Введите или отсканируйте штрих-код" autofocus>
                    </div>
                    <button type="submit" class="btn btn-primary">
                        <i class="fas fa-plus"></i> Добавить
                    </button>
                </form>
                <div class="action-buttons">
                    <form method="POST" style="display: inline;">
                        <input type="hidden" name="action" value="reset">
                        <button type="submit" class="btn btn-warning" onclick="return confirm('Сбросить все результаты инвентаризации?')">
                            <i class="fas fa-undo"></i> Сбросить
                        </button>
                    </form>
                    <form method="POST" style="display: inline;">
                        <input type="hidden" name="action" value="complete">
                        <button type="submit" class="btn btn-info">
                            <i class="fas fa-check-circle"></i> Завершить инвентаризацию
                        </button>
                    </form>
                </div>
            </div>

            <!-- Ожидаемые товары (по учету) -->
            <div class="products-table-container fade-in-up">
                <div class="table-header">
                    <h2><i class="fas fa-boxes"></i> Ожидаемые товары (учет)</h2>
                    <div class="table-count">
                        <i class="fas fa-box"></i> <?php echo count($expectedItems); ?> позиций
                    </div>
                </div>
                <?php if (count($expectedItems) > 0): ?>
                <table class="products-table">
                    <thead>
                        <tr>
                            <th>Товар</th>
                            <th>Штрих-код</th>
                            <th>Ожидаемое количество</th>
                            <th>Отсканировано</th>
                            <th>Статус</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($expectedItems as $item): 
                            $scannedQty = $scanned[$item['id']] ?? 0;
                            $status = $scannedQty >= $item['expected_quantity'] ? 'ok' : 'missing';
                        ?>
                        <tr>
                            <td>
                                <div class="product-name"><?php echo htmlspecialchars($item['name']); ?></div>
                                <?php if ($item['description']): ?>
                                <div class="product-description"><?php echo htmlspecialchars(substr($item['description'], 0, 50)); ?></div>
                                <?php endif; ?>
                            </td>
                            <td><span class="badge badge-info"><?php echo htmlspecialchars($item['barcode']); ?></span></td>
                            <td><?php echo $item['expected_quantity']; ?> шт.</td>
                            <td><?php echo $scannedQty; ?> шт.</td>
                            <td>
                                <?php if ($status == 'ok'): ?>
                                <span class="badge badge-success"><i class="fas fa-check"></i> В наличии</span>
                                <?php else: ?>
                                <span class="badge badge-warning"><i class="fas fa-exclamation-triangle"></i> Недостача</span>
                                <?php endif; ?>
                            </td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
                <?php else: ?>
                <div class="empty-state">
                    <i class="fas fa-box-open"></i>
                    <h3>На складе нет товаров</h3>
                </div>
                <?php endif; ?>
            </div>

            <!-- Отсканированные товары -->
            <div class="products-table-container fade-in-up">
                <div class="table-header">
                    <h2><i class="fas fa-check-double"></i> Отсканированные товары (факт)</h2>
                    <div class="table-count">
                        <i class="fas fa-cubes"></i> <?php echo count($scannedItems) + count($unknown); ?> позиций
                    </div>
                </div>
                <?php if (!empty($scannedItems) || !empty($unknown)): ?>
                <table class="products-table">
                    <thead>
                        <tr>
                            <th>Товар</th>
                            <th>Штрих-код</th>
                            <th>Отсканировано</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($scannedItems as $item): ?>
                        <tr>
                            <td><?php echo htmlspecialchars($item['name']); ?></td>
                            <td><span class="badge badge-info"><?php echo htmlspecialchars($item['barcode']); ?></span></td>
                            <td><?php echo $item['scanned_quantity']; ?> шт.</td>
                        </tr>
                        <?php endforeach; ?>
                        <?php foreach ($unknown as $barcode => $qty): ?>
                        <tr>
                            <td><em style="color: #999;">Неизвестный товар</em></td>
                            <td><span class="badge badge-danger"><?php echo htmlspecialchars($barcode); ?></span></td>
                            <td><?php echo $qty; ?> шт.</td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
                <?php else: ?>
                <div class="empty-state">
                    <i class="fas fa-camera"></i>
                    <h3>Пока ничего не отсканировано</h3>
                </div>
                <?php endif; ?>
            </div>

            <!-- Расхождения -->
            <?php if (!empty($missing) || !empty($excess)): ?>
            <div class="products-table-container fade-in-up">
                <div class="table-header">
                    <h2><i class="fas fa-chart-bar"></i> Расхождения</h2>
                </div>
                <?php if (!empty($missing)): ?>
                <h3 style="margin: 20px 0 10px; color: var(--warning);">Недостающие товары</h3>
                <table class="products-table">
                    <thead>
                        <tr>
                            <th>Товар</th>
                            <th>Штрих-код</th>
                            <th>Ожидалось</th>
                            <th>Отсканировано</th>
                            <th>Недостача</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($missing as $item): ?>
                        <tr>
                            <td><?php echo htmlspecialchars($item['name']); ?></td>
                            <td><span class="badge badge-info"><?php echo htmlspecialchars($item['barcode']); ?></span></td>
                            <td><?php echo $item['expected']; ?></td>
                            <td><?php echo $item['scanned']; ?></td>
                            <td><span class="badge badge-danger">-<?php echo $item['difference']; ?></span></td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
                <?php endif; ?>

                <?php if (!empty($excess)): ?>
                <h3 style="margin: 20px 0 10px; color: var(--danger);">Излишки</h3>
                <table class="products-table">
                    <thead>
                        <tr>
                            <th>Товар</th>
                            <th>Штрих-код</th>
                            <th>Ожидалось</th>
                            <th>Отсканировано</th>
                            <th>Излишек</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($excess as $item): ?>
                        <tr>
                            <td><?php echo htmlspecialchars($item['name']); ?></td>
                            <td><span class="badge badge-info"><?php echo htmlspecialchars($item['barcode']); ?></span></td>
                            <td><?php echo $item['expected']; ?></td>
                            <td><?php echo $item['scanned']; ?></td>
                            <td><span class="badge badge-success">+<?php echo $item['difference']; ?></span></td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
                <?php endif; ?>
            </div>
            <?php endif; ?>

        <?php elseif ($warehouse_id && !$warehouseInfo): ?>
            <div class="alert alert-danger">Склад не найден</div>
        <?php else: ?>
            <div class="empty-state">
                <i class="fas fa-hand-pointer"></i>
                <h3>Выберите склад для начала инвентаризации</h3>
            </div>
        <?php endif; ?>
    </div>

    <script>
        // Автофокус на поле ввода штрих-кода
        document.addEventListener('DOMContentLoaded', function() {
            const barcodeInput = document.getElementById('barcode');
            if (barcodeInput) {
                barcodeInput.focus();
            }
        });
    </script>
</body>
</html>