/* ═══════════════════════════════════════════════════════════════
   BairlyMN — Main JavaScript
   ═══════════════════════════════════════════════════════════════ */

document.addEventListener('DOMContentLoaded', () => {

    // ── 1. Auto-dismiss toasts ─────────────────────────────────
    document.querySelectorAll('.toast').forEach(el => {
        const toast = bootstrap.Toast.getOrCreateInstance(el, { delay: 4500 });
        toast.show();
        setTimeout(() => toast.hide(), 5000);
    });

    // ── 2. Navbar scroll shadow ────────────────────────────────
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        const onScroll = () => {
            navbar.style.boxShadow = window.scrollY > 10
                ? '0 4px 20px rgba(0,0,0,.10)'
                : '';
        };
        window.addEventListener('scroll', onScroll, { passive: true });
    }

    // ── 3. Gallery thumbnail sync ──────────────────────────────
    const gallery = document.getElementById('gallery');
    if (gallery) {
        gallery.addEventListener('slide.bs.carousel', e => {
            document.querySelectorAll('.gallery-thumb').forEach((t, i) =>
                t.classList.toggle('active', i === e.to));
        });
    }

    // ── 4. Lazy image fade-in ──────────────────────────────────
    document.querySelectorAll('img[loading="lazy"]').forEach(img => {
        if (img.complete) img.classList.add('loaded');
        else img.addEventListener('load', () => img.classList.add('loaded'));
    });

    // ── 5. Mobile nav close on link ────────────────────────────
    document.querySelectorAll('.navbar-collapse .nav-link:not(.dropdown-toggle)')
        .forEach(link => {
            link.addEventListener('click', () => {
                const c = document.querySelector('.navbar-collapse');
                if (c?.classList.contains('show'))
                    bootstrap.Collapse.getOrCreateInstance(c).hide();
            });
        });

    // ── 6. Image upload drag & drop ────────────────────────────
    setupImageUpload();

    // ── 7. Bio character counter ───────────────────────────────
    const bio = document.querySelector('textarea[name="Bio"]');
    const bioCount = document.getElementById('bioCount');
    if (bio && bioCount) {
        const update = () => {
            const len = bio.value.length;
            bioCount.textContent = `${len}/500`;
            bioCount.style.color = len > 450 ? '#ef4444' : '#64748b';
        };
        bio.addEventListener('input', update);
        update();
    }

    // ── 8. Smooth anchor scroll ────────────────────────────────
    document.querySelectorAll('a[href^="#"]').forEach(a => {
        a.addEventListener('click', e => {
            const t = document.querySelector(a.getAttribute('href'));
            if (t) { e.preventDefault(); t.scrollIntoView({ behavior: 'smooth' }); }
        });
    });

});

// ── Image upload setup ────────────────────────────────────────────
function setupImageUpload() {
    const dropZone = document.querySelector('.image-upload-area');
    if (!dropZone) return;

    ['dragenter', 'dragover'].forEach(ev =>
        dropZone.addEventListener(ev, e => {
            e.preventDefault();
            dropZone.style.borderColor = '#16a34a';
            dropZone.style.background = '#f0fdf4';
        }));

    ['dragleave', 'drop'].forEach(ev =>
        dropZone.addEventListener(ev, e => {
            e.preventDefault();
            dropZone.style.borderColor = '';
            dropZone.style.background = '';
        }));

    dropZone.addEventListener('drop', e => {
        const input = document.getElementById('imgInput') ||
            document.getElementById('newImgInput');
        if (!input) return;
        // Transfer files
        const dt = new DataTransfer();
        Array.from(e.dataTransfer.files).forEach(f => {
            if (f.type.startsWith('image/')) dt.items.add(f);
        });
        input.files = dt.files;
        previewImages(input);
    });
}

// ── Image preview ─────────────────────────────────────────────────
window.previewImages = function (input) {
    const preview = document.getElementById('imgPreview');
    if (!preview) return;
    preview.innerHTML = '';

    if (!input.files?.length) return;

    Array.from(input.files).forEach((file, i) => {
        if (!file.type.startsWith('image/')) return;

        const reader = new FileReader();
        reader.onload = e => {
            const div = document.createElement('div');
            div.className = 'position-relative';
            div.style.cssText = 'width:100px;height:75px';
            div.innerHTML = `
                <img src="${e.target.result}"
                     class="w-100 h-100 rounded-3"
                     style="object-fit:cover"
                     alt="preview-${i}" />
                <div class="position-absolute top-0 start-0 w-100 h-100
                            d-flex align-items-center justify-content-center
                            rounded-3 bg-dark bg-opacity-25 opacity-0"
                     style="transition:.15s"
                     onmouseenter="this.style.opacity='1'"
                     onmouseleave="this.style.opacity='0'">
                    <span class="text-white" style="font-size:.65rem;font-weight:600">
                        ${file.name.substring(0, 12)}
                    </span>
                </div>`;
            preview.appendChild(div);
        };
        reader.readAsDataURL(file);
    });
};

// ── Avatar preview ────────────────────────────────────────────────
window.prevAvatar = function (input) {
    if (!input.files?.[0]) return;
    const reader = new FileReader();
    reader.onload = e => {
        const img = document.getElementById('avatarPreview');
        if (img) {
            img.style.transition = 'opacity .2s';
            img.style.opacity = '0';
            setTimeout(() => { img.src = e.target.result; img.style.opacity = '1'; }, 200);
        }
    };
    reader.readAsDataURL(input.files[0]);
};

// ── Property type switcher ────────────────────────────────────────
window.switchDetails = window.onPropertyTypeChange = function (val) {
    document.querySelectorAll('.detail-section').forEach(el => {
        el.classList.add('d-none');
    });

    const noMsg = document.getElementById('noTypeMsg');
    if (noMsg) noMsg.classList.toggle('d-none', !!val);

    const map = { '1': 'aptSection', '2': 'houseSection', '3': 'landSection' };
    const target = map[String(val)];
    if (target) {
        const el = document.getElementById(target);
        if (el) {
            el.classList.remove('d-none');
            el.style.animation = 'fadeInUp .3s ease';
        }
    }
};

// ── Delete confirmation ───────────────────────────────────────────
window.confirmDelete = function (id) {
    const form = document.getElementById('deleteForm');
    if (form) form.action = '/Listings/Delete/' + id;
    const modal = document.getElementById('deleteModal');
    if (modal) new bootstrap.Modal(modal).show();
};

// ── Favorite toggle ───────────────────────────────────────────────
window.toggleFav = async function (id) {
    const btn = document.getElementById('favBtn');
    const token = document.querySelector('[name=__RequestVerificationToken]')?.value || '';

    if (btn) btn.disabled = true;

    try {
        const r = await fetch('/Listings/ToggleFavorite/' + id, {
            method: 'POST',
            headers: { 'RequestVerificationToken': token }
        });
        const d = await r.json();

        if (btn) {
            if (d.added) {
                btn.className = btn.className
                    .replace('btn-outline-warning', 'btn-warning');
                btn.innerHTML = '<i class="bi bi-bookmark-fill me-2"></i>Хадгалагдсан';
            } else {
                btn.className = btn.className
                    .replace('btn-warning', 'btn-outline-warning');
                btn.innerHTML = '<i class="bi bi-bookmark me-2"></i>Хадгалах';
            }
        }
        showToast(d.message, d.added ? 'success' : 'info');
    } catch {
        showToast('Алдаа гарлаа', 'danger');
    } finally {
        if (btn) btn.disabled = false;
    }
};

// ── Dynamic toast ─────────────────────────────────────────────────
window.showToast = function (message, type = 'success') {
    const colors = {
        success: '#16a34a', danger: '#ef4444',
        warning: '#f59e0b', info: '#0ea5e9'
    };
    const icons = {
        success: 'check-circle-fill', danger: 'exclamation-circle-fill',
        warning: 'exclamation-triangle-fill', info: 'info-circle-fill'
    };

    const bg = colors[type] || colors.success;
    const icon = icons[type] || icons.success;

    let container = document.querySelector('.toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }

    const wrapper = document.createElement('div');
    wrapper.className = 'toast show align-items-center text-white border-0 mb-2';
    wrapper.style.cssText = `
        background:${bg};
        border-radius:12px;
        box-shadow:0 10px 40px rgba(0,0,0,.15);
        min-width:280px;
        animation:toastSlide .3s ease`;
    wrapper.innerHTML = `
        <div class="d-flex">
            <div class="toast-body fw-500 d-flex align-items-center gap-2">
                <i class="bi bi-${icon}"></i>${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto"></button>
        </div>`;

    wrapper.querySelector('.btn-close').addEventListener('click',
        () => wrapper.remove());

    container.appendChild(wrapper);
    setTimeout(() => wrapper.remove(), 4500);
};

// ── Copy to clipboard ─────────────────────────────────────────────
window.copyLink = function () {
    navigator.clipboard.writeText(window.location.href)
        .then(() => showToast('Холбоос хуулагдлаа!', 'success'))
        .catch(() => showToast('Хуулж чадсангүй', 'danger'));
};

// ── Format price ──────────────────────────────────────────────────
window.formatPrice = n =>
    new Intl.NumberFormat('mn-MN').format(n) + ' ₮';

// ── Shared apartment toggle ───────────────────────────────────────
window.toggleSharedFields = function (cb) {
    const fields = document.getElementById('sharedFields');
    if (fields) fields.classList.toggle('d-none', !cb.checked);
};

// ── Transaction type change (Create form) ─────────────────────────
window.onTransactionTypeChange = function (val) {
    const rentConds = document.getElementById('rentConditions');
    const sharedSec = document.getElementById('sharedSection');
    const isRent = val === '2';

    if (rentConds) rentConds.classList.toggle('d-none', !isRent);
    if (sharedSec) sharedSec.classList.toggle('d-none', !isRent);

    // Update price placeholder
    const priceInput = document.querySelector('[name="Price"]');
    if (priceInput) {
        priceInput.placeholder = isRent ? 'Сарийн түрээс' : '0';
    }
};