<?php
require_once '../../includes/auth_check.php';

// Получаем список чеков для возврата
$stmt = $pdo->prepare("
    SELECT c.*, u.full_name as cashier_name, d.name as division_name
    FROM checks c
    LEFT JOIN users u ON c.user_id = u.id
    LEFT JOIN divisions d ON c.division_id = d.id
    WHERE c.type = 'sale'
    ORDER BY c.created_at DESC
    LIMIT 50
");
$stmt->execute();
$checks = $stmt->fetchAll();

// Получаем список уже созданных возвратов
$stmt_returns = $pdo->prepare("
    SELECT c.*, u.full_name as cashier_name, d.name as division_name
    FROM checks c
    LEFT JOIN users u ON c.user_id = u.id
    LEFT JOIN divisions d ON c.division_id = d.id
    WHERE c.type = 'return'
    ORDER BY c.created_at DESC
    LIMIT 50
");
$stmt_returns->execute();
$return_checks = $stmt_returns->fetchAll();

// Обработка возврата чека
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['action']) && $_POST['action'] === 'return_check') {
    returnCheck();
}

function returnCheck() {
    global $pdo;
    
    $checkId = intval($_POST['check_id'] ?? 0);
    $reason = trim($_POST['reason'] ?? '');
    
    if ($checkId <= 0) {
        $_SESSION['error'] = 'Неверный чек';
        return;
    }
    
    // Получаем данные оригинального чека
    $stmt = $pdo->prepare("SELECT * FROM checks WHERE id = ?");
    $stmt->execute([$checkId]);
    $originalCheck = $stmt->fetch();
    
    if (!$originalCheck) {
        $_SESSION['error'] = 'Чек не найден';
        return;
    }
    
    try {
        // Создаем запись о возврате
        $stmt = $pdo->prepare("
            INSERT INTO checks (
                shift_id, user_id, division_id, type, 
                kkm_check_number, total_amount, cash_amount, 
                card_amount, items, fiscal_data
            ) VALUES (?, ?, ?, 'return', ?, ?, ?, ?, ?, ?)
        ");
        
        $returnData = [
            'original_check_id' => $originalCheck['id'],
            'return_reason' => $reason,
            'return_date' => date('Y-m-d H:i:s')
        ];
        
        $fiscalData = json_decode($originalCheck['fiscal_data'] ?? '{}', true);
        $fiscalData['return_reason'] = $reason;
        $fiscalData['original_check_number'] = $originalCheck['kkm_check_number'];
        
        $stmt->execute([
            $originalCheck['shift_id'],
            $_SESSION['user_id'],
            $originalCheck['division_id'],
            $originalCheck['kkm_check_number'] + 1000,
            $originalCheck['total_amount'],
            $originalCheck['cash_amount'],
            $originalCheck['card_amount'],
            $originalCheck['items'],
            json_encode($fiscalData)
        ]);
        
        $returnCheckId = $pdo->lastInsertId();
        
        // Сохраняем данные для отправки в KKM
        $_SESSION['pending_kkm_return'] = [
            'check_id' => $returnCheckId,
            'original_check_id' => $originalCheck['id'],
            'amount' => $originalCheck['total_amount'],
            'reason' => $reason,
            'items' => json_decode($originalCheck['items'] ?? '[]', true)
        ];
        
        $_SESSION['success'] = 'Возврат подготовлен. Выполните возврат в ККТ.';
        
    } catch (PDOException $e) {
        $_SESSION['error'] = 'Ошибка базы данных: ' . $e->getMessage();
    }
}

// Функция для кодирования JSON с безопасностью
function htmlspecialjson($data) {
    return htmlspecialchars(json_encode($data, JSON_UNESCAPED_UNICODE | JSON_HEX_QUOT), ENT_QUOTES, 'UTF-8');
}

// Вспомогательная функция для проверки возможности отмены возврата
function canCancelReturn($check) {
    if ($check['type'] !== 'return') return false;
    
    $fiscalData = json_decode($check['fiscal_data'] ?? '{}', true);
    return !empty($fiscalData['universalId']) || !empty($fiscalData['UniversalID']);
}

// Получение UniversalID из данных чека
function getUniversalId($check) {
    $fiscalData = json_decode($check['fiscal_data'] ?? '{}', true);
    return $fiscalData['universalId'] ?? $fiscalData['UniversalID'] ?? '';
}
?>

<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Возврат чеков - RunaRMK</title>
    <link rel="stylesheet" href="../../css/style.css">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <script>var KkmServerAddIn = {};</script>
    <style>
        .container { max-width: 1200px; margin: 0 auto; padding: 20px; }
        .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 30px; }
        .btn { padding: 10px 20px; border-radius: 5px; cursor: pointer; display: inline-flex; align-items: center; gap: 8px; }
        .btn-primary { background: #3498db; color: white; border: none; }
        .btn-danger { background: #e74c3c; color: white; border: none; }
        .btn-success { background: #2ecc71; color: white; border: none; }
        .btn-warning { background: #f39c12; color: white; border: none; }
        
        .check-card {
            background: white;
            border-radius: 10px;
            padding: 20px;
            margin-bottom: 15px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
            border-left: 4px solid #3498db;
        }
        
        .check-return {
            border-left-color: #e74c3c;
        }
        
        .check-items {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 8px;
            margin-top: 15px;
        }
        
        .item-row {
            display: flex;
            justify-content: space-between;
            padding: 8px 0;
            border-bottom: 1px solid #eee;
        }
        
        .return-form {
            background: #fff3cd;
            padding: 15px;
            border-radius: 8px;
            margin-top: 15px;
        }
        
        .form-group {
            margin-bottom: 15px;
        }
        
        .form-control {
            width: 100%;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 5px;
        }
        
        .modal {
            display: none;
            position: fixed;
            top: 0; left: 0;
            width: 100%; height: 100%;
            background: rgba(0,0,0,0.5);
            justify-content: center;
            align-items: center;
            z-index: 1000;
        }
        
        .modal-content {
            background: white;
            border-radius: 10px;
            padding: 30px;
            max-width: 500px;
            width: 90%;
        }
        
        .section-title {
            font-size: 24px;
            margin: 30px 0 15px 0;
            padding-bottom: 10px;
            border-bottom: 2px solid #eee;
        }
        
        .alert {
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 20px;
        }
        
        .alert-success {
            background: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        
        .alert-danger {
            background: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
        
        .alert-warning {
            background: #fff3cd;
            color: #856404;
            border: 1px solid #ffeaa7;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1><i class="fas fa-undo"></i> Возврат чеков</h1>
            <a href="../checks/new_check.php" class="btn btn-primary">
                <i class="fas fa-arrow-left"></i> К новому чеку
            </a>
        </div>
        
        <?php if (isset($_SESSION['success'])): ?>
            <div class="alert alert-success">
                <i class="fas fa-check-circle"></i> <?php echo $_SESSION['success']; ?>
                <?php unset($_SESSION['success']); ?>
            </div>
        <?php endif; ?>
        
        <?php if (isset($_SESSION['error'])): ?>
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-circle"></i> <?php echo $_SESSION['error']; ?>
                <?php unset($_SESSION['error']); ?>
            </div>
        <?php endif; ?>
        
        <!-- Модальное окно KKM -->
        <div id="kkmModal" class="modal">
            <div class="modal-content">
                <h3><i class="fas fa-cash-register"></i> Возврат в ККТ</h3>
                <div id="kkmStatus">
                    <div style="text-align: center; margin: 20px 0;">
                        <i class="fas fa-spinner fa-spin fa-3x" style="color: #3498db;"></i>
                        <p id="kkmStatusText">Выполняется возврат...</p>
                        <p id="kkmDetails" style="font-size: 14px; color: #666;"></p>
                    </div>
                </div>
                <div id="kkmResult" style="display: none;">
                    <div class="alert alert-success" id="kkmSuccess">
                        <i class="fas fa-check-circle"></i> Возврат выполнен успешно!
                    </div>
                    <div class="alert alert-danger" id="kkmError" style="display: none;">
                        <i class="fas fa-times-circle"></i> <span id="kkmErrorMessage"></span>
                    </div>
                </div>
                <div style="text-align: center; margin-top: 20px;">
                    <button id="kkmCloseBtn" class="btn btn-primary" onclick="closeKkmModal()" style="display: none;">
                        <i class="fas fa-times"></i> Закрыть
                    </button>
                </div>
            </div>
        </div>
        
        <h2 class="section-title"><i class="fas fa-shopping-cart"></i> Чеки продаж (для возврата)</h2>
        
        <!-- Список чеков для возврата -->
        <div id="checksList">
            <?php if (count($checks) > 0): ?>
                <?php foreach ($checks as $check): ?>
                <?php 
                $items = json_decode($check['items'] ?? '[]', true);
                $fiscalData = json_decode($check['fiscal_data'] ?? '{}', true);
                ?>
                <div class="check-card">
                    <div style="display: flex; justify-content: space-between; align-items: center;">
                        <div>
                            <h3 style="margin: 0;">
                                Чек #<?php echo $check['id']; ?>
                                <span style="font-size: 14px; color: #666;">
                                    (ККТ: <?php echo $check['kkm_check_number'] ?? '—'; ?>)
                                </span>
                            </h3>
                            <div style="color: #666; margin-top: 5px;">
                                <i class="fas fa-user"></i> <?php echo htmlspecialchars($check['cashier_name']); ?> |
                                <i class="fas fa-store"></i> <?php echo htmlspecialchars($check['division_name']); ?> |
                                <i class="fas fa-calendar"></i> <?php echo date('d.m.Y H:i', strtotime($check['created_at'])); ?> |
                                <i class="fas fa-money-bill"></i> <?php echo number_format($check['total_amount'], 2, '.', ' '); ?> ₽
                            </div>
                        </div>
                        <div>
                            <button class="btn btn-danger" onclick="openReturnModal(<?php echo $check['id']; ?>, <?php echo htmlspecialjson($check); ?>)">
                                <i class="fas fa-undo"></i> Создать возврат
                            </button>
                        </div>
                    </div>
                    
                    <?php if (!empty($items)): ?>
                    <div class="check-items">
                        <strong>Товары:</strong>
                        <?php foreach ($items as $item): ?>
                        <div class="item-row">
                            <span><?php echo htmlspecialchars($item['name'] ?? 'Товар'); ?></span>
                            <span><?php echo ($item['quantity'] ?? 1) . ' × ' . number_format($item['price'] ?? 0, 2, '.', ' ') . ' ₽'; ?></span>
                        </div>
                        <?php endforeach; ?>
                    </div>
                    <?php endif; ?>
                </div>
                <?php endforeach; ?>
            <?php else: ?>
                <div style="text-align: center; padding: 50px; color: #666;">
                    <i class="fas fa-receipt fa-3x" style="margin-bottom: 20px; color: #ddd;"></i>
                    <h3>Нет чеков для возврата</h3>
                    <p>Сначала создайте чек продажи</p>
                </div>
            <?php endif; ?>
        </div>
        
        <h2 class="section-title"><i class="fas fa-history"></i> Созданные возвраты</h2>
        
        <!-- Список возвратов для отмены -->
        <div id="returnsList">
            <?php if (count($return_checks) > 0): ?>
                <?php foreach ($return_checks as $check): ?>
                <?php 
                $items = json_decode($check['items'] ?? '[]', true);
                $fiscalData = json_decode($check['fiscal_data'] ?? '{}', true);
                $universalId = getUniversalId($check);
                $canCancel = canCancelReturn($check);
                ?>
                <div class="check-card check-return">
                    <div style="display: flex; justify-content: space-between; align-items: center;">
                        <div>
                            <h3 style="margin: 0;">
                                Возврат #<?php echo $check['id']; ?>
                                <span style="font-size: 14px; color: #666;">
                                    (ККТ: <?php echo $check['kkm_check_number'] ?? '—'; ?>)
                                </span>
                            </h3>
                            <div style="color: #666; margin-top: 5px;">
                                <i class="fas fa-user"></i> <?php echo htmlspecialchars($check['cashier_name']); ?> |
                                <i class="fas fa-store"></i> <?php echo htmlspecialchars($check['division_name']); ?> |
                                <i class="fas fa-calendar"></i> <?php echo date('d.m.Y H:i', strtotime($check['created_at'])); ?> |
                                <i class="fas fa-money-bill"></i> <?php echo number_format($check['total_amount'], 2, '.', ' '); ?> ₽
                            </div>
                            <?php if ($universalId): ?>
                                <div style="margin-top: 5px; font-size: 12px; color: #666;">
                                    <i class="fas fa-key"></i> ID транзакции: <?php echo htmlspecialchars(substr($universalId, 0, 30)); ?>...
                                </div>
                            <?php endif; ?>
                        </div>
                        <div>
                            <?php if ($canCancel): ?>
                                <button class="btn btn-warning" onclick="openCancelModal(<?php echo $check['id']; ?>, <?php echo htmlspecialjson($check); ?>)"
                                        id="cancelBtn_<?php echo $check['id']; ?>">
                                    <i class="fas fa-ban"></i> Отменить возврат
                                </button>
                            <?php else: ?>
                                <button class="btn btn-warning" style="opacity: 0.5; cursor: not-allowed;" disabled>
                                    <i class="fas fa-ban"></i> Нельзя отменить
                                </button>
                            <?php endif; ?>
                        </div>
                    </div>
                    
                    <?php if (!empty($items)): ?>
                    <div class="check-items">
                        <strong>Товары:</strong>
                        <?php foreach ($items as $item): ?>
                        <div class="item-row">
                            <span><?php echo htmlspecialchars($item['name'] ?? 'Товар'); ?></span>
                            <span><?php echo ($item['quantity'] ?? 1) . ' × ' . number_format($item['price'] ?? 0, 2, '.', ' ') . ' ₽'; ?></span>
                        </div>
                        <?php endforeach; ?>
                    </div>
                    <?php endif; ?>
                    
                    <?php if (isset($fiscalData['return_reason'])): ?>
                    <div style="margin-top: 10px; padding: 10px; background: #e9f7fe; border-radius: 5px;">
                        <strong>Причина возврата:</strong> <?php echo htmlspecialchars($fiscalData['return_reason']); ?>
                    </div>
                    <?php endif; ?>
                </div>
                <?php endforeach; ?>
            <?php else: ?>
                <div style="text-align: center; padding: 50px; color: #666;">
                    <i class="fas fa-undo fa-3x" style="margin-bottom: 20px; color: #ddd;"></i>
                    <h3>Нет созданных возвратов</h3>
                    <p>Создайте возврат из чека продажи выше</p>
                </div>
            <?php endif; ?>
        </div>
    </div>
    
    <!-- Модальное окно для возврата -->
    <div id="returnModal" class="modal">
        <div class="modal-content">
            <h3><i class="fas fa-undo"></i> Возврат чека</h3>
            <form id="returnForm" method="POST">
                <input type="hidden" name="action" value="return_check">
                <input type="hidden" id="returnCheckId" name="check_id">
                
                <div class="form-group">
                    <label>Причина возврата:</label>
                    <select name="reason" class="form-control" required>
                        <option value="">-- Выберите причину --</option>
                        <option value="Не понравился товар">Не понравился товар</option>
                        <option value="Не подошел размер">Не подошел размер</option>
                        <option value="Бракованный товар">Бракованный товар</option>
                        <option value="Ошибка кассира">Ошибка кассира</option>
                        <option value="Другое">Другое</option>
                    </select>
                </div>
                
                <div class="form-group">
                    <label>Комментарий:</label>
                    <textarea name="comment" class="form-control" rows="3" placeholder="Дополнительная информация..."></textarea>
                </div>
                
                <div id="returnItems" style="margin: 15px 0;"></div>
                
                <div style="display: flex; justify-content: space-between; margin-top: 25px;">
                    <button type="button" class="btn" onclick="closeReturnModal()">Отмена</button>
                    <button type="submit" class="btn btn-danger">
                        <i class="fas fa-undo"></i> Выполнить возврат
                    </button>
                </div>
            </form>
        </div>
    </div>
    
    <!-- Модальное окно для отмены возврата -->
    <div id="cancelReturnModal" class="modal">
        <div class="modal-content">
            <h3><i class="fas fa-ban"></i> Отмена возврата</h3>
            <div id="cancelReturnInfo" style="margin-bottom: 20px;"></div>
            
            <div class="form-group">
                <label>Причина отмены:</label>
                <select id="cancelReason" class="form-control">
                    <option value="">-- Выберите причину --</option>
                    <option value="Ошибка при возврате">Ошибка при возврате</option>
                    <option value="Отмена клиентом">Отмена клиентом</option>
                    <option value="Техническая ошибка">Техническая ошибка</option>
                    <option value="Другое">Другое</option>
                </select>
            </div>
            
            <div class="form-group">
                <label>Комментарий:</label>
                <textarea id="cancelComment" class="form-control" rows="2" placeholder="Дополнительная информация..."></textarea>
            </div>
            
            <div id="cancelResult" style="margin: 15px 0; display: none;"></div>
            
            <div style="display: flex; justify-content: space-between; margin-top: 25px;">
                <button type="button" class="btn" onclick="closeCancelModal()">Отмена</button>
                <button type="button" class="btn btn-warning" onclick="executeCancelReturn()" id="executeCancelBtn" style="background: #f39c12;">
                    <i class="fas fa-ban"></i> Отменить возврат
                </button>
            </div>
        </div>
    </div>
    
    <script>
        // Вспомогательная функция для JSON
        function htmlspecialjson(data) {
            return JSON.stringify(data).replace(/"/g, '&quot;');
        }
        
        // Модальное окно возврата
        let currentCheckData = null;
        
        function openReturnModal(checkId, checkData) {
            currentCheckData = typeof checkData === 'string' ? JSON.parse(checkData) : checkData;
            document.getElementById('returnCheckId').value = checkId;
            document.getElementById('returnModal').style.display = 'flex';
            
            // Показываем товары
            const itemsContainer = document.getElementById('returnItems');
            itemsContainer.innerHTML = '<strong>Товары в чеке:</strong>';
            
            if (currentCheckData.items && typeof currentCheckData.items === 'string') {
                currentCheckData.items = JSON.parse(currentCheckData.items);
            }
            
            const items = currentCheckData.items || [];
            items.forEach((item, index) => {
                itemsContainer.innerHTML += `
                    <div style="padding: 8px; border-bottom: 1px solid #eee; display: flex; justify-content: space-between;">
                        <span>${item.name || 'Товар'}</span>
                        <span>${item.quantity || 1} × ${parseFloat(item.price || 0).toFixed(2)} ₽</span>
                    </div>
                `;
            });
        }
        
        function closeReturnModal() {
            document.getElementById('returnModal').style.display = 'none';
        }
        
        // Модальное окно отмены возврата
        let currentCancelCheckData = null;
        let currentCancelCheckId = null;
        
        function openCancelModal(checkId, checkData) {
            currentCancelCheckData = typeof checkData === 'string' ? JSON.parse(checkData) : checkData;
            currentCancelCheckId = checkId;
            
            const infoDiv = document.getElementById('cancelReturnInfo');
            const universalId = getUniversalIdFromCheck(currentCancelCheckData);
            
            infoDiv.innerHTML = `
                <div style="background: #f8f9fa; padding: 15px; border-radius: 8px;">
                    <p><strong>Возврат:</strong> #${checkId}</p>
                    <p><strong>Сумма возврата:</strong> ${currentCancelCheckData.total_amount} ₽</p>
                    <p><strong>ID транзакции:</strong> <small>${universalId || 'Не указан'}</small></p>
                    <p><strong>Дата возврата:</strong> ${new Date(currentCancelCheckData.created_at).toLocaleString()}</p>
                    <p style="color: #e74c3c; font-weight: bold;">⚠️ Эта операция вернет деньги покупателю!</p>
                </div>
            `;
            
            document.getElementById('cancelReturnModal').style.display = 'flex';
        }
        
        function closeCancelModal() {
            document.getElementById('cancelReturnModal').style.display = 'none';
            currentCancelCheckData = null;
            currentCancelCheckId = null;
            document.getElementById('cancelResult').style.display = 'none';
        }
        
        // Функция получения UniversalID из данных чека
        function getUniversalIdFromCheck(checkData) {
            try {
                const fiscalData = typeof checkData.fiscal_data === 'string' 
                    ? JSON.parse(checkData.fiscal_data || '{}') 
                    : (checkData.fiscal_data || {});
                
                return fiscalData.universalId || fiscalData.UniversalID || '';
            } catch (e) {
                return '';
            }
        }
        
        // Функция отмены возврата через KKM
        function cancelReturnTransaction(checkId, universalId, amount) {
            return new Promise((resolve, reject) => {
                try {
                    if (typeof KkmServer === 'undefined') {
                        reject('Расширение KKM Server не найдено');
                        return;
                    }
                    
                    // Команда отмены возврата платежа
                    const command = {
                        Command: "CancelPaymentByPaymentCard",
                        NumDevice: 0,
                        Amount: amount,
                        UniversalID: universalId,
                        IdCommand: guid(),
                        Timeout: 60
                    };
                    
                    console.log("Команда отмены возврата:", command);
                    
                    KkmServer.Execute(function(result) {
                        console.log("Ответ KKM на отмену возврата:", result);
                        
                        if (result.Status === 0 || result.Status === "0") {
                            resolve({
                                success: true,
                                message: 'Возврат отменен',
                                slip: result.Slip,
                                universalId: result.UniversalID
                            });
                        } else {
                            reject(result.Error || result.error || 'Ошибка отмены возврата в ККТ');
                        }
                    }, command);
                    
                } catch (error) {
                    reject('Ошибка выполнения: ' + error.message);
                }
            });
        }
        
        // Выполнение отмены возврата
        function executeCancelReturn() {
            if (!currentCancelCheckData || !currentCancelCheckId) return;
            
            const reason = document.getElementById('cancelReason').value;
            const comment = document.getElementById('cancelComment').value;
            const universalId = getUniversalIdFromCheck(currentCancelCheckData);
            
            if (!universalId) {
                showCancelResult('error', 'Не найден ID транзакции для отмены');
                return;
            }
            
            if (!reason) {
                showCancelResult('error', 'Укажите причину отмены');
                return;
            }
            
            // Блокируем кнопку
            const btn = document.getElementById('executeCancelBtn');
            btn.disabled = true;
            btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Выполнение...';
            
            // Показываем модальное окно KKM
            const modal = document.getElementById('kkmModal');
            modal.style.display = 'flex';
            document.getElementById('kkmStatusText').textContent = 'Отмена возврата...';
            document.getElementById('kkmDetails').textContent = `Сумма: ${currentCancelCheckData.total_amount} ₽`;
            
            // Выполняем отмену через KKM
            cancelReturnTransaction(
                currentCancelCheckId,
                universalId,
                currentCancelCheckData.total_amount
            )
            .then(result => {
                console.log("Отмена возврата успешна:", result);
                
                // Обновляем статус в KKM модалке
                document.getElementById('kkmStatusText').textContent = 'Отмена выполнена!';
                document.querySelector('#kkmStatus i').className = 'fas fa-check-circle fa-3x';
                document.querySelector('#kkmStatus i').style.color = '#2ecc71';
                
                // Показываем результат
                document.getElementById('kkmResult').style.display = 'block';
                document.getElementById('kkmSuccess').style.display = 'block';
                document.getElementById('kkmSuccess').innerHTML = `
                    <i class="fas fa-check-circle"></i> Возврат успешно отменен!
                    ${result.slip ? '<pre style="font-size: 12px; margin-top: 10px;">' + result.slip + '</pre>' : ''}
                `;
                
                // Обновляем статус в базе данных
                updateReturnStatus(currentCancelCheckId, 'cancelled', reason, comment, result);
                
                // Закрываем модальное окно отмены
                setTimeout(() => {
                    closeCancelModal();
                    modal.style.display = 'none';
                    window.location.reload();
                }, 2000);
            })
            .catch(error => {
                console.error("Ошибка отмены возврата:", error);
                
                document.getElementById('kkmStatusText').textContent = 'Ошибка отмены';
                document.querySelector('#kkmStatus i').className = 'fas fa-times-circle fa-3x';
                document.querySelector('#kkmStatus i').style.color = '#e74c3c';
                
                document.getElementById('kkmResult').style.display = 'block';
                document.getElementById('kkmError').style.display = 'block';
                document.getElementById('kkmErrorMessage').textContent = error;
                
                // Разблокируем кнопку
                btn.disabled = false;
                btn.innerHTML = '<i class="fas fa-ban"></i> Отменить возврат';
                
                // Показываем кнопку закрыть
                document.getElementById('kkmCloseBtn').style.display = 'inline-block';
            });
        }
        
        // Обновление статуса возврата в БД
        function updateReturnStatus(checkId, status, reason, comment, kkmResult) {
            const formData = new FormData();
            formData.append('action', 'cancel_return');
            formData.append('check_id', checkId);
            formData.append('status', status);
            formData.append('reason', reason);
            formData.append('comment', comment);
            formData.append('kkm_result', JSON.stringify(kkmResult));
            
            fetch('../../api/cancel_return.php', {
                method: 'POST',
                body: formData
            })
            .then(response => response.text())
            .then(data => {
                console.log("Статус обновлен:", data);
            })
            .catch(error => {
                console.error("Ошибка обновления статуса:", error);
            });
        }
        
        // Показ результата отмены
        function showCancelResult(type, message) {
            const resultDiv = document.getElementById('cancelResult');
            resultDiv.style.display = 'block';
            resultDiv.className = type === 'success' ? 'alert alert-success' : 'alert alert-danger';
            resultDiv.innerHTML = `<i class="fas fa-${type === 'success' ? 'check' : 'exclamation'}-circle"></i> ${message}`;
        }
        
        // Функция возврата в KKM
        function executeReturnInKKM(checkId, amount, reason, items) {
            return new Promise((resolve, reject) => {
                try {
                    if (typeof KkmServer === 'undefined') {
                        reject('Расширение KKM Server не найдено');
                        return;
                    }
                    
                    // Подготавливаем данные для возврата
                    const command = {
                        Command: "RegisterCheck",
                        NumDevice: 0,
                        TypeCheck: 1, // 1 = возврат продажи
                        IsFiscalCheck: true,
                        CashierName: "<?php echo $_SESSION['full_name'] ?? 'Кассир'; ?>",
                        ClientAddress: "example@email.com",
                        NotPrint: false,
                        IdCommand: guid(),
                        Timeout: 60,
                        
                        // Данные для возврата
                        CheckStrings: prepareReturnItems(items),
                        Cash: 0,
                        ElectronicPayment: amount,
                        AdvancePayment: 0,
                        Credit: 0,
                        CashProvision: 0
                    };
                    
                    console.log("Команда возврата в KKM:", command);
                    
                    KkmServer.Execute(function(result) {
                        console.log("Ответ KKM на возврат:", result);
                        
                        if (result.Status === 0 || result.Status === "0") {
                            resolve({
                                success: true,
                                checkNumber: result.CheckNumber,
                                sessionNumber: result.SessionNumber,
                                qrCode: result.QRCode,
                                UniversalID: result.UniversalID // Сохраняем ID транзакции!
                            });
                        } else {
                            reject(result.Error || result.error || 'Ошибка возврата в ККТ');
                        }
                    }, command);
                    
                } catch (error) {
                    reject('Ошибка: ' + error.message);
                }
            });
        }
        
        // Подготовка товаров для возврата
        function prepareReturnItems(items) {
            if (!Array.isArray(items)) return [];
            
            return items.map(item => ({
                Register: {
                    Name: item.name || 'Товар',
                    Quantity: item.quantity || 1,
                    Price: item.price || 0,
                    Amount: (item.quantity || 1) * (item.price || 0),
                    Department: item.department || 0,
                    Tax: item.tax || -1,
                    SignMethodCalculation: 4,
                    SignCalculationObject: 1
                }
            }));
        }
        
        // GUID генератор
        function guid() {
            function S4() {
                return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
            }
            return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
        }
        
        // Обработка формы возврата
        document.getElementById('returnForm').addEventListener('submit', function(e) {
            e.preventDefault();
            
            const formData = new FormData(this);
            
            // Показываем модальное окно KKM
            const modal = document.getElementById('kkmModal');
            modal.style.display = 'flex';
            document.getElementById('kkmStatusText').textContent = 'Выполнение возврата...';
            document.getElementById('kkmDetails').textContent = `Сумма: ${currentCheckData.total_amount} ₽`;
            
            // Сначала выполняем возврат в KKM
            executeReturnInKKM(
                formData.get('check_id'),
                currentCheckData.total_amount,
                formData.get('reason'),
                currentCheckData.items || []
            )
            .then(result => {
                console.log("Возврат в KKM успешен:", result);
                
                // Сохраняем UniversalID в форме для передачи на сервер
                const universalIdInput = document.createElement('input');
                universalIdInput.type = 'hidden';
                universalIdInput.name = 'universal_id';
                universalIdInput.value = result.UniversalID || '';
                this.appendChild(universalIdInput);
                
                // Обновляем статус
                document.getElementById('kkmStatusText').textContent = 'Возврат выполнен!';
                document.querySelector('#kkmStatus i').className = 'fas fa-check-circle fa-3x';
                document.querySelector('#kkmStatus i').style.color = '#2ecc71';
                
                // Показываем результат
                document.getElementById('kkmResult').style.display = 'block';
                document.getElementById('kkmSuccess').style.display = 'block';
                
                // Отправляем форму на сервер
                fetch(window.location.href, {
                    method: 'POST',
                    body: formData
                })
                .then(() => {
                    // Обновляем страницу через 2 секунды
                    setTimeout(() => {
                        window.location.reload();
                    }, 2000);
                });
            })
            .catch(error => {
                console.error("Ошибка возврата:", error);
                
                document.getElementById('kkmStatusText').textContent = 'Ошибка возврата';
                document.querySelector('#kkmStatus i').className = 'fas fa-times-circle fa-3x';
                document.querySelector('#kkmStatus i').style.color = '#e74c3c';
                
                document.getElementById('kkmResult').style.display = 'block';
                document.getElementById('kkmError').style.display = 'block';
                document.getElementById('kkmErrorMessage').textContent = error;
                
                document.getElementById('kkmCloseBtn').style.display = 'inline-block';
            });
        });
        
        function closeKkmModal() {
            document.getElementById('kkmModal').style.display = 'none';
            window.location.reload();
        }
        
        // Автоматически открываем модальное окно KKM если есть pending возврат
        <?php if (isset($_SESSION['pending_kkm_return'])): ?>
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(() => {
                const pending = <?php echo json_encode($_SESSION['pending_kkm_return']); ?>;
                openKkmModalForReturn(pending);
                <?php unset($_SESSION['pending_kkm_return']); ?>
            }, 500);
        });
        
        function openKkmModalForReturn(pending) {
            const modal = document.getElementById('kkmModal');
            modal.style.display = 'flex';
            document.getElementById('kkmStatusText').textContent = 'Подготовка возврата...';
            document.getElementById('kkmDetails').textContent = `Сумма: ${pending.amount} ₽`;
        }
        <?php endif; ?>
    </script>
</body>
</html>