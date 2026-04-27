<?php
require_once '../../includes/auth_check.php';

// Проверяем выбрано ли подразделение
if (!isset($_SESSION['current_division_id'])) {
    header('Location: ../../select_division.php');
    exit();
}

// Получаем информацию о текущем подразделении
$currentDivisionId = $_SESSION['current_division_id'];
$currentDivisionName = $_SESSION['current_division_name'];
$currentWarehouseId = $_SESSION['current_warehouse_id'];

// Обработка открытия новой смены
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['action']) && $_POST['action'] === 'open_shift') {
    try {
        // Проверяем, есть ли уже открытая смена в этом подразделении
        $stmt = $pdo->prepare("SELECT id FROM shifts WHERE division_id = ? AND status = 'open'");
        $stmt->execute([$currentDivisionId]);
        
        if ($stmt->fetch()) {
            $_SESSION['error'] = 'В этом подразделении уже есть открытая смена';
            header('Location: ' . $_SERVER['PHP_SELF']);
            exit();
        }
        
        // Сохраняем данные для открытия в KKM
        $_SESSION['pending_kkm_open'] = [
            'division_id' => $currentDivisionId,
            'division_name' => $currentDivisionName
        ];
        
        $_SESSION['info'] = 'Подготовка к открытию смены...';
        
    } catch (PDOException $e) {
        $_SESSION['error'] = 'Ошибка базы данных: ' . $e->getMessage();
    }
    
    header('Location: ' . $_SERVER['PHP_SELF']);
    exit();
}

// Обработка закрытия смены
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['action']) && $_POST['action'] === 'close_shift') {
    $shiftId = intval($_POST['shift_id'] ?? 0);
    
    if ($shiftId <= 0) {
        $_SESSION['error'] = 'Неверные параметры смены';
        header('Location: ' . $_SERVER['PHP_SELF']);
        exit();
    }
    
    try {
        // Получаем данные смены
        $stmt = $pdo->prepare("
            SELECT s.*, u.full_name, d.name as division_name
            FROM shifts s
            LEFT JOIN users u ON s.user_id = u.id
            LEFT JOIN divisions d ON s.division_id = d.id
            WHERE s.id = ? AND s.division_id = ? AND s.status = 'open'
        ");
        $stmt->execute([$shiftId, $currentDivisionId]);
        $shift = $stmt->fetch();
        
        if (!$shift) {
            $_SESSION['error'] = 'Смена не найдена или уже закрыта';
            header('Location: ' . $_SERVER['PHP_SELF']);
            exit();
        }
        
        // Сохраняем данные для закрытия в KKM
        $_SESSION['pending_kkm_close'] = [
            'shift_id' => $shiftId,
            'division_name' => $shift['division_name'],
            'cashier_name' => $shift['full_name'],
            'division_id' => $shift['division_id']
        ];
        
        $_SESSION['info'] = 'Подготовка к закрытию смены в ККТ...';
        
    } catch (PDOException $e) {
        $_SESSION['error'] = 'Ошибка базы данных: ' . $e->getMessage();
    }
    
    header('Location: ' . $_SERVER['PHP_SELF']);
    exit();
}

// Обработка успешного открытия смены в KKM
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['action']) && $_POST['action'] === 'confirm_shift_open') {
    $kkmData = $_POST['kkm_data'] ?? '{}';
    
    try {
        // Парсим данные от KKM
        $kkmDataArray = json_decode($kkmData, true);
        
        // Создаем смену в базе
        $stmt = $pdo->prepare("
            INSERT INTO shifts (user_id, division_id, kkm_shift_number, opened_at, status)
            VALUES (?, ?, ?, NOW(), 'open')
        ");
        
        // Используем номер смены из KKM или 0
        $kkmShiftNumber = $kkmDataArray['sessionNumber'] ?? 0;
        $stmt->execute([$_SESSION['user_id'], $currentDivisionId, $kkmShiftNumber]);
        $shiftId = $pdo->lastInsertId();
        
        // Логируем операцию
        $stmt = $pdo->prepare("
            INSERT INTO operation_log (user_id, action, details, ip_address)
            VALUES (?, ?, ?, ?)
        ");
        $stmt->execute([
            $_SESSION['user_id'],
            'shift_opened_kkm',
            'Смена #' . $shiftId . ' открыта в ККТ. Номер смены в ККТ: ' . $kkmShiftNumber,
            $_SERVER['REMOTE_ADDR']
        ]);
        
        echo json_encode([
            'success' => true, 
            'message' => 'Смена успешно открыта',
            'shift_id' => $shiftId,
            'kkm_shift_number' => $kkmShiftNumber
        ]);
        
    } catch (PDOException $e) {
        echo json_encode(['success' => false, 'error' => 'Ошибка базы данных: ' . $e->getMessage()]);
    }
    exit();
}

// Обработка успешного закрытия смены в KKM
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['action']) && $_POST['action'] === 'confirm_shift_close') {
    $shiftId = intval($_POST['shift_id'] ?? 0);
    $kkmData = $_POST['kkm_data'] ?? '{}';
    
    if ($shiftId <= 0) {
        echo json_encode(['success' => false, 'error' => 'Неверные параметры']);
        exit();
    }
    
    try {
        // Парсим данные от KKM
        $kkmDataArray = json_decode($kkmData, true);
        
        // Обновляем смену как закрытую
        $stmt = $pdo->prepare("
            UPDATE shifts 
            SET closed_at = NOW(), 
                status = 'closed',
                kkm_shift_number = ?
            WHERE id = ? AND division_id = ? AND status = 'open'
        ");
        
        $kkmShiftNumber = $kkmDataArray['sessionNumber'] ?? 0;
        $stmt->execute([$kkmShiftNumber, $shiftId, $currentDivisionId]);
        
        if ($stmt->rowCount() > 0) {
            // Логируем операцию
            $stmt = $pdo->prepare("
                INSERT INTO operation_log (user_id, action, details, ip_address)
                VALUES (?, ?, ?, ?)
            ");
            $stmt->execute([
                $_SESSION['user_id'],
                'shift_closed_kkm',
                'Смена #' . $shiftId . ' закрыта в ККТ. Номер смены в ККТ: ' . $kkmShiftNumber,
                $_SERVER['REMOTE_ADDR']
            ]);
            
            echo json_encode([
                'success' => true, 
                'message' => 'Смена успешно закрыта',
                'shift_id' => $shiftId,
                'kkm_shift_number' => $kkmShiftNumber
            ]);
        } else {
            echo json_encode(['success' => false, 'error' => 'Смена не найдена или уже закрыта']);
        }
        
    } catch (PDOException $e) {
        echo json_encode(['success' => false, 'error' => 'Ошибка базы данных: ' . $e->getMessage()]);
    }
    exit();
}

// Получение текущей открытой смены
$stmt = $pdo->prepare("
    SELECT 
        s.*, 
        u.full_name as cashier_name,
        d.name as division_name
    FROM shifts s
    LEFT JOIN users u ON s.user_id = u.id
    LEFT JOIN divisions d ON s.division_id = d.id
    WHERE s.division_id = ? AND s.status = 'open'
    ORDER BY s.opened_at DESC
    LIMIT 1
");
$stmt->execute([$currentDivisionId]);
$current_shift = $stmt->fetch();

// Получение истории смен для текущего подразделения
function getShifts() {
    global $pdo, $currentDivisionId;
    
    $stmt = $pdo->prepare("
        SELECT 
            s.*, 
            u.full_name as cashier_name,
            d.name as division_name
        FROM shifts s
        LEFT JOIN users u ON s.user_id = u.id
        LEFT JOIN divisions d ON s.division_id = d.id
        WHERE s.division_id = ?
        ORDER BY s.opened_at DESC
        LIMIT 100
    ");
    $stmt->execute([$currentDivisionId]);
    return $stmt->fetchAll(PDO::FETCH_ASSOC);
}

// Получаем список смен
$shifts = getShifts();

// Статистика для текущего подразделения
$openShifts = array_filter($shifts, fn($s) => $s['status'] === 'open');
$closedShifts = array_filter($shifts, fn($s) => $s['status'] === 'closed');
$totalRevenue = array_sum(array_column($shifts, 'total_cash')) + array_sum(array_column($shifts, 'total_card'));
$totalChecks = array_sum(array_column($shifts, 'total_checks'));

// Получаем статистику продаж за сегодня
$todayStart = date('Y-m-d 00:00:00');
$todayEnd = date('Y-m-d 23:59:59');

$stmt = $pdo->prepare("
    SELECT 
        COUNT(*) as today_checks,
        SUM(total_amount) as today_revenue,
        SUM(cash_amount) as today_cash,
        SUM(card_amount) as today_card
    FROM checks 
    WHERE shift_id IN (
        SELECT id FROM shifts 
        WHERE division_id = ? 
        AND DATE(opened_at) = DATE(NOW())
    )
");
$stmt->execute([$currentDivisionId]);
$todayStats = $stmt->fetch();

// Вспомогательная функция для расчета длительности смены
function getShiftDuration($openedAt) {
    $now = new DateTime();
    $opened = new DateTime($openedAt);
    $interval = $now->diff($opened);
    
    $hours = $interval->h;
    $minutes = $interval->i;
    
    if ($interval->days > 0) {
        $hours += $interval->days * 24;
    }
    
    return sprintf("%02d:%02d", $hours, $minutes);
}
?>

<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Управление сменами - RunaRMK</title>
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
            display: flex;
            flex-direction: column;
            gap: 30px;
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
        
        .alert-warning {
            background: linear-gradient(135deg, #fff3cd 0%, #ffeaa7 100%);
            color: #856404;
            border: 2px solid #ffc107;
        }
        
        .alert-info {
            background: linear-gradient(135deg, #d1ecf1 0%, #bee5eb 100%);
            color: #0c5460;
            border: 2px solid #17a2b8;
        }
        
        .stats-grid {
            order: 1;
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 25px;
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
        
        .stat-card.total::before {
            background: linear-gradient(90deg, var(--primary), #3a56d4);
        }
        
        .stat-card.open::before {
            background: linear-gradient(90deg, var(--secondary), #27ae60);
        }
        
        .stat-card.closed::before {
            background: linear-gradient(90deg, var(--warning), #e67e22);
        }
        
        .stat-card.revenue::before {
            background: linear-gradient(90deg, var(--danger), #c0392b);
        }
        
        .stat-card.total-checks::before {
            background: linear-gradient(90deg, var(--info), #2980b9);
        }
        
        .stat-card.today-revenue::before {
            background: linear-gradient(90deg, #e84393, #fd79a8);
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
        
        .stat-icon.total {
            background: linear-gradient(135deg, #f0f7ff 0%, #d4e4ff 100%);
            color: var(--primary);
        }
        
        .stat-icon.open {
            background: linear-gradient(135deg, #f0fff4 0%, #dcffe4 100%);
            color: var(--secondary);
        }
        
        .stat-icon.closed {
            background: linear-gradient(135deg, #fff8e1 0%, #ffeaa7 100%);
            color: var(--warning);
        }
        
        .stat-icon.revenue {
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
        
        .current-shift-section {
            order: 2;
        }
        
        .history-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 25px;
            padding-bottom: 15px;
            border-bottom: 2px solid #e2e8f0;
        }
        
        .history-header h3 {
            color: var(--dark);
            font-size: 24px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        
        .current-shift-card {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-md);
            border: 3px solid var(--secondary);
            position: relative;
            overflow: hidden;
            margin-bottom: 40px;
        }
        
        .current-shift-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--secondary), #27ae60);
        }
        
        .current-shift-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 25px;
        }
        
        .current-shift-title {
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .current-shift-icon {
            width: 70px;
            height: 70px;
            border-radius: 15px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 32px;
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.2) 0%, rgba(46, 204, 113, 0.1) 100%);
            color: var(--secondary);
        }
        
        .current-shift-info h3 {
            color: var(--dark);
            font-size: 24px;
            font-weight: 800;
            margin-bottom: 8px;
            line-height: 1.4;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .current-shift-badge {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 8px 20px;
            border-radius: 30px;
            font-weight: 800;
            font-size: 14px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.2) 0%, rgba(46, 204, 113, 0.1) 100%);
            color: var(--secondary);
            border: 2px solid rgba(46, 204, 113, 0.3);
        }
        
        .shift-badge {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 8px 18px;
            border-radius: 30px;
            font-weight: 700;
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .badge-open {
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.15) 0%, rgba(46, 204, 113, 0.05) 100%);
            color: var(--secondary);
            border: 2px solid rgba(46, 204, 113, 0.3);
        }
        
        .badge-closed {
            background: linear-gradient(135deg, rgba(149, 165, 166, 0.15) 0%, rgba(149, 165, 166, 0.05) 100%);
            color: var(--gray);
            border: 2px solid rgba(149, 165, 166, 0.3);
        }
        
        .badge-kkm {
            background: linear-gradient(135deg, rgba(52, 152, 219, 0.15) 0%, rgba(52, 152, 219, 0.05) 100%);
            color: var(--info);
            border: 2px solid rgba(52, 152, 219, 0.3);
        }
        
        .shift-meta {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            margin-top: 15px;
            color: #64748b;
            font-size: 14px;
        }
        
        .meta-item {
            display: flex;
            align-items: center;
            gap: 6px;
            padding: 6px 12px;
            background: #f8fafc;
            border-radius: 8px;
        }
        
        .shift-totals {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 15px;
            margin: 25px 0;
            padding: 20px;
            background: #f8fafc;
            border-radius: 12px;
            border: 1px solid rgba(0,0,0,0.05);
        }
        
        .total-item {
            text-align: center;
            padding: 15px;
            background: white;
            border-radius: 8px;
            border: 1px solid rgba(0,0,0,0.05);
        }
        
        .total-label {
            color: #64748b;
            font-size: 13px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 8px;
        }
        
        .total-value {
            font-size: 24px;
            font-weight: 800;
            color: var(--primary);
        }
        
        .current-shift-details {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
            margin-top: 20px;
            padding: 20px;
            background: #f8fafc;
            border-radius: 12px;
        }
        
        .detail-item {
            flex: 1;
            min-width: 200px;
            padding: 15px;
            background: white;
            border-radius: 8px;
            border: 1px solid rgba(0,0,0,0.05);
            display: flex;
            flex-direction: column;
            gap: 5px;
        }
        
        .detail-label {
            color: #64748b;
            font-size: 13px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .detail-value {
            font-size: 20px;
            font-weight: 800;
            color: var(--dark);
        }
        
        .current-shift-actions {
            display: flex;
            gap: 15px;
            margin-top: 25px;
            justify-content: center;
        }
        
        .no-current-shift {
            text-align: center;
            padding: 60px 30px;
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            border: 2px dashed #cbd5e1;
            margin-bottom: 40px;
        }
        
        .no-current-shift i {
            font-size: 64px;
            color: #cbd5e1;
            margin-bottom: 25px;
        }
        
        .no-current-shift h3 {
            color: #64748b;
            font-size: 22px;
            font-weight: 700;
            margin-bottom: 15px;
        }
        
        .no-current-shift p {
            color: #94a3b8;
            font-size: 16px;
            margin-bottom: 25px;
            max-width: 500px;
            margin-left: auto;
            margin-right: auto;
            line-height: 1.6;
        }
        
        .division-info {
            display: flex;
            align-items: center;
            gap: 15px;
            padding: 15px;
            background: #f8fafc;
            border-radius: 12px;
            margin-bottom: 20px;
            border: 1px solid rgba(0,0,0,0.05);
        }
        
        .division-info i {
            color: var(--primary);
            font-size: 20px;
        }
        
        .division-info span {
            font-weight: 600;
            color: var(--dark);
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
        
        .readonly-input {
            background: #f8fafc;
            border: 2px solid #e2e8f0;
            color: #64748b;
            cursor: not-allowed;
        }
        
        .form-actions {
            display: flex;
            justify-content: center;
            gap: 15px;
            margin-top: 30px;
            padding-top: 25px;
            border-top: 1px solid #e2e8f0;
        }
        
        .shifts-history {
            order: 3;
        }
        
        .shifts-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(600px, 1fr));
            gap: 25px;
        }
        
        .shift-card {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid transparent;
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            position: relative;
            overflow: hidden;
        }
        
        .shift-card:hover {
            transform: translateY(-8px);
            box-shadow: var(--shadow-lg);
            border-color: var(--primary);
        }
        
        .shift-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
        }
        
        .shift-open::before {
            background: linear-gradient(90deg, var(--secondary), #27ae60);
        }
        
        .shift-closed::before {
            background: linear-gradient(90deg, #95a5a6, #7f8c8d);
        }
        
        .shift-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 25px;
        }
        
        .shift-title {
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .shift-icon {
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
        
        .shift-info h3 {
            color: var(--dark);
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 8px;
            line-height: 1.4;
            display: flex;
            align-items: center;
            gap: 10px;
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
            position: relative;
        }
        
        .modal-content::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--primary), var(--info));
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
        
        .modal-header h3 {
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
        
        .modal-status {
            text-align: center;
            margin: 30px 0;
            padding: 30px;
            background: #f8fafc;
            border-radius: 12px;
        }
        
        .status-spinner {
            font-size: 48px;
            color: var(--primary);
            margin-bottom: 20px;
        }
        
        .status-text {
            color: var(--dark);
            font-size: 18px;
            font-weight: 600;
            margin-bottom: 10px;
        }
        
        .status-details {
            color: #64748b;
            font-size: 14px;
            line-height: 1.5;
        }
        
        .modal-result {
            display: none;
            margin-top: 25px;
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
            
            .shifts-grid {
                grid-template-columns: repeat(auto-fill, minmax(500px, 1fr));
            }
        }
        
        @media (max-width: 992px) {
            .header {
                flex-direction: column;
                gap: 20px;
                text-align: center;
                padding: 25px 20px;
            }
            
            .shifts-grid {
                grid-template-columns: 1fr;
            }
            
            .stats-grid {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .current-shift-header {
                flex-direction: column;
                align-items: stretch;
                gap: 15px;
            }
            
            .shift-totals {
                grid-template-columns: repeat(2, 1fr);
            }
            
            .current-shift-details {
                grid-template-columns: repeat(2, 1fr);
                display: grid;
            }
        }
        
        @media (max-width: 768px) {
            .current-shift-title {
                flex-direction: column;
                align-items: flex-start;
                gap: 10px;
            }
            
            .current-shift-actions {
                flex-wrap: wrap;
            }
            
            .stats-grid {
                grid-template-columns: 1fr;
            }
            
            .shift-totals {
                grid-template-columns: 1fr;
            }
            
            .current-shift-details {
                grid-template-columns: 1fr;
            }
            
            .form-actions {
                flex-direction: column;
            }
        }
        
        @media (max-width: 576px) {
            .container {
                padding: 0 15px 30px;
            }
            
            .stat-value {
                font-size: 32px;
            }
            
            .detail-value {
                font-size: 18px;
            }
            
            .total-value {
                font-size: 20px;
            }
            
            .shift-meta {
                flex-direction: column;
                align-items: flex-start;
                gap: 10px;
            }
        }
    </style>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">
    <script>var KkmServerAddIn = {};</script>
</head>
<body>
    <!-- Шапка -->
    <div class="header">
        <h1>
            <i class="fas fa-clock"></i>
            Управление сменами
        </h1>
        <div class="user-info" style="display: flex; gap: 20px; align-items: center;">
            <span style="color: white; font-weight: 500; display: flex; align-items: center; gap: 8px;">
                <i class="fas fa-store"></i> <?php echo htmlspecialchars($currentDivisionName); ?>
            </span>
            <a href="../../index.php" class="btn btn-primary">
                <i class="fas fa-arrow-left"></i> На главную
            </a>
        </div>
    </div>
    
    <!-- Основной контент -->
    <div class="container">
        <!-- Сообщения -->
        <?php if (isset($_SESSION['info'])): ?>
            <div class="alert alert-info fade-in-up">
                <i class="fas fa-info-circle"></i> <?php echo $_SESSION['info']; ?>
                <?php unset($_SESSION['info']); ?>
            </div>
        <?php endif; ?>
        
        <?php if (isset($_SESSION['success'])): ?>
            <div class="alert alert-success fade-in-up">
                <i class="fas fa-check-circle"></i> <?php echo $_SESSION['success']; ?>
                <?php unset($_SESSION['success']); ?>
            </div>
        <?php endif; ?>
        
        <?php if (isset($_SESSION['error'])): ?>
            <div class="alert alert-danger fade-in-up">
                <i class="fas fa-exclamation-circle"></i> <?php echo $_SESSION['error']; ?>
                <?php unset($_SESSION['error']); ?>
            </div>
        <?php endif; ?>
        
        <!-- Статистика -->
        <div class="stats-grid fade-in-up" style="animation-delay: 0.1s;">
            <div class="stat-card total">
                <div class="stat-icon total">
                    <i class="fas fa-clock"></i>
                </div>
                <h3>Всего смен</h3>
                <div class="stat-value"><?php echo count($shifts); ?></div>
                <div style="color: #64748b; font-size: 14px;">В подразделении</div>
            </div>
            
            <div class="stat-card revenue">
                <div class="stat-icon revenue">
                    <i class="fas fa-money-bill-wave"></i>
                </div>
                <h3>Общая выручка</h3>
                <div class="stat-value"><?php echo number_format($totalRevenue, 0, '.', ' '); ?> ₽</div>
                <div style="color: #64748b; font-size: 14px;">За все время</div>
            </div>
            
            <div class="stat-card total-checks">
                <div class="stat-icon" style="background: linear-gradient(135deg, #f0f7ff 0%, #d4e4ff 100%); color: var(--info);">
                    <i class="fas fa-receipt"></i>
                </div>
                <h3>Всего чеков</h3>
                <div class="stat-value"><?php echo $totalChecks; ?></div>
                <div style="color: #64748b; font-size: 14px;">За все время</div>
            </div>
            
            <div class="stat-card today-revenue">
                <div class="stat-icon" style="background: linear-gradient(135deg, #fff0f5 0%, #ffe5ee 100%); color: #e84393;">
                    <i class="fas fa-calendar-day"></i>
                </div>
                <h3>Выручка за сегодня</h3>
                <div class="stat-value"><?php echo number_format($todayStats['today_revenue'] ?? 0, 0, '.', ' '); ?> ₽</div>
                <div style="color: #64748b; font-size: 14px;"><?php echo date('d.m.Y'); ?></div>
            </div>
        </div>
        
        <!-- Текущая смена -->
        <div class="current-shift-section fade-in-up" style="animation-delay: 0.2s;">
            <div class="history-header">
                <h3><i class="fas fa-cash-register"></i> Текущая смена</h3>
            </div>
            
            <?php if ($current_shift): ?>
                <div class="current-shift-card">
                    <div class="current-shift-header">
                        <div class="current-shift-title">
                            <div class="current-shift-icon">
                                <i class="fas fa-cash-register"></i>
                            </div>
                            <div class="current-shift-info">
                                <h3>
                                    Смена #<?php echo $current_shift['id']; ?>
                                    <span class="current-shift-badge">
                                        <i class="fas fa-lock-open"></i>
                                        ОТКРЫТА
                                    </span>
                                    <?php if ($current_shift['kkm_shift_number']): ?>
                                    <span class="shift-badge badge-kkm">
                                        <i class="fas fa-cash-register"></i>
                                        ККТ: <?php echo $current_shift['kkm_shift_number']; ?>
                                    </span>
                                    <?php endif; ?>
                                </h3>
                                <div class="shift-meta">
                                    <div class="meta-item">
                                        <i class="fas fa-user"></i>
                                        <?php echo htmlspecialchars($current_shift['cashier_name']); ?>
                                    </div>
                                    <div class="meta-item">
                                        <i class="fas fa-store"></i>
                                        <?php echo htmlspecialchars($current_shift['division_name']); ?>
                                    </div>
                                    <div class="meta-item">
                                        <i class="fas fa-calendar"></i>
                                        Открыта: <?php echo date('d.m.Y H:i', strtotime($current_shift['opened_at'])); ?>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <?php if ($current_shift['total_cash'] > 0 || $current_shift['total_card'] > 0): ?>
                    <div class="shift-totals">
                        <div class="total-item">
                            <div class="total-label">Наличные</div>
                            <div class="total-value"><?php echo number_format($current_shift['total_cash'], 2, '.', ' '); ?> ₽</div>
                        </div>
                        <div class="total-item">
                            <div class="total-label">Безналичные</div>
                            <div class="total-value"><?php echo number_format($current_shift['total_card'], 2, '.', ' '); ?> ₽</div>
                        </div>
                        <div class="total-item">
                            <div class="total-label">Общая сумма</div>
                            <div class="total-value"><?php echo number_format($current_shift['total_cash'] + $current_shift['total_card'], 2, '.', ' '); ?> ₽</div>
                        </div>
                        <div class="total-item">
                            <div class="total-label">Чеков</div>
                            <div class="total-value"><?php echo $current_shift['total_checks']; ?></div>
                        </div>
                    </div>
                    <?php else: ?>
                    <div style="text-align: center; padding: 20px; color: #64748b; background: #f8fafc; border-radius: 12px; margin: 25px 0;">
                        <i class="fas fa-receipt" style="font-size: 24px; margin-bottom: 10px; display: block;"></i>
                        <p>В этой смене еще нет чеков</p>
                    </div>
                    <?php endif; ?>
                    
                    <div class="current-shift-details">
                        <div class="detail-item">
                            <span class="detail-label">Сегодня чеков</span>
                            <span class="detail-value"><?php echo $todayStats['today_checks'] ?? 0; ?></span>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">Сегодня наличные</span>
                            <span class="detail-value"><?php echo number_format($todayStats['today_cash'] ?? 0, 2, '.', ' '); ?> ₽</span>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">Сегодня картой</span>
                            <span class="detail-value"><?php echo number_format($todayStats['today_card'] ?? 0, 2, '.', ' '); ?> ₽</span>
                        </div>
                        <div class="detail-item">
                            <span class="detail-label">Сегодня всего</span>
                            <span class="detail-value"><?php echo number_format($todayStats['today_revenue'] ?? 0, 2, '.', ' '); ?> ₽</span>
                        </div>
                    </div>
                    
                    <div class="current-shift-actions">
                        <form method="POST" class="close-shift-form" 
                              data-shift-id="<?php echo $current_shift['id']; ?>" 
                              data-division-id="<?php echo $current_shift['division_id']; ?>"
                              data-division-name="<?php echo htmlspecialchars($current_shift['division_name']); ?>" 
                              data-cashier-name="<?php echo htmlspecialchars($current_shift['cashier_name']); ?>">
                            <input type="hidden" name="action" value="close_shift">
                            <input type="hidden" name="shift_id" value="<?php echo $current_shift['id']; ?>">
                            <button type="button" class="btn btn-danger close-shift-btn" style="padding: 16px 30px; font-size: 16px;">
                                <i class="fas fa-door-closed"></i> Закрыть смену
                            </button>
                        </form>
                        <a href="../history/" class="btn btn-secondary" style="padding: 16px 30px; font-size: 16px;">
                            <i class="fas fa-receipt"></i> Продажи
                        </a>
                        <a href="../checks/new_check.php" class="btn btn-primary" style="padding: 16px 30px; font-size: 16px;">
                            <i class="fas fa-plus-circle"></i> Новый чек
                        </a>
                    </div>
                </div>
            <?php else: ?>
                <!-- Форма открытия новой смены -->
                <div class="no-current-shift">
                    <i class="fas fa-door-closed"></i>
                    <h3>Смена не открыта</h3>
                    <p>Откройте смену, чтобы начать работу с кассой и чеками</p>
                    
                    <div style="max-width: 400px; margin: 0 auto;">
                        <div class="division-info">
                            <i class="fas fa-store"></i>
                            <span>Подразделение: <?php echo htmlspecialchars($currentDivisionName); ?></span>
                        </div>
                        
                        <form method="POST" id="openShiftForm">
                            <input type="hidden" name="action" value="open_shift">
                            
                            <div class="form-group">
                                <label><i class="fas fa-user"></i> Кассир</label>
                                <input type="text" class="form-control readonly-input" 
                                       value="<?php echo htmlspecialchars($_SESSION['full_name'] ?? 'Пользователь'); ?>" 
                                       readonly>
                                <small style="color: #64748b; font-size: 13px; margin-top: 5px; display: block;">
                                    Вы вошли как: <?php echo htmlspecialchars($_SESSION['user_name'] ?? 'Кассир'); ?>
                                </small>
                            </div>
                            
                            <div class="form-actions" style="justify-content: center; border-top: none; margin-top: 20px;">
                                <button type="submit" class="btn btn-secondary" style="padding: 16px 40px; font-size: 16px;">
                                    <i class="fas fa-door-open"></i> Открыть смену
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            <?php endif; ?>
        </div>
        
        <!-- История смен -->
        <div class="shifts-history fade-in-up" style="animation-delay: 0.3s;">
            <div class="history-header">
                <h3><i class="fas fa-history"></i> История смен</h3>
                <span style="color: #64748b; font-size: 14px; font-weight: 600;">
                    <?php echo count($shifts); ?> смен в подразделении
                </span>
            </div>
            
            <div class="shifts-grid">
                <?php if (count($shifts) > 0): ?>
                    <?php foreach ($shifts as $shift): ?>
                    <?php if ($current_shift && $shift['id'] == $current_shift['id']) continue; // Пропускаем текущую смену ?>
                    <div class="shift-card shift-<?php echo $shift['status']; ?>">
                        <div class="shift-header">
                            <div class="shift-title">
                                <div class="shift-icon">
                                    <i class="fas fa-cash-register"></i>
                                </div>
                                <div class="shift-info">
                                    <h3>
                                        Смена #<?php echo $shift['id']; ?>
                                        <span class="shift-badge <?php echo $shift['status'] === 'open' ? 'badge-open' : 'badge-closed'; ?>">
                                            <i class="fas fa-<?php echo $shift['status'] === 'open' ? 'lock-open' : 'lock'; ?>"></i>
                                            <?php echo $shift['status'] === 'open' ? 'Открыта' : 'Закрыта'; ?>
                                        </span>
                                        <?php if ($shift['kkm_shift_number']): ?>
                                        <span class="shift-badge badge-kkm">
                                            <i class="fas fa-cash-register"></i>
                                            ККТ: <?php echo $shift['kkm_shift_number']; ?>
                                        </span>
                                        <?php endif; ?>
                                    </h3>
                                    <div class="shift-meta">
                                        <div class="meta-item">
                                            <i class="fas fa-user"></i>
                                            <?php echo htmlspecialchars($shift['cashier_name']); ?>
                                        </div>
                                        <div class="meta-item">
                                            <i class="fas fa-store"></i>
                                            <?php echo htmlspecialchars($shift['division_name']); ?>
                                        </div>
                                        <div class="meta-item">
                                            <i class="fas fa-calendar"></i>
                                            <?php echo date('d.m.Y H:i', strtotime($shift['opened_at'])); ?>
                                        </div>
                                        <?php if ($shift['closed_at']): ?>
                                        <div class="meta-item">
                                            <i class="fas fa-calendar-times"></i>
                                            <?php echo date('d.m.Y H:i', strtotime($shift['closed_at'])); ?>
                                        </div>
                                        <?php endif; ?>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <?php if ($shift['total_cash'] > 0 || $shift['total_card'] > 0): ?>
                        <div class="shift-totals">
                            <div class="total-item">
                                <div class="total-label">Наличные</div>
                                <div class="total-value"><?php echo number_format($shift['total_cash'], 2, '.', ' '); ?> ₽</div>
                            </div>
                            <div class="total-item">
                                <div class="total-label">Безналичные</div>
                                <div class="total-value"><?php echo number_format($shift['total_card'], 2, '.', ' '); ?> ₽</div>
                            </div>
                            <div class="total-item">
                                <div class="total-label">Общая сумма</div>
                                <div class="total-value"><?php echo number_format($shift['total_cash'] + $shift['total_card'], 2, '.', ' '); ?> ₽</div>
                            </div>
                            <div class="total-item">
                                <div class="total-label">Чеков</div>
                                <div class="total-value"><?php echo $shift['total_checks']; ?></div>
                            </div>
                        </div>
                        <?php else: ?>
                        <div style="text-align: center; padding: 20px; color: #64748b; background: #f8fafc; border-radius: 12px; margin: 25px 0;">
                            <i class="fas fa-receipt" style="font-size: 24px; margin-bottom: 10px; display: block;"></i>
                            <p>В этой смене еще нет чеков</p>
                        </div>
                        <?php endif; ?>
                    </div>
                    <?php endforeach; ?>
                <?php else: ?>
                    <div class="empty-state">
                        <i class="fas fa-history"></i>
                        <h3>История смен пуста</h3>
                        <p>После открытия и закрытия смен они будут отображаться здесь</p>
                    </div>
                <?php endif; ?>
            </div>
        </div>
    </div>
    
    <!-- Модальное окно KKM -->
    <div id="kkmModal" class="modal-overlay">
        <div class="modal-content">
            <div class="modal-header">
                <h3><i class="fas fa-cash-register"></i> <span id="kkmModalTitle">Работа с ККТ</span></h3>
                <button class="close-modal" onclick="closeKkmModal()">&times;</button>
            </div>
            
            <div class="modal-status">
                <div class="status-spinner">
                    <i class="fas fa-spinner fa-spin"></i>
                </div>
                <div class="status-text" id="kkmStatusText">Подготовка...</div>
                <div class="status-details" id="kkmDetails"></div>
            </div>
            
            <div class="modal-result" id="kkmResult">
                <div class="alert alert-success" id="kkmSuccess" style="display: none;">
                    <i class="fas fa-check-circle"></i> <span id="kkmSuccessMessage"></span>
                </div>
                <div class="alert alert-danger" id="kkmError" style="display: none;">
                    <i class="fas fa-times-circle"></i> <span id="kkmErrorMessage"></span>
                </div>
            </div>
            
            <div style="text-align: center; margin-top: 25px;">
                <button id="kkmCloseBtn" class="btn btn-primary" onclick="closeKkmModal()" style="display: none;">
                    <i class="fas fa-times"></i> Закрыть
                </button>
            </div>
        </div>
    </div>
    
    <script>
        // Функция для генерации GUID
        function guid() {
            function S4() {
                return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
            }
            return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
        }
        
        // Проверяем, установлено ли расширение KKM
        function checkKkmExtension() {
            return typeof KkmServer !== 'undefined';
        }
        
        // Функция открытия смены в KKM Server
        function openShiftInKKM(divisionName) {
            return new Promise((resolve, reject) => {
                try {
                    if (!checkKkmExtension()) {
                        reject('Расширение KKM Server не найдено. Установите расширение и обновите страницу.');
                        return;
                    }
                    
                    const command = {
                        Command: "OpenShift",
                        NumDevice: 0,
                        CashierName: "<?php echo $_SESSION['full_name'] ?? 'Кассир'; ?>",
                        IdCommand: guid(),
                        Timeout: 60
                    };
                    
                    console.log('Отправка команды открытия смены:', command);
                    
                    KkmServer.Execute(function(result) {
                        console.log('Ответ от KKM на открытие смены:', result);
                        
                        if (result.Status === 0 || result.Status === "0") {
                            const responseData = {
                                success: true,
                                sessionNumber: result.SessionNumber || 0,
                                checkNumber: result.CheckNumber || 0
                            };
                            resolve(responseData);
                        } else if (result.Status === 1 || result.Status === "1") {
                            setTimeout(() => {
                                checkKkmCommandStatus(command.IdCommand)
                                    .then(resolve)
                                    .catch(reject);
                            }, 2000);
                        } else {
                            const errorMsg = result.Error || result.error || result.Message || result.message || 'Неизвестная ошибка ККТ';
                            reject(errorMsg);
                        }
                    }, command);
                    
                } catch (error) {
                    console.error('Ошибка выполнения команды:', error);
                    reject('Ошибка выполнения команды: ' + error.message);
                }
            });
        }
        
        // Функция закрытия смены в KKM Server
        function closeShiftInKKM(divisionName, cashierName) {
            return new Promise((resolve, reject) => {
                try {
                    if (!checkKkmExtension()) {
                        reject('Расширение KKM Server не найдено. Установите расширение и обновите страницу.');
                        return;
                    }
                    
                    const command = {
                        Command: "CloseShift",
                        NumDevice: 0,
                        CashierName: cashierName || "<?php echo $_SESSION['full_name'] ?? 'Кассир'; ?>",
                        IdCommand: guid(),
                        Timeout: 60
                    };
                    
                    console.log('Отправка команды закрытия смены:', command);
                    
                    KkmServer.Execute(function(result) {
                        console.log('Ответ от KKM на закрытие смены:', result);
                        
                        if (result.Status === 0 || result.Status === "0") {
                            const responseData = {
                                success: true,
                                sessionNumber: result.SessionNumber || 0,
                                checkNumber: result.CheckNumber || 0
                            };
                            resolve(responseData);
                        } else if (result.Status === 1 || result.Status === "1") {
                            setTimeout(() => {
                                checkKkmCommandStatus(command.IdCommand)
                                    .then(resolve)
                                    .catch(reject);
                            }, 2000);
                        } else {
                            const errorMsg = result.Error || result.error || result.Message || result.message || 'Неизвестная ошибка ККТ';
                            reject(errorMsg);
                        }
                    }, command);
                    
                } catch (error) {
                    console.error('Ошибка выполнения команды:', error);
                    reject('Ошибка выполнения команды: ' + error.message);
                }
            });
        }
        
        // Функция проверки статуса команды
        function checkKkmCommandStatus(commandId) {
            return new Promise((resolve, reject) => {
                const checkCommand = {
                    Command: "GetRezult",
                    IdCommand: commandId
                };
                
                KkmServer.Execute(function(result) {
                    console.log('Проверка статуса команды:', result);
                    
                    if (result.Status === 0 || result.Status === "0") {
                        const responseData = {
                            success: true,
                            sessionNumber: result.SessionNumber || 0,
                            checkNumber: result.CheckNumber || 0
                        };
                        resolve(responseData);
                    } else if (result.Status === 1 || result.Status === "1") {
                        setTimeout(() => {
                            checkKkmCommandStatus(commandId)
                                .then(resolve)
                                .catch(reject);
                        }, 1000);
                    } else {
                        const errorMsg = result.Error || result.error || 'Ошибка при выполнении команды';
                        reject(errorMsg);
                    }
                }, checkCommand);
            });
        }
        
        // Модальное окно для открытия смены
        function showKkmModalOpen(divisionId, divisionName) {
            const modal = document.getElementById('kkmModal');
            modal.style.display = 'flex';
            document.getElementById('kkmModalTitle').textContent = 'Открытие смены в ККТ';
            document.getElementById('kkmStatusText').textContent = 'Открытие смены в ККТ...';
            document.getElementById('kkmDetails').textContent = `Подразделение: ${divisionName}`;
            
            // Сброс состояния
            document.querySelector('.status-spinner').innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
            document.querySelector('.status-spinner').style.color = 'var(--primary)';
            document.getElementById('kkmResult').style.display = 'none';
            document.getElementById('kkmSuccess').style.display = 'none';
            document.getElementById('kkmError').style.display = 'none';
            document.getElementById('kkmCloseBtn').style.display = 'none';
            
            openShiftInKKM(divisionName)
                .then(result => {
                    console.log('Успешный ответ от KKM:', result);
                    document.getElementById('kkmStatusText').textContent = 'Смена открыта в ККТ!';
                    document.querySelector('.status-spinner').innerHTML = '<i class="fas fa-check-circle"></i>';
                    document.querySelector('.status-spinner').style.color = '#2ecc71';
                    
                    return confirmShiftOpenOnServer(divisionId, result);
                })
                .then(serverResult => {
                    console.log('Ответ от сервера:', serverResult);
                    
                    if (serverResult.success) {
                        document.getElementById('kkmResult').style.display = 'block';
                        document.getElementById('kkmSuccess').style.display = 'block';
                        document.getElementById('kkmSuccessMessage').textContent = 
                            `Смена #${serverResult.shift_id} успешно открыта!`;
                        
                        document.getElementById('kkmCloseBtn').style.display = 'inline-block';
                        
                        setTimeout(() => {
                            window.location.reload();
                        }, 2000);
                        
                    } else {
                        throw new Error(serverResult.error || 'Ошибка сохранения в базу данных');
                    }
                })
                .catch(error => {
                    console.error('Ошибка при открытии смены:', error);
                    
                    document.getElementById('kkmStatusText').textContent = 'Ошибка открытия смены';
                    document.querySelector('.status-spinner').innerHTML = '<i class="fas fa-times-circle"></i>';
                    document.querySelector('.status-spinner').style.color = '#e74c3c';
                    
                    document.getElementById('kkmResult').style.display = 'block';
                    document.getElementById('kkmError').style.display = 'block';
                    document.getElementById('kkmErrorMessage').textContent = error.toString();
                    
                    document.getElementById('kkmCloseBtn').style.display = 'inline-block';
                });
        }
        
        // Модальное окно для закрытия смены
        function showKkmModalClose(shiftId, divisionId, divisionName, cashierName) {
            const modal = document.getElementById('kkmModal');
            modal.style.display = 'flex';
            document.getElementById('kkmModalTitle').textContent = 'Закрытие смены в ККТ';
            document.getElementById('kkmStatusText').textContent = 'Закрытие смены в ККТ...';
            document.getElementById('kkmDetails').textContent = 
                `Смена #${shiftId}, Подразделение: ${divisionName}`;
            
            // Сброс состояния
            document.querySelector('.status-spinner').innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
            document.querySelector('.status-spinner').style.color = 'var(--primary)';
            document.getElementById('kkmResult').style.display = 'none';
            document.getElementById('kkmSuccess').style.display = 'none';
            document.getElementById('kkmError').style.display = 'none';
            document.getElementById('kkmCloseBtn').style.display = 'none';
            
            closeShiftInKKM(divisionName, cashierName)
                .then(result => {
                    console.log('Успешный ответ от KKM:', result);
                    document.getElementById('kkmStatusText').textContent = 'Смена закрыта в ККТ!';
                    document.querySelector('.status-spinner').innerHTML = '<i class="fas fa-check-circle"></i>';
                    document.querySelector('.status-spinner').style.color = '#2ecc71';
                    
                    return confirmShiftCloseOnServer(shiftId, result);
                })
                .then(serverResult => {
                    console.log('Ответ от сервера:', serverResult);
                    
                    if (serverResult.success) {
                        document.getElementById('kkmResult').style.display = 'block';
                        document.getElementById('kkmSuccess').style.display = 'block';
                        document.getElementById('kkmSuccessMessage').textContent = 
                            `Смена #${shiftId} успешно закрыта!`;
                        
                        document.getElementById('kkmCloseBtn').style.display = 'inline-block';
                        
                        setTimeout(() => {
                            window.location.reload();
                        }, 2000);
                        
                    } else {
                        throw new Error(serverResult.error || 'Ошибка обновления базы данных');
                    }
                })
                .catch(error => {
                    console.error('Ошибка при закрытии смены:', error);
                    
                    document.getElementById('kkmStatusText').textContent = 'Ошибка закрытия смены';
                    document.querySelector('.status-spinner').innerHTML = '<i class="fas fa-times-circle"></i>';
                    document.querySelector('.status-spinner').style.color = '#e74c3c';
                    
                    document.getElementById('kkmResult').style.display = 'block';
                    document.getElementById('kkmError').style.display = 'block';
                    document.getElementById('kkmErrorMessage').textContent = error.toString();
                    
                    document.getElementById('kkmCloseBtn').style.display = 'inline-block';
                });
        }
        
        // Отправка подтверждения открытия на сервер
        function confirmShiftOpenOnServer(divisionId, kkmData) {
            return new Promise((resolve, reject) => {
                const formData = new FormData();
                formData.append('action', 'confirm_shift_open');
                formData.append('division_id', divisionId);
                formData.append('kkm_data', JSON.stringify(kkmData));
                
                fetch(window.location.href, {
                    method: 'POST',
                    body: formData
                })
                .then(response => response.json())
                .then(data => {
                    resolve(data);
                })
                .catch(error => {
                    reject('Ошибка связи с сервером: ' + error.message);
                });
            });
        }
        
        // Отправка подтверждения закрытия на сервер
        function confirmShiftCloseOnServer(shiftId, kkmData) {
            return new Promise((resolve, reject) => {
                const formData = new FormData();
                formData.append('action', 'confirm_shift_close');
                formData.append('shift_id', shiftId);
                formData.append('kkm_data', JSON.stringify(kkmData));
                
                fetch(window.location.href, {
                    method: 'POST',
                    body: formData
                })
                .then(response => response.json())
                .then(data => {
                    resolve(data);
                })
                .catch(error => {
                    reject('Ошибка связи с сервером: ' + error.message);
                });
            });
        }
        
        function closeKkmModal() {
            document.getElementById('kkmModal').style.display = 'none';
            window.location.reload();
        }
        
        // Обработка формы открытия смены
        document.addEventListener('DOMContentLoaded', function() {
            const openForm = document.getElementById('openShiftForm');
            if (openForm) {
                openForm.addEventListener('submit', function(e) {
                    e.preventDefault();
                    
                    if (!confirm(`Открыть смену в подразделении "<?php echo htmlspecialchars($currentDivisionName); ?>"?`)) {
                        return;
                    }
                    
                    this.submit();
                });
            }
            
            // Обработка кнопок закрытия смены
            const closeButtons = document.querySelectorAll('.close-shift-btn');
            
            closeButtons.forEach(btn => {
                btn.addEventListener('click', function() {
                    const form = this.closest('.close-shift-form');
                    const shiftId = form.getAttribute('data-shift-id');
                    const divisionId = form.getAttribute('data-division-id');
                    const divisionName = form.getAttribute('data-division-name');
                    const cashierName = form.getAttribute('data-cashier-name');
                    
                    if (!confirm(`Закрыть смену #${shiftId} в подразделении "${divisionName}"?`)) {
                        return;
                    }
                    
                    form.submit();
                });
            });
            
            // Анимация появления элементов
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('fade-in-up');
                    }
                });
            }, { threshold: 0.1 });
            
            document.querySelectorAll('.alert, .stat-card, .current-shift-section, .shift-card').forEach(el => {
                observer.observe(el);
            });
        });
        
        // Автоматически открываем модальное окно, если есть ожидающая операция
        <?php if (isset($_SESSION['pending_kkm_open'])): ?>
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(() => {
                const pending = <?php echo json_encode($_SESSION['pending_kkm_open']); ?>;
                console.log('Автоматическое открытие модального окна для открытия смены:', pending);
                showKkmModalOpen(pending.division_id, pending.division_name);
                <?php unset($_SESSION['pending_kkm_open']); ?>
            }, 500);
        });
        <?php endif; ?>
        
        <?php if (isset($_SESSION['pending_kkm_close'])): ?>
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(() => {
                const pending = <?php echo json_encode($_SESSION['pending_kkm_close']); ?>;
                console.log('Автоматическое открытие модального окна для закрытия смены:', pending);
                showKkmModalClose(
                    pending.shift_id,
                    pending.division_id,
                    pending.division_name, 
                    pending.cashier_name
                );
                <?php unset($_SESSION['pending_kkm_close']); ?>
            }, 500);
        });
        <?php endif; ?>
    </script>
</body>
</html>