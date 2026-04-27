<!-- Модальное окно создания -->
<div id="createModal" class="modal-overlay">
    <div class="modal-content">
        <div class="modal-header">
            <h2><i class="fas fa-plus-circle"></i> Новая номенклатура</h2>
            <button class="close-modal" onclick="closeCreateModal()">&times;</button>
        </div>
        
        <form method="POST" id="createForm">
            <input type="hidden" name="action" value="create">
            
            <div class="form-group">
                <label><i class="fas fa-signature"></i> Наименование *</label>
                <input type="text" name="name" id="createName" class="form-control" required 
                       placeholder="Наименование товара">
            </div>
            
            <div class="form-group">
                <label><i class="fas fa-barcode"></i> Штрих-код *</label>
                <input type="text" name="barcode" class="form-control" required 
                       placeholder="Уникальный штрих-код">
            </div>
            
            <div class="form-group">
                <label><i class="fas fa-align-left"></i> Описание</label>
                <textarea name="description" class="form-control" rows="3" 
                          placeholder="Описание товара (необязательно)"></textarea>
            </div>
            
            <div class="modal-actions">
                <button type="button" class="btn btn-secondary" onclick="closeCreateModal()">
                    Отмена
                </button>
                <button type="submit" class="btn btn-success">
                    <i class="fas fa-save"></i> Сохранить
                </button>
            </div>
        </form>
    </div>
</div>

<!-- Модальное окно редактирования -->
<div id="editModal" class="modal-overlay">
    <div class="modal-content">
        <div class="modal-header">
            <h2><i class="fas fa-edit"></i> Редактировать номенклатуру</h2>
            <button class="close-modal" onclick="closeEditModal()">&times;</button>
        </div>
        
        <form method="POST" id="editForm">
            <input type="hidden" name="action" value="update">
            <input type="hidden" name="id" id="editId">
            
            <div class="form-group">
                <label>Наименование *</label>
                <input type="text" name="name" id="editName" class="form-control" required>
            </div>
            
            <div class="form-group">
                <label>Штрих-код *</label>
                <input type="text" name="barcode" id="editBarcode" class="form-control" required>
            </div>
            
            <div class="form-group">
                <label>Описание</label>
                <textarea name="description" id="editDescription" class="form-control" rows="3"></textarea>
            </div>
            
            <div class="modal-actions">
                <button type="button" class="btn btn-secondary" onclick="closeEditModal()">
                    Отмена
                </button>
                <button type="submit" class="btn btn-primary">
                    <i class="fas fa-save"></i> Сохранить
                </button>
            </div>
        </form>
    </div>
</div>

<script>
// Закрытие модальных окон по клику вне
document.querySelectorAll('.modal-overlay').forEach(modal => {
    modal.addEventListener('click', function(e) {
        if (e.target === this) {
            this.style.display = 'none';
        }
    });
});
</script>