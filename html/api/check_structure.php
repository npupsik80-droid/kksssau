<?php
// api/check_structure.php
require_once '../config/database.php';

header('Content-Type: application/json');

try {
    // Получаем структуру таблицы checks
    $stmt = $pdo->query("DESCRIBE checks");
    $columns = $stmt->fetchAll(PDO::FETCH_ASSOC);
    
    // Получаем структуру таблицы shifts
    $stmt = $pdo->query("DESCRIBE shifts");
    $shiftsColumns = $stmt->fetchAll(PDO::FETCH_ASSOC);
    
    echo json_encode([
        'success' => true,
        'checks_columns' => $columns,
        'shifts_columns' => $shiftsColumns
    ]);
    
} catch (Exception $e) {
    echo json_encode([
        'success' => false,
        'error' => $e->getMessage()
    ]);
}
?>