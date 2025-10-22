# Azure Function App with PostgreSQL and GitHub Pages Frontend

A full-stack serverless application using Azure Functions, PostgreSQL Flexible Server, and GitHub Pages for the frontend.

## Architecture Overview

- **Frontend**: Static HTML/JavaScript hosted on GitHub Pages
- **Backend**: Azure Functions (C# .NET 8.0) with HTTP Trigger
- **Database**: Azure PostgreSQL Flexible Server
- **Monitoring**: Application Insights
- **Storage**: Azure Storage Account

---

## Azure Portal Setup

### 1. Create Resource Group

1. Navigate to Azure Portal
2. Click "Create a resource group"
3. Choose subscription, region, and name
4. Click "Review + Create"

### 2. Create Resources in Resource Group

#### PostgreSQL Flexible Server
1. Search for "Azure Database for PostgreSQL Flexible Server"
2. Create server with desired configuration
3. Note down: **Server name**, **Admin username**, **Password**
4. Configure networking (allow Azure services)
5. Create a database within the server

#### Function App
1. Search for "Function App"
2. Create new Function App
   - Runtime: .NET
   - Version: 8.0
   - Region: Same as resource group
   - Plan type: Consumption (Serverless)
3. Note down: **Default Domain** (e.g., `https://your-function-app.azurewebsites.net`)

#### Application Insights
- Created automatically with Function App, or create manually and link to Function App

#### Storage Account
- Created automatically with Function App, or create manually for additional storage needs

### 3. Configure Function App Settings

1. Go to Function App → **Configuration** → **Connection strings**
2. Add PostgreSQL connection string:
   - **Name**: `PostgresConnection`
   - **Value**: `Host=your-server.postgres.database.azure.com;Database=yourdb;Username=your-admin;Password=your-password;SslMode=Require`
   - **Type**: PostgreSQL

3. Go to Function App → **CORS**
4. Add allowed origins:
   - `http://localhost:5500` (or your local dev port)
   - `https://your-username.github.io` (your GitHub Pages URL)

---

## Backend Development

### Project Structure

```
YourFunctionApp/
├── HttpTrigger.cs          # Main HTTP trigger endpoint
├── PostgreService.cs       # Database query service
├── Visitor.cs              # Data model/entity
├── Program.cs              # Host configuration (HostBuilder)
├── local.settings.json     # Local secrets (not committed)
└── YourFunctionApp.csproj  # Project file
```

### 1. Install NuGet Packages

```bash
dotnet add package Npgsql
dotnet add package Newtonsoft.Json
```

### 2. Configure Files

- **Program.cs**: Configure HostBuilder
- **local.settings.json**: Add local connection strings (add to `.gitignore`)
- **Visitor.cs**: Define the model/entity
- **PostgreService.cs**: Add all database queries
- **HttpTrigger.cs**: Add main backend code and call PostgreService

### 3. Build and Test Locally

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run locally
dotnet run
```

Resolve any bugs and repeat until successful, then test the API.

### 4. Deploy to Azure

```bash
# Build release version
dotnet build --configuration Release

# Publish
dotnet publish --configuration Release

# Deploy from publish folder
cd bin/Release/net8.0/publish
```

Then in Visual Studio Code or Azure Portal: **Deploy to Function App...**

---

## Frontend Setup (GitHub Pages)

### 1. Create Frontend Files

- Create `index.html` with a form containing:
  - Input field for name
  - Input field for email
  - Submit button

### 2. Add Fetch in Script Tag

In the `<script>` section, add `fetch()` call to the Azure Function App Default Domain Address:
- URL: `https://your-function-app.azurewebsites.net/api/YourFunctionName`
- Method: POST
- Headers: Content-Type: application/json
- Body: JSON with name and email

### 3. Deploy to GitHub Pages

1. Create a GitHub repository
2. Push `index.html` to the repository
3. Go to **Settings** → **Pages**
4. Select source branch (usually `main`)
5. Save and wait for deployment
6. Access at: `https://your-username.github.io/your-repo-name`

### 4. Update CORS in Azure Function App

Add your GitHub Pages URL to CORS settings in Azure Portal.

---

## Database Setup

### Create PostgreSQL Table

Connect to your Azure PostgreSQL database and create the necessary table for storing visitor data (id, name, email, created_at).

---

## Testing

### Test Azure Function Locally

Test the endpoint at `http://localhost:7071/api/YourFunctionName` with a POST request containing name and email.

### Test Deployed Function

Test the endpoint at `https://your-function-app.azurewebsites.net/api/YourFunctionName` with a POST request.

---

## Troubleshooting

### Common Issues

1. **CORS Error**: Verify CORS settings in Function App include your frontend URL (both localhost and GitHub Pages)
2. **Connection String Error**: Check connection string format and credentials in Azure Configuration
3. **Database Connection Failed**: Ensure firewall rules allow Azure services
4. **Function Not Found**: Verify function name matches route in HttpTrigger.cs
5. **Build Errors**: Run `dotnet restore` and check NuGet package versions

### View Logs

- **Azure Portal**: Function App → Monitor → Logs
- **Application Insights**: Query logs and track requests
- **Local**: Check terminal output when running locally

---

## Security Best Practices

1. Never commit `local.settings.json` to version control
2. Store connection strings in Azure Function App Configuration (not in code)
3. Use Azure Key Vault for sensitive connection strings (Note! Not used in this application)
4. Validate and sanitize all inputs
5. Use parameterized queries with Npgsql

---

## Development Workflow Summary

1. **Azure Portal**: Create resource group with PostgreSQL, Function App, Application Insights, Storage Account
2. **Function App Settings**: Add connection strings to PostgreSQL
3. **CORS Configuration**: Add localhost and GitHub Pages URLs
4. **Backend Code**: Create HttpTrigger, PostgreService, Visitor model, configure Program.cs
5. **Local Settings**: Add connection strings to local.settings.json
6. **NuGet Packages**: Install Npgsql and Newtonsoft.Json
7. **Build & Test**: Run dotnet restore → build → run, resolve bugs
8. **Deploy**: Publish from bin/Release/net8.0/publish folder
9. **Frontend**: Create HTML form with fetch() to Azure Function URL
10. **GitHub Pages**: Deploy frontend and verify CORS settings

---

## Resources

- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [GitHub Pages Documentation](https://docs.github.com/pages)
- [Azure PostgreSQL Documentation](https://docs.microsoft.com/azure/postgresql/)
