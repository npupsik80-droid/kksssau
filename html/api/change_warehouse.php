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

if (empty($data['warehouse_id'])) {
    echo json_encode(['success' => false, 'error' => 'Не указан склад']);
    exit();
}

try {
    // Проверяем, существует ли склад
    $stmt = $pdo->prepare("
        SELECT w.*, d.name as division_name 
        FROM warehouses w
        JOIN divisions d ON w.division_id = d.id
        WHERE w.id = ?
    ");
    $stmt->execute([$data['warehouse_id']]);
    $warehouse = $stmt->fetch();
    
    if (!$warehouse) {
        echo json_encode(['success' => false, 'error' => 'Склад не найден']);
        exit();
    }
    
    // Проверяем, принадлежит ли склад текущему подразделению
    if ($warehouse['division_id'] != $_SESSION['current_division_id']) {
        echo json_encode(['success' => false, 'error' => 'Склад не принадлежит текущему подразделению']);
        exit();
    }
    
    // Сохраняем в сессию
    $_SESSION['current_warehouse_id'] = $data['warehouse_id'];
    
    echo json_encode([
        'success' => true,
        'warehouse_name' => $warehouse['name'],
        'message' => 'Склад изменен'
    ]);
    
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode([
        'success' => false,
        'error' => 'Ошибка смены склада: ' . $e->getMessage()
    ]);
}
?>