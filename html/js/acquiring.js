// acquiring.js - Управление эквайрингом
class AcquiringManager {
    constructor() {
        this.transactions = {};
        this.currentTransaction = null;
        this.isProcessing = false;
    }

    // Проведение оплаты по карте
    async processCardPayment(amount, numDevice = 0) {
        if (this.isProcessing) {
            throw new Error('Уже выполняется другая операция');
        }

        this.isProcessing = true;
        this.currentTransaction = {
            id: this.generateTransactionId(),
            amount: amount,
            status: 'pending',
            startTime: new Date(),
            numDevice: numDevice
        };

        return new Promise((resolve, reject) => {
            if (typeof KkmServer === 'undefined') {
                this.isProcessing = false;
                reject(new Error('Расширение ККТ не подключено'));
                return;
            }

            const paymentData = {
                Command: "PayByPaymentCard",
                NumDevice: numDevice,
                Amount: amount,
                IdCommand: this.currentTransaction.id,
                InnKkm: "",
                Timeout: 120 // 2 минуты для эквайринга
            };

            console.log('Начинаем оплату по карте:', paymentData);

            KkmServer.Execute((result) => {
                console.log('Ответ от эквайринга:', result);
                this.isProcessing = false;
                
                if (result.Status === 0) {
                    this.currentTransaction.status = 'success';
                    this.currentTransaction.response = result;
                    this.currentTransaction.endTime = new Date();
                    this.currentTransaction.universalId = result.UniversalID;
                    
                    resolve({
                        success: true,
                        universalId: result.UniversalID,
                        amount: result.Amount,
                        slip: result.Slip,
                        transactionId: this.currentTransaction.id
                    });
                } else {
                    this.currentTransaction.status = 'error';
                    this.currentTransaction.error = result.Error;
                    this.currentTransaction.endTime = new Date();
                    
                    reject(new Error(result.Error || 'Ошибка оплаты по карте'));
                }
            }, paymentData);

            // Таймаут
            setTimeout(() => {
                if (this.currentTransaction && this.currentTransaction.status === 'pending') {
                    this.currentTransaction.status = 'timeout';
                    this.currentTransaction.endTime = new Date();
                    this.isProcessing = false;
                    reject(new Error('Таймаут ожидания ответа от терминала'));
                }
            }, 130000); // 130 секунд
        });
    }

    // Возврат оплаты по карте
    async refundCardPayment(amount, universalId, numDevice = 0) {
        return new Promise((resolve, reject) => {
            if (typeof KkmServer === 'undefined') {
                reject(new Error('Расширение ККТ не подключено'));
                return;
            }

            const refundData = {
                Command: "ReturnPaymentByPaymentCard",
                NumDevice: numDevice,
                Amount: amount,
                UniversalID: universalId,
                IdCommand: this.generateTransactionId(),
                InnKkm: ""
            };

            KkmServer.Execute((result) => {
                if (result.Status === 0) {
                    resolve({
                        success: true,
                        universalId: result.UniversalID,
                        amount: result.Amount
                    });
                } else {
                    reject(new Error(result.Error || 'Ошибка возврата оплаты'));
                }
            }, refundData);
        });
    }

    // Отмена оплаты по карте
    async cancelCardPayment(amount, universalId, numDevice = 0) {
        return new Promise((resolve, reject) => {
            if (typeof KkmServer === 'undefined') {
                reject(new Error('Расширение ККТ не подключено'));
                return;
            }

            const cancelData = {
                Command: "CancelPaymentByPaymentCard",
                NumDevice: numDevice,
                Amount: amount,
                UniversalID: universalId,
                IdCommand: this.generateTransactionId(),
                InnKkm: ""
            };

            KkmServer.Execute((result) => {
                if (result.Status === 0) {
                    resolve({
                        success: true,
                        universalId: result.UniversalID,
                        amount: result.Amount
                    });
                } else {
                    reject(new Error(result.Error || 'Ошибка отмены оплаты'));
                }
            }, cancelData);
        });
    }

    // Генерация ID транзакции
    generateTransactionId() {
        return 'txn_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    // Получение информации о последней транзакции
    getLastTransaction() {
        return this.currentTransaction;
    }

    // Очистка текущей транзакции
    clearTransaction() {
        this.currentTransaction = null;
    }

    // Извлечение RRN из UniversalID
    static extractRRN(universalId) {
        if (!universalId) return '';
        const match = universalId.match(/RRN:([^;]+)/);
        return match ? match[1] : '';
    }

    // Извлечение кода авторизации из UniversalID
    static extractAuthCode(universalId) {
        if (!universalId) return '';
        const match = universalId.match(/AC:([^;]+)/);
        return match ? match[1] : '';
    }
}

// Создаем глобальный экземпляр
window.acquiringManager = new AcquiringManager();