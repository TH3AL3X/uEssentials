(() => {
  const supported = ["es", "en", "pt", "ru"];
  let saved = "";
  try { saved = localStorage.getItem("uessentials-language") || ""; } catch {}
  const browser = (navigator.language || "es").slice(0, 2).toLowerCase();
  let language = supported.includes(saved) ? saved : supported.includes(browser) ? browser : "es";
  const select = document.querySelector("#language-select");
  const translate = key => window.UESSENTIALS_I18N?.[language]?.[key] ?? window.UESSENTIALS_I18N?.es?.[key] ?? key;

  function apply(next, persist = true) {
    language = supported.includes(next) ? next : "es";
    document.documentElement.lang = language === "pt" ? "pt-BR" : language;
    document.title = translate(document.body.dataset.titleKey || "title");
    document.querySelectorAll("[data-i18n]").forEach(element => {
      element.textContent = translate(element.dataset.i18n);
    });
    document.querySelectorAll("[data-i18n-html]").forEach(element => {
      element.innerHTML = translate(element.dataset.i18nHtml);
    });
    document.querySelectorAll("[data-i18n-aria-label]").forEach(element => {
      element.setAttribute("aria-label", translate(element.dataset.i18nAriaLabel));
    });
    if (select) select.value = language;
    if (persist) {
      try { localStorage.setItem("uessentials-language", language); } catch {}
    }
  }

  select?.addEventListener("change", () => apply(select.value));
  apply(language, false);
})();
