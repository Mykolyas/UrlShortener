# URL Shortener Application

A full-featured URL shortening application built with ASP.NET Core MVC, Angular, and Entity Framework Code First.

## Features

- **URL Shortening**: Convert long URLs to short 6-character codes using Base62 encoding
- **User Authentication**: Login system with Admin and regular user roles
- **Authorization**:
  - Anonymous users: View table and use shortened URLs
  - Regular users: Add URLs, view details, delete own URLs
  - Admin users: Full access including deleting all URLs
- **Real-time Updates**: React-powered table with instant updates without page reload
- **Click Tracking**: Monitor how many times each shortened URL is accessed
- **About Page**: Editable content page (admin-only editing)

## Technical Stack

- **Backend**: ASP.NET Core MVC 8.0
- **Frontend**: React 18 (separate project with Vite), Razor Views for static pages
- **Database**: SQL Server with Entity Framework Core Code First
- **Authentication**: ASP.NET Core Identity
- **Testing**: xUnit with Moq for unit tests
- **Build Tool**: Vite for React bundling

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

7. Navigate to `https://localhost:5001` or `http://localhost:5000`

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
├── Models/              # Data Models
│   ├── ApplicationUser.cs
│   ├── ShortUrl.cs
│   └── AboutContent.cs
├── Data/                # Entity Framework DbContext
│   └── ApplicationDbContext.cs
├── Services/            # Business Logic
│   ├── IUrlShortenerService.cs
│   └── UrlShortenerService.cs
├── Views/               # Razor Views
│   ├── Account/
│   ├── ShortUrl/
│   ├── About/
│   └── Shared/
├── wwwroot/            # Static Files (CSS, JS, built React bundle)
│   └── js/dist/        # Built React bundle (generated)
└── Tests/              # Unit Tests
    ├── UrlShortenerServiceTests.cs
    └── ShortUrlControllerTests.cs
```

## URL Shortening Algorithm

The application uses **Base62 encoding** to generate short codes:

1. **Input Validation**: Validates URL format
2. **Duplicate Check**: Ensures URLs are unique
3. **Code Generation**: Creates a random 6-character code using Base62 (0-9, A-Z, a-z)
4. **Uniqueness Verification**: Checks database to ensure code doesn't exist
5. **Storage**: Saves original URL, short code, creator, and timestamp
6. **Redirection**: Maps short code to original URL and tracks clicks

**Possible Combinations**: 62^6 = 56,800,235,584 unique short codes

## Running Tests

```bash
cd Tests
dotnet test
```

## API Endpoints

- `GET /ShortUrl/Index` - Main table view
- `POST /ShortUrl/CreateShortUrl` - Create new short URL (Authorized)
- `DELETE /ShortUrl/Delete?id={id}` - Delete URL (Authorized)
- `GET /ShortUrl/Info/{id}` - View URL details (Authorized)
- `GET /r/{shortCode}` - Redirect to original URL
- `GET /About` - About page
- `POST /About/UpdateContent` - Update about content (Admin only)
- `GET /Account/Login` - Login page
- `POST /Account/Login` - Login action

## Key Features Explained

### Authorization Matrix

| Action | Anonymous | Regular User | Admin |
|--------|-----------|--------------|-------|
| View Table | ✓ | ✓ | ✓ |
| Use Short URLs | ✓ | ✓ | ✓ |
| Add URLs | ✗ | ✓ | ✓ |
| View Details | ✗ | ✓ | ✓ |
| Delete Own URLs | ✗ | ✓ | ✓ |
| Delete All URLs | ✗ | ✗ | ✓ |
| Edit About Page | ✗ | ✗ | ✓ |

### Real-time Updates

The Short URLs Table view uses React 18 to provide:
- Instant URL addition without page reload
- Real-time deletion with confirmation
- Error message display with auto-hide
- Loading states for better UX
- Automatic data refresh when window gains focus
- Periodic updates every 30 seconds
- Click count updates without page reload

## License

This project is created for demonstration purposes.

