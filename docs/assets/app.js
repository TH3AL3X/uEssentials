(() => {
  "use strict";

  const commands = [...(window.UESSENTIALS_COMMANDS || [])].sort((a, b) => a.n.localeCompare(b.n));
  const grid = document.querySelector("#command-grid");
  const search = document.querySelector("#command-search");
  const count = document.querySelector("#result-count");
  const empty = document.querySelector("#empty-state");
  const clearButton = document.querySelector("#clear-search");
  const dialog = document.querySelector("#command-dialog");
  const dialogContent = document.querySelector("#dialog-content");
  const toast = document.querySelector("#toast");
  const languageSelect = document.querySelector("#language-select");
  const supportedLanguages = ["es", "en", "pt", "ru"];
  let savedLanguage = "";
  try { savedLanguage = localStorage.getItem("uessentials-language") || ""; } catch {}
  const browserLanguage = (navigator.language || "es").slice(0, 2).toLowerCase();
  let currentLanguage = supportedLanguages.includes(savedLanguage)
    ? savedLanguage
    : supportedLanguages.includes(browserLanguage) ? browserLanguage : "es";
  let activeFilter = "all";
  let toastTimer;
  let openCommandName = "";

  const escapeHtml = (value = "") => String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;");

  const normalize = (value = "") => String(value)
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase()
    .trim();

  const t = key => window.UESSENTIALS_I18N?.[currentLanguage]?.[key]
    ?? window.UESSENTIALS_I18N?.es?.[key]
    ?? key;
  const commandDescription = command => window.UESSENTIALS_COMMAND_TEXT?.[currentLanguage]?.[command.n] || command.d;
  const categoryText = command => window.UESSENTIALS_CATEGORIES?.[currentLanguage]?.[command.c] || command.c;
  const localizedUsage = command => {
    let usage = command.u || "";
    const tokens = window.UESSENTIALS_USAGE_TOKENS?.[currentLanguage] || {};
    Object.entries(tokens).sort((a, b) => b[0].length - a[0].length).forEach(([from, to]) => {
      usage = usage.replaceAll(from, to);
    });
    return usage;
  };
  const basePermission = command => "essentials.command." + command.n;
  const permissions = command => [
    basePermission(command),
    ...(command.x || []).map(suffix => basePermission(command) + "." + suffix),
    ...(command.p || [])
  ];
  const isModule = command => ["Kits", "Warps"].includes(command.c);
  const sourceText = command => command.s === "PLAYER" ? t("player_only") : t("both");
  const badgeClass = command => isModule(command) ? "module" : command.s === "PLAYER" ? "player" : "";
  const badgeText = command => isModule(command) ? categoryText(command) : command.s === "PLAYER" ? t("player") : t("console_player");

  function applyLanguage(language, persist = true) {
    currentLanguage = supportedLanguages.includes(language) ? language : "es";
    document.documentElement.lang = currentLanguage === "pt" ? "pt-BR" : currentLanguage;
    document.title = t("title");
    const meta = document.querySelector("#meta-description");
    if (meta) meta.content = t("meta");
    document.querySelectorAll("[data-i18n]").forEach(element => {
      element.textContent = t(element.dataset.i18n);
    });
    document.querySelectorAll("[data-i18n-html]").forEach(element => {
      element.innerHTML = t(element.dataset.i18nHtml);
    });
    document.querySelectorAll("[data-i18n-placeholder]").forEach(element => {
      element.placeholder = t(element.dataset.i18nPlaceholder);
    });
    document.querySelectorAll("[data-i18n-aria-label]").forEach(element => {
      element.setAttribute("aria-label", t(element.dataset.i18nAriaLabel));
    });
    languageSelect.value = currentLanguage;
    if (persist) {
      try { localStorage.setItem("uessentials-language", currentLanguage); } catch {}
    }
    render();
    if (dialog.open && openCommandName) openCommand(openCommandName);
  }

  function matches(command, query) {
    const haystack = [
      command.n,
      command.d,
      ...Object.values(window.UESSENTIALS_COMMAND_TEXT || {}).map(language => language[command.n] || ""),
      command.u,
      localizedUsage(command),
      command.c,
      categoryText(command),
      ...(command.a || []),
      ...permissions(command)
    ].join(" ");
    return normalize(haystack).includes(normalize(query));
  }

  function cardTemplate(command) {
    return [
      '<button class="command-card" type="button" data-command="' + escapeHtml(command.n) + '" aria-label="' + escapeHtml(t("view_command")) + " " + escapeHtml(command.n) + '">',
      '  <span class="card-top">',
      '    <span class="source-badge ' + badgeClass(command) + '">' + escapeHtml(badgeText(command)) + '</span>',
      '    <span class="card-arrow" aria-hidden="true">↗</span>',
      '  </span>',
      '  <h3><span>/</span>' + escapeHtml(command.n) + '</h3>',
      '  <p>' + escapeHtml(commandDescription(command)) + '</p>',
      '  <span class="permission-row"><code>' + escapeHtml(basePermission(command)) + '</code><span>⌘</span></span>',
      '</button>'
    ].join("");
  }

  function render() {
    const query = search.value;
    const visible = commands.filter(command => {
      const filterMatch = activeFilter === "all"
        || (activeFilter === "module" && isModule(command))
        || command.s === activeFilter;
      return filterMatch && matches(command, query);
    });

    grid.innerHTML = visible.map(cardTemplate).join("");
    count.textContent = visible.length + " " + (visible.length === 1 ? t("command_singular") : t("command_plural"));
    empty.hidden = visible.length !== 0;
    grid.hidden = visible.length === 0;
    clearButton.hidden = !query && activeFilter === "all";
  }

  function showToast(message = t("copied")) {
    toast.textContent = message;
    toast.classList.add("show");
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove("show"), 1800);
  }

  async function copyText(text) {
    try {
      await navigator.clipboard.writeText(text);
    } catch {
      const area = document.createElement("textarea");
      area.value = text;
      area.style.position = "fixed";
      area.style.opacity = "0";
      document.body.append(area);
      area.select();
      document.execCommand("copy");
      area.remove();
    }
    showToast();
  }

  function openCommand(name) {
    const command = commands.find(item => item.n === name);
    if (!command) return;
    const allPermissions = permissions(command);
    const aliases = command.a?.length ? command.a.map(alias => "/" + alias).join(", ") : t("no_aliases");
    const translatedUsage = localizedUsage(command);
    const usage = "/" + command.n + (translatedUsage ? " " + translatedUsage : "");
    openCommandName = command.n;

    dialogContent.innerHTML = [
      '<div class="dialog-head">',
      '  <span class="source-badge ' + badgeClass(command) + '">' + escapeHtml(badgeText(command)) + '</span>',
      '  <h2 id="dialog-title">/' + escapeHtml(command.n) + '</h2>',
      '  <p>' + escapeHtml(commandDescription(command)) + '</p>',
      '</div>',
      '<div class="dialog-body">',
      '  <div class="dialog-row"><span>' + escapeHtml(t("syntax")) + '</span><code>' + escapeHtml(usage) + '</code><button class="copy-permission" type="button" data-copy="' + escapeHtml(usage) + '">' + escapeHtml(t("copy")) + '</button></div>',
      '  <div class="dialog-row"><span>' + escapeHtml(t("aliases")) + '</span><code>' + escapeHtml(aliases) + '</code></div>',
      '  <div class="dialog-row"><span>' + escapeHtml(t("available_from")) + '</span><code>' + escapeHtml(sourceText(command)) + '</code></div>',
      '  <div class="dialog-row">',
      '    <span>' + escapeHtml(t("permissions")) + '</span>',
      '    <div class="permission-list">' + allPermissions.map(permission => "<code>" + escapeHtml(permission) + "</code>").join("") + '</div>',
      '    <button class="copy-permission" type="button" data-copy="' + escapeHtml(allPermissions.join("\n")) + '">' + escapeHtml(t("copy_all")) + '</button>',
      '  </div>',
      '  <div class="dialog-note">' + escapeHtml(t("category")) + ': <strong>' + escapeHtml(categoryText(command)) + '</strong>. ' + escapeHtml(t("dialog_note")) + '</div>',
      '</div>'
    ].join("");
    if (!dialog.open) dialog.showModal();
  }

  grid.addEventListener("click", event => {
    const card = event.target.closest("[data-command]");
    if (card) openCommand(card.dataset.command);
  });

  search.addEventListener("input", render);

  document.querySelectorAll(".filter").forEach(button => {
    button.addEventListener("click", () => {
      document.querySelector(".filter.active")?.classList.remove("active");
      button.classList.add("active");
      activeFilter = button.dataset.filter;
      render();
    });
  });

  clearButton.addEventListener("click", () => {
    search.value = "";
    activeFilter = "all";
    document.querySelector(".filter.active")?.classList.remove("active");
    document.querySelector('[data-filter="all"]').classList.add("active");
    render();
    search.focus();
  });

  const heroForm = document.querySelector("#hero-search");
  const heroQuery = document.querySelector("#hero-query");
  heroForm.addEventListener("submit", event => {
    event.preventDefault();
    search.value = heroQuery.value;
    activeFilter = "all";
    document.querySelector(".filter.active")?.classList.remove("active");
    document.querySelector('[data-filter="all"]').classList.add("active");
    render();
    document.querySelector("#commands").scrollIntoView({behavior: "smooth"});
    setTimeout(() => search.focus(), 450);
  });

  document.querySelectorAll("[data-query]").forEach(button => {
    button.addEventListener("click", () => {
      heroQuery.value = button.dataset.query;
      heroForm.requestSubmit();
    });
  });

  document.addEventListener("keydown", event => {
    const isTyping = /INPUT|TEXTAREA/.test(document.activeElement?.tagName);
    if (event.key === "/" && !isTyping && !dialog.open) {
      event.preventDefault();
      search.focus();
      document.querySelector("#commands").scrollIntoView({behavior: "smooth"});
    }
    if (event.key === "Escape" && dialog.open) dialog.close();
  });

  document.addEventListener("click", event => {
    const copyButton = event.target.closest("[data-copy]");
    if (copyButton) copyText(copyButton.dataset.copy);
  });

  document.querySelector(".dialog-close").addEventListener("click", () => dialog.close());
  dialog.addEventListener("click", event => {
    if (event.target === dialog) dialog.close();
  });
  dialog.addEventListener("close", () => { openCommandName = ""; });

  languageSelect.addEventListener("change", () => applyLanguage(languageSelect.value));

  const navToggle = document.querySelector(".nav-toggle");
  const nav = document.querySelector(".site-nav");
  navToggle.addEventListener("click", () => {
    const open = nav.classList.toggle("open");
    navToggle.setAttribute("aria-expanded", String(open));
  });
  nav.addEventListener("click", () => {
    nav.classList.remove("open");
    navToggle.setAttribute("aria-expanded", "false");
  });

  const wikiArticles = [...document.querySelectorAll(".wiki-content article")];
  const wikiLinks = [...document.querySelectorAll(".wiki-nav a")];
  const observer = new IntersectionObserver(entries => {
    const visible = entries.filter(entry => entry.isIntersecting).sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];
    if (!visible) return;
    wikiLinks.forEach(link => link.classList.toggle("active", link.hash === "#" + visible.target.id));
  }, {rootMargin: "-20% 0px -65%", threshold: [0, .25, .5]});
  wikiArticles.forEach(article => observer.observe(article));

  document.querySelector("#stat-commands").textContent = commands.length;
  applyLanguage(currentLanguage, false);
})();
