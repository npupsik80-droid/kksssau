<?php
require_once '../config/database.php';
session_start();

// Проверяем авторизацию и права
if (!isset($_SESSION['user_id']) || $_SESSION['permission_group'] !== 'admin') {
    http_response_code(403);
    echo json_encode(['error' => 'Доступ запрещен']);
    exit();
}

header('Content-Type: application/json');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['error' => 'Method not allowed']);
    exit();
}

$data = json_decode(file_get_contents('php://input'), true);

if (empty($data['name'])) {
    echo json_encode(['success' => false, 'error' => 'Название подразделения обязательно']);
    exit();
}

try {
    $pdo->beginTransaction();
    
    // 1. Создаем подразделение
    $stmt = $pdo->prepare("
        INSERT INTO divisions (name, address) 
        VALUES (?, ?)
    ");
    $stmt->execute([
        trim($data['name']),
        !empty($data['address']) ? trim($data['address']) : null
    ]);
    
    $division_id = $pdo->lastInsertId();
    
    // 2. Автоматически создаем склад для этого подразделения
    $stmt = $pdo->prepare("
        INSERT INTO warehouses (division_id, name) 
        VALUES (?, ?)
    ");
    $warehouse_name = "Склад " . trim($data['name']);
    $stmt->execute([$division_id, $warehouse_name]);
    
    // 3. Логируем действие
    $stmt = $pdo->prepare("
        INSERT INTO operation_log 
        (user_id, action, details, ip_address)
        VALUES (?, ?, ?, ?)
    ");
    $stmt->execute([
        $_SESSION['user_id'],
        'division_created',
        "Создано подразделение: " . trim($data['name']),
        $_SERVER['REMOTE_ADDR']
    ]);
    
    $pdo->commit();
    
    echo json_encode([
        'success' => true,
        'division_id' => $division_id,
        'message' => 'Подразделение и склад успешно созданы'
    ]);
    
} catch (Exception $e) {
    $pdo->rollBack();
    http_response_code(500);
    echo json_encode([
        'success' => false,
        'error' => 'Ошибка создания подразделения: ' . $e->getMessage()
    ]);
}
?>