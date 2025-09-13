function showToast(message, type) {
    var bg = type === 'success' ? 'bg-success' : type === 'error' ? 'bg-danger' : 'bg-dark';

    // Create (or reuse) a container so toasts stack nicely in the top-right
    var container = document.getElementById('appToastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'appToastContainer';
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '1080';
        document.body.appendChild(container);
    }

    var toast = document.createElement('div');
    toast.className = 'toast text-white border-0 shadow ' + bg;
    toast.style.borderRadius = '12px';
    toast.innerHTML = '\n        <div class="d-flex">\n            <div class="toast-body">' + (message || '') + '</div>\n            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>\n        </div>';

    container.appendChild(toast);

    var bsToast = new bootstrap.Toast(toast, { delay: 3500 });
    bsToast.show();
    toast.addEventListener('hidden.bs.toast', function () { toast.remove(); });
}

// Richer toast for private messages with sender name and optional avatar
function showPmToast(senderName, message, avatarUrl, onClickUrl) {
    // Ensure container exists for stacking
    var container = document.getElementById('appToastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'appToastContainer';
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '1080';
        document.body.appendChild(container);
    }

    var toast = document.createElement('div');
    toast.className = 'toast bg-dark text-white border-0 shadow';
    toast.style.borderRadius = '12px';
    toast.style.minWidth = '320px';
    toast.innerHTML = ''+
        '<div class="d-flex p-2">'+
        '  <div class="me-2 d-flex align-items-center justify-content-center" style="width:36px;height:36px;border-radius:50%;overflow:hidden;background:#2a2d33;">'+
        (avatarUrl ? ('<img src="'+avatarUrl+'" style="width:100%;height:100%;object-fit:cover;" onerror="this.parentElement.innerHTML=\'📩\'" />') : '📩')+
        '  </div>'+
        '  <div class="toast-body">'+
        '    <div class="fw-bold">'+(senderName||'مستخدم')+'</div>'+
        '    <div class="small text-white-50" style="white-space:nowrap;overflow:hidden;text-overflow:ellipsis;max-width:240px;">'+(message||'')+'</div>'+
        '  </div>'+
        '  <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>'+
        '</div>';

    container.appendChild(toast);

    if (onClickUrl) {
        toast.style.cursor = 'pointer';
        toast.addEventListener('click', function (e) {
            var target = e && e.target;
            if (target && target.classList && target.classList.contains('btn-close')) return;
            window.location.href = onClickUrl;
        });
    }

    var bsToast = new bootstrap.Toast(toast, { delay: 4500 });
    bsToast.show();
    toast.addEventListener('hidden.bs.toast', function () { toast.remove(); });
}