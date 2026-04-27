<?php
define('DB_HOST', 'localhost:3306');
define('DB_NAME', 'SvyazCRM');
define('DB_USER', 'root');
define('DB_PASS', 'Q1212qki!');

try {
    $pdo = new PDO(
        "mysql:host=" . DB_HOST . ";dbname=" . DB_NAME . ";charset=utf8mb4",
        DB_USER,
        DB_PASS,
        [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
    );
} catch (PDOException $e) {
    die("Ошибка подключения к БД: " . $e->getMessage());
}
?>