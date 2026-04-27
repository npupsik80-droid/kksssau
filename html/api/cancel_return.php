<?php
// api/cancel_return.php
require_once '../includes/auth_check.php';

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    die('Method not allowed');
}

if (!isset($_POST['action']) || $_POST['action'] !== 'cancel_return') {
    die('Invalid action');
}

$checkId = intval($_POST['check_id'] ?? 0);
$status = $_POST['status'] ?? '';
$reason = $_POST['reason'] ?? '';
$comment = $_POST['comment'] ?? '';
$kkmResult = json_decode($_POST['kkm_result'] ?? '{}', true);

if ($checkId <= 0) {
    die('Invalid check ID');
}

try {
    // Обновляем статус чека возврата
    $stmt = $pdo->prepare("
        UPDATE checks 
        SET status = ?, 
            updated_at = NOW(),
            fiscal_data = JSON_SET(
                COALESCE(fiscal_data, '{}'),
                '$.cancel_reason', ?,
                '$.cancel_comment', ?,
                '$.cancel_date', NOW(),
                '$.cancel_result', ?
            )
        WHERE id = ? AND type = 'return'
    ");
    
    $stmt->execute([
        $status,
        $reason,
        $comment,
        json_encode($kkmResult),
        $checkId
    ]);
    
    echo 'OK';
    
} catch (PDOException $e) {
    die('Database error: ' . $e->getMessage());
}
?>