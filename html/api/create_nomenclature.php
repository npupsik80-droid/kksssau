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

// Проверяем обязательные поля
if (empty($data['barcode']) || empty($data['name'])) {
    echo json_encode(['success' => false, 'error' => 'Не заполнены обязательные поля']);
    exit();
}

try {
    // Проверяем, нет ли уже номенклатуры с таким штрих-кодом
    $stmt = $pdo->prepare("SELECT id FROM nomenclatures WHERE barcode = ?");
    $stmt->execute([$data['barcode']]);
    
    if ($stmt->fetch()) {
        echo json_encode(['success' => false, 'error' => 'Номенклатура с таким штрих-кодом уже существует']);
        exit();
    }
    
    // Создаем номенклатуру
    $stmt = $pdo->prepare("
        INSERT INTO nomenclatures 
        (name, barcode, description)
        VALUES (?, ?, ?)
    ");
    
    $stmt->execute([
        trim($data['name']),
        trim($data['barcode']),
        !empty($data['description']) ? trim($data['description']) : null
    ]);
    
    $nomenclature_id = $pdo->lastInsertId();
    
    // Логируем действие
    $stmt = $pdo->prepare("
        INSERT INTO operation_log 
        (user_id, action, details, ip_address)
        VALUES (?, ?, ?, ?)
    ");
    
    $stmt->execute([
        $_SESSION['user_id'],
        'nomenclature_created',
        "Создана номенклатура: " . trim($data['name']),
        $_SERVER['REMOTE_ADDR']
    ]);
    
    echo json_encode([
        'success' => true,
        'nomenclature_id' => $nomenclature_id,
        'message' => 'Номенклатура успешно создана'
    ]);
    
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode([
        'success' => false,
        'error' => 'Ошибка создания номенклатуры: ' . $e->getMessage()
    ]);
}
?>