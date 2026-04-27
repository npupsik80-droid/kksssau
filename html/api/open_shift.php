<?php
require_once '../config/database.php';
session_start();

header('Content-Type: application/json');

if (!isset($_SESSION['user_id'])) {
    http_response_code(401);
    echo json_encode(['error' => 'Не авторизован']);
    exit();
}

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['error' => 'Method not allowed']);
    exit();
}

$data = json_decode(file_get_contents('php://input'), true);

try {
    // 1. Проверяем, нет ли уже открытой смены
    $stmt = $pdo->prepare("
        SELECT id FROM shifts 
        WHERE division_id = ? 
        AND status = 'open'
    ");
    $stmt->execute([$_SESSION['current_division_id']]);
    
    if ($stmt->fetch()) {
        echo json_encode(['success' => false, 'error' => 'Смена уже открыта']);
        exit();
    }
    
    // 2. Получаем номер смены из ККТ (если подключено)
    $kkm_session_number = 0;
    
    // 3. Создаем смену в базе
    $stmt = $pdo->prepare("
        INSERT INTO shifts 
        (user_id, division_id, kkm_shift_number, status, opened_at)
        VALUES (?, ?, ?, 'open', NOW())
    ");
    
    $stmt->execute([
        $_SESSION['user_id'],
        $_SESSION['current_division_id'],
        $kkm_session_number
    ]);
    
    $shift_id = $pdo->lastInsertId();
    
    // 4. Логируем действие
    $stmt = $pdo->prepare("
        INSERT INTO operation_log 
        (user_id, action, details, ip_address)
        VALUES (?, ?, ?, ?)
    ");
    $stmt->execute([
        $_SESSION['user_id'],
        'shift_opened',
        "Открыта смена #{$shift_id}",
        $_SERVER['REMOTE_ADDR']
    ]);
    
    echo json_encode([
        'success' => true,
        'shift_id' => $shift_id,
        'session_number' => $kkm_session_number,
        'message' => 'Смена успешно открыта'
    ]);
    
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode([
        'success' => false,
        'error' => 'Ошибка открытия смены: ' . $e->getMessage()
    ]);
}
?>