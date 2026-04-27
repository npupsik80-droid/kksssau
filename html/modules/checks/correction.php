<?php
require_once '../../includes/auth_check.php';

if ($_SESSION['permission_group'] !== 'admin') {
    header('Location: ../../index.php');
    exit();
}

// Обработка коррекции
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['action']) && $_POST['action'] === 'correction_check') {
    correctionCheck();
}

function correctionCheck() {
    global $pdo;
    
    $correctionType = intval($_POST['correction_type'] ?? 0);
    $correctionAmount = floatval($_POST['correction_amount'] ?? 0);
    $reason = trim($_POST['reason'] ?? '');
    $baseDate = $_POST['base_date'] ?? '';
    $baseNumber = trim($_POST['base_number'] ?? '');
    $divisionId = intval($_POST['division_id'] ?? 0);
    
    if ($correctionAmount <= 0 || empty($reason) || empty($baseDate) || empty($baseNumber)) {
        $_SESSION['error'] = 'Заполните все обязательные поля';
        return;
    }
    
    try {
        // Создаем запись о коррекции
        $stmt = $pdo->prepare("
            INSERT INTO checks (
                user_id, division_id, type, 
                total_amount, cash_amount, 
                card_amount, fiscal_data
            ) VALUES (?, ?, 'correction', ?, ?, ?, ?)
        ");
        
        $fiscalData = [
            'correction_type' => $correctionType,
            'correction_amount' => $correctionAmount,
            'reason' => $reason,
            'base_date' => $baseDate,
            'base_number' => $baseNumber,
            'correction_date' => date('Y-m-d H:i:s')
        ];
        
        $cashAmount = $correctionType === 0 ? $correctionAmount : 0;
        $cardAmount = $correctionType === 1 ? $correctionAmount : 0;
        
        $stmt->execute([
            $_SESSION['user_id'],
            $divisionId,
            $correctionAmount,
            $cashAmount,
            $cardAmount,
            json_encode($fiscalData)
        ]);
        
        $correctionId = $pdo->lastInsertId();
        
        // Сохраняем данные для отправки в KKM
        $_SESSION['pending_kkm_correction'] = [
            'correction_id' => $correctionId,
            'type' => $correctionType,
            'amount' => $correctionAmount,
            'reason' => $reason,
            'base_date' => $baseDate,
            'base_number' => $baseNumber
        ];
        
        $_SESSION['success'] = 'Коррекция подготовлена. Выполните коррекцию в ККТ.';
        
    } catch (PDOException $e) {
        $_SESSION['error'] = 'Ошибка базы данных: ' . $e->getMessage();
    }
}

// Получаем список подразделений
$stmt = $pdo->query("SELECT id, name FROM divisions ORDER BY name");
$divisions = $stmt->fetchAll();
?>

<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Коррекция чека - RunaRMK</title>
    <link rel="stylesheet" href="../../css/style.css">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <script>var KkmServerAddIn = {};</script>
    <style>
        .container { max-width: 800px; margin: 0 auto; padding: 20px; }
        .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 30px; }
        .btn { padding: 10px 20px; border-radius: 5px; cursor: pointer; display: inline-flex; align-items: center; gap: 8px; }
        .btn-primary { background: #3498db; color: white; border: none; }
        .btn-warning { background: #f39c12; color: white; border: none; }
        
        .correction-form {
            background: white;
            border-radius: 10px;
            padding: 30px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        
        .form-group {
            margin-bottom: 20px;
        }
        
        .form-group label {
            display: block;
            margin-bottom: 8px;
            font-weight: 600;
            color: #2c3e50;
        }
        
        .form-control {
            width: 100%;
            padding: 12px 15px;
            border: 1px solid #ddd;
            border-radius: 5px;
            font-size: 16px;
            box-sizing: border-box;
        }
        
        .form-row {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
        }
        
        .form-actions {
            display: flex;
            justify-content: space-between;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eee;
        }
        
        .info-box {
            background: #f8f9fa;
            border-left: 4px solid #3498db;
            padding: 15px;
            margin-bottom: 25px;
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
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1><i class="fas fa-edit"></i> Коррекция чека</h1>
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
                <h3><i class="fas fa-cash-register"></i> Коррекция в ККТ</h3>
                <div id="kkmStatus">
                    <div style="text-align: center; margin: 20px 0;">
                        <i class="fas fa-spinner fa-spin fa-3x" style="color: #3498db;"></i>
                        <p id="kkmStatusText">Выполняется коррекция...</p>
                        <p id="kkmDetails" style="font-size: 14px; color: #666;"></p>
                    </div>
                </div>
                <div id="kkmResult" style="display: none;">
                    <div class="alert alert-success" id="kkmSuccess">
                        <i class="fas fa-check-circle"></i> Коррекция выполнена успешно!
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
        
        <!-- Информационный блок -->
        <div class="info-box">
            <h4><i class="fas fa-info-circle"></i> Информация о коррекции</h4>
            <p>Коррекция чека используется для исправления ошибок в ранее зарегистрированных чеках. 
            Выберите тип коррекции и укажите документ-основание.</p>
        </div>
        
        <!-- Форма коррекции -->
        <div class="correction-form">
            <form id="correctionForm" method="POST">
                <input type="hidden" name="action" value="correction_check">
                
                <div class="form-row">
                    <div class="form-group">
                        <label><i class="fas fa-store"></i> Подразделение:</label>
                        <select name="division_id" class="form-control" required>
                            <option value="">-- Выберите подразделение --</option>
                            <?php foreach ($divisions as $division): ?>
                            <option value="<?php echo $division['id']; ?>">
                                <?php echo htmlspecialchars($division['name']); ?>
                            </option>
                            <?php endforeach; ?>
                        </select>
                    </div>
                    
                    <div class="form-group">
                        <label><i class="fas fa-edit"></i> Тип коррекции:</label>
                        <select name="correction_type" class="form-control" required id="correctionType">
                            <option value="0">Самостоятельно</option>
                            <option value="1">По предписанию</option>
                        </select>
                    </div>
                </div>
                
                <div class="form-row">
                    <div class="form-group">
                        <label><i class="fas fa-calendar"></i> Дата документа-основания:</label>
                        <input type="date" name="base_date" class="form-control" required 
                               value="<?php echo date('Y-m-d'); ?>">
                    </div>
                    
                    <div class="form-group">
                        <label><i class="fas fa-hashtag"></i> Номер документа-основания:</label>
                        <input type="text" name="base_number" class="form-control" required 
                               placeholder="Например: ПР-2024-001">
                    </div>
                </div>
                
                <div class="form-row">
                    <div class="form-group">
                        <label><i class="fas fa-money-bill"></i> Сумма коррекции (₽):</label>
                        <input type="number" name="correction_amount" class="form-control" required 
                               min="0.01" step="0.01" placeholder="0.00">
                    </div>
                    
                    <div class="form-group">
                        <label><i class="fas fa-comment"></i> Причина коррекции:</label>
                        <select name="reason" class="form-control" required>
                            <option value="">-- Выберите причину --</option>
                            <option value="Ошибка кассира">Ошибка кассира</option>
                            <option value="Технический сбой">Технический сбой</option>
                            <option value="Изменение законодательства">Изменение законодательства</option>
                            <option value="Исправление бухгалтерской ошибки">Исправление бухгалтерской ошибки</option>
                            <option value="Другое">Другое</option>
                        </select>
                    </div>
                </div>
                
                <div class="form-group">
                    <label><i class="fas fa-sticky-note"></i> Дополнительные сведения:</label>
                    <textarea name="additional_info" class="form-control" rows="3" 
                              placeholder="Подробное описание причины коррекции..."></textarea>
                </div>
                
                <div class="form-actions">
                    <button type="button" class="btn" onclick="window.history.back()">
                        <i class="fas fa-times"></i> Отмена
                    </button>
                    <button type="submit" class="btn btn-warning">
                        <i class="fas fa-check"></i> Выполнить коррекцию
                    </button>
                </div>
            </form>
        </div>
        
        <!-- История коррекций -->
        <div style="margin-top: 40px;">
            <h3><i class="fas fa-history"></i> История коррекций</h3>
            <div style="background: white; border-radius: 10px; padding: 20px; margin-top: 15px;">
                <?php
                $stmt = $pdo->prepare("
                    SELECT c.*, u.full_name, d.name as division_name
                    FROM checks c
                    LEFT JOIN users u ON c.user_id = u.id
                    LEFT JOIN divisions d ON c.division_id = d.id
                    WHERE c.type = 'correction'
                    ORDER BY c.created_at DESC
                    LIMIT 10
                ");
                $stmt->execute();
                $corrections = $stmt->fetchAll();
                
                if (count($corrections) > 0): 
                ?>
                    <table style="width: 100%; border-collapse: collapse;">
                        <thead>
                            <tr style="border-bottom: 2px solid #eee;">
                                <th style="padding: 10px; text-align: left;">Дата</th>
                                <th style="padding: 10px; text-align: left;">Подразделение</th>
                                <th style="padding: 10px; text-align: left;">Сумма</th>
                                <th style="padding: 10px; text-align: left;">Причина</th>
                                <th style="padding: 10px; text-align: left;">Кассир</th>
                            </tr>
                        </thead>
                        <tbody>
                            <?php foreach ($corrections as $corr): 
                            $fiscalData = json_decode($corr['fiscal_data'] ?? '{}', true);
                            ?>
                            <tr style="border-bottom: 1px solid #eee;">
                                <td style="padding: 10px;"><?php echo date('d.m.Y H:i', strtotime($corr['created_at'])); ?></td>
                                <td style="padding: 10px;"><?php echo htmlspecialchars($corr['division_name']); ?></td>
                                <td style="padding: 10px;"><?php echo number_format($corr['total_amount'], 2, '.', ' '); ?> ₽</td>
                                <td style="padding: 10px;"><?php echo htmlspecialchars($fiscalData['reason'] ?? '—'); ?></td>
                                <td style="padding: 10px;"><?php echo htmlspecialchars($corr['full_name']); ?></td>
                            </tr>
                            <?php endforeach; ?>
                        </tbody>
                    </table>
                <?php else: ?>
                    <div style="text-align: center; padding: 30px; color: #999;">
                        <i class="fas fa-history fa-2x" style="margin-bottom: 15px;"></i>
                        <p>Нет выполненных коррекций</p>
                    </div>
                <?php endif; ?>
            </div>
        </div>
    </div>
    
    <script>
        // Функция коррекции в KKM
        function executeCorrectionInKKM(correctionData) {
            return new Promise((resolve, reject) => {
                try {
                    if (typeof KkmServer === 'undefined') {
                        reject('Расширение KKM Server не найдено');
                        return;
                    }
                    
                    // Подготавливаем данные для коррекции
                    const command = {
                        Command: "RegisterCheck",
                        NumDevice: 0,
                        TypeCheck: 2, // 2 = корректировка продажи
                        IsFiscalCheck: true,
                        CashierName: "<?php echo $_SESSION['full_name'] ?? 'Кассир'; ?>",
                        NotPrint: false,
                        IdCommand: guid(),
                        Timeout: 60,
                        
                        // Данные для коррекции
                        CorrectionType: parseInt(correctionData.type) || 0,
                        CorrectionBaseDate: correctionData.base_date,
                        CorrectionBaseNumber: correctionData.base_number,
                        CheckStrings: [{
                            Register: {
                                Name: "Коррекция чека",
                                Quantity: 1,
                                Price: correctionData.amount,
                                Amount: correctionData.amount,
                                Department: 0,
                                Tax: -1,
                                SignMethodCalculation: 4,
                                SignCalculationObject: 10 // 10 = платеж
                            }
                        }],
                        Cash: correctionData.type === 0 ? correctionData.amount : 0,
                        ElectronicPayment: correctionData.type === 1 ? correctionData.amount : 0,
                        AdvancePayment: 0,
                        Credit: 0,
                        CashProvision: 0
                    };
                    
                    console.log("Команда коррекции в KKM:", command);
                    
                    KkmServer.Execute(function(result) {
                        console.log("Ответ KKM на коррекцию:", result);
                        
                        if (result.Status === 0 || result.Status === "0") {
                            resolve({
                                success: true,
                                checkNumber: result.CheckNumber,
                                sessionNumber: result.SessionNumber,
                                qrCode: result.QRCode
                            });
                        } else {
                            reject(result.Error || result.error || 'Ошибка коррекции в ККТ');
                        }
                    }, command);
                    
                } catch (error) {
                    reject('Ошибка: ' + error.message);
                }
            });
        }
        
        // GUID генератор
        function guid() {
            function S4() {
                return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
            }
            return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
        }
        
        // Обработка формы коррекции
        document.getElementById('correctionForm').addEventListener('submit', function(e) {
            e.preventDefault();
            
            const formData = new FormData(this);
            const correctionData = {
                type: formData.get('correction_type'),
                amount: parseFloat(formData.get('correction_amount')),
                reason: formData.get('reason'),
                base_date: formData.get('base_date'),
                base_number: formData.get('base_number'),
                additional_info: formData.get('additional_info')
            };
            
            // Валидация
            if (correctionData.amount <= 0) {
                alert('Укажите сумму коррекции больше 0');
                return;
            }
            
            if (!confirm(`Выполнить коррекцию на сумму ${correctionData.amount} ₽?\nПричина: ${correctionData.reason}`)) {
                return;
            }
            
            // Показываем модальное окно KKM
            const modal = document.getElementById('kkmModal');
            modal.style.display = 'flex';
            document.getElementById('kkmStatusText').textContent = 'Выполнение коррекции...';
            document.getElementById('kkmDetails').textContent = 
                `Сумма: ${correctionData.amount} ₽, Тип: ${correctionData.type === '0' ? 'Самостоятельно' : 'По предписанию'}`;
            
            // Сначала выполняем коррекцию в KKM
            executeCorrectionInKKM(correctionData)
            .then(result => {
                console.log("Коррекция в KKM успешна:", result);
                
                // Обновляем статус
                document.getElementById('kkmStatusText').textContent = 'Коррекция выполнена!';
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
                console.error("Ошибка коррекции:", error);
                
                document.getElementById('kkmStatusText').textContent = 'Ошибка коррекции';
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
        
        // Автоматически открываем модальное окно KKM если есть pending коррекция
        <?php if (isset($_SESSION['pending_kkm_correction'])): ?>
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(() => {
                const pending = <?php echo json_encode($_SESSION['pending_kkm_correction']); ?>;
                openKkmModalForCorrection(pending);
                <?php unset($_SESSION['pending_kkm_correction']); ?>
            }, 500);
        });
        
        function openKkmModalForCorrection(pending) {
            const modal = document.getElementById('kkmModal');
            modal.style.display = 'flex';
            document.getElementById('kkmStatusText').textContent = 'Подготовка коррекции...';
            document.getElementById('kkmDetails').textContent = 
                `Сумма: ${pending.amount} ₽, Тип: ${pending.type === 0 ? 'Самостоятельно' : 'По предписанию'}`;
            
            // Здесь можно автоматически запустить коррекцию в KKM
            // executeCorrectionInKKM(pending)
        }
        <?php endif; ?>
    </script>
</body>
</html>