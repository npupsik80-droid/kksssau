<?php
session_start();
require_once './includes/auth_check.php'; // подключает PDO и проверяет авторизацию

define('FIELD_WIDTH', 15);
define('FIELD_HEIGHT', 15);
define('START_LENGTH', 3);

$SPEED_LEVELS = [
    1 => ['delay' => 500, 'mult' => 1],
    2 => ['delay' => 400, 'mult' => 2],
    3 => ['delay' => 300, 'mult' => 3],
    4 => ['delay' => 200, 'mult' => 4],
    5 => ['delay' => 100, 'mult' => 5]
];

// Сохранение результата в БД
function save_score($user_id, $score, $speed_level, $multiplier) {
    global $pdo;
    $final_score = $score * $multiplier;
    $stmt = $pdo->prepare("INSERT INTO snake_scores (user_id, score, speed_level, multiplier) VALUES (?, ?, ?, ?)");
    return $stmt->execute([$user_id, $final_score, $speed_level, $multiplier]);
}

// Получение таблицы рекордов
function get_scores($user_id) {
    global $pdo;
    // Личные рекорды (топ-10)
    $stmt = $pdo->prepare("SELECT score, speed_level, multiplier, achieved_at FROM snake_scores WHERE user_id = ? ORDER BY score DESC LIMIT 10");
    $stmt->execute([$user_id]);
    $personal = $stmt->fetchAll(PDO::FETCH_ASSOC);

    // Общий топ-10 (лучший результат каждого пользователя)
    $stmt = $pdo->query("
        SELECT s.user_id, MAX(s.score) as best_score, u.full_name 
        FROM snake_scores s 
        JOIN users u ON s.user_id = u.id 
        GROUP BY s.user_id 
        ORDER BY best_score DESC 
        LIMIT 10
    ");
    $global = $stmt->fetchAll(PDO::FETCH_ASSOC);
    $global = array_map(function($row) {
        return ['score' => $row['best_score'], 'full_name' => $row['full_name']];
    }, $global);

    return ['personal' => $personal, 'global' => $global];
}

// Обработка AJAX-запросов
function handle_action($action) {
    global $SPEED_LEVELS, $pdo, $_SESSION;

    $response = ['success' => false];

    switch ($action) {
        case 'save_score':
            if (!isset($_SESSION['user_id'])) {
                $response['error'] = 'Не авторизован';
                break;
            }
            $score = (int)($_POST['score'] ?? 0);
            $speed_level = (int)($_POST['speed_level'] ?? 3);
            if (!isset($SPEED_LEVELS[$speed_level])) {
                $response['error'] = 'Неверный уровень скорости';
                break;
            }
            $mult = $SPEED_LEVELS[$speed_level]['mult'];
            if (save_score($_SESSION['user_id'], $score, $speed_level, $mult)) {
                $response['success'] = true;
                $response['saved'] = true;
            } else {
                $response['error'] = 'Ошибка сохранения';
            }
            session_write_close();
            break;

        case 'get_scores':
            if (!isset($_SESSION['user_id'])) {
                $response['error'] = 'Не авторизован';
                break;
            }
            $scores = get_scores($_SESSION['user_id']);
            $response = array_merge(['success' => true], $scores);
            session_write_close();
            break;
    }

    return $response;
}

// Если это AJAX-запрос
if (isset($_REQUEST['action'])) {
    header('Content-Type: application/json');
    echo json_encode(handle_action($_REQUEST['action']));
    exit;
}
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Змейка · RunaRMK</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <style>
        /* === Переменные и базовые стили из new_check.php === */
        :root {
            --primary: #4361ee;
            --secondary: #2ecc71;
            --danger: #e74c3c;
            --warning: #f39c12;
            --light: #f8f9fa;
            --dark: #2c3e50;
            --gray: #95a5a6;
            --light-blue: #e9f2ff;
            --border-radius: 12px;
            --shadow-sm: 0 2px 8px rgba(0,0,0,0.08);
            --shadow-md: 0 4px 12px rgba(0,0,0,0.12);
            --shadow-lg: 0 8px 24px rgba(0,0,0,0.15);
        }
        
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        }
        
        body {
            background: linear-gradient(135deg, #f5f7fa 0%, #e4e8f0 100%);
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: flex-start;
            padding: 0;
        }
        
        /* Шапка как в new_check.php */
        .header {
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            padding: 20px 30px;
            box-shadow: var(--shadow-md);
            display: flex;
            justify-content: space-between;
            align-items: center;
            position: sticky;
            top: 0;
            z-index: 1000;
            border-bottom: 4px solid rgba(255,255,255,0.1);
            width: 100%;
        }
        
        .header h1 {
            color: white;
            font-size: 24px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 12px;
            text-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        
        .user-info {
            display: flex;
            gap: 24px;
            align-items: center;
            background: rgba(255,255,255,0.15);
            padding: 10px 20px;
            border-radius: 50px;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255,255,255,0.2);
        }
        
        .user-info span {
            color: white;
            font-weight: 500;
            display: flex;
            align-items: center;
            gap: 8px;
            font-size: 14px;
        }
        
        .game-wrapper {
            width: 100%;
            max-width: 1600px;
            display: grid;
            grid-template-columns: 300px 1fr 350px;
            gap: 25px;
            background: transparent;
            margin: 25px auto;
            padding: 0 20px;
        }
        
        .scores-panel {
            background: white;
            border-radius: var(--border-radius);
            padding: 20px;
            box-shadow: var(--shadow-md);
            border: 1px solid rgba(0,0,0,0.05);
            display: flex;
            flex-direction: column;
            gap: 20px;
        }
        
        .scores-panel h3 {
            color: var(--dark);
            font-size: 18px;
            font-weight: 700;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 8px;
            padding-bottom: 8px;
            border-bottom: 2px solid var(--light);
        }
        
        .score-table {
            width: 100%;
            border-collapse: collapse;
        }
        
        .score-table th {
            text-align: left;
            font-size: 12px;
            color: var(--gray);
            padding-bottom: 8px;
        }
        
        .score-table td {
            padding: 6px 0;
            font-size: 14px;
        }
        
        .score-table .rank {
            font-weight: 700;
            color: var(--primary);
            width: 30px;
        }
        
        .score-table .name {
            color: var(--dark);
            font-weight: 500;
        }
        
        .score-table .score {
            font-weight: 800;
            color: var(--secondary);
            text-align: right;
        }
        
        .field-panel {
            background: white;
            border-radius: var(--border-radius);
            padding: 25px;
            box-shadow: var(--shadow-md);
            border: 1px solid rgba(0,0,0,0.05);
            display: flex;
            flex-direction: column;
            align-items: center;
        }
        
        .field-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            width: 100%;
            margin-bottom: 20px;
            padding-bottom: 15px;
            border-bottom: 2px solid var(--light);
        }
        
        .field-header h2 {
            color: var(--dark);
            font-size: 24px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .score-badge {
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            color: white;
            padding: 8px 20px;
            border-radius: 30px;
            font-weight: 700;
            font-size: 18px;
            box-shadow: var(--shadow-sm);
        }
        
        .snake-field {
            border-collapse: collapse;
            margin: 0 auto;
            background: #1e293b;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: var(--shadow-lg);
        }
        
        .snake-field td {
            width: 32px;
            height: 32px;
            border: 1px solid #334155;
            transition: all 0.1s ease;
        }
        
        .snake {
            background: linear-gradient(135deg, var(--secondary) 0%, #27ae60 100%);
            box-shadow: inset 0 -2px 5px rgba(0,0,0,0.2);
        }
        
        .snake-head {
            background: linear-gradient(135deg, #f1c40f 0%, #f39c12 100%);
            box-shadow: 0 0 15px #f1c40f;
        }
        
        .food {
            background: radial-gradient(circle, var(--danger) 0%, #c0392b 100%);
            border-radius: 50%;
            animation: pulse 1s infinite;
        }
        
        @keyframes pulse {
            0% { transform: scale(0.8); opacity: 0.8; }
            50% { transform: scale(1.1); opacity: 1; }
            100% { transform: scale(0.8); opacity: 0.8; }
        }
        
        .empty {
            background: #0f172a;
        }
        
        .control-panel {
            background: white;
            border-radius: var(--border-radius);
            padding: 25px;
            box-shadow: var(--shadow-md);
            border: 1px solid rgba(0,0,0,0.05);
            display: flex;
            flex-direction: column;
            gap: 25px;
        }
        
        .speed-card, .actions-card, .stats-card {
            background: var(--light);
            border-radius: var(--border-radius);
            padding: 20px;
            box-shadow: var(--shadow-sm);
        }
        
        .speed-card h3, .actions-card h3, .stats-card h3 {
            color: var(--dark);
            font-size: 18px;
            font-weight: 700;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
            padding-bottom: 10px;
            border-bottom: 2px solid #d1d9e6;
        }
        
        .speed-option {
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 12px 0;
            border-bottom: 1px solid #e2e8f0;
        }
        
        .speed-option:last-child {
            border-bottom: none;
        }
        
        .speed-option label {
            display: flex;
            align-items: center;
            gap: 10px;
            cursor: pointer;
            font-weight: 500;
            color: var(--dark);
        }
        
        .speed-option input[type="radio"] {
            width: 18px;
            height: 18px;
            accent-color: var(--primary);
            cursor: pointer;
        }
        
        .multiplier {
            background: var(--warning);
            color: white;
            padding: 4px 10px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 700;
        }
        
        .btn {
            padding: 16px;
            border: none;
            border-radius: var(--border-radius);
            font-size: 16px;
            font-weight: 700;
            cursor: pointer;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            letter-spacing: 0.5px;
            width: 100%;
            margin-bottom: 12px;
        }
        
        .btn-primary {
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            color: white;
        }
        
        .btn-primary:hover {
            transform: translateY(-3px);
            box-shadow: var(--shadow-lg);
        }
        
        .btn-success {
            background: linear-gradient(135deg, var(--secondary) 0%, #27ae60 100%);
            color: white;
        }
        
        .btn-success:hover {
            transform: translateY(-3px);
            box-shadow: var(--shadow-lg);
        }
        
        .btn-warning {
            background: linear-gradient(135deg, var(--warning) 0%, #e67e22 100%);
            color: white;
        }
        
        .btn-warning:hover {
            transform: translateY(-3px);
            box-shadow: var(--shadow-lg);
        }
        
        .stats-row {
            display: flex;
            justify-content: space-between;
            padding: 12px 0;
            border-bottom: 1px solid #e2e8f0;
            font-size: 16px;
        }
        
        .stats-row:last-child {
            border-bottom: none;
        }
        
        .stats-label {
            font-weight: 600;
            color: #64748b;
        }
        
        .stats-value {
            font-weight: 800;
            color: var(--dark);
        }
        
        .status-message {
            text-align: center;
            padding: 12px;
            border-radius: 8px;
            font-weight: 600;
            margin-top: 10px;
        }
        
        .status-message.game-over {
            background: #fee2e2;
            color: var(--danger);
        }
        
        .status-message.win {
            background: #dcfce7;
            color: var(--secondary);
        }

        .countdown-overlay {
            position: fixed;
            top: 0; left: 0; right: 0; bottom: 0;
            background: rgba(0,0,0,0.6);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 9999;
            font-size: 100px;
            color: white;
            font-weight: 800;
            text-shadow: 0 0 20px var(--primary);
            backdrop-filter: blur(5px);
        }

        /* Адаптивность */
        @media (max-width: 1200px) {
            .game-wrapper {
                grid-template-columns: 1fr;
                gap: 20px;
            }
            
            .snake-field td {
                width: 24px;
                height: 24px;
            }
        }
    </style>
</head>
<body>
    <!-- Шапка как в new_check.php -->
    <div class="header">
        <h1><i class="fas fa-gamepad"></i> Змейка</h1>
        <div class="user-info">
            <span><i class="fas fa-user-circle"></i> <?php echo htmlspecialchars($_SESSION['user_name'] ?? 'Пользователь'); ?></span>
            <span><i class="fas fa-store-alt"></i> <?php echo htmlspecialchars($_SESSION['current_division_name'] ?? 'Подразделение'); ?></span>
            <a href="../../index.php" style="color: white; text-decoration: none; display: flex; align-items: center; gap: 8px;">
                <i class="fas fa-arrow-left"></i> На главную
            </a>
        </div>
    </div>

    <div class="game-wrapper">
        <!-- Левая колонка: таблица рекордов -->
        <div class="scores-panel">
            <h3><i class="fas fa-trophy" style="color: var(--warning);"></i> Личные рекорды</h3>
            <table class="score-table" id="personalScores">
                <thead>
                    <tr><th>#</th><th>Скорость</th><th>Счёт</th></tr>
                </thead>
                <tbody id="personalScoresBody">
                    <tr><td colspan="3" style="text-align:center;">Загрузка...</td></tr>
                </tbody>
            </table>
            
            <h3 style="margin-top: 20px;"><i class="fas fa-globe"></i> Топ-10 сотрудников</h3>
            <table class="score-table" id="globalScores">
                <thead>
                    <tr><th>#</th><th>Имя</th><th>Счёт</th></tr>
                </thead>
                <tbody id="globalScoresBody">
                    <tr><td colspan="3" style="text-align:center;">Загрузка...</td></tr>
                </tbody>
            </table>
        </div>
        
        <!-- Центральная колонка: игровое поле -->
        <div class="field-panel">
            <div class="field-header">
                <h2><i class="fas fa-gamepad" style="color: var(--primary);"></i> Snake</h2>
                <div class="score-badge" id="scoreDisplay">0</div>
            </div>
            <div id="gameField"></div>
            <div style="margin-top: 15px; font-size: 14px; color: var(--gray);">
                Управление: стрелки на клавиатуре
            </div>
        </div>
        
        <!-- Правая колонка: управление -->
        <div class="control-panel">
            <div class="speed-card">
                <h3><i class="fas fa-tachometer-alt"></i> Скорость</h3>
                <div class="speed-option">
                    <label><input type="radio" name="speed" value="1"> 500 мс</label>
                    <span class="multiplier">×1</span>
                </div>
                <div class="speed-option">
                    <label><input type="radio" name="speed" value="2"> 400 мс</label>
                    <span class="multiplier">×2</span>
                </div>
                <div class="speed-option">
                    <label><input type="radio" name="speed" value="3" checked> 300 мс</label>
                    <span class="multiplier">×3</span>
                </div>
                <div class="speed-option">
                    <label><input type="radio" name="speed" value="4"> 200 мс</label>
                    <span class="multiplier">×4</span>
                </div>
                <div class="speed-option">
                    <label><input type="radio" name="speed" value="5"> 100 мс</label>
                    <span class="multiplier">×5</span>
                </div>
            </div>
            
            <div class="actions-card">
                <h3><i class="fas fa-cog"></i> Управление</h3>
                <button class="btn btn-primary" id="newGameBtn"><i class="fas fa-play"></i> Новая игра</button>
                <button class="btn btn-success" id="restartBtn"><i class="fas fa-redo-alt"></i> Рестарт</button>
            </div>
            
            <div class="stats-card">
                <h3><i class="fas fa-chart-line"></i> Статистика</h3>
                <div class="stats-row">
                    <span class="stats-label">Базовые очки</span>
                    <span class="stats-value" id="baseScore">0</span>
                </div>
                <div class="stats-row">
                    <span class="stats-label">Множитель</span>
                    <span class="stats-value" id="multiplier">x3</span>
                </div>
                <div class="stats-row">
                    <span class="stats-label">Итоговый счёт</span>
                    <span class="stats-value" id="finalScore">0</span>
                </div>
                <div id="statusMessage" class="status-message"></div>
            </div>
        </div>
    </div>

    <script>
        // Глобальные переменные
        let gameInterval = null;
        let gameActive = false;
        let countdownActive = false;
        let currentSpeed = 300;
        let currentSpeedLevel = 3;
        let currentMultiplier = 3;
        let baseScore = 0;
        
        const width = 15;
        const height = 15;
        const startLength = 3;

        let snake = [];
        let direction = 1; // 0=up,1=right,2=down,3=left
        let nextDirection = 1;
        let food = [0,0];
        let gameOver = false;
        let win = false;

        // DOM элементы
        const gameFieldDiv = document.getElementById('gameField');
        const scoreDisplay = document.getElementById('scoreDisplay');
        const baseScoreSpan = document.getElementById('baseScore');
        const multiplierSpan = document.getElementById('multiplier');
        const finalScoreSpan = document.getElementById('finalScore');
        const statusMessageDiv = document.getElementById('statusMessage');
        const personalBody = document.getElementById('personalScoresBody');
        const globalBody = document.getElementById('globalScoresBody');

        // Функция обновления множителя при смене скорости
        function changeSpeed(level) {
            currentSpeedLevel = parseInt(level);
            const speedMap = {1:500, 2:400, 3:300, 4:200, 5:100};
            const multMap = {1:1, 2:2, 3:3, 4:4, 5:5};
            currentSpeed = speedMap[level];
            currentMultiplier = multMap[level];
            multiplierSpan.textContent = 'x' + currentMultiplier;
            if (gameActive) {
                stopGame();
                startGame();
            }
        }

        // Привязка radio
        document.querySelectorAll('input[name="speed"]').forEach(radio => {
            radio.addEventListener('change', (e) => changeSpeed(e.target.value));
        });

        // Генерация случайной еды
        function generateFood() {
            const snakeSet = new Set(snake.map(seg => seg[0]+','+seg[1]));
            for (let attempt = 0; attempt < 1000; attempt++) {
                const x = Math.floor(Math.random() * width);
                const y = Math.floor(Math.random() * height);
                if (!snakeSet.has(x+','+y)) {
                    return [x, y];
                }
            }
            return null;
        }

        // Инициализация новой игры
        function initGame() {
            const cx = Math.floor(width / 2);
            const cy = Math.floor(height / 2);
            snake = [];
            for (let i = 0; i < startLength; i++) {
                snake.push([cx - i, cy]);
            }
            snake = snake.map(seg => [Math.max(0, seg[0]), seg[1]]);
            direction = 1;
            nextDirection = 1;
            gameOver = false;
            win = false;
            baseScore = 0;
            const newFood = generateFood();
            if (newFood) {
                food = newFood;
            } else {
                win = true;
                food = [0,0];
            }
            updateScore();
        }

        // Проверка столкновения
        function checkCollision(newHead) {
            if (newHead[0] < 0 || newHead[0] >= width || newHead[1] < 0 || newHead[1] >= height) return true;
            for (let i = 0; i < snake.length; i++) {
                if (snake[i][0] === newHead[0] && snake[i][1] === newHead[1]) return true;
            }
            return false;
        }

        // Шаг игры
        function stepGame() {
            if (!gameActive || gameOver || win || countdownActive) return;

            // Разворот запрещён
            if ((direction === 0 && nextDirection !== 2) ||
                (direction === 2 && nextDirection !== 0) ||
                (direction === 1 && nextDirection !== 3) ||
                (direction === 3 && nextDirection !== 1)) {
                direction = nextDirection;
            }

            const head = snake[0];
            let newHead = [...head];
            switch (direction) {
                case 0: newHead[1]--; break;
                case 1: newHead[0]++; break;
                case 2: newHead[1]++; break;
                case 3: newHead[0]--; break;
            }

            if (checkCollision(newHead)) {
                gameOver = true;
                gameActive = false;
                stopGame();
                renderGameField();
                updateStatus(true, false);
                saveScore(baseScore, currentSpeedLevel);
                return;
            }

            const eaten = (newHead[0] === food[0] && newHead[1] === food[1]);

            snake.unshift(newHead);
            if (!eaten) {
                snake.pop();
            }

            if (eaten) {
                baseScore++;
                const newFood = generateFood();
                if (newFood) {
                    food = newFood;
                } else {
                    win = true;
                    gameActive = false;
                    stopGame();
                    updateStatus(false, true);
                    saveScore(baseScore, currentSpeedLevel);
                }
            }

            updateScore();
            renderGameField();

            if (gameOver || win) {
                stopGame();
            }
        }

        // Обновление счёта
        function updateScore() {
            baseScoreSpan.textContent = baseScore;
            const final = baseScore * currentMultiplier;
            finalScoreSpan.textContent = final;
            scoreDisplay.textContent = final;
        }

        // Отрисовка поля
        function renderGameField() {
            if (!gameFieldDiv.querySelector('table')) {
                let html = '<table class="snake-field" id="snakeTable">';
                for (let y = 0; y < height; y++) {
                    html += '<tr>';
                    for (let x = 0; x < width; x++) {
                        html += `<td id="cell-${x}-${y}" class="empty"></td>`;
                    }
                    html += '</tr>';
                }
                html += '</table>';
                gameFieldDiv.innerHTML = html;
            }

            for (let y = 0; y < height; y++) {
                for (let x = 0; x < width; x++) {
                    const cell = document.getElementById(`cell-${x}-${y}`);
                    if (cell) cell.className = 'empty';
                }
            }

            snake.forEach((seg, index) => {
                const [x, y] = seg;
                const cell = document.getElementById(`cell-${x}-${y}`);
                if (cell) cell.className = index === 0 ? 'snake-head' : 'snake';
            });

            const [fx, fy] = food;
            const foodCell = document.getElementById(`cell-${fx}-${fy}`);
            if (foodCell) foodCell.className = 'food';
        }

        // Обновление статуса
        function updateStatus(game_over, win_flag) {
            if (game_over) {
                statusMessageDiv.innerHTML = '<i class="fas fa-skull"></i> Game Over';
                statusMessageDiv.className = 'status-message game-over';
            } else if (win_flag) {
                statusMessageDiv.innerHTML = '<i class="fas fa-trophy"></i> Победа!';
                statusMessageDiv.className = 'status-message win';
            } else {
                statusMessageDiv.innerHTML = '';
            }
        }

        // Сохранение результата
        function saveScore(score, speedLevel) {
            const formData = new FormData();
            formData.append('action', 'save_score');
            formData.append('score', score);
            formData.append('speed_level', speedLevel);

            fetch('zmey.php', { method: 'POST', body: formData })
                .then(r => r.json())
                .then(data => {
                    if (data.success) {
                        loadScores();
                    }
                });
        }

        // Загрузка таблицы рекордов
        function loadScores() {
            fetch('zmey.php?action=get_scores')
                .then(r => r.json())
                .then(data => {
                    if (!data.success) return;
                    let personalHtml = '';
                    if (data.personal.length === 0) {
                        personalHtml = '<tr><td colspan="3" style="text-align:center;">Нет результатов</td></tr>';
                    } else {
                        data.personal.forEach((row, idx) => {
                            personalHtml += `<tr>
                                <td class="rank">${idx+1}</td>
                                <td>${row.speed_level} (x${row.multiplier})</td>
                                <td class="score">${row.score}</td>
                            </tr>`;
                        });
                    }
                    personalBody.innerHTML = personalHtml;

                    let globalHtml = '';
                    if (data.global.length === 0) {
                        globalHtml = '<tr><td colspan="3" style="text-align:center;">Нет результатов</td></tr>';
                    } else {
                        data.global.forEach((row, idx) => {
                            globalHtml += `<tr>
                                <td class="rank">${idx+1}</td>
                                <td class="name">${row.full_name || 'Аноним'}</td>
                                <td class="score">${row.score}</td>
                            </tr>`;
                        });
                    }
                    globalBody.innerHTML = globalHtml;
                });
        }

        // Установка направления
        function setDirection(dir) {
            if (!gameActive || gameOver || win || countdownActive) return;
            if ((direction === 0 && dir !== 2) ||
                (direction === 2 && dir !== 0) ||
                (direction === 1 && dir !== 3) ||
                (direction === 3 && dir !== 1)) {
                nextDirection = dir;
            }
        }

        // Управление стрелками
        document.addEventListener('keydown', e => {
            const keyMap = { 'ArrowUp': 0, 'ArrowRight': 1, 'ArrowDown': 2, 'ArrowLeft': 3 };
            if (keyMap.hasOwnProperty(e.key)) {
                e.preventDefault();
                setDirection(keyMap[e.key]);
            }
        });

        // Обратный отсчёт
        function startCountdown(callback) {
            if (countdownActive) return;
            countdownActive = true;
            stopGame();

            const overlay = document.createElement('div');
            overlay.className = 'countdown-overlay';
            overlay.id = 'countdownOverlay';
            document.body.appendChild(overlay);

            let count = 3;
            overlay.textContent = count;

            const interval = setInterval(() => {
                count--;
                if (count > 0) {
                    overlay.textContent = count;
                } else if (count === 0) {
                    overlay.textContent = 'GO!';
                } else {
                    clearInterval(interval);
                    document.body.removeChild(overlay);
                    countdownActive = false;
                    callback();
                }
            }, 800);
        }

        // Новая игра
        function newGame() {
            stopGame();
            initGame();
            renderGameField();
            updateStatus(false, false);
            document.body.focus();
            startCountdown(() => {
                gameActive = true;
                startGame();
            });
        }

        function restartGame() { newGame(); }

        function startGame() {
            stopGame();
            gameInterval = setInterval(stepGame, currentSpeed);
        }

        function stopGame() {
            if (gameInterval) clearInterval(gameInterval);
            gameInterval = null;
        }

        // Кнопки
        document.getElementById('newGameBtn').addEventListener('click', newGame);
        document.getElementById('restartBtn').addEventListener('click', restartGame);

        // Инициализация
        window.onload = () => {
            initGame();
            renderGameField();
            loadScores();
            document.body.focus();
        };
    </script>
</body>
</html>