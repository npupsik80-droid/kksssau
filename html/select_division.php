<?php
session_start();
require_once 'config/database.php';

// Проверяем авторизацию
if (!isset($_SESSION['user_id'])) {
    header('Location: login.php');
    exit();
}

// Обработка выбора подразделения
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['division_id'])) {
    $division_id = intval($_POST['division_id']);
    
    // Получаем информацию о подразделении
    $stmt = $pdo->prepare("SELECT * FROM divisions WHERE id = ?");
    $stmt->execute([$division_id]);
    $division = $stmt->fetch();
    
    if ($division) {
        // Сохраняем в сессию
        $_SESSION['current_division_id'] = $division['id'];
        $_SESSION['current_division_name'] = $division['name'];
        
        // Получаем ID склада для этого подразделения
        $stmt = $pdo->prepare("SELECT id FROM warehouses WHERE division_id = ? LIMIT 1");
        $stmt->execute([$division['id']]);
        $warehouse = $stmt->fetch();
        
        if ($warehouse) {
            $_SESSION['current_warehouse_id'] = $warehouse['id'];
        }
        
        // Перенаправляем на главную
        header('Location: index.php');
        exit();
    } else {
        $error = "Выбранное подразделение не найдено";
    }
}

// Получаем все подразделения
$stmt = $pdo->prepare("SELECT * FROM divisions ORDER BY name");
$stmt->execute();
$divisions = $stmt->fetchAll();
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Выбор подразделения - RunaRMK</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        
        body {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }
        
        .selection-container {
            background: white;
            border-radius: 20px;
            box-shadow: 0 15px 35px rgba(0, 0, 0, 0.2);
            padding: 40px;
            width: 100%;
            max-width: 600px;
            animation: slideUp 0.5s ease;
        }
        
        @keyframes slideUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        .header {
            text-align: center;
            margin-bottom: 30px;
        }
        
        .header h1 {
            color: #2c3e50;
            font-size: 28px;
            margin-bottom: 10px;
        }
        
        .user-info {
            color: #7f8c8d;
            font-size: 14px;
            margin-bottom: 5px;
        }
        
        .welcome-text {
            color: #3498db;
            font-size: 18px;
            font-weight: 600;
            margin-bottom: 20px;
            text-align: center;
        }
        
        .error-message {
            background: #ffeaea;
            color: #e74c3c;
            padding: 15px;
            border-radius: 10px;
            margin-bottom: 20px;
            text-align: center;
            border-left: 4px solid #e74c3c;
        }
        
        .divisions-list {
            margin-bottom: 30px;
        }
        
        .division-item {
            background: #f8f9fa;
            border: 2px solid #e9ecef;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 15px;
            cursor: pointer;
            transition: all 0.3s ease;
            display: flex;
            align-items: center;
        }
        
        .division-item:hover {
            background: #e3f2fd;
            border-color: #3498db;
            transform: translateX(5px);
        }
        
        .division-item.selected {
            background: #d4edda;
            border-color: #28a745;
        }
        
        .division-radio {
            margin-right: 15px;
            transform: scale(1.3);
            accent-color: #3498db;
        }
        
        .division-info h3 {
            color: #2c3e50;
            margin-bottom: 5px;
            font-size: 18px;
        }
        
        .division-info p {
            color: #6c757d;
            font-size: 14px;
        }
        
        .no-divisions {
            text-align: center;
            padding: 40px;
            color: #6c757d;
        }
        
        .no-divisions i {
            font-size: 48px;
            margin-bottom: 15px;
            color: #bdc3c7;
        }
        
        .btn-select {
            width: 100%;
            padding: 18px;
            background: linear-gradient(135deg, #3498db 0%, #2980b9 100%);
            color: white;
            border: none;
            border-radius: 12px;
            font-size: 18px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 10px;
        }
        
        .btn-select:hover {
            background: linear-gradient(135deg, #2980b9 0%, #1c639b 100%);
            transform: translateY(-2px);
            box-shadow: 0 7px 14px rgba(41, 128, 185, 0.3);
        }
        
        .btn-select:disabled {
            background: #bdc3c7;
            cursor: not-allowed;
            transform: none;
        }
        
        .btn-select i {
            font-size: 20px;
        }
        
        .logout-link {
            text-align: center;
            margin-top: 20px;
        }
        
        .logout-link a {
            color: #7f8c8d;
            text-decoration: none;
            font-size: 14px;
            transition: color 0.3s;
        }
        
        .logout-link a:hover {
            color: #e74c3c;
        }
        
        /* Для администратора - кнопка создания подразделения */
        .admin-actions {
            margin-top: 25px;
            padding-top: 25px;
            border-top: 1px solid #eee;
        }
        
        .btn-create {
            width: 100%;
            padding: 15px;
            background: #27ae60;
            color: white;
            border: none;
            border-radius: 10px;
            font-size: 16px;
            cursor: pointer;
            transition: all 0.3s ease;
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 10px;
        }
        
        .btn-create:hover {
            background: #219653;
        }
        
        /* Модальное окно создания подразделения */
        .modal {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
            z-index: 1000;
            justify-content: center;
            align-items: center;
        }
        
        .modal-content {
            background: white;
            padding: 30px;
            border-radius: 15px;
            width: 90%;
            max-width: 500px;
            animation: modalAppear 0.3s ease;
        }
        
        @keyframes modalAppear {
            from {
                opacity: 0;
                transform: scale(0.9);
            }
            to {
                opacity: 1;
                transform: scale(1);
            }
        }
        
        .modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }
        
        .modal-header h2 {
            color: #2c3e50;
        }
        
        .close-modal {
            background: none;
            border: none;
            font-size: 24px;
            color: #7f8c8d;
            cursor: pointer;
            padding: 5px;
        }
        
        .close-modal:hover {
            color: #e74c3c;
        }
        
        .form-group {
            margin-bottom: 20px;
        }
        
        .form-group label {
            display: block;
            margin-bottom: 8px;
            color: #2c3e50;
            font-weight: 500;
        }
        
        .form-group input,
        .form-group textarea {
            width: 100%;
            padding: 12px;
            border: 2px solid #e0e0e0;
            border-radius: 8px;
            font-size: 16px;
            transition: border-color 0.3s;
        }
        
        .form-group input:focus,
        .form-group textarea:focus {
            border-color: #3498db;
            outline: none;
        }
        
        .form-group textarea {
            min-height: 100px;
            resize: vertical;
        }
        
        .modal-actions {
            display: flex;
            gap: 10px;
            margin-top: 25px;
        }
        
        .btn-primary {
            flex: 1;
            padding: 12px;
            background: #3498db;
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            cursor: pointer;
            transition: background 0.3s;
        }
        
        .btn-primary:hover {
            background: #2980b9;
        }
        
        .btn-secondary {
            flex: 1;
            padding: 12px;
            background: #95a5a6;
            color: white;
            border: none;
            border-radius: 8px;
            font-size: 16px;
            cursor: pointer;
            transition: background 0.3s;
        }
        
        .btn-secondary:hover {
            background: #7f8c8d;
        }
        
        /* Адаптивность */
        @media (max-width: 768px) {
            .selection-container {
                padding: 25px;
                margin: 10px;
            }
            
            .header h1 {
                font-size: 24px;
            }
            
            .division-item {
                padding: 15px;
            }
        }
        
        @media (max-width: 480px) {
            body {
                padding: 10px;
            }
            
            .selection-container {
                padding: 20px;
                border-radius: 15px;
            }
            
            .modal-content {
                padding: 20px;
                width: 95%;
            }
        }
    </style>
    <!-- Иконки Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
</head>
<body>
    <div class="selection-container">
        <div class="header">
            <h1><i class="fas fa-store"></i> Выбор подразделения</h1>
            <div class="user-info">
                Вы вошли как: <strong><?php echo htmlspecialchars($_SESSION['user_name']); ?></strong>
                <br>
                <?php if ($_SESSION['permission_group'] === 'admin'): ?>
                <span style="color: #e74c3c;"><i class="fas fa-crown"></i> Администратор</span>
                <?php else: ?>
                <span style="color: #3498db;"><i class="fas fa-user"></i> Пользователь</span>
                <?php endif; ?>
            </div>
        </div>
        
        <div class="welcome-text">
            <i class="fas fa-map-marker-alt"></i> Выберите подразделение для работы
        </div>
        
        <?php if (isset($error)): ?>
        <div class="error-message">
            <i class="fas fa-exclamation-circle"></i> <?php echo htmlspecialchars($error); ?>
        </div>
        <?php endif; ?>
        
        <form method="POST" action="" id="divisionForm">
            <div class="divisions-list">
                <?php if (count($divisions) > 0): ?>
                    <?php foreach ($divisions as $division): ?>
                    <label class="division-item" for="division_<?php echo $division['id']; ?>">
                        <input type="radio" 
                               name="division_id" 
                               id="division_<?php echo $division['id']; ?>" 
                               value="<?php echo $division['id']; ?>" 
                               class="division-radio" 
                               required>
                        <div class="division-info">
                            <h3><i class="fas fa-building"></i> <?php echo htmlspecialchars($division['name']); ?></h3>
                            <?php if (!empty($division['address'])): ?>
                            <p><i class="fas fa-map-pin"></i> <?php echo htmlspecialchars($division['address']); ?></p>
                            <?php endif; ?>
                            <p><small>ID: <?php echo $division['id']; ?></small></p>
                        </div>
                    </label>
                    <?php endforeach; ?>
                <?php else: ?>
                    <div class="no-divisions">
                        <i class="fas fa-store-slash"></i>
                        <h3>Подразделения не найдены</h3>
                        <p>Для начала работы необходимо создать подразделение</p>
                    </div>
                <?php endif; ?>
            </div>
            
            <button type="submit" class="btn-select" id="selectButton" <?php echo count($divisions) === 0 ? 'disabled' : ''; ?>>
                <i class="fas fa-check-circle"></i> Продолжить
            </button>
        </form>
        
        <?php if ($_SESSION['permission_group'] === 'admin'): ?>
        <div class="admin-actions">
            <button type="button" class="btn-create" onclick="openCreateModal()">
                <i class="fas fa-plus-circle"></i> Создать новое подразделение
            </button>
        </div>
        <?php endif; ?>
        
        <div class="logout-link">
            <a href="logout.php"><i class="fas fa-sign-out-alt"></i> Выйти из системы</a>
        </div>
    </div>
    
    <!-- Модальное окно создания подразделения (только для админа) -->
    <?php if ($_SESSION['permission_group'] === 'admin'): ?>
    <div id="createModal" class="modal">
        <div class="modal-content">
            <div class="modal-header">
                <h2><i class="fas fa-plus-circle"></i> Создать подразделение</h2>
                <button class="close-modal" onclick="closeCreateModal()">&times;</button>
            </div>
            
            <form id="createDivisionForm">
                <div class="form-group">
                    <label for="division_name"><i class="fas fa-signature"></i> Название подразделения *</label>
                    <input type="text" id="division_name" name="division_name" required placeholder="Например: Магазин 'Центральный'">
                </div>
                
                <div class="form-group">
                    <label for="division_address"><i class="fas fa-map-marker-alt"></i> Адрес</label>
                    <textarea id="division_address" name="division_address" placeholder="Введите адрес подразделения..."></textarea>
                </div>
                
                <div class="modal-actions">
                    <button type="button" class="btn-secondary" onclick="closeCreateModal()">Отмена</button>
                    <button type="submit" class="btn-primary">Создать</button>
                </div>
            </form>
        </div>
    </div>
    <?php endif; ?>
    
    <script>
        // Выбор подразделения при клике на весь блок
        document.querySelectorAll('.division-item').forEach(item => {
            item.addEventListener('click', function(e) {
                if (!e.target.matches('input[type="radio"]')) {
                    const radio = this.querySelector('input[type="radio"]');
                    radio.checked = true;
                    
                    // Снимаем выделение с других
                    document.querySelectorAll('.division-item').forEach(other => {
                        other.classList.remove('selected');
                    });
                    
                    // Выделяем текущий
                    this.classList.add('selected');
                }
            });
        });
        
        // Активация кнопки при выборе
        document.querySelectorAll('input[name="division_id"]').forEach(radio => {
            radio.addEventListener('change', function() {
                document.getElementById('selectButton').disabled = false;
                
                // Выделяем выбранный элемент
                document.querySelectorAll('.division-item').forEach(item => {
                    item.classList.remove('selected');
                    if (item.querySelector('input[type="radio"]').checked) {
                        item.classList.add('selected');
                    }
                });
            });
        });
        
        // Модальное окно создания подразделения (для админа)
        <?php if ($_SESSION['permission_group'] === 'admin'): ?>
        function openCreateModal() {
            document.getElementById('createModal').style.display = 'flex';
            document.getElementById('division_name').focus();
        }
        
        function closeCreateModal() {
            document.getElementById('createModal').style.display = 'none';
            document.getElementById('createDivisionForm').reset();
        }
        
        // Обработка создания подразделения
        document.getElementById('createDivisionForm').addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const formData = new FormData(this);
            const data = {
                name: formData.get('division_name'),
                address: formData.get('division_address')
            };
            
            try {
                const response = await fetch('api/create_division.php', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(data)
                });
                
                const result = await response.json();
                
                if (result.success) {
                    alert('Подразделение успешно создано!');
                    closeCreateModal();
                    location.reload(); // Перезагружаем страницу для показа нового подразделения
                } else {
                    alert('Ошибка: ' + result.error);
                }
            } catch (error) {
                alert('Ошибка сети: ' + error.message);
            }
        });
        
        // Закрытие модального окна по клику вне его
        document.getElementById('createModal').addEventListener('click', function(e) {
            if (e.target === this) {
                closeCreateModal();
            }
        });
        <?php endif; ?>
        
        // Автовыбор, если подразделение только одно
        document.addEventListener('DOMContentLoaded', function() {
            const divisions = document.querySelectorAll('input[name="division_id"]');
            if (divisions.length === 1) {
                divisions[0].checked = true;
                document.getElementById('selectButton').disabled = false;
                
                // Выделяем выбранный элемент
                divisions[0].closest('.division-item').classList.add('selected');
                
                // Автоматически отправляем форму через 3 секунды
                setTimeout(() => {
                    document.getElementById('divisionForm').submit();
                }, 3000);
            }
        });
        
        // Эффект нажатия на кнопку
        document.getElementById('selectButton')?.addEventListener('click', function() {
            this.style.transform = 'scale(0.98)';
            setTimeout(() => {
                this.style.transform = '';
            }, 150);
        });
    </script>
</body>
</html>