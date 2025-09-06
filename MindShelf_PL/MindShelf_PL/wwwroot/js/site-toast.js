function showToast(message, type) {
    let bg = type === 'success' ? 'bg-success' :
        type === 'error' ? 'bg-danger' :
            'bg-info';

    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white ${bg} border-0 position-fixed bottom-0 end-0 m-3`;
    toast.role = 'alert';
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto"
                    data-bs-dismiss="toast" aria-label="Close"></button>
        </div>`;

    document.body.appendChild(toast);

    const bsToast = new bootstrap.Toast(toast, { delay: 3000 });
    bsToast.show();

    toast.addEventListener('hidden.bs.toast', () => toast.remove());
}
