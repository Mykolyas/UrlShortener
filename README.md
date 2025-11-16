# URL Shortener Application

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Node.js 18+ and npm (for React client)
- SQL Server LocalDB (or SQL Server Express)
- Visual Studio 2022 or VS Code with C# extension

### Installation

1. Clone or extract the project to your local machine

2. Restore NuGet packages:

```bash
dotnet restore
```

3. Install React client dependencies:

```bash
cd ClientApp
npm install
cd ..
```

4. Build React client (for production):

```bash
cd ClientApp
npm run build
cd ..
```

5. Update the connection string in `appsettings.json` if needed (default uses LocalDB)

6. Run the application:

```bash
dotnet run
```


### Development Mode

For development with hot reload:

1. In one terminal, start the React dev server:

```bash
cd ClientApp
npm run dev
```

2. In another terminal, start the ASP.NET app:

```bash
dotnet run
```

The React app will be served from `http://localhost:3000` and proxied to the ASP.NET backend.

### Default Users

The application creates default users on first run:

- **Admin**: 
  - Username: `admin`
  - Password: `admin`
  
- **Regular User**:
  - Username: `user`
  - Password: `user`

## Project Structure

```
UrlShortener/
├── ClientApp/           # React Client Project
│   ├── src/
│   │   ├── UrlShortenerApp.jsx
│   │   └── main.jsx
│   ├── package.json
│   └── vite.config.js
├── Controllers/          # MVC Controllers
│   ├── AccountController.cs
│   ├── ShortUrlController.cs
│   └── AboutController.cs
├── Models/              # Data Models and ViewModels
│   ├── ApplicationUser.cs
│   ├── ShortUrl.cs
│   ├── AboutContent.cs
│   ├── AboutViewModel.cs
│   ├── LoginViewModel.cs
│   └── RegisterViewModel.cs
├── Data/                # Entity Framework DbContext and Seeding
│   ├── ApplicationDbContext.cs
│   └── DataSeeder.cs
├── Services/            # Business Logic
│   ├── IUrlShortenerService.cs
│   └── UrlShortenerService.cs
├── Views/               # Razor Views
│   ├── Account/
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── ShortUrl/
│   │   ├── Index.cshtml
│   │   ├── Info.cshtml
│   │   └── InvalidUrl.cshtml
│   ├── About/
│   │   └── Index.cshtml
│   └── Shared/
│       ├── _Layout.cshtml
│       └── _ValidationScriptsPartial.cshtml
├── wwwroot/            # Static Files (CSS, JS, built React bundle)
│   ├── css/
│   │   └── site.css
│   └── js/
│       └── dist/        # Built React bundle (generated)
│           └── url-shortener.js
├── Tests/              # Unit Tests
│   ├── UrlShortenerServiceTests.cs
│   └── ShortUrlControllerTests.cs
└── Program.cs         # Application entry point and configuration
```



## License

This project is created for demonstration purposes.
