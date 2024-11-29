# ConnectHub - Social Media Application

ConnectHub is a modern, cross-platform social media application built using .NET MAUI for the client application and ASP.NET Core for the backend API. The application enables users to connect, share posts, chat, and manage their profiles in a seamless and intuitive interface.

## Project Structure

The solution consists of three main projects:

1. **ConnectHub.App** - .NET MAUI Client Application
2. **ConnectHub.API** - ASP.NET Core Backend API
3. **ConnectHub.Shared** - Shared DTOs and Models

## Client Application (ConnectHub.App)

### Core Components

#### ViewModels

1. **BaseViewModel.cs**
   - Base class for all ViewModels
   - Implements `INotifyPropertyChanged`
   - Handles common properties like `IsBusy` and `Title`

2. **LoginViewModel.cs**
   - Manages user authentication
   - Methods:
     - `LoginAsync`: Handles user login
     - `ValidateInput`: Validates user credentials
   - Properties for email and password binding

3. **RegisterViewModel.cs**
   - Handles new user registration
   - Methods:
     - `RegisterAsync`: Processes user registration
     - `ValidateInput`: Validates registration data
   - Properties for user registration fields

4. **FeedViewModel.cs**
   - Manages the main feed display
   - Methods:
     - `LoadPostsAsync`: Loads posts with pagination
     - `RefreshAsync`: Refreshes the feed
     - `LikePostAsync`: Handles post liking

5. **ProfileViewModel.cs**
   - Manages user profile display and editing
   - Methods:
     - `LoadProfileAsync`: Loads user profile data
     - `UpdateProfileAsync`: Updates user information

6. **ChatViewModel.cs**
   - Handles real-time chat functionality
   - Methods:
     - `SendMessageAsync`: Sends chat messages
     - `LoadChatHistoryAsync`: Loads previous messages

### Services

1. **ApiService.cs**
   - Implements `IApiService`
   - Handles all HTTP communications with the backend
   - Key Methods:
     - `LoginAsync`: User authentication
     - `RegisterAsync`: User registration
     - `GetFeedAsync`: Retrieves posts
     - `CreatePostAsync`: Creates new posts
     - `UpdateProfileAsync`: Updates user profile
   - Manages authentication token storage and headers

2. **NavigationService.cs**
   - Implements `INavigationService`
   - Handles navigation between pages
   - Methods:
     - `NavigateToAsync`: Navigation to specific pages
     - `GoBackAsync`: Navigation to previous page

### Views

1. **LoginPage.xaml/cs**
   - User login interface
   - Bindings to LoginViewModel

2. **RegisterPage.xaml/cs**
   - User registration interface
   - Bindings to RegisterViewModel

3. **FeedPage.xaml/cs**
   - Main feed display
   - Infinite scrolling implementation
   - Post interaction UI

4. **ProfilePage.xaml/cs**
   - User profile display and editing
   - Profile picture management

5. **ChatPage.xaml/cs**
   - Real-time chat interface
   - Message display and input

### UI Components and Styling

1. **Converters**
   - `StringNotNullOrEmptyConverter`: Validates non-empty strings
   - `BoolToColorConverter`: Converts boolean states to colors
   - `StringLengthToBoolConverter`: Validates text length constraints
   - `BoolToLikeIconConverter`: Toggles between like icon states
   - `DateTimeToRelativeTimeConverter`: Displays relative time (e.g., "2h ago")
   - `InverseBoolConverter`: Inverts boolean values for UI states

2. **Styles (AppStyles.xaml)**
   - Comprehensive dark theme support
   - Post card styles with proper spacing and typography
   - Profile and post image styles
   - Action button styles for interactions
   - Loading and status indicators
   - Character count and post editor styles
   - Consistent color palette across the app

3. **UI Features**
   - Infinite scrolling in feed
   - Image preview with removal option
   - Character count for posts
   - Loading overlays for async operations
   - Status message displays
   - Responsive layout design

### App Shell

**AppShell.xaml.cs**
- Manages application navigation structure
- Handles tab visibility based on authentication state
- Methods:
  - `ShowMainTabs`: Toggles between auth and main tabs
  - `RegisterRoutes`: Registers page routes

## Backend API (ConnectHub.API)

### Controllers

1. **AuthController.cs**
   - Handles authentication endpoints
   - Endpoints:
     - `POST /api/auth/login`
     - `POST /api/auth/register`
     - `POST /api/auth/logout`

2. **PostController.cs**
   - Manages post-related operations
   - Endpoints:
     - `GET /api/post`: Retrieves posts with pagination
     - `POST /api/post`: Creates new post
     - `PUT /api/post/{id}`: Updates post
     - `DELETE /api/post/{id}`: Deletes post

3. **UserController.cs**
   - Manages user-related operations
   - Endpoints:
     - `GET /api/user/{id}`: Gets user profile
     - `PUT /api/user/{id}`: Updates user profile
     - `GET /api/user/{id}/posts`: Gets user's posts

4. **ChatController.cs**
   - Handles chat-related operations
   - Endpoints:
     - `GET /api/chat/history`: Gets chat history
     - `POST /api/chat/message`: Sends message

5. **FilesController.cs**
   - Manages file uploads
   - Endpoints:
     - `POST /api/files/upload`: Uploads files
     - `GET /api/files/{id}`: Retrieves files

### Services

1. **UserService.cs**
   - Business logic for user operations
   - Methods:
     - `CreateUserAsync`
     - `UpdateUserAsync`
     - `GetUserByIdAsync`
     - `ValidateCredentialsAsync`

2. **PostService.cs**
   - Business logic for post operations
   - Methods:
     - `CreatePostAsync`
     - `UpdatePostAsync`
     - `DeletePostAsync`
     - `GetPostsAsync`

3. **ChatService.cs**
   - Business logic for chat operations
   - Methods:
     - `SaveMessageAsync`
     - `GetChatHistoryAsync`

### Data Layer

**ConnectHubContext.cs**
- Entity Framework Core DbContext
- Defines database schema and relationships
- Includes configurations for:
  - Users
  - Posts
  - Messages
  - Files

### Real-time Communication

**ChatHub.cs**
- SignalR hub for real-time chat
- Methods:
  - `SendMessage`: Broadcasts messages to clients
  - `JoinGroup`: Manages chat room membership
  - `LeaveGroup`: Handles disconnection

## Authentication Flow

1. User enters credentials in LoginPage
2. LoginViewModel calls ApiService.LoginAsync
3. ApiService sends request to /api/auth/login
4. AuthController validates credentials and returns JWT token
5. ApiService stores token in preferences
6. AppShell shows main tabs upon successful authentication

## Data Flow

### Post Creation
1. User creates post in FeedPage
2. FeedViewModel calls ApiService.CreatePostAsync
3. ApiService sends POST request to /api/post
4. PostController processes request and saves to database
5. Real-time updates pushed to other users

### Chat
1. User sends message in ChatPage
2. ChatViewModel calls SignalR hub
3. ChatHub broadcasts message to recipients
4. ChatService saves message to database
5. Recipients receive real-time updates

## Error Handling

- ApiService implements comprehensive error handling
- HTTP status codes mapped to appropriate user messages
- Network connectivity monitoring
- Automatic token refresh mechanism
- Retry logic for failed requests

## Dependencies

### Client
- .NET MAUI
- Microsoft.Extensions.DependencyInjection
- System.Net.Http.Json
- Microsoft.AspNetCore.SignalR.Client

### API
- ASP.NET Core
- Entity Framework Core
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.SignalR

## Development Setup

1. Clone repository
2. Restore NuGet packages
3. Update connection string in API appsettings.json
4. Run database migrations
5. Start API project
6. Run MAUI application

## Common Issues and Solutions

1. **Build Errors**
   - Ensure all NuGet packages are restored
   - Verify Android SDK installation
   - Check MAUI workload installation

2. **API Connection Issues**
   - Verify API URL in ApiService
   - Check network connectivity
   - Validate SSL certificates

3. **Authentication Problems**
   - Clear stored preferences
   - Verify token expiration
   - Check API authentication settings
