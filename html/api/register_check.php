<?php
require_once '../config/database.php';
require_once '../includes/auth_check.php';

header('Content-Type: application/json');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['error' => 'Method not allowed']);
    exit;
}

$data = json_decode(file_get_contents('php://input'), true);

try {
    // Начинаем транзакцию
    $pdo->beginTransaction();
    
    // 1. Сохраняем чек в БД
    $stmt = $pdo->prepare("
        INSERT INTO checks 
        (shift_id, user_id, division_id, type, total_amount, cash_amount, card_amount, items)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?)
    ");
    
    $stmt->execute([
        $data['shift_id'],
        $_SESSION['user_id'],
        $_SESSION['current_division_id'],
        $data['type'],
        $data['total_amount'],
        $data['cash_amount'],
        $data['card_amount'],
        json_encode($data['items'], JSON_UNESCAPED_UNICODE)
    ]);
    
    $check_id = $pdo->lastInsertId();
    
    // 2. Обновляем остатки на складе
    foreach ($data['items'] as $item) {
        $stmt = $pdo->prepare("
            UPDATE warehouse_items 
            SET quantity = quantity - ?
            WHERE id = ? AND warehouse_id = ?
        ");
        $stmt->execute([
            $item['quantity'],
            $item['warehouse_item_id'],
            $_SESSION['current_warehouse_id']
        ]);
    }
    
    // 3. Обновляем статистику смены
    $stmt = $pdo->prepare("
        UPDATE shifts 
        SET 
            total_cash = total_cash + ?,
            total_card = total_card + ?,
            total_checks = total_checks + 1
        WHERE id = ?
    ");
    $stmt->execute([
        $data['cash_amount'],
        $data['card_amount'],
        $data['shift_id']
    ]);
    
    // 4. Логируем операцию
    $stmt = $pdo->prepare("
        INSERT INTO operation_log 
        (user_id, action, details, ip_address)
        VALUES (?, ?, ?, ?)
    ");
    $stmt->execute([
        $_SESSION['user_id'],
        'check_printed',
        "Чек #{$check_id} на сумму {$data['total_amount']}",
        $_SERVER['REMOTE_ADDR']
    ]);
    
    $pdo->commit();
    
    echo json_encode([
        'success' => true,
        'check_id' => $check_id,
        'message' => 'Чек успешно сохранен'
    ]);
    
} catch (Exception $e) {
    $pdo->rollBack();
    http_response_code(500);
    echo json_encode([
        'error' => 'Ошибка сохранения чека',
        'details' => $e->getMessage()
    ]);
}
?>