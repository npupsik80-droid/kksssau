// Проверка расширения и соединения
class KKMConnection {
    constructor() {
        this.isConnected = false;
        this.checkInterval = null;
    }
    
    init() {
        this.checkConnection();
        this.checkInterval = setInterval(() => this.checkConnection(), 5000);
    }
    
    checkConnection() {
        try {
            if (typeof KkmServer !== 'undefined') {
                // Проверяем доступность расширения
                KkmServer.Execute((result) => {
                    if (result && result.Status !== undefined) {
                        this.isConnected = true;
                        this.updateUI(true);
                    } else {
                        this.isConnected = false;
                        this.updateUI(false);
                    }
                }, { Command: "GetDataKKT" });
            } else {
                this.isConnected = false;
                this.updateUI(false);
            }
        } catch (error) {
            console.error('Ошибка проверки ККТ:', error);
            this.isConnected = false;
            this.updateUI(false);
        }
    }
    
    updateUI(connected) {
        const statusElement = document.getElementById('connection-status');
        if (statusElement) {
            if (connected) {
                statusElement.innerHTML = '<span class="connected">Соединение установлено</span>';
                statusElement.className = 'connection-status connected';
            } else {
                statusElement.innerHTML = '<span class="disconnected">Нет соединения с ККТ</span>';
                statusElement.className = 'connection-status disconnected';
            }
        }
    }
    
    // Открытие смены
    async openShift(cashierName, cashierInn = '') {
        return new Promise((resolve, reject) => {
            const commandData = {
                Command: "OpenShift",
                CashierName: cashierName,
                CashierVATIN: cashierInn,
                NotPrint: false,
                IdCommand: this.generateGuid()
            };
            
            if (this.isConnected && typeof KkmServer !== 'undefined') {
                KkmServer.Execute((result) => {
                    if (result.Status === 0) {
                        // Сохраняем в БД
                        this.saveShiftToDB(result, cashierName);
                        resolve(result);
                    } else {
                        reject(new Error(result.Error || 'Ошибка открытия смены'));
                    }
                }, commandData);
            } else {
                // Fallback через API
                fetch('api/open_shift.php', {
                    method: 'POST',
                    headers: {'Content-Type': 'application/json'},
                    body: JSON.stringify(commandData)
                })
                .then(response => response.json())
                .then(resolve)
                .catch(reject);
            }
        });
    }
    
    // Печать чека
    async printCheck(checkData) {
        const commandData = {
            Command: "RegisterCheck",
            TypeCheck: checkData.type === 'sale' ? 0 : 
                      checkData.type === 'return' ? 1 : 2,
            CashierName: checkData.cashierName,
            IsFiscalCheck: true,
            NotPrint: false,
            CheckStrings: checkData.items.map(item => ({
                Register: {
                    Name: item.name,
                    Quantity: item.quantity,
                    Price: item.price,
                    Amount: item.quantity * item.price,
                    Tax: item.tax || 20,
                    SignMethodCalculation: 4,
                    SignCalculationObject: 1
                }
            })),
            Cash: checkData.payment.cash,
            ElectronicPayment: checkData.payment.card,
            IdCommand: this.generateGuid()
        };
        
        try {
            const result = await this.executeCommand(commandData);
            
            if (result.Status === 0) {
                // Сохраняем чек в БД
                await this.saveCheckToDB(result, checkData);
                return {
                    success: true,
                    checkNumber: result.CheckNumber,
                    fiscalSign: result.QRCode,
                    total: checkData.total
                };
            } else {
                throw new Error(result.Error);
            }
        } catch (error) {
            console.error('Ошибка печати чека:', error);
            throw error;
        }
    }
    
    generateGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
}

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', () => {
    window.kkmConnection = new KKMConnection();
    window.kkmConnection.init();
});