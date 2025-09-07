# MindShelf API Documentation

## Overview
MindShelf provides a comprehensive RESTful API for managing books, users, orders, events, and reviews. This document outlines all available endpoints and their usage.

## Base URL
- **Development**: `https://localhost:7000`
- **Production**: `https://mindshelf-web.onrender.com`

## Authentication
Most endpoints require authentication. Use either:
- **Session-based authentication** (for web interface)
- **Google OAuth** (for third-party integrations)

## API Endpoints

### 📚 Books

#### Get All Books
```http
GET /Books
```
**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)
- `category` (string): Filter by category
- `author` (string): Filter by author
- `minPrice` (decimal): Minimum price filter
- `maxPrice` (decimal): Maximum price filter
- `rating` (double): Minimum rating filter

**Response:**
```json
{
  "statusCode": 200,
  "message": "Books retrieved successfully",
  "data": [
    {
      "bookId": 1,
      "title": "Sample Book",
      "description": "Book description",
      "price": 29.99,
      "authorName": "John Doe",
      "categoryName": "Fiction",
      "imageUrl": "/images/book1.jpg",
      "reviewCount": 15,
      "rating": 4.5,
      "publishedDate": "2024-01-01T00:00:00Z",
      "state": "Available"
    }
  ],
  "success": true,
  "totalPages": 10
}
```

#### Get Book by ID
```http
GET /Books/{id}
```

#### Create Book (Admin Only)
```http
POST /Books
Content-Type: application/json

{
  "title": "New Book",
  "description": "Book description",
  "price": 29.99,
  "publishedDate": "2024-01-01T00:00:00Z",
  "imageUrl": "/images/newbook.jpg",
  "stock": 10,
  "categoryId": 1,
  "authorId": 1
}
```

#### Update Book (Admin Only)
```http
PUT /Books/{id}
Content-Type: application/json
```

#### Delete Book (Admin Only)
```http
DELETE /Books/{id}
```

### 👥 Users & Authentication

#### Register User
```http
POST /Account/Register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "address": "123 Main St",
  "gender": "M",
  "dateOfBirth": "1990-01-01T00:00:00Z"
}
```

#### Login User
```http
POST /Account/Login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

#### Google OAuth Login
```http
GET /signin-google
```

### 🛒 Shopping Cart

#### Get User Cart
```http
GET /Cart
```

#### Add Item to Cart
```http
POST /Cart/Add
Content-Type: application/json

{
  "bookId": 1,
  "quantity": 2
}
```

#### Update Cart Item
```http
PUT /Cart/Update
Content-Type: application/json

{
  "cartItemId": 1,
  "quantity": 3
}
```

#### Remove Item from Cart
```http
DELETE /Cart/Remove
Content-Type: application/json

{
  "cartItemId": 1
}
```

#### Clear Cart
```http
DELETE /Cart/Clear
```

### 📦 Orders

#### Get User Orders
```http
GET /Order
```

#### Create Order
```http
POST /Order/Create
Content-Type: application/json

{
  "address": "123 Main St, City, Country",
  "paymentMethod": "stripe"
}
```

#### Get Order Details
```http
GET /Order/{id}
```

#### Update Order Status (Admin Only)
```http
PUT /Order/{id}/Status
Content-Type: application/json

{
  "state": "Processing"
}
```

### ⭐ Reviews

#### Get Book Reviews
```http
GET /Review/Book/{bookId}
```

#### Create Review
```http
POST /Review/Create
Content-Type: application/json

{
  "bookId": 1,
  "rating": 5,
  "comment": "Excellent book!"
}
```

#### Update Review
```http
PUT /Review/{id}
Content-Type: application/json

{
  "rating": 4,
  "comment": "Updated review"
}
```

#### Delete Review
```http
DELETE /Review/{id}
```

### 📅 Events

#### Get All Events
```http
GET /Event
```

#### Get Event Details
```http
GET /Event/{id}
```

#### Create Event (Admin Only)
```http
POST /Event
Content-Type: application/json

{
  "title": "Book Launch Event",
  "description": "Join us for the book launch",
  "startingDate": "2024-02-01T18:00:00Z",
  "endingDate": "2024-02-01T20:00:00Z",
  "location": "Main Library",
  "isOnline": false
}
```

#### Register for Event
```http
POST /Event/{id}/Register
```

#### Get Event Registrations
```http
GET /Event/{id}/Registrations
```

### ❤️ Favorites

#### Get User Favorites
```http
GET /FavouriteBook
```

#### Add to Favorites
```http
POST /FavouriteBook/Add
Content-Type: application/json

{
  "bookId": 1
}
```

#### Remove from Favorites
```http
DELETE /FavouriteBook/{id}
```

### 🏷️ Categories

#### Get All Categories
```http
GET /Category
```

#### Get Category Details
```http
GET /Category/{id}
```

#### Create Category (Admin Only)
```http
POST /Category
Content-Type: application/json

{
  "name": "Fiction",
  "description": "Fictional literature"
}
```

### 👨‍💼 Authors

#### Get All Authors
```http
GET /Author
```

#### Get Author Details
```http
GET /Author/{id}
```

#### Create Author (Admin Only)
```http
POST /Author
Content-Type: application/json

{
  "name": "John Doe",
  "biography": "Author biography",
  "imageUrl": "/images/author1.jpg"
}
```

### 💳 Payment

#### Process Payment
```http
POST /Payment/Process
Content-Type: application/json

{
  "orderId": 1,
  "amount": 29.99,
  "currency": "USD",
  "paymentMethodId": "pm_card_visa"
}
```

#### Get Payment History
```http
GET /Payment/History
```

## Response Format

MindShelf uses a standardized `ResponseMVC<T>` format for all API responses to ensure consistency and better error handling.

### ResponseMVC Structure
```csharp
public class ResponseMVC<T>
{
    public int StatusCode { get; set; }           // HTTP status code
    public string Message { get; set; }             // Response message
    public T Data { get; set; }                   // Response data
    public bool Success { get; set; }              // Computed property: StatusCode >= 200 && StatusCode < 300
    public int? TotalPages { get; set; }            // For paginated responses

    // Helper methods for creating responses
    public static ResponseMVC<T> SuccessResponse(T data, string message = "Operation completed successfully", int statusCode = 200)
    public static ResponseMVC<T> ErrorResponse(string message, int statusCode = 400)
}
```

### Success Response
```json
{
  "statusCode": 200,
  "message": "Operation completed successfully",
  "data": {
    "bookId": 1,
    "title": "Sample Book",
    "description": "Book description",
    "price": 29.99,
    "authorName": "John Doe",
    "categoryName": "Fiction",
    "imageUrl": "/images/book1.jpg",
    "reviewCount": 15,
    "rating": 4.5,
    "publishedDate": "2024-01-01T00:00:00Z",
    "state": "Available"
  },
  "success": true,
  "totalPages": null
}
```

### Success Response with Pagination
```json
{
  "statusCode": 200,
  "message": "Books retrieved successfully",
  "data": [
    {
      "bookId": 1,
      "title": "Book 1",
      "price": 29.99,
      "rating": 4.5
    },
    {
      "bookId": 2,
      "title": "Book 2", 
      "price": 19.99,
      "rating": 4.2
    }
  ],
  "success": true,
  "totalPages": 5
}
```

### Error Response
```json
{
  "statusCode": 400,
  "message": "Invalid request data",
  "data": null,
  "success": false,
  "totalPages": null
}
```

### Not Found Response
```json
{
  "statusCode": 404,
  "message": "Book not found",
  "data": null,
  "success": false,
  "totalPages": null
}
```

### Server Error Response
```json
{
  "statusCode": 500,
  "message": "Internal server error occurred",
  "data": null,
  "success": false,
  "totalPages": null
}
```

### Using ResponseMVC Helper Methods

The `ResponseMVC<T>` class provides static helper methods for creating standardized responses:

#### Success Response Helper
```csharp
// Basic success response
var response = ResponseMVC<BookResponseDto>.SuccessResponse(bookData);

// Success response with custom message
var response = ResponseMVC<BookResponseDto>.SuccessResponse(bookData, "Book retrieved successfully");

// Success response with custom status code
var response = ResponseMVC<BookResponseDto>.SuccessResponse(bookData, "Book created", 201);
```

#### Error Response Helper
```csharp
// Basic error response (400 Bad Request)
var response = ResponseMVC<BookResponseDto>.ErrorResponse("Invalid book data");

// Error response with custom status code
var response = ResponseMVC<BookResponseDto>.ErrorResponse("Book not found", 404);
var response = ResponseMVC<BookResponseDto>.ErrorResponse("Unauthorized access", 401);
var response = ResponseMVC<BookResponseDto>.ErrorResponse("Internal server error", 500);
```

#### Paginated Response Example
```csharp
var books = await GetBooksAsync(page, pageSize);
var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

return new ResponseMVC<IEnumerable<BookResponseDto>>
{
    StatusCode = 200,
    Message = "Books retrieved successfully",
    Data = books,
    TotalPages = totalPages
};
```

## Status Codes

- `200 OK`: Request successful
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Rate Limiting

- **General API**: 1000 requests per hour per user
- **Authentication**: 10 requests per minute per IP
- **Payment**: 100 requests per hour per user

## SDKs and Libraries

### .NET Client
```csharp
var client = new MindShelfClient("https://api.mindshelf.com");
var books = await client.Books.GetAllAsync();
```

### JavaScript Client
```javascript
const client = new MindShelfClient('https://api.mindshelf.com');
const books = await client.books.getAll();
```

## Webhooks

### Order Status Updates
```http
POST /webhooks/order-status
Content-Type: application/json

{
  "event": "order.updated",
  "data": {
    "orderId": 1,
    "status": "Shipped",
    "timestamp": "2024-01-01T12:00:00Z"
  }
}
```

## Support

For API support and questions:
- **Email**: api-support@mindshelf.com
- **Documentation**: [docs.mindshelf.com](https://docs.mindshelf.com)
- **Status Page**: [status.mindshelf.com](https://status.mindshelf.com)
