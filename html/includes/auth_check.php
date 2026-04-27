<?php
// Устанавливаем время жизни cookie сессии 10 часов
ini_set('session.cookie_lifetime', 36000);
// Устанавливаем время жизни данных на сервере тоже 10 часов
ini_set('session.gc_maxlifetime', 36000);
// Убеждаемся, что сборщик мусора запускается с вероятностью 1% (значения по умолчанию)
ini_set('session.gc_probability', 1);
ini_set('session.gc_divisor', 100);

session_start();

if (!isset($_SESSION['user_id'])) {
    header('Location: /login.php');
    exit();
}

// Проверяем время бездействия (30 минут)
if (isset($_SESSION['last_activity']) && (time() - $_SESSION['last_activity'] > 36900)) {
    session_unset();
    session_destroy();
    header('Location: /login.php?timeout=1');
    exit();
}
$_SESSION['last_activity'] = time();

// Подключаемся к БД
require_once __DIR__ . '/../config/database.php';
?>