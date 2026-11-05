(() => {
    if (document.body.dataset.vuAuthenticated !== "true") {
        return;
    }

    window.addEventListener("pagehide", (event) => {
        if (event.persisted) {
            document.documentElement.style.visibility = "hidden";
        }
    });

    window.addEventListener("pageshow", (event) => {
        if (event.persisted) {
            window.location.reload();
        }
    });
})();
