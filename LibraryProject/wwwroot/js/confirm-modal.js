// data-confirm="mesaj" taşıyan tüm formlar için onay modalı
(function () {
    // Modal iskeletini bir kez oluştur
    const overlay = document.createElement('div');
    overlay.className = 'cm-overlay';
    overlay.innerHTML = `
        <div class="cm-box">
            <div class="cm-icon"><i class="bi bi-question-circle"></i></div>
            <p class="cm-message"></p>
            <div class="cm-actions">
                <button type="button" class="btn btn-secondary cm-cancel">Vazgeç</button>
                <button type="button" class="btn btn-primary cm-ok">Evet, Devam Et</button>
            </div>
        </div>`;
    document.body.appendChild(overlay);

    const msgEl = overlay.querySelector('.cm-message');
    const okBtn = overlay.querySelector('.cm-ok');
    const cancelBtn = overlay.querySelector('.cm-cancel');
    let pendingForm = null;

    function open(message, form) {
        msgEl.textContent = message;
        pendingForm = form;

        // Tehlikeli işlemse (silme/iptal) onay butonu kırmızı olsun
        const danger = /sil|iptal|yasak/i.test(message);
        okBtn.className = 'btn cm-ok ' + (danger ? 'btn-danger' : 'btn-primary');
        okBtn.textContent = danger ? 'Evet, Eminim' : 'Evet, Devam Et';

        overlay.classList.add('show');
    }

    function close() {
        overlay.classList.remove('show');
        pendingForm = null;
    }

    okBtn.addEventListener('click', () => {
        if (pendingForm) { const f = pendingForm; pendingForm = null; f.submit(); }
        overlay.classList.remove('show');
    });

    cancelBtn.addEventListener('click', close);
    overlay.addEventListener('click', (e) => { if (e.target === overlay) close(); });
    document.addEventListener('keydown', (e) => { if (e.key === 'Escape') close(); });

    // Tüm data-confirm'li formları yakala (event delegation: sonradan eklenenler dahil)
    document.addEventListener('submit', (e) => {
        const form = e.target.closest('form[data-confirm]');
        if (!form) return;
        e.preventDefault();
        open(form.dataset.confirm, form);
    });
})();