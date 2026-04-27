<?php
require_once '../../includes/auth_check.php';

// Только для администраторов
if ($_SESSION['permission_group'] !== 'admin') {
    header('Location: ../../index.php');
    exit();
}

// Обработка действий
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    if (isset($_POST['action'])) {
        switch ($_POST['action']) {
            case 'create':
                createDivision();
                break;
            case 'update':
                updateDivision();
                break;
            case 'delete':
                deleteDivision();
                break;
        }
    }
}

// Получаем все подразделения
$stmt = $pdo->prepare("
    SELECT 
        d.*,
        COUNT(DISTINCT w.id) as warehouse_count,
        COUNT(DISTINCT u.id) as user_count,
        COUNT(DISTINCT s.id) as shift_count
    FROM divisions d
    LEFT JOIN warehouses w ON d.id = w.division_id
    LEFT JOIN users u ON u.id IN (
        SELECT user_id FROM shifts WHERE division_id = d.id GROUP BY user_id
    )
    LEFT JOIN shifts s ON d.id = s.division_id
    GROUP BY d.id
    ORDER BY d.created_at DESC
");
$stmt->execute();
$divisions = $stmt->fetchAll();

// Статистика
$total_warehouses = array_sum(array_column($divisions, 'warehouse_count'));
$total_users = array_sum(array_column($divisions, 'user_count'));
$total_shifts = array_sum(array_column($divisions, 'shift_count'));

// Функции обработки
function createDivision() {
    global $pdo;
    
    $name = trim($_POST['name']);
    $address = trim($_POST['address'] ?? '');
    
    if (empty($name)) {
        $_SESSION['error'] = 'Введите название подразделения';
        return;
    }
    
    try {
        $pdo->beginTransaction();
        
        // Создаем подразделение
        $stmt = $pdo->prepare("INSERT INTO divisions (name, address) VALUES (?, ?)");
        $stmt->execute([$name, $address]);
        $division_id = $pdo->lastInsertId();
        
        // Автоматически создаем склад
        $stmt = $pdo->prepare("INSERT INTO warehouses (division_id, name) VALUES (?, ?)");
        $warehouse_name = "Основной склад " . $name;
        $stmt->execute([$division_id, $warehouse_name]);
        
        // Логируем
        $stmt = $pdo->prepare("
            INSERT INTO operation_log (user_id, action, details, ip_address)
            VALUES (?, ?, ?, ?)
        ");
        $stmt->execute([
            $_SESSION['user_id'],
            'division_created',
            "Создано подразделение: {$name}",
            $_SERVER['REMOTE_ADDR']
        ]);
        
        $pdo->commit();
        $_SESSION['success'] = 'Подразделение создано успешно!';
        
    } catch (Exception $e) {
        $pdo->rollBack();
        $_SESSION['error'] = 'Ошибка: ' . $e->getMessage();
    }
}

function updateDivision() {
    global $pdo;
    
    $id = intval($_POST['id']);
    $name = trim($_POST['name']);
    $address = trim($_POST['address'] ?? '');
    
    try {
        $stmt = $pdo->prepare("UPDATE divisions SET name = ?, address = ? WHERE id = ?");
        $stmt->execute([$name, $address, $id]);
        
        $_SESSION['success'] = 'Подразделение обновлено!';
        
    } catch (Exception $e) {
        $_SESSION['error'] = 'Ошибка: ' . $e->getMessage();
    }
}

function deleteDivision() {
    global $pdo;
    
    $id = intval($_POST['id']);
    
    // Проверяем, нет ли связанных данных
    $stmt = $pdo->prepare("SELECT COUNT(*) as count FROM shifts WHERE division_id = ?");
    $stmt->execute([$id]);
    $shifts = $stmt->fetch();
    
    if ($shifts['count'] > 0) {
        $_SESSION['error'] = 'Нельзя удалить подразделение с историей смен!';
        return;
    }
    
    try {
        $pdo->beginTransaction();
        
        // Удаляем склады
        $stmt = $pdo->prepare("DELETE FROM warehouses WHERE division_id = ?");
        $stmt->execute([$id]);
        
        // Удаляем подразделение
        $stmt = $pdo->prepare("DELETE FROM divisions WHERE id = ?");
        $stmt->execute([$id]);
        
        // Логируем
        $stmt = $pdo->prepare("
            INSERT INTO operation_log (user_id, action, details, ip_address)
            VALUES (?, ?, ?, ?)
        ");
        $stmt->execute([
            $_SESSION['user_id'],
            'division_deleted',
            "Удалено подразделение ID: {$id}",
            $_SERVER['REMOTE_ADDR']
        ]);
        
        $pdo->commit();
        $_SESSION['success'] = 'Подразделение удалено!';
        
    } catch (Exception $e) {
        $pdo->rollBack();
        $_SESSION['error'] = 'Ошибка: ' . $e->getMessage();
    }
}
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Управление подразделениями - RunaRMK</title>
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
        
        .user-panel {
            display: flex;
            align-items: center;
            gap: 25px;
        }
        
        .user-panel span {
            color: white;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 10px;
            font-size: 15px;
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
        
        .stat-card:nth-child(4)::before {
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
        
        .stat-icon.danger {
            background: linear-gradient(135deg, #fff5f5 0%, #ffe5e5 100%);
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
        
        .controls-bar {
            background: white;
            border-radius: var(--border-radius);
            padding: 25px 30px;
            margin-bottom: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 20px;
        }
        
        .controls-bar h2 {
            color: var(--dark);
            font-size: 24px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .divisions-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(550px, 1fr));
            gap: 25px;
        }
        
        .division-card {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid transparent;
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            position: relative;
            overflow: hidden;
        }
        
        .division-card:hover {
            transform: translateY(-8px);
            box-shadow: var(--shadow-lg);
            border-color: var(--primary);
        }
        
        .division-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--primary), var(--info));
        }
        
        .division-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 25px;
        }
        
        .division-title {
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .division-icon {
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
        
        .division-info h3 {
            color: var(--dark);
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 5px;
        }
        
        .division-id {
            color: #64748b;
            font-size: 14px;
            font-weight: 600;
        }
        
        .division-meta {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 20px;
            margin-bottom: 25px;
        }
        
        .meta-item {
            text-align: center;
            padding: 15px;
            background: white;
            border-radius: 12px;
            box-shadow: var(--shadow-sm);
            border: 1px solid rgba(0,0,0,0.05);
        }
        
        .meta-value {
            font-size: 24px;
            font-weight: 800;
            color: var(--primary);
            margin: 8px 0;
        }
        
        .meta-label {
            color: #64748b;
            font-size: 13px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .division-details {
            background: white;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 25px;
            border: 1px solid rgba(0,0,0,0.05);
        }
        
        .detail-item {
            display: flex;
            align-items: center;
            gap: 12px;
            margin-bottom: 12px;
        }
        
        .detail-item:last-child {
            margin-bottom: 0;
        }
        
        .detail-item i {
            color: var(--primary);
            width: 24px;
            text-align: center;
        }
        
        .division-actions {
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
            
            .divisions-grid {
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
            
            .divisions-grid {
                grid-template-columns: 1fr;
            }
            
            .stats-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .division-meta {
                grid-template-columns: repeat(2, 1fr);
            }
        }
        
        @media (max-width: 768px) {
            .division-meta {
                grid-template-columns: 1fr;
            }
            
            .controls-bar {
                flex-direction: column;
                align-items: stretch;
            }
            
            .controls-bar h2 {
                text-align: center;
                justify-content: center;
            }
            
            .division-actions {
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
            
            .division-header {
                flex-direction: column;
                align-items: stretch;
                gap: 15px;
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
            <i class="fas fa-building"></i>
            Управление подразделениями
        </h1>
        <div class="user-panel">
            <a href="../../index.php" class="btn btn-primary">
                <i class="fas fa-arrow-left"></i> На главную
            </a>
        </div>
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
                    <i class="fas fa-building"></i>
                </div>
                <h3>Всего подразделений</h3>
                <div class="stat-value"><?php echo count($divisions); ?></div>
                <div style="color: #64748b; font-size: 14px;">Создано в системе</div>
            </div>
            
            <div class="stat-card fade-in-up" style="animation-delay: 0.2s;">
                <div class="stat-icon secondary">
                    <i class="fas fa-warehouse"></i>
                </div>
                <h3>Всего складов</h3>
                <div class="stat-value"><?php echo $total_warehouses; ?></div>
                <div style="color: #64748b; font-size: 14px;">В подразделениях</div>
            </div>
            
            <div class="stat-card fade-in-up" style="animation-delay: 0.3s;">
                <div class="stat-icon warning">
                    <i class="fas fa-users"></i>
                </div>
                <h3>Активных пользователей</h3>
                <div class="stat-value"><?php echo $total_users; ?></div>
                <div style="color: #64748b; font-size: 14px;">Работают в системе</div>
            </div>
            
            <div class="stat-card fade-in-up" style="animation-delay: 0.4s;">
                <div class="stat-icon danger">
                    <i class="fas fa-clock"></i>
                </div>
                <h3>Всего смен</h3>
                <div class="stat-value"><?php echo $total_shifts; ?></div>
                <div style="color: #64748b; font-size: 14px;">Проведено за всё время</div>
            </div>
        </div>
        
        <!-- Панель управления -->
        <div class="controls-bar fade-in-up" style="animation-delay: 0.5s;">
            <h2><i class="fas fa-list"></i> Список подразделений</h2>
            <button class="btn btn-secondary" onclick="openCreateModal()" id="createBtn">
                <i class="fas fa-plus"></i> Создать подразделение
            </button>
        </div>
        
        <!-- Сетка подразделений -->
        <div class="divisions-grid fade-in-up" style="animation-delay: 0.6s;">
            <?php if (count($divisions) > 0): ?>
                <?php foreach ($divisions as $division): ?>
                <div class="division-card" data-id="<?php echo $division['id']; ?>">
                    <div class="division-header">
                        <div class="division-title">
                            <div class="division-icon">
                                <i class="fas fa-store-alt"></i>
                            </div>
                            <div class="division-info">
                                <h3><?php echo htmlspecialchars($division['name']); ?></h3>
                                <div class="division-id">
                                    ID: <?php echo $division['id']; ?>
                                </div>
                            </div>
                        </div>
                        <div style="color: #64748b; font-size: 13px; font-weight: 600;">
                            <?php echo date('d.m.Y', strtotime($division['created_at'])); ?>
                        </div>
                    </div>
                    
                    <div class="division-meta">
                        <div class="meta-item">
                            <div class="meta-value"><?php echo $division['warehouse_count']; ?></div>
                            <div class="meta-label">Складов</div>
                        </div>
                        <div class="meta-item">
                            <div class="meta-value"><?php echo $division['user_count']; ?></div>
                            <div class="meta-label">Пользователей</div>
                        </div>
                        <div class="meta-item">
                            <div class="meta-value"><?php echo $division['shift_count']; ?></div>
                            <div class="meta-label">Смен</div>
                        </div>
                    </div>
                    
                    <?php if (!empty($division['address'])): ?>
                    <div class="division-details">
                        <div class="detail-item">
                            <i class="fas fa-map-marker-alt"></i>
                            <span><?php echo htmlspecialchars($division['address']); ?></span>
                        </div>
                    </div>
                    <?php endif; ?>
                    
                    <div class="division-actions">
                        <button class="btn btn-primary btn-sm" onclick="editDivision(<?php echo $division['id']; ?>, '<?php echo addslashes($division['name']); ?>', '<?php echo addslashes($division['address']); ?>')">
                            <i class="fas fa-edit"></i> Редактировать
                        </button>
                        <button class="btn btn-info btn-sm" onclick="viewDivisionDetails(<?php echo $division['id']; ?>)">
                            <i class="fas fa-info-circle"></i> Подробнее
                        </button>
                        <button class="btn btn-danger btn-sm" onclick="deleteDivision(<?php echo $division['id']; ?>, '<?php echo addslashes($division['name']); ?>')">
                            <i class="fas fa-trash"></i> Удалить
                        </button>
                    </div>
                </div>
                <?php endforeach; ?>
            <?php else: ?>
                <div class="empty-state">
                    <i class="fas fa-building"></i>
                    <h3>Нет подразделений</h3>
                    <p>
                        Подразделения позволяют организовать работу нескольких магазинов или складов в одной системе. 
                        Создайте первое подразделение для начала работы.
                    </p>
                    <button class="btn btn-secondary" onclick="openCreateModal()">
                        <i class="fas fa-plus"></i> Создать подразделение
                    </button>
                </div>
            <?php endif; ?>
        </div>
    </div>
    
    <!-- Модальное окно создания -->
    <div id="createModal" class="modal-overlay">
        <div class="modal-content">
            <div class="modal-header">
                <h2><i class="fas fa-plus-circle"></i> Создать подразделение</h2>
                <button class="close-modal" onclick="closeCreateModal()">&times;</button>
            </div>
            
            <form method="POST" id="createForm">
                <input type="hidden" name="action" value="create">
                
                <div class="form-group">
                    <label><i class="fas fa-signature"></i> Название подразделения *</label>
                    <input type="text" name="name" class="form-control" required 
                           placeholder="Например: Магазин 'Центральный'">
                </div>
                
                <div class="form-group">
                    <label><i class="fas fa-map-marker-alt"></i> Адрес (необязательно)</label>
                    <input type="text" name="address" class="form-control" 
                           placeholder="Адрес подразделения">
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
                <h2><i class="fas fa-edit"></i> Редактировать подразделение</h2>
                <button class="close-modal" onclick="closeEditModal()">&times;</button>
            </div>
            
            <form method="POST" id="editForm">
                <input type="hidden" name="action" value="update">
                <input type="hidden" name="id" id="editId">
                
                <div class="form-group">
                    <label>Название подразделения *</label>
                    <input type="text" name="name" id="editName" class="form-control" required>
                </div>
                
                <div class="form-group">
                    <label>Адрес</label>
                    <input type="text" name="address" id="editAddress" class="form-control">
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
            createBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Создание...';
            createBtn.disabled = true;
        }
        
        function closeCreateModal() {
            document.getElementById('createModal').style.display = 'none';
            document.getElementById('createForm').reset();
            
            const createBtn = document.getElementById('createBtn');
            createBtn.innerHTML = '<i class="fas fa-plus"></i> Создать подразделение';
            createBtn.disabled = false;
        }
        
        function openEditModal() {
            document.getElementById('editModal').style.display = 'flex';
            document.querySelector('#editForm input[name="name"]').focus();
        }
        
        function closeEditModal() {
            document.getElementById('editModal').style.display = 'none';
        }
        
        // Редактирование подразделения
        function editDivision(id, name, address) {
            document.getElementById('editId').value = id;
            document.getElementById('editName').value = name;
            document.getElementById('editAddress').value = address || '';
            openEditModal();
        }
        
        // Просмотр деталей подразделения
        function viewDivisionDetails(id) {
            // В реальном приложении здесь можно сделать AJAX запрос для получения полной информации
            alert('Функция просмотра деталей подразделения. ID: ' + id);
        }
        
        // Удаление подразделения
        function deleteDivision(id, name) {
            if (!confirm(`Удалить подразделение "${name}"?\n\nВнимание: Все связанные склады также будут удалены.\nЭта операция необратима.`)) {
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
        
        // Анимация появления элементов
        document.addEventListener('DOMContentLoaded', function() {
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('fade-in-up');
                    }
                });
            }, { threshold: 0.1 });
            
            document.querySelectorAll('.stat-card, .controls-bar, .division-card').forEach(el => {
                observer.observe(el);
            });
            
            // Обработка отправки формы создания
            document.getElementById('createForm').addEventListener('submit', function() {
                const createBtn = document.getElementById('createBtn');
                createBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Создание...';
                createBtn.disabled = true;
            });
            
            // Обработка отправки формы редактирования
            document.getElementById('editForm').addEventListener('submit', function() {
                const submitBtn = this.querySelector('button[type="submit"]');
                const originalHTML = submitBtn.innerHTML;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Сохранение...';
                submitBtn.disabled = true;
                
                setTimeout(() => {
                    submitBtn.innerHTML = originalHTML;
                    submitBtn.disabled = false;
                }, 3000);
            });
        });
    </script>
</body>
</html>