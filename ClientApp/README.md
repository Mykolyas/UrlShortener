# URL Shortener React Client

Окремий React проєкт для Short URLs Table View.

## Встановлення

```bash
npm install
```

## Розробка

Для розробки з hot reload:

```bash
npm run dev
```

Це запустить Vite dev server на порту 3000 з proxy до ASP.NET сервера.

## Збірка для продакшену

```bash
npm run build
```

Це створить оптимізований bundle в `wwwroot/js/dist/url-shortener.js`, який автоматично підключається в Razor View.

## Структура

```
ClientApp/
  src/
    UrlShortenerApp.jsx  # Основний React компонент
    main.jsx              # Точка входу та ініціалізація
  package.json
  vite.config.js
```

## Інтеграція з ASP.NET

Після збірки, bundle автоматично підключається в `Views/ShortUrl/Index.cshtml` через `<script>` тег.

Компонент отримує дані через props, які передаються з Razor View.

