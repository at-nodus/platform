(function () {
  function initUploads(root) {
    (root || document).querySelectorAll("[data-upload]").forEach(function (wrap) {
      var input = wrap.querySelector('input[type="file"]');
      var preview = wrap.querySelector("[data-upload-preview]");
      var nameEl = wrap.querySelector("[data-upload-name]");
      var clearBtn = wrap.querySelector("[data-upload-clear]");
      if (!input || !preview) return;

      function setPreview(file) {
        var img = preview.querySelector("img");
        if (!file) {
          if (img) img.remove();
          preview.removeAttribute("data-has-file");
          if (nameEl) nameEl.textContent = "Nenhum arquivo selecionado";
          return;
        }
        var url = URL.createObjectURL(file);
        if (!img) {
          img = document.createElement("img");
          img.alt = "Pré-visualização da logomarca";
          preview.appendChild(img);
        }
        img.onload = function () {
          URL.revokeObjectURL(url);
        };
        img.src = url;
        preview.setAttribute("data-has-file", "true");
        if (nameEl) nameEl.textContent = file.name;
      }

      input.addEventListener("change", function () {
        var file = input.files && input.files[0];
        setPreview(file || null);
      });

      if (clearBtn) {
        clearBtn.addEventListener("click", function () {
          input.value = "";
          setPreview(null);
        });
      }
    });
  }

  function initProductRows(root) {
    (root || document).querySelectorAll("[data-product-row]").forEach(function (row) {
      var toggle = row.querySelector(".form-check-input");
      var plan = row.querySelector("[data-product-plan]");
      var label = row.querySelector("[data-switch-label]");
      if (!toggle) return;

      function sync() {
        row.classList.toggle("is-enabled", toggle.checked);
        if (plan) plan.disabled = !toggle.checked;
        if (label) label.textContent = toggle.checked ? "Liberado" : "Bloqueado";
      }

      toggle.addEventListener("change", sync);
      sync();
    });
  }

  function activateHashTab() {
    var hash = (location.hash || "").replace("#", "");
    if (!hash) return;
    var trigger = document.querySelector('[data-bs-toggle="tab"][data-bs-target="#' + hash + '"], [data-bs-toggle="tab"][href="#' + hash + '"]');
    if (!trigger || typeof bootstrap === "undefined") return;
    bootstrap.Tab.getOrCreateInstance(trigger).show();
  }

  function syncActiveNavigation(root) {
    var current = location.pathname.replace(/\/+$/, "").toLowerCase();
    (root || document).querySelectorAll(".app-sidebar .nav-link, .app-nav-offcanvas .nav-link").forEach(function (link) {
      var href = link.getAttribute("href");
      if (!href || href.charAt(0) === "#") return;
      try {
        var path = new URL(href, location.origin).pathname.replace(/\/+$/, "").toLowerCase();
        var exact = path === current;
        var section = path.length > 1 && current.indexOf(path + "/") === 0;
        if (exact || section) {
          link.classList.add("active");
          link.setAttribute("aria-current", "page");
        }
      } catch (_) {
        // Ignore malformed/non-http links.
      }
    });
  }

  function syncTabHash(root) {
    (root || document).querySelectorAll('[data-bs-toggle="tab"]').forEach(function (trigger) {
      trigger.addEventListener("shown.bs.tab", function (event) {
        var target = event.target.getAttribute("data-bs-target") || event.target.getAttribute("href");
        if (!target || target.charAt(0) !== "#") return;
        history.replaceState(null, "", target);
      });
    });
  }

  initUploads(document);
  initProductRows(document);
  activateHashTab();
  syncActiveNavigation(document);
  syncTabHash(document);
  window.addEventListener("hashchange", activateHashTab);
})();
