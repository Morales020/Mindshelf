# MindShelf Technical Architecture

## System Overview

MindShelf is built using modern .NET technologies following Clean Architecture principles. The system is designed for scalability, maintainability, and high performance.

---

## 🏗️ Architecture Layers

### Presentation Layer (MindShelf_PL)
- **Technology**: ASP.NET Core 8.0 MVC
- **Purpose**: Handles user interface and HTTP requests
- **Components**:
  - Controllers for handling HTTP requests
  - Razor Views for rendering UI
  - Static files (CSS, JS, Images)
  - Authentication middleware

### Business Logic Layer (MindShelf_BL)
- **Purpose**: Contains business rules and application logic
- **Components**:
  - Services for business operations
  - DTOs for data transfer
  - Interfaces for dependency injection
  - Unit of Work pattern implementation

### Data Access Layer (MindShelf_DAL)
- **Technology**: Entity Framework Core with SQL Server
- **Purpose**: Manages data persistence and database operations
- **Components**:
  - Entity models
  - DbContext configuration
  - Database migrations
  - Repository implementations

---

## 🗄️ Database Design

### Core Entities

#### User Management
```sql
Users (IdentityUser)
├── Address
├── Gender
├── DateOfBirth
├── ProfileImageUrl
└── Relationships: ShoppingCart, Orders, EventRegistrations, FavouriteBooks
```

#### Book Management
```sql
Books
├── BookId (PK)
├── Title
├── Description
├── Price
├── PublishedDate
├── ImageUrl
├── ReviewCount
├── Rating
├── Stock
├── State (Available, Reserved, Borrowed, OutOfStock)
├── CategoryId (FK)
├── AuthorId (FK)
└── Relationships: Reviews, CartItems, OrderItems, FavouriteBooks
```

#### Order Management
```sql
Orders
├── OrderId (PK)
├── OrderDate
├── UserId (FK)
├── UserName
├── TotalAmount
├── Discount
├── Address
├── State (Pending, Processing, Confirmed, etc.)
└── Relationships: OrderItems, Payments
```

#### Event Management
```sql
Events
├── EventId (PK)
├── Title
├── Description
├── StartingDate
├── EndingDate
├── Location
├── IsOnline
├── IsActive
└── Relationships: EventRegistrations
```

### Database Relationships

```mermaid
erDiagram
    User ||--o{ Order : places
    User ||--|| ShoppingCart : has
    User ||--o{ EventRegistration : registers
    User ||--o{ FavouriteBook : favorites
    User ||--o{ Review : writes
    
    Book ||--o{ Review : receives
    Book ||--o{ CartItem : "added to cart"
    Book ||--o{ OrderItem : "ordered in"
    Book ||--o{ FavouriteBook : "favorited by"
    Book }o--|| Category : belongs_to
    Book }o--|| Author : written_by
    
    Order ||--o{ OrderItem : contains
    Order ||--o{ Payment : paid_by
    
    Event ||--o{ EventRegistration : has_registrations
```

---

## 🔧 Technology Stack

### Backend Technologies
- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core
- **Database**: SQL Server (Development) / PostgreSQL (Production)
- **Authentication**: ASP.NET Core Identity + Google OAuth
- **Payment**: Stripe API
- **Logging**: Serilog
- **Validation**: FluentValidation

### Frontend Technologies
- **UI Framework**: Razor Views
- **CSS Framework**: Bootstrap 5
- **JavaScript**: Vanilla JS + jQuery
- **Icons**: Font Awesome
- **Responsive Design**: Mobile-first approach

### DevOps & Deployment
- **Containerization**: Docker
- **Cloud Platform**: Render.com
- **CI/CD**: GitHub Actions
- **Environment Management**: .env files
- **Monitoring**: Built-in ASP.NET Core logging

---

## 🔐 Security Architecture

### Authentication & Authorization
```csharp
// Identity Configuration
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<MindShelfDbContext>();

// Google OAuth
builder.Services.AddAuthentication()
    .AddGoogle(options => {
        options.ClientId = configuration["Authentication:Google:ClientId"];
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
    });
```

### Security Measures
- **Password Hashing**: ASP.NET Core Identity default hashing
- **CSRF Protection**: Anti-forgery tokens
- **SQL Injection Prevention**: Entity Framework parameterized queries
- **XSS Protection**: Input validation and output encoding
- **HTTPS Enforcement**: SSL/TLS encryption
- **Environment Variables**: Sensitive data protection

---

## 📊 Performance Optimization

### Database Optimization
- **Indexing**: Strategic indexes on frequently queried columns
- **Connection Pooling**: Entity Framework connection pooling
- **Query Optimization**: Efficient LINQ queries
- **Caching**: In-memory caching for frequently accessed data

### Application Performance
- **Async/Await**: Non-blocking operations
- **Dependency Injection**: Efficient object lifecycle management
- **Response Compression**: Gzip compression for static files
- **CDN**: Content delivery network for static assets

### Monitoring & Logging
```csharp
// Logging Configuration
builder.Services.AddLogging(loggingBuilder => {
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
    loggingBuilder.AddSerilog();
});
```

---

## 🔄 Data Flow Architecture

### Request Processing Flow
```mermaid
sequenceDiagram
    participant Client
    participant Controller
    participant Service
    participant Repository
    participant Database
    
    Client->>Controller: HTTP Request
    Controller->>Service: Business Logic Call
    Service->>Repository: Data Access Call
    Repository->>Database: SQL Query
    Database-->>Repository: Data
    Repository-->>Service: Entity
    Service-->>Controller: DTO
    Controller-->>Client: HTTP Response
```

### Service Layer Pattern
```csharp
public interface IBookService
{
    Task<Response<BookResponseDto>> GetBookByIdAsync(int id);
    Task<Response<IEnumerable<BookResponseDto>>> GetAllBooksAsync();
    Task<Response<BookResponseDto>> CreateBookAsync(CreateBookDto dto);
    Task<Response<BookResponseDto>> UpdateBookAsync(int id, UpdateBookDto dto);
    Task<Response<bool>> DeleteBookAsync(int id);
}
```

---

## 🏛️ Design Patterns

### Repository Pattern
```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
```

### Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<Book> Books { get; }
    IRepository<Author> Authors { get; }
    IRepository<Category> Categories { get; }
    Task<int> SaveChangesAsync();
}
```

### Dependency Injection
```csharp
// Service Registration
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
```

---

## 🌐 API Architecture

### RESTful Design Principles
- **Resource-Based URLs**: `/api/books`, `/api/orders`
- **HTTP Methods**: GET, POST, PUT, DELETE
- **Status Codes**: Proper HTTP status code usage
- **Content Negotiation**: JSON response format

### Response Format
```json
{
  "data": { ... },
  "isSuccess": true,
  "message": "Operation completed successfully",
  "errors": []
}
```

---

## 🔧 Configuration Management

### Environment-Specific Settings
```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "Cs": "Server=(localdb)\\mssqllocaldb;Database=MindShelfDev;Trusted_Connection=true;"
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_..."
  }
}
```

### Environment Variables
```bash
# Production Environment
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Cs=postgresql://user:pass@host:port/db
STRIPE_SECRET_KEY=sk_live_...
GOOGLE_CLIENT_ID=your_client_id
```

---

## 🚀 Deployment Architecture

### Docker Configuration
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MindShelf_PL/MindShelf_PL.csproj", "MindShelf_PL/"]
RUN dotnet restore "MindShelf_PL/MindShelf_PL.csproj"
COPY . .
WORKDIR "/src/MindShelf_PL"
RUN dotnet build "MindShelf_PL.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MindShelf_PL.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MindShelf_PL.dll"]
```

### Render.com Deployment
- **Platform**: Render.com Web Service
- **Database**: PostgreSQL (managed)
- **Environment**: Production
- **SSL**: Automatic HTTPS
- **Scaling**: Auto-scaling based on traffic

---

## 📈 Scalability Considerations

### Horizontal Scaling
- **Load Balancing**: Multiple application instances
- **Database Sharding**: Partition data across multiple databases
- **CDN**: Global content delivery
- **Microservices**: Break into smaller services

### Vertical Scaling
- **Server Resources**: Increase CPU, memory, storage
- **Database Optimization**: Better hardware, indexing
- **Caching**: Redis for session and data caching
- **Connection Pooling**: Optimize database connections

---

## 🔍 Monitoring & Observability

### Application Monitoring
- **Health Checks**: Built-in ASP.NET Core health checks
- **Performance Counters**: CPU, memory, request metrics
- **Error Tracking**: Exception logging and monitoring
- **User Analytics**: User behavior and usage patterns

### Database Monitoring
- **Query Performance**: Slow query identification
- **Connection Monitoring**: Active connections tracking
- **Storage Usage**: Database size and growth monitoring
- **Backup Status**: Automated backup verification

---

## 🧪 Testing Strategy

### Unit Testing
```csharp
[Test]
public async Task GetBookById_ValidId_ReturnsBook()
{
    // Arrange
    var bookId = 1;
    var expectedBook = new Book { BookId = bookId, Title = "Test Book" };
    
    // Act
    var result = await _bookService.GetBookByIdAsync(bookId);
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(expectedBook.Title, result.Data.Title);
}
```

### Integration Testing
- **API Testing**: Test complete request/response cycles
- **Database Testing**: Test data persistence and retrieval
- **Authentication Testing**: Test login and authorization flows

### Performance Testing
- **Load Testing**: Simulate high user traffic
- **Stress Testing**: Test system limits
- **Database Performance**: Query optimization testing

---

## 📚 Development Guidelines

### Code Standards
- **C# Coding Conventions**: Microsoft coding standards
- **SOLID Principles**: Object-oriented design principles
- **Clean Code**: Readable and maintainable code
- **Documentation**: Comprehensive code documentation

### Git Workflow
- **Feature Branches**: Separate branches for features
- **Pull Requests**: Code review process
- **Continuous Integration**: Automated testing
- **Deployment Pipeline**: Automated deployment

---

**Architecture designed for scalability, maintainability, and performance** 🚀

*This technical architecture supports MindShelf's current implementation and design principles.*
