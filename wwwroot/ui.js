/* ============================================
   UMUMIY UI — mavzu (yorug'/qorong'i) + til (Lot/Kir)
   login, register, my-orders, admin sahifalarida ishlatiladi.
   Tanlovlar localStorage'da — market bilan bir xil kalitlar.
   ============================================ */

// Mavzuni miltillashsiz darrov o'rnatish (ui.js <head>'da yuklanadi)
(function () {
    try { document.documentElement.setAttribute('data-theme', localStorage.getItem('theme') || 'light'); } catch (e) { }
})();

(function () {
    const LAT2CYR = [
        ["O‘", "Ў"], ["O'", "Ў"], ["Oʻ", "Ў"], ["O’", "Ў"], ["o‘", "ў"], ["o'", "ў"], ["oʻ", "ў"], ["o’", "ў"],
        ["G‘", "Ғ"], ["G'", "Ғ"], ["Gʻ", "Ғ"], ["G’", "Ғ"], ["g‘", "ғ"], ["g'", "ғ"], ["gʻ", "ғ"], ["g’", "ғ"],
        ["Sh", "Ш"], ["SH", "Ш"], ["sh", "ш"], ["Ch", "Ч"], ["CH", "Ч"], ["ch", "ч"],
        ["Yo", "Ё"], ["YO", "Ё"], ["yo", "ё"], ["Yu", "Ю"], ["YU", "Ю"], ["yu", "ю"],
        ["Ya", "Я"], ["YA", "Я"], ["ya", "я"], ["Ye", "Е"], ["YE", "Е"], ["ye", "е"], ["Ts", "Ц"], ["ts", "ц"],
        ["A", "А"], ["a", "а"], ["B", "Б"], ["b", "б"], ["D", "Д"], ["d", "д"], ["E", "Э"], ["e", "е"],
        ["F", "Ф"], ["f", "ф"], ["G", "Г"], ["g", "г"], ["H", "Ҳ"], ["h", "ҳ"], ["I", "И"], ["i", "и"],
        ["J", "Ж"], ["j", "ж"], ["K", "К"], ["k", "к"], ["L", "Л"], ["l", "л"], ["M", "М"], ["m", "м"],
        ["N", "Н"], ["n", "н"], ["O", "О"], ["o", "о"], ["P", "П"], ["p", "п"], ["Q", "Қ"], ["q", "қ"],
        ["R", "Р"], ["r", "р"], ["S", "С"], ["s", "с"], ["T", "Т"], ["t", "т"], ["U", "У"], ["u", "у"],
        ["V", "В"], ["v", "в"], ["X", "Х"], ["x", "х"], ["Y", "Й"], ["y", "й"], ["Z", "З"], ["z", "з"],
        ["C", "К"], ["c", "к"], ["'", "ъ"]
    ];
    function latinToCyrillic(s) {
        if (!s || !/[A-Za-z']/.test(s)) return s;
        let out = "", i = 0;
        while (i < s.length) {
            let matched = false;
            for (const [lat, cyr] of LAT2CYR) {
                if (s.startsWith(lat, i)) { out += cyr; i += lat.length; matched = true; break; }
            }
            if (!matched) { out += s[i]; i++; }
        }
        return out;
    }
    function translitEl(root) {
        if (!root) return;
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, {
            acceptNode(n) {
                const p = n.parentNode;
                if (!p) return NodeFilter.FILTER_REJECT;
                const t = p.nodeName;
                if (t === 'SCRIPT' || t === 'STYLE' || t === 'TEXTAREA') return NodeFilter.FILTER_REJECT;
                if (p.closest && p.closest('[data-no-translit]')) return NodeFilter.FILTER_REJECT;
                return NodeFilter.FILTER_ACCEPT;
            }
        });
        const nodes = []; let n;
        while (n = walker.nextNode()) nodes.push(n);
        nodes.forEach(t => { t.nodeValue = latinToCyrillic(t.nodeValue); });
        root.querySelectorAll && root.querySelectorAll('input[placeholder]').forEach(el => {
            if (!el.closest('[data-no-translit]')) el.placeholder = latinToCyrillic(el.placeholder);
        });
    }
    let observer = null;
    function enableCyrillic() {
        translitEl(document.body);
        if (observer) return;
        observer = new MutationObserver(muts => {
            muts.forEach(m => m.addedNodes.forEach(node => {
                if (node.nodeType === 1) translitEl(node);
                else if (node.nodeType === 3 && !(node.parentNode && node.parentNode.closest('[data-no-translit]')))
                    node.nodeValue = latinToCyrillic(node.nodeValue);
            }));
        });
        observer.observe(document.body, { childList: true, subtree: true });
    }

    window.setScript = function (s) { localStorage.setItem('script', s); location.reload(); };
    window.toggleTheme = function () {
        const cur = document.documentElement.getAttribute('data-theme') || 'light';
        const next = cur === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', next);
        localStorage.setItem('theme', next);
        const b = document.getElementById('uicTheme');
        if (b) b.textContent = next === 'dark' ? '🌙' : '☀️';
    };

    document.addEventListener('DOMContentLoaded', () => {
        const script = localStorage.getItem('script') || 'latin';
        const theme = document.documentElement.getAttribute('data-theme') || 'light';

        // Tugmalarni joylash
        const box = document.createElement('div');
        box.className = 'uic';
        box.setAttribute('data-no-translit', '');
        box.innerHTML =
            '<div class="uic-lang">' +
            '<button id="uicLat" class="' + (script === 'latin' ? 'on' : '') + '">Lot</button>' +
            '<button id="uicCyr" class="' + (script === 'cyrillic' ? 'on' : '') + '">Kir</button>' +
            '</div>' +
            '<button class="uic-theme" id="uicTheme" title="Mavzu">' + (theme === 'dark' ? '🌙' : '☀️') + '</button>';
        document.body.appendChild(box);
        document.getElementById('uicLat').onclick = () => setScript('latin');
        document.getElementById('uicCyr').onclick = () => setScript('cyrillic');
        document.getElementById('uicTheme').onclick = () => toggleTheme();

        if (script === 'cyrillic') enableCyrillic();
    });
})();
