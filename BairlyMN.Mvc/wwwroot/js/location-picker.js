/**
 * BairlyMN — 3-түвшний Cascading Location Picker
 * ================================================
 * Хэрхэн ашиглах:
 *
 *   LocationPicker.init({
 *     prefix:     'create',          // id prefix (create_l1, create_l2...)
 *     selectedId: 42,                // edit mode-д pre-select хийх id
 *     onChange:   (id) => {}         // сонгосон үед дуудагдана (заавал биш)
 *   });
 */

const LocationPicker = (() => {

    const API = {
        level1: '/api/location/level1',
        children: (id) => `/api/location/children/${id}`,
        ancestors: (id) => `/api/location/ancestors/${id}`
    };

    // ── DOM helpers ────────────────────────────────────────────────
    function el(id) { return document.getElementById(id); }

    function buildId(prefix, suffix) { return `${prefix}_${suffix}`; }

    // ── Fetch wrapper ──────────────────────────────────────────────
    async function fetchJson(url) {
        try {
            const res = await fetch(url, {
                headers: { 'Accept': 'application/json' }
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            return await res.json();
        } catch (err) {
            console.error('[LocationPicker] fetch алдаа:', url, err);
            return [];
        }
    }

    // ── Select populate ────────────────────────────────────────────
    function populate(selectEl, items, placeholder) {
        // Хуучин option-уудыг арилгана
        selectEl.innerHTML = '';

        const empty = document.createElement('option');
        empty.value = '';
        empty.textContent = placeholder;
        selectEl.appendChild(empty);

        items.forEach(item => {
            const opt = document.createElement('option');
            opt.value = item.id;
            opt.textContent = item.name;
            selectEl.appendChild(opt);
        });
    }

    // ── Reset & disable ────────────────────────────────────────────
    function reset(selectEl, placeholder) {
        populate(selectEl, [], placeholder);
        selectEl.disabled = true;
        selectEl.classList.remove('is-valid', 'border-success');
    }

    // ── Enable select ──────────────────────────────────────────────
    function enable(selectEl) {
        selectEl.disabled = false;
    }

    // ── Set value and trigger change ───────────────────────────────
    function setValue(selectEl, value) {
        selectEl.value = value;
        // Visual feedback
        if (value) {
            selectEl.classList.add('border-success');
        }
    }

    // ── Main init function ─────────────────────────────────────────
    async function init(options = {}) {
        const {
            prefix = 'loc',
            selectedId = null,
            onChange = null
        } = options;

        const l1El = el(buildId(prefix, 'l1'));
        const l2El = el(buildId(prefix, 'l2'));
        const l3El = el(buildId(prefix, 'l3'));
        const hiddenEl = el(buildId(prefix, 'final'));

        if (!l1El || !l2El || !l3El || !hiddenEl) {
            console.warn('[LocationPicker] DOM элементүүд олдсонгүй. prefix:', prefix);
            return;
        }

        // Loading state
        function setLoading(selectEl, loading) {
            if (loading) {
                selectEl.disabled = true;
                selectEl.style.opacity = '0.6';
            } else {
                selectEl.style.opacity = '1';
            }
        }

        // Update hidden input & callback
        function updateFinal(value) {
            hiddenEl.value = value || '';
            if (onChange && typeof onChange === 'function') {
                onChange(value ? parseInt(value) : null);
            }
        }

        // ── Level 1 change ─────────────────────────────────────────
        l1El.addEventListener('change', async function () {
            const id = this.value;

            // Reset L2 & L3
            reset(l2El, '— Дүүрэг / Сум —');
            reset(l3El, '— Хороо / Баг —');
            updateFinal(id || null);

            if (!id) return;

            setLoading(l2El, true);
            const children = await fetchJson(API.children(id));
            setLoading(l2El, false);

            populate(l2El, children, '— Дүүрэг / Сум —');
            enable(l2El);

            // L2-т зөвхөн 1 хүүхэд байвал auto-select
            if (children.length === 1) {
                l2El.value = children[0].id;
                l2El.dispatchEvent(new Event('change'));
            }
        });

        // ── Level 2 change ─────────────────────────────────────────
        l2El.addEventListener('change', async function () {
            const id = this.value;

            // Reset L3
            reset(l3El, '— Хороо / Баг —');

            // L2 сонгоход final-г шинэчлэнэ (L3 байхгүй байж болно)
            updateFinal(id || hiddenEl.value);

            if (!id) return;

            setLoading(l3El, true);
            const children = await fetchJson(API.children(id));
            setLoading(l3El, false);

            if (children.length === 0) {
                // L3 байхгүй — L2-ийг final болгоно
                updateFinal(id);
                return;
            }

            populate(l3El, children, '— Хороо / Баг —');
            enable(l3El);
        });

        // ── Level 3 change ─────────────────────────────────────────
        l3El.addEventListener('change', function () {
            const id = this.value;
            // L3 сонгоогүй бол L2-ийг ашиглана
            updateFinal(id || l2El.value || l1El.value);
        });

        // ── Initial load — Level 1 ─────────────────────────────────
        setLoading(l1El, true);
        const level1 = await fetchJson(API.level1);
        setLoading(l1El, false);

        populate(l1El, level1, '— Хот / Аймаг —');
        enable(l1El);

        // ── Pre-select (Edit mode) ─────────────────────────────────
        if (selectedId) {
            await preselect(prefix, selectedId, {
                l1El, l2El, l3El, hiddenEl, updateFinal, setLoading
            });
        }
    }

    // ── Pre-select for edit mode ───────────────────────────────────
    async function preselect(prefix, selectedId, refs) {
        const { l1El, l2El, l3El, hiddenEl, updateFinal, setLoading } = refs;

        const ancestors = await fetchJson(API.ancestors(selectedId));
        if (!ancestors || ancestors.length === 0) return;

        const [a1, a2, a3] = ancestors; // Level 1, 2, 3

        // Set L1
        if (a1) {
            setValue(l1El, a1.id);

            // Load L2
            if (a2) {
                setLoading(l2El, true);
                const l2Items = await fetchJson(API.children(a1.id));
                setLoading(l2El, false);
                populate(l2El, l2Items, '— Дүүрэг / Сум —');
                enable(l2El);
                setValue(l2El, a2.id);

                // Load L3
                if (a3) {
                    setLoading(l3El, true);
                    const l3Items = await fetchJson(API.children(a2.id));
                    setLoading(l3El, false);

                    if (l3Items.length > 0) {
                        populate(l3El, l3Items, '— Хороо / Баг —');
                        enable(l3El);
                        setValue(l3El, a3.id);
                        updateFinal(a3.id);
                    } else {
                        updateFinal(a2.id);
                    }
                } else {
                    updateFinal(a2.id);
                }
            } else {
                updateFinal(a1.id);
            }
        }
    }

    // Public API
    return { init };

})();