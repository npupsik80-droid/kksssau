<?php
require_once '../../includes/auth_check.php';

if ($_SESSION['permission_group'] !== 'admin') {
    header('Location: ../../index.php');
    exit();
}
?>

<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Устройства ККТ - RunaRMK</title>
    <!-- Обязательно для работы расширения KKM Server -->
    <script>var KkmServerAddIn = {};</script>
    <style>
        :root {
            --primary: #4361ee;
            --secondary: #2ecc71;
            --danger: #e74c3c;
            --warning: #f39c12;
            --info: #3498db;
            --light: #f8f9fa;
            --dark: #2c3e50;
            --gray: #95a5a6;
            --light-blue: #e9f2ff;
            --border-radius: 16px;
            --shadow-sm: 0 4px 12px rgba(0,0,0,0.05);
            --shadow-md: 0 8px 24px rgba(0,0,0,0.1);
            --shadow-lg: 0 12px 32px rgba(0,0,0,0.15);
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
            color: #333;
        }
        
        .header {
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            padding: 25px 35px;
            box-shadow: var(--shadow-lg);
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-bottom: 4px solid rgba(255,255,255,0.15);
            margin-bottom: 35px;
        }
        
        .header h1 {
            color: white;
            font-size: 32px;
            font-weight: 800;
            display: flex;
            align-items: center;
            gap: 15px;
            text-shadow: 0 4px 8px rgba(0,0,0,0.2);
        }
        
        .header h1 i {
            background: rgba(255,255,255,0.15);
            padding: 18px;
            border-radius: 18px;
            backdrop-filter: blur(10px);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .user-panel {
            display: flex;
            align-items: center;
            gap: 25px;
        }
        
        .user-panel span {
            color: white;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 10px;
            font-size: 15px;
        }
        
        .btn {
            padding: 14px 28px;
            border-radius: 50px;
            font-weight: 700;
            font-size: 15px;
            cursor: pointer;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            border: none;
            letter-spacing: 0.3px;
        }
        
        .btn-primary {
            background: linear-gradient(135deg, var(--primary) 0%, #3a56d4 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(67, 97, 238, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-primary:hover {
            transform: translateY(-3px);
            box-shadow: 0 12px 25px rgba(67, 97, 238, 0.4);
        }
        
        .btn-secondary {
            background: linear-gradient(135deg, var(--secondary) 0%, #27ae60 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(46, 204, 113, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-info {
            background: linear-gradient(135deg, var(--info) 0%, #2980b9 100%);
            color: white;
            box-shadow: 0 6px 20px rgba(52, 152, 219, 0.3);
            border: 2px solid rgba(255,255,255,0.2);
        }
        
        .btn-sm {
            padding: 10px 20px;
            font-size: 14px;
        }
        
        .container {
            max-width: 1400px;
            margin: 0 auto;
            padding: 0 30px 50px;
        }
        
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 25px;
            margin-bottom: 40px;
        }
        
        .stat-card {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            border: 2px solid transparent;
            position: relative;
            overflow: hidden;
            display: flex;
            flex-direction: column;
        }
        
        .stat-card:hover {
            transform: translateY(-10px);
            box-shadow: var(--shadow-lg);
            border-color: var(--primary);
        }
        
        .stat-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--primary), var(--info));
        }
        
        .stat-icon {
            width: 70px;
            height: 70px;
            border-radius: 18px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 32px;
            margin-bottom: 25px;
            position: relative;
            z-index: 1;
        }
        
        .stat-icon.total {
            background: linear-gradient(135deg, #f0f7ff 0%, #d4e4ff 100%);
            color: var(--primary);
        }
        
        .stat-icon.online {
            background: linear-gradient(135deg, #f0fff4 0%, #dcffe4 100%);
            color: var(--secondary);
        }
        
        .stat-icon.warning {
            background: linear-gradient(135deg, #fff8e1 0%, #ffeaa7 100%);
            color: var(--warning);
        }
        
        .stat-icon.error {
            background: linear-gradient(135deg, #fff5f5 0%, #ffe5e5 100%);
            color: var(--danger);
        }
        
        .stat-card h3 {
            color: var(--dark);
            font-size: 16px;
            font-weight: 600;
            margin-bottom: 12px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .stat-value {
            font-size: 42px;
            font-weight: 800;
            color: var(--dark);
            margin: 15px 0;
            line-height: 1;
        }
        
        .controls-bar {
            background: white;
            border-radius: var(--border-radius);
            padding: 25px 30px;
            margin-bottom: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(67, 97, 238, 0.1);
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 20px;
        }
        
        .controls-bar h2 {
            color: var(--dark);
            font-size: 24px;
            font-weight: 700;
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .devices-container {
            background: white;
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid rgba(0,0,0,0.05);
            min-height: 300px;
            position: relative;
        }
        
        .loading-overlay {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(255,255,255,0.95);
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            border-radius: var(--border-radius);
            z-index: 100;
        }
        
        .loading-spinner {
            font-size: 48px;
            color: var(--primary);
            margin-bottom: 20px;
        }
        
        .loading-text {
            color: var(--dark);
            font-size: 18px;
            font-weight: 600;
            margin-bottom: 10px;
        }
        
        .loading-subtext {
            color: #64748b;
            font-size: 14px;
            text-align: center;
            max-width: 400px;
        }
        
        .devices-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(450px, 1fr));
            gap: 25px;
        }
        
        .device-card {
            background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
            border-radius: var(--border-radius);
            padding: 30px;
            box-shadow: var(--shadow-sm);
            border: 2px solid transparent;
            transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
            position: relative;
            overflow: hidden;
        }
        
        .device-card:hover {
            transform: translateY(-8px);
            box-shadow: var(--shadow-lg);
        }
        
        .device-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 6px;
            background: linear-gradient(90deg, var(--primary), var(--info));
        }
        
        .device-card.online::before {
            background: linear-gradient(90deg, var(--secondary), #27ae60);
        }
        
        .device-card.warning::before {
            background: linear-gradient(90deg, var(--warning), #e67e22);
        }
        
        .device-card.offline::before {
            background: linear-gradient(90deg, var(--danger), #c0392b);
        }
        
        .device-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 25px;
        }
        
        .device-title {
            display: flex;
            align-items: center;
            gap: 15px;
        }
        
        .device-icon {
            width: 60px;
            height: 60px;
            border-radius: 15px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 28px;
        }
        
        .device-icon.online {
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.15) 0%, rgba(46, 204, 113, 0.05) 100%);
            color: var(--secondary);
        }
        
        .device-icon.warning {
            background: linear-gradient(135deg, rgba(243, 156, 18, 0.15) 0%, rgba(243, 156, 18, 0.05) 100%);
            color: var(--warning);
        }
        
        .device-icon.offline {
            background: linear-gradient(135deg, rgba(231, 76, 60, 0.15) 0%, rgba(231, 76, 60, 0.05) 100%);
            color: var(--danger);
        }
        
        .device-info h3 {
            color: var(--dark);
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 5px;
        }
        
        .device-status {
            padding: 8px 20px;
            border-radius: 30px;
            font-weight: 700;
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .status-online {
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.15) 0%, rgba(46, 204, 113, 0.05) 100%);
            color: var(--secondary);
            border: 2px solid rgba(46, 204, 113, 0.3);
        }
        
        .status-warning {
            background: linear-gradient(135deg, rgba(243, 156, 18, 0.15) 0%, rgba(243, 156, 18, 0.05) 100%);
            color: var(--warning);
            border: 2px solid rgba(243, 156, 18, 0.3);
        }
        
        .status-offline {
            background: linear-gradient(135deg, rgba(231, 76, 60, 0.15) 0%, rgba(231, 76, 60, 0.05) 100%);
            color: var(--danger);
            border: 2px solid rgba(231, 76, 60, 0.3);
        }
        
        .device-details {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 20px;
            margin-bottom: 25px;
        }
        
        .detail-item {
            display: flex;
            flex-direction: column;
        }
        
        .detail-label {
            color: #64748b;
            font-size: 13px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 5px;
        }
        
        .detail-value {
            color: var(--dark);
            font-size: 16px;
            font-weight: 700;
            font-family: 'Roboto Mono', monospace;
        }
        
        .device-actions {
            display: flex;
            gap: 12px;
            margin-top: 20px;
        }
        
        .device-error {
            background: linear-gradient(135deg, #fff5f5 0%, #ffe5e5 100%);
            border-radius: 12px;
            padding: 20px;
            margin-top: 20px;
            border: 2px solid rgba(231, 76, 60, 0.3);
        }
        
        .error-header {
            display: flex;
            align-items: center;
            gap: 12px;
            margin-bottom: 10px;
        }
        
        .error-header i {
            color: var(--danger);
            font-size: 18px;
        }
        
        .error-header h4 {
            color: var(--danger);
            font-size: 15px;
            font-weight: 700;
        }
        
        .error-text {
            color: #721c24;
            font-size: 14px;
            line-height: 1.5;
        }
        
        .empty-state {
            text-align: center;
            padding: 60px 30px;
        }
        
        .empty-state i {
            font-size: 64px;
            color: #cbd5e1;
            margin-bottom: 25px;
        }
        
        .empty-state h3 {
            color: #64748b;
            font-size: 22px;
            font-weight: 700;
            margin-bottom: 15px;
        }
        
        .empty-state p {
            color: #94a3b8;
            font-size: 16px;
            margin-bottom: 25px;
            max-width: 500px;
            margin-left: auto;
            margin-right: auto;
            line-height: 1.6;
        }
        
        .error-state {
            background: linear-gradient(135deg, #fff5f5 0%, #ffe5e5 100%);
            border-radius: var(--border-radius);
            padding: 40px;
            text-align: center;
            border: 2px solid rgba(231, 76, 60, 0.3);
        }
        
        .error-state i {
            font-size: 48px;
            color: var(--danger);
            margin-bottom: 20px;
        }
        
        .error-state h3 {
            color: #721c24;
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 15px;
        }
        
        .error-state p {
            color: #721c24;
            margin-bottom: 20px;
            line-height: 1.6;
        }
        
        .connection-status {
            position: fixed;
            bottom: 30px;
            right: 30px;
            padding: 16px 30px;
            border-radius: 50px;
            font-weight: 700;
            z-index: 1000;
            box-shadow: var(--shadow-lg);
            display: flex;
            align-items: center;
            gap: 12px;
            font-size: 15px;
            transition: all 0.3s;
            backdrop-filter: blur(10px);
            border: 2px solid rgba(255,255,255,0.3);
            animation: slideIn 0.5s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        @keyframes slideIn {
            from {
                transform: translateX(100%);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }
        
        .connected {
            background: linear-gradient(135deg, rgba(46, 204, 113, 0.95) 0%, rgba(39, 174, 96, 0.95) 100%);
            color: white;
        }
        
        .disconnected {
            background: linear-gradient(135deg, rgba(231, 76, 60, 0.95) 0%, rgba(192, 57, 43, 0.95) 100%);
            color: white;
        }
        
        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        .fade-in-up {
            animation: fadeInUp 0.6s cubic-bezier(0.4, 0, 0.2, 1);
        }
        
        @media (max-width: 1200px) {
            .container {
                padding: 0 25px 40px;
            }
            
            .header {
                padding: 20px 25px;
            }
            
            .header h1 {
                font-size: 28px;
            }
            
            .devices-grid {
                grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
            }
        }
        
        @media (max-width: 992px) {
            .header {
                flex-direction: column;
                gap: 20px;
                text-align: center;
                padding: 25px 20px;
            }
            
            .devices-grid {
                grid-template-columns: 1fr;
            }
            
            .stats-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }
        
        @media (max-width: 768px) {
            .device-details {
                grid-template-columns: 1fr;
            }
            
            .controls-bar {
                flex-direction: column;
                align-items: stretch;
            }
            
            .controls-bar h2 {
                text-align: center;
                justify-content: center;
            }
            
            .device-actions {
                flex-wrap: wrap;
            }
            
            .stats-grid {
                grid-template-columns: 1fr;
            }
        }
        
        @media (max-width: 576px) {
            .container {
                padding: 0 15px 30px;
            }
            
            .device-header {
                flex-direction: column;
                align-items: stretch;
                gap: 15px;
            }
            
            .device-status {
                align-self: flex-start;
            }
            
            .connection-status {
                bottom: 20px;
                right: 20px;
                left: 20px;
                text-align: center;
                justify-content: center;
            }
        }
    </style>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&family=Roboto+Mono:wght@400;500&display=swap" rel="stylesheet">
</head>
<body>
    <!-- Статус соединения -->
    <div id="connectionStatus" class="connection-status" style="display: none;">
        <i class="fas fa-sync fa-spin"></i> Проверка связи с ККТ...
    </div>
    
    <!-- Шапка -->
    <div class="header">
        <h1>
            <i class="fas fa-cash-register"></i>
            Управление устройствами ККТ
        </h1>
        <div class="user-panel">
            <a href="../../index.php" class="btn btn-primary">
                <i class="fas fa-arrow-left"></i> На главную
            </a>
        </div>
    </div>
    
    <!-- Основной контент -->
    <div class="container">
        <!-- Статистика -->
        <div class="stats-grid">
            <div class="stat-card fade-in-up" style="animation-delay: 0.1s;">
                <div class="stat-icon total">
                    <i class="fas fa-cash-register"></i>
                </div>
                <h3>Всего устройств</h3>
                <div class="stat-value" id="totalDevices">0</div>
                <div style="color: #64748b; font-size: 14px;">Подключено к системе</div>
            </div>
            
            <div class="stat-card fade-in-up" style="animation-delay: 0.2s;">
                <div class="stat-icon online">
                    <i class="fas fa-wifi"></i>
                </div>
                <h3>В сети</h3>
                <div class="stat-value" id="onlineDevices">0</div>
                <div style="color: #64748b; font-size: 14px;">Готовы к работе</div>
            </div>
            
            <div class="stat-card fade-in-up" style="animation-delay: 0.3s;">
                <div class="stat-icon warning">
                    <i class="fas fa-exclamation-triangle"></i>
                </div>
                <h3>С предупреждениями</h3>
                <div class="stat-value" id="warningDevices">0</div>
                <div style="color: #64748b; font-size: 14px;">Требуют внимания</div>
            </div>
            
            <div class="stat-card fade-in-up" style="animation-delay: 0.4s;">
                <div class="stat-icon error">
                    <i class="fas fa-times-circle"></i>
                </div>
                <h3>Не в сети</h3>
                <div class="stat-value" id="offlineDevices">0</div>
                <div style="color: #64748b; font-size: 14px;">Нет соединения</div>
            </div>
        </div>
        
        <!-- Панель управления -->
        <div class="controls-bar fade-in-up" style="animation-delay: 0.5s;">
            <h2><i class="fas fa-sliders-h"></i> Управление устройствами</h2>
            <div style="display: flex; gap: 15px;">
                <button class="btn btn-primary" onclick="loadDevices()" id="refreshBtn">
                    <i class="fas fa-sync-alt"></i> Обновить список
                </button>
                <button class="btn btn-secondary" onclick="scanForDevices()" id="scanBtn">
                    <i class="fas fa-search"></i> Поиск устройств
                </button>
            </div>
        </div>
        
        <!-- Контейнер устройств -->
        <div class="devices-container fade-in-up" style="animation-delay: 0.6s;">
            <!-- Загрузка -->
            <div id="loadingOverlay" class="loading-overlay">
                <div class="loading-spinner">
                    <i class="fas fa-spinner fa-spin"></i>
                </div>
                <div class="loading-text">Загрузка устройств...</div>
                <div class="loading-subtext">
                    Проверка подключения к расширению KKM Server. 
                    Пожалуйста, подождите.
                </div>
            </div>
            
            <!-- Пустое состояние -->
            <div id="emptyState" class="empty-state" style="display: none;">
                <i class="fas fa-cash-register"></i>
                <h3>Устройства не найдены</h3>
                <p>
                    Не удалось обнаружить подключенные устройства ККТ. 
                    Проверьте подключение кассовых аппаратов и убедитесь, 
                    что расширение KKM Server установлено и активно.
                </p>
                <button class="btn btn-primary" onclick="loadDevices()">
                    <i class="fas fa-redo"></i> Попробовать снова
                </button>
            </div>
            
            <!-- Ошибка -->
            <div id="errorState" class="error-state" style="display: none;">
                <i class="fas fa-exclamation-triangle"></i>
                <h3>Ошибка подключения</h3>
                <p id="errorMessage">Произошла ошибка при загрузке устройств</p>
                <div style="background: rgba(255,255,255,0.5); padding: 15px; border-radius: 8px; margin: 20px 0;">
                    <p style="color: #721c24; font-size: 14px; margin-bottom: 10px; font-weight: 600;">
                        Проверьте следующие пункты:
                    </p>
                    <ul style="color: #721c24; font-size: 13px; text-align: left; margin-left: 20px;">
                        <li>Установлено ли расширение KKM Server</li>
                        <li>Разрешен ли доступ к сайту в расширении</li>
                        <li>Запущен ли KKM Server на компьютере</li>
                        <li>Подключены ли устройства ККТ к компьютеру</li>
                    </ul>
                </div>
                <button class="btn btn-primary" onclick="loadDevices()">
                    <i class="fas fa-redo"></i> Попробовать снова
                </button>
            </div>
            
            <!-- Список устройств -->
            <div id="devicesGrid" class="devices-grid" style="display: none;">
                <!-- Устройства будут загружены через JavaScript -->
            </div>
        </div>
    </div>
    
    <!-- JavaScript для работы с KKM Server API -->
    <script>
        // Глобальные переменные
        let allDevices = [];
        let isKKMConnected = false;
        
        // Функция для генерации GUID
        function guid() {
            function S4() {
                return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
            }
            return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
        }
        
        // Основная функция выполнения команд
        function ExecuteCommand(Data, FunSuccess, FunError, timeout) {
            // Проверка есть ли расширение
            try {
                if (typeof KkmServer !== 'undefined' && KkmServer.Execute) {
                    if (typeof Data === "string") Data = JSON.parse(Data);
                    
                    KkmServer.Execute(FunSuccess || ExecuteSuccess, Data);
                    return;
                }
            } catch (e) {
                console.error("Ошибка расширения KKM Server:", e);
            }
            
            if (FunError) {
                FunError('Расширение KKM Server не найдено. Установите расширение и обновите страницу.');
            }
        }
        
        // Обработка успешного ответа
        function ExecuteSuccess(Result) {
            if (Result.Status === 2) {
                showError('Ошибка KKM Server: ' + Result.Error);
            } else if (Result.Status === 0 || Result.Status === 1) {
                if (Result.Command === "GetDataKKT") {
                    processDeviceData(Result);
                }
            }
        }
        
        // Проверка связи с ККТ
        function checkKKTConnection() {
            const statusElement = document.getElementById('connectionStatus');
            
            if (typeof KkmServer === 'undefined') {
                isKKMConnected = false;
                statusElement.innerHTML = '<i class="fas fa-wifi-slash"></i> Расширение ККТ не найдено';
                statusElement.className = 'connection-status disconnected';
                statusElement.style.display = 'block';
                return;
            }
            
            const testData = {
                Command: "GetDataKKT",
                NumDevice: 0,
                IdCommand: guid(),
                Timeout: 5
            };
            
            try {
                KkmServer.Execute(function(result) {
                    if (result && result.Status !== undefined) {
                        isKKMConnected = true;
                        statusElement.innerHTML = '<i class="fas fa-wifi"></i> Соединение с ККТ установлено';
                        statusElement.className = 'connection-status connected';
                        statusElement.style.display = 'block';
                    } else {
                        isKKMConnected = false;
                        statusElement.innerHTML = '<i class="fas fa-wifi-slash"></i> Нет ответа от ККТ';
                        statusElement.className = 'connection-status disconnected';
                        statusElement.style.display = 'block';
                    }
                }, testData);
            } catch (error) {
                isKKMConnected = false;
                statusElement.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Ошибка связи с ККТ';
                statusElement.className = 'connection-status disconnected';
                statusElement.style.display = 'block';
            }
        }
        
        // Загрузка списка устройств
        function loadDevices() {
            const loadingOverlay = document.getElementById('loadingOverlay');
            const devicesGrid = document.getElementById('devicesGrid');
            const emptyState = document.getElementById('emptyState');
            const errorState = document.getElementById('errorState');
            
            loadingOverlay.style.display = 'flex';
            devicesGrid.style.display = 'none';
            emptyState.style.display = 'none';
            errorState.style.display = 'none';
            
            // Обновляем текст кнопки
            const refreshBtn = document.getElementById('refreshBtn');
            refreshBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Обновление...';
            refreshBtn.disabled = true;
            
            const command = {
                Command: "GetDataKKT",
                NumDevice: 0,
                IdCommand: guid(),
                Timeout: 10
            };
            
            ExecuteCommand(
                command,
                function(result) {
                    if (result.Status === 3) {
                        showNoDevices();
                    } else if (result.Status === 0 || result.Status === 1) {
                        processDeviceData(result);
                    }
                    refreshBtn.innerHTML = '<i class="fas fa-sync-alt"></i> Обновить список';
                    refreshBtn.disabled = false;
                },
                function(error) {
                    showError(error);
                    refreshBtn.innerHTML = '<i class="fas fa-sync-alt"></i> Обновить список';
                    refreshBtn.disabled = false;
                }
            );
        }
        
        // Поиск устройств
        function scanForDevices() {
            const scanBtn = document.getElementById('scanBtn');
            scanBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Поиск...';
            scanBtn.disabled = true;
            
            // В реальности здесь может быть более сложная логика поиска
            setTimeout(() => {
                loadDevices();
                scanBtn.innerHTML = '<i class="fas fa-search"></i> Поиск устройств';
                scanBtn.disabled = false;
            }, 2000);
        }
        
        // Обработка данных устройства
        function processDeviceData(deviceData) {
            const devices = [];
            
            if (deviceData.NumDevice !== undefined) {
                const status = getSessionStatus(deviceData.Info?.SessionState);
                
                devices.push({
                    id: deviceData.NumDevice,
                    name: deviceData.Info?.KktModel || `ККТ #${deviceData.NumDevice}`,
                    status: status,
                    inn: deviceData.Info?.InnOrganization || 'Не указан',
                    kktNumber: deviceData.Info?.KktNumber || 'Не указан',
                    fnNumber: deviceData.Info?.FnNumber || 'Не указан',
                    regNumber: deviceData.Info?.RegNumber || 'Не указан',
                    sessionNumber: deviceData.SessionNumber || 0,
                    checkNumber: deviceData.CheckNumber || 0,
                    lineLength: deviceData.LineLength || 48,
                    lastError: deviceData.Error || '',
                    info: deviceData.Info || {},
                    model: deviceData.Info?.KktModel || 'Неизвестная модель',
                    firmware: deviceData.Info?.FirmwareDate || 'Неизвестно'
                });
            }
            
            allDevices = devices;
            renderDevices(devices);
        }
        
        // Преобразование статуса сессии
        function getSessionStatus(sessionState) {
            switch(sessionState) {
                case 1: return 'offline';
                case 2: return 'online';
                case 3: return 'warning';
                default: return 'offline';
            }
        }
        
        // Отображение списка устройств
        function renderDevices(devices) {
            const loadingOverlay = document.getElementById('loadingOverlay');
            const devicesGrid = document.getElementById('devicesGrid');
            const emptyState = document.getElementById('emptyState');
            
            loadingOverlay.style.display = 'none';
            
            if (devices.length === 0) {
                emptyState.style.display = 'block';
                return;
            }
            
            // Обновляем статистику
            const total = devices.length;
            const online = devices.filter(d => d.status === 'online').length;
            const warning = devices.filter(d => d.status === 'warning').length;
            const offline = devices.filter(d => d.status === 'offline').length;
            
            document.getElementById('totalDevices').textContent = total;
            document.getElementById('onlineDevices').textContent = online;
            document.getElementById('warningDevices').textContent = warning;
            document.getElementById('offlineDevices').textContent = offline;
            
            // Рендерим сетку устройств
            devicesGrid.innerHTML = '';
            
            devices.forEach(device => {
                const statusClass = device.status;
                const statusText = getStatusText(device.status);
                const statusIcon = getStatusIcon(device.status);
                
                const deviceHtml = `
                    <div class="device-card ${statusClass}">
                        <div class="device-header">
                            <div class="device-title">
                                <div class="device-icon ${statusClass}">
                                    ${statusIcon}
                                </div>
                                <div class="device-info">
                                    <h3>${device.name}</h3>
                                    <div style="color: #64748b; font-size: 14px;">
                                        <i class="fas fa-microchip"></i> ${device.model}
                                    </div>
                                </div>
                            </div>
                            <span class="device-status status-${statusClass}">
                                ${statusText}
                            </span>
                        </div>
                        
                        <div class="device-details">
                            <div class="detail-item">
                                <span class="detail-label">ИНН организации</span>
                                <span class="detail-value">${device.inn}</span>
                            </div>
                            <div class="detail-item">
                                <span class="detail-label">Рег. номер</span>
                                <span class="detail-value">${device.regNumber}</span>
                            </div>
                            <div class="detail-item">
                                <span class="detail-label">Заводской номер</span>
                                <span class="detail-value">${device.kktNumber}</span>
                            </div>
                            <div class="detail-item">
                                <span class="detail-label">Номер ФН</span>
                                <span class="detail-value">${device.fnNumber}</span>
                            </div>
                            <div class="detail-item">
                                <span class="detail-label">Текущая смена</span>
                                <span class="detail-value">${device.sessionNumber}</span>
                            </div>
                            <div class="detail-item">
                                <span class="detail-label">Последний чек</span>
                                <span class="detail-value">${device.checkNumber}</span>
                            </div>
                        </div>
                        
                        ${device.lastError ? `
                        <div class="device-error">
                            <div class="error-header">
                                <i class="fas fa-exclamation-circle"></i>
                                <h4>Последняя ошибка</h4>
                            </div>
                            <div class="error-text">${device.lastError}</div>
                        </div>
                        ` : ''}
                        
                        <div class="device-actions">
                            <button class="btn btn-primary btn-sm" onclick="openShift(${device.id})" 
                                    ${device.status !== 'online' ? 'disabled' : ''}>
                                <i class="fas fa-play"></i> Открыть смену
                            </button>
                            <button class="btn btn-secondary btn-sm" onclick="closeShift(${device.id})" 
                                    ${device.status !== 'online' ? 'disabled' : ''}>
                                <i class="fas fa-stop"></i> Закрыть смену
                            </button>
                            <button class="btn btn-info btn-sm" onclick="getDeviceDetails(${device.id})">
                                <i class="fas fa-info-circle"></i> Подробнее
                            </button>
                        </div>
                    </div>
                `;
                
                devicesGrid.innerHTML += deviceHtml;
            });
            
            devicesGrid.style.display = 'grid';
        }
        
        function getStatusText(status) {
            switch(status) {
                case 'online': return 'В сети';
                case 'offline': return 'Не в сети';
                case 'warning': return 'Внимание';
                default: return 'Неизвестно';
            }
        }
        
        function getStatusIcon(status) {
            switch(status) {
                case 'online': return '<i class="fas fa-wifi"></i>';
                case 'offline': return '<i class="fas fa-wifi-slash"></i>';
                case 'warning': return '<i class="fas fa-exclamation-triangle"></i>';
                default: return '<i class="fas fa-question-circle"></i>';
            }
        }
        
        function showNoDevices() {
            const loadingOverlay = document.getElementById('loadingOverlay');
            const emptyState = document.getElementById('emptyState');
            
            loadingOverlay.style.display = 'none';
            emptyState.style.display = 'block';
        }
        
        function showError(message) {
            const loadingOverlay = document.getElementById('loadingOverlay');
            const errorState = document.getElementById('errorState');
            const errorMessage = document.getElementById('errorMessage');
            
            loadingOverlay.style.display = 'none';
            errorMessage.textContent = message;
            errorState.style.display = 'block';
        }
        
        // Функции для работы с устройствами
        function openShift(deviceId) {
            const command = {
                Command: "OpenShift",
                NumDevice: deviceId,
                CashierName: "<?php echo $_SESSION['user_name'] ?? 'Оператор'; ?>",
                CashierVATIN: "",
                IdCommand: guid(),
                Timeout: 30
            };
            
            ExecuteCommand(command, function(result) {
                if (result.Status === 0) {
                    alert(`Смена успешно открыта! Номер смены: ${result.SessionNumber}`);
                    loadDevices();
                } else {
                    alert('Ошибка при открытии смены: ' + (result.Error || 'Неизвестная ошибка'));
                }
            });
        }
        
        function closeShift(deviceId) {
            if (!confirm('Вы уверены, что хотите закрыть смену? После закрытия будет распечатан Z-отчет.')) {
                return;
            }
            
            const command = {
                Command: "CloseShift",
                NumDevice: deviceId,
                IdCommand: guid(),
                Timeout: 30
            };
            
            ExecuteCommand(command, function(result) {
                if (result.Status === 0) {
                    alert('Смена успешно закрыта! Z-отчет распечатан.');
                    loadDevices();
                } else {
                    alert('Ошибка при закрытии смены: ' + (result.Error || 'Неизвестная ошибка'));
                }
            });
        }
        
        function getDeviceDetails(deviceId) {
            const device = allDevices.find(d => d.id === deviceId);
            if (!device) return;
            
            let details = `Детальная информация об устройстве:\n\n`;
            details += `📱 Модель: ${device.model}\n`;
            details += `🔢 Номер устройства: ${device.id}\n`;
            details += `📋 Название: ${device.name}\n`;
            details += `🏢 ИНН организации: ${device.inn}\n`;
            details += `🏷️ Рег. номер: ${device.regNumber}\n`;
            details += `🔧 Заводской номер: ${device.kktNumber}\n`;
            details += `💾 Номер ФН: ${device.fnNumber}\n`;
            details += `🔄 Текущая смена: ${device.sessionNumber}\n`;
            details += `🧾 Последний чек: ${device.checkNumber}\n`;
            details += `📏 Ширина строки: ${device.lineLength} симв.\n`;
            details += `🛠️ Прошивка: ${device.firmware}\n`;
            details += `📊 Статус: ${getStatusText(device.status)}\n`;
            
            if (device.lastError) {
                details += `\n⚠️ Последняя ошибка:\n${device.lastError}`;
            }
            
            if (device.info) {
                details += `\n\n📊 Дополнительная информация:`;
                if (device.info.FirmwareDate) details += `\nДата прошивки: ${device.info.FirmwareDate}`;
                if (device.info.KktVersion) details += `\nВерсия ККТ: ${device.info.KktVersion}`;
                if (device.info.FnSerial) details += `\nСерийный номер ФН: ${device.info.FnSerial}`;
                if (device.info.FnEndDate) details += `\nСрок действия ФН: ${device.info.FnEndDate}`;
            }
            
            alert(details);
        }
        
        // Инициализация
        document.addEventListener('DOMContentLoaded', function() {
            checkKKTConnection();
            setTimeout(loadDevices, 1000);
            setInterval(checkKKTConnection, 10000);
            
            // Анимация появления элементов
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('fade-in-up');
                    }
                });
            }, { threshold: 0.1 });
            
            document.querySelectorAll('.stat-card, .controls-bar, .devices-container').forEach(el => {
                observer.observe(el);
            });
        });
    </script>
</body>
</html>