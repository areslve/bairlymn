document.addEventListener('DOMContentLoaded', function () {
    // Auto-dismiss toasts
    document.querySelectorAll('.toast').forEach(el => {
        setTimeout(() => bootstrap.Toast.getOrCreateInstance(el).hide(), 4000);
    });

    // Gallery thumbnail sync
    const carousel = document.getElementById('gallery');
    if (carousel) {
        carousel.addEventListener('slide.bs.carousel', e => {
            document.querySelectorAll('.gallery-thumb').forEach((t, i) =>
                t.classList.toggle('active', i === e.to));
        });
    }
});