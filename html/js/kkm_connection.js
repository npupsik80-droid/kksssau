// kkm_connection.js - Общий класс для работы с ККТ
class KKMConnection {
    constructor() {
        this.isConnected = false;
        this.connectionCallbacks = [];
        this.deviceInfo = null;
        this.checkConnection();
        // Проверяем каждые 3 секунды
        setInterval(() => this.checkConnection(), 3000);
    }
    
    // Проверка соединения
    checkConnection() {
        try {
            if (typeof KkmServer !== 'undefined') {
                this.isConnected = true;
                this.updateConnectionStatus(true);
                this.notifyConnectionChange(true);
                
                // Получаем информацию о ККТ
                this.getDeviceInfo();
            } else {
                this.isConnected = false;
                this.updateConnectionStatus(false);
                this.notifyConnectionChange(false);
            }
        } catch (error) {
            console.error('Ошибка проверки ККТ:', error);
            this.isConnected = false;
            this.updateConnectionStatus(false);
            this.notifyConnectionChange(false);
        }
    }
    
    // Получение информации об устройстве
    async getDeviceInfo() {
        if (!this.isConnected) return;
        
        try {
            const result = await this.executeCommand({
                Command: "GetDataKKT",
                NumDevice: 0,
                IdCommand: this.generateGuid()
            });
            
            if (result.Status === 0) {
                this.deviceInfo = result.Info;
            }
        } catch (error) {
            console.error('Ошибка получения информации ККТ:', error);
        }
    }
    
    // Обновление статуса на странице
    updateConnectionStatus(connected) {
        const statusElements = document.querySelectorAll('.connection-status, #connectionStatus, #connection-status');
        
        statusElements.forEach(element => {
            if (connected) {
                element.innerHTML = '<i class="fas fa-wifi"></i> Соединение установлено';
                element.className = element.className.replace(/disconnected/g, '') + ' connected';
                element.style.backgroundColor = '#28a745';
                element.style.color = 'white';
                element.style.display = 'block';
            } else {
                element.innerHTML = '<i class="fas fa-wifi-slash"></i> Нет соединения с ККТ';
                element.className = element.className.replace(/connected/g, '') + ' disconnected';
                element.style.backgroundColor = '#dc3545';
                element.style.color = 'white';
                element.style.display = 'block';
            }
        });
    }
    
    // Уведомление об изменении статуса
    notifyConnectionChange(connected) {
        this.connectionCallbacks.forEach(callback => {
            if (typeof callback === 'function') {
                callback(connected);
            }
        });
    }
    
    // Подписка на изменение статуса
    onConnectionChange(callback) {
        this.connectionCallbacks.push(callback);
    }
    
    // Выполнение команды
    executeCommand(commandData) {
        return new Promise((resolve, reject) => {
            if (this.isConnected && typeof KkmServer !== 'undefined') {
                KkmServer.Execute((result) => {
                    resolve(result);
                }, commandData);
            } else {
                reject(new Error('ККТ не подключена'));
            }
        });
    }
    
    // Открытие смены
    openShift(cashierName, cashierInn = '') {
        return this.executeCommand({
            Command: "OpenShift",
            CashierName: cashierName,
            CashierVATIN: cashierInn,
            NotPrint: false,
            IdCommand: this.generateGuid()
        });
    }
    
    // Закрытие смены
    closeShift(cashierName, cashierInn = '') {
        return this.executeCommand({
            Command: "CloseShift",
            CashierName: cashierName,
            CashierVATIN: cashierInn,
            NotPrint: false,
            IdCommand: this.generateGuid()
        });
    }
    
    // Получение состояния ККТ
    getKKTStatus(numDevice = 0) {
        return this.executeCommand({
            Command: "GetDataKKT",
            NumDevice: numDevice,
            IdCommand: this.generateGuid()
        });
    }
    
    // Печать чека (общая функция)
    async printCheck(checkData) {
        // Если есть оплата картой и эквайринг, добавляем параметры транзакции
        const commandData = {
            Command: "RegisterCheck",
            TypeCheck: checkData.TypeCheck || 0,
            CashierName: checkData.CashierName || '',
            IsFiscalCheck: true,
            NotPrint: false,
            Timeout: 120,
            IdCommand: checkData.IdCommand || this.generateGuid(),
            
            // Поля эквайринга (если есть)
            ...(checkData.ElectronicPayment > 0 && checkData.acquiringResult ? {
                RRNCode: AcquiringManager.extractRRN(checkData.acquiringResult.universalId),
                AuthorizationCode: AcquiringManager.extractAuthCode(checkData.acquiringResult.universalId)
            } : {}),
            
            // Выключаем автоматический эквайринг, так как уже обработали вручную
            PayByProcessing: false,
            ElectronicPayment: checkData.ElectronicPayment || 0,
            Cash: checkData.Cash || 0,
            AdvancePayment: checkData.AdvancePayment || 0,
            Credit: checkData.Credit || 0,
            CashProvision: checkData.CashProvision || 0,
            
            // Данные чека
            CheckStrings: checkData.CheckStrings || []
        };
        
        // Добавляем дополнительные поля если они есть
        if (checkData.ClientAddress) commandData.ClientAddress = checkData.ClientAddress;
        if (checkData.SenderEmail) commandData.SenderEmail = checkData.SenderEmail;
        if (checkData.NotPrint !== undefined) commandData.NotPrint = checkData.NotPrint;
        
        return this.executeCommand(commandData);
    }
    
    // Генерация GUID
    generateGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    
    // Получение информации о текущем устройстве
    getDeviceInfo() {
        return this.deviceInfo;
    }
    
    // Проверка открытой смены
    async checkShiftStatus() {
        try {
            const status = await this.getKKTStatus(0);
            return status.Info?.SessionState === 2; // 2 = смена открыта
        } catch (error) {
            return false;
        }
    }
}

// Глобальный экземпляр
window.kkmConnection = new KKMConnection();