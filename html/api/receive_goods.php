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
if (empty($data['warehouse_id']) || empty($data['products'])) {
    echo json_encode(['success' => false, 'error' => 'Недостаточно данных']);
    exit();
}

try {
    $pdo->beginTransaction();
    
    foreach ($data['products'] as $product) {
        // Проверяем, есть ли уже товар на складе
        $stmt = $pdo->prepare("
            SELECT id, quantity FROM warehouse_items 
            WHERE warehouse_id = ? AND nomenclature_id = ?
        ");
        $stmt->execute([$data['warehouse_id'], $product['nomenclature_id']]);
        $existing_item = $stmt->fetch();
        
        if ($existing_item) {
            // Обновляем количество и цену
            $stmt = $pdo->prepare("
                UPDATE warehouse_items 
                SET quantity = quantity + ?, 
                    price = ?
                WHERE id = ?
            ");
            $stmt->execute([
                $product['quantity'],
                $product['price'],
                $existing_item['id']
            ]);
        } else {
            // Создаем новую запись
            $stmt = $pdo->prepare("
                INSERT INTO warehouse_items 
                (warehouse_id, nomenclature_id, quantity, price)
                VALUES (?, ?, ?, ?)
            ");
            $stmt->execute([
                $data['warehouse_id'],
                $product['nomenclature_id'],
                $product['quantity'],
                $product['price']
            ]);
        }
    }
    
    // Логируем действие
    $stmt = $pdo->prepare("
        INSERT INTO operation_log 
        (user_id, action, details, ip_address)
        VALUES (?, ?, ?, ?)
    ");
    
    $products_count = count($data['products']);
    $stmt->execute([
        $_SESSION['user_id'],
        'goods_received',
        "Оприходовано {$products_count} товаров",
        $_SERVER['REMOTE_ADDR']
    ]);
    
    $pdo->commit();
    
    echo json_encode([
        'success' => true,
        'message' => 'Товары успешно оприходованы'
    ]);
    
} catch (Exception $e) {
    $pdo->rollBack();
    http_response_code(500);
    echo json_encode([
        'success' => false,
        'error' => 'Ошибка оприходования: ' . $e->getMessage()
    ]);
}
?>