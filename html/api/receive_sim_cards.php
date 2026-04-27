<?php
require_once '../config/database.php';
session_start();

header('Content-Type: application/json');

// Проверяем авторизацию
if (!isset($_SESSION['user_id'])) {
    echo json_encode(['success' => false, 'error' => 'Не авторизован']);
    exit();
}

// Проверяем метод запроса
if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

// Получаем данные
$input = file_get_contents('php://input');
if (empty($input)) {
    echo json_encode(['success' => false, 'error' => 'Нет данных']);
    exit();
}

$data = json_decode($input, true);

// Валидация данных
if (empty($data['warehouse_id']) || empty($data['product_name']) || !isset($data['product_price']) || empty($data['barcodes'])) {
    echo json_encode(['success' => false, 'error' => 'Не все обязательные поля заполнены']);
    exit();
}

$warehouse_id = intval($data['warehouse_id']);
$product_name = trim($data['product_name']);
$product_price = floatval($data['product_price']);
$barcodes = $data['barcodes'];

// Проверяем, что barcodes - это массив
if (!is_array($barcodes)) {
    echo json_encode(['success' => false, 'error' => 'Штрих-коды должны быть массивом']);
    exit();
}

// Проверяем, что массив не пустой
if (count($barcodes) === 0) {
    echo json_encode(['success' => false, 'error' => 'Добавьте хотя бы один штрих-код']);
    exit();
}

try {
    $pdo->beginTransaction();
    
    $created_nomenclatures = [];
    $created_products = []; // Массив для хранения созданных товаров
    $total_quantity = 0;
    
    // Генерируем номер документа как в receive_goods.php (если есть document_number в stock_movements)
    $document_number = 'SIM-' . date('Ymd') . '-' . mt_rand(1000, 9999);
    
    foreach ($barcodes as $barcode) {
        $barcode = trim($barcode);
        
        if (empty($barcode)) {
            continue; // Пропускаем пустые штрих-коды
        }
        
        // Проверяем, существует ли уже номенклатура с таким штрих-кодом (как в create_nomenclature.php)
        $stmt = $pdo->prepare("SELECT id, name FROM nomenclatures WHERE barcode = ?");
        $stmt->execute([$barcode]);
        $existing = $stmt->fetch();
        
        $nomenclature_id = null;
        
        if ($existing) {
            // Используем существующую номенклатуру
            $nomenclature_id = $existing['id'];
            
            // Обновляем название (если изменилась цена или формат)
            $new_name = $product_name . " (" . $product_price . "/" . $product_price . "/0) " . $barcode;
            if ($existing['name'] !== $new_name) {
                $stmt = $pdo->prepare("UPDATE nomenclatures SET name = ? WHERE id = ?");
                $stmt->execute([$new_name, $nomenclature_id]);
            }
        } else {
            // Создаем новую номенклатуру (как в create_nomenclature.php)
            $nomenclature_name = $product_name . " (" . $product_price . "/" . $product_price . "/0) " . $barcode;
            
            $stmt = $pdo->prepare("
                INSERT INTO nomenclatures 
                (name, barcode, description)
                VALUES (?, ?, ?)
            ");
            
            $stmt->execute([
                $nomenclature_name,
                $barcode,
                "SIM-карта, цена: " . $product_price . " ₽"
            ]);
            
            $nomenclature_id = $pdo->lastInsertId();
            
            // Логируем создание номенклатуры (как в create_nomenclature.php)
            $stmt = $pdo->prepare("
                INSERT INTO operation_log 
                (user_id, action, details, ip_address)
                VALUES (?, ?, ?, ?)
            ");
            
            $stmt->execute([
                $_SESSION['user_id'],
                'nomenclature_created',
                "Создана номенклатура: " . $nomenclature_name,
                $_SERVER['REMOTE_ADDR']
            ]);
        }
        
        // Проверяем, есть ли уже товар на складе (как в receive_goods.php)
        $stmt = $pdo->prepare("
            SELECT id, quantity FROM warehouse_items 
            WHERE warehouse_id = ? AND nomenclature_id = ?
        ");
        $stmt->execute([$warehouse_id, $nomenclature_id]);
        $existing_item = $stmt->fetch();
        
        $warehouse_item_id = null;
        
        if ($existing_item) {
            // Обновляем количество и цену (как в receive_goods.php)
            $stmt = $pdo->prepare("
                UPDATE warehouse_items 
                SET quantity = quantity + 1, 
                    price = ?
                WHERE id = ?
            ");
            $stmt->execute([
                $product_price,
                $existing_item['id']
            ]);
            $warehouse_item_id = $existing_item['id'];
        } else {
            // Создаем новую запись (как в receive_goods.php)
            $stmt = $pdo->prepare("
                INSERT INTO warehouse_items 
                (warehouse_id, nomenclature_id, quantity, price)
                VALUES (?, ?, 1, ?)
            ");
            $stmt->execute([
                $warehouse_id,
                $nomenclature_id,
                $product_price
            ]);
            $warehouse_item_id = $pdo->lastInsertId();
        }
        
        // Если таблица stock_movements существует, создаем запись (проверяем по примеру receive_goods.php)
        try {
            $stmt = $pdo->prepare("
                INSERT INTO stock_movements 
                (warehouse_id, nomenclature_id, quantity, price, movement_type, document_type, document_number) 
                VALUES (?, ?, 1, ?, 'incoming', 'receipt', ?)
            ");
            $stmt->execute([
                $warehouse_id,
                $nomenclature_id,
                $product_price,
                $document_number
            ]);
        } catch (Exception $e) {
            // Если таблицы нет - игнорируем ошибку
            // Можно залогировать, но не прерывать выполнение
            error_log("stock_movements error: " . $e->getMessage());
        }
        
        // Теперь получаем полные данные созданного товара для возврата фронтенду
        $stmt = $pdo->prepare("
            SELECT 
                wi.id as warehouse_item_id,
                wi.quantity,
                wi.price,
                n.name,
                n.barcode,
                w.name as warehouse_name
            FROM warehouse_items wi
            JOIN nomenclatures n ON wi.nomenclature_id = n.id
            JOIN warehouses w ON wi.warehouse_id = w.id
            WHERE wi.id = ?
        ");
        $stmt->execute([$warehouse_item_id]);
        $created_product = $stmt->fetch(PDO::FETCH_ASSOC);
        
        if ($created_product) {
            $created_products[] = $created_product;
        }
        
        $total_quantity++;
        $created_nomenclatures[] = [
            'id' => $nomenclature_id,
            'barcode' => $barcode
        ];
    }
    
    // Логируем действие оприходования (как в receive_goods.php)
    $stmt = $pdo->prepare("
        INSERT INTO operation_log 
        (user_id, action, details, ip_address)
        VALUES (?, ?, ?, ?)
    ");
    
    $stmt->execute([
        $_SESSION['user_id'],
        'sim_cards_received',
        "Оприходовано {$total_quantity} SIM-карт на общую сумму " . ($product_price * $total_quantity) . " ₽",
        $_SERVER['REMOTE_ADDR']
    ]);
    
    $pdo->commit();
    
    // ВОТ ГЛАВНОЕ ИЗМЕНЕНИЕ: возвращаем созданные товары в ответе
    echo json_encode([
        'success' => true,
        'message' => 'Оприходовано ' . $total_quantity . ' SIM-карт',
        'created_count' => count($created_nomenclatures),
        'total_quantity' => $total_quantity,
        'total_amount' => $product_price * $total_quantity,
        'products' => $created_products // Возвращаем массив созданных товаров
    ]);
    
} catch (Exception $e) {
    if (isset($pdo) && $pdo->inTransaction()) {
        $pdo->rollBack();
    }
    
    echo json_encode([
        'success' => false,
        'error' => 'Ошибка оприходования SIM-карт: ' . $e->getMessage()
    ]);
}