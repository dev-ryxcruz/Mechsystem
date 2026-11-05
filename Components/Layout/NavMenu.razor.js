// Interop da sidebar (NavMenu).
// Este módulo NÃO decide largura nem rótulos: isso vem exclusivamente do SidebarState (C#).
// Aqui ficam apenas (1) o tooltip do modo recolhido e (2) a notificação de viewport mobile.

const MOBILE_QUERY = "(max-width: 767.98px)";
const TOOLTIP_GAP = 12;
const VIEWPORT_MARGIN = 8;

export function isMobile() {
    return window.matchMedia(MOBILE_QUERY).matches;
}

export function init(sidebar, tooltip, dotNetRef) {
    let currentItem = null;
    let frame = 0;

    const isRail = () => sidebar.classList.contains("sidebar--collapsed");

    function hide() {
        cancelAnimationFrame(frame);
        currentItem = null;
        tooltip.hidden = true;
    }

    function place(item) {
        const label = item.getAttribute("data-label");
        if (!label || !isRail() || !item.isConnected) {
            hide();
            return;
        }

        tooltip.textContent = label;
        tooltip.hidden = false;

        const itemRect = item.getBoundingClientRect();
        const tipRect = tooltip.getBoundingClientRect();

        let left = itemRect.right + TOOLTIP_GAP;
        if (left + tipRect.width > window.innerWidth - VIEWPORT_MARGIN) {
            left = Math.max(VIEWPORT_MARGIN, window.innerWidth - VIEWPORT_MARGIN - tipRect.width);
        }

        let top = itemRect.top + (itemRect.height - tipRect.height) / 2;
        top = Math.min(top, window.innerHeight - VIEWPORT_MARGIN - tipRect.height);
        top = Math.max(top, VIEWPORT_MARGIN);

        tooltip.style.left = `${Math.round(left)}px`;
        tooltip.style.top = `${Math.round(top)}px`;
    }

    function show(item) {
        if (!isRail()) {
            hide();
            return;
        }

        currentItem = item;
        // Aguarda o frame: no foco por teclado o navegador pode estar rolando o item para a área visível.
        cancelAnimationFrame(frame);
        frame = requestAnimationFrame(() => {
            if (currentItem === item) {
                place(item);
            }
        });
    }

    const itemOf = (target) =>
        target instanceof Element ? target.closest("[data-label]") : null;

    const onMouseOver = (e) => {
        const item = itemOf(e.target);
        if (item && sidebar.contains(item)) {
            show(item);
        }
    };

    const onMouseOut = (e) => {
        const item = itemOf(e.target);
        if (item && !item.contains(e.relatedTarget)) {
            hide();
        }
    };

    const onFocusIn = (e) => {
        const item = itemOf(e.target);
        if (item && sidebar.contains(item)) {
            show(item);
        }
    };

    const onFocusOut = (e) => {
        if (itemOf(e.target)) {
            hide();
        }
    };

    const onKeyDown = (e) => {
        if (e.key === "Escape") {
            hide();
        }
    };

    const mediaQuery = window.matchMedia(MOBILE_QUERY);
    const onViewportChange = (e) => {
        hide();
        dotNetRef.invokeMethodAsync("OnViewportChanged", e.matches);
    };

    sidebar.addEventListener("mouseover", onMouseOver);
    sidebar.addEventListener("mouseout", onMouseOut);
    sidebar.addEventListener("focusin", onFocusIn);
    sidebar.addEventListener("focusout", onFocusOut);
    sidebar.addEventListener("click", hide);
    document.addEventListener("keydown", onKeyDown);
    // Captura: também recebe o scroll do contêiner .nav-scrollable (scroll não borbulha).
    window.addEventListener("scroll", hide, true);
    window.addEventListener("resize", hide);
    mediaQuery.addEventListener("change", onViewportChange);

    return DotNet.createJSObjectReference({
        dispose() {
            hide();
            sidebar.removeEventListener("mouseover", onMouseOver);
            sidebar.removeEventListener("mouseout", onMouseOut);
            sidebar.removeEventListener("focusin", onFocusIn);
            sidebar.removeEventListener("focusout", onFocusOut);
            sidebar.removeEventListener("click", hide);
            document.removeEventListener("keydown", onKeyDown);
            window.removeEventListener("scroll", hide, true);
            window.removeEventListener("resize", hide);
            mediaQuery.removeEventListener("change", onViewportChange);
        }
    });
}
