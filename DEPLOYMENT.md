# MindShelf Deployment Guide

## Deploying to Render

This guide will help you deploy your MindShelf application to Render.

### Prerequisites

1. A GitHub account with your MindShelf repository
2. A Render account (free tier available)
3. Environment variables for production

### Step 1: Prepare Your Repository

1. **Push your changes to GitHub:**
   ```bash
   git add .
   git commit -m "Prepare for Render deployment"
   git push origin deploy
   ```

### Step 2: Create Render Account and Connect Repository

1. Go to [render.com](https://render.com) and sign up/login
2. Click "New +" and select "Web Service"
3. Connect your GitHub account and select your MindShelf repository
4. Choose the `deploy` branch

### Step 3: Configure Render Service

**Basic Settings:**
- **Name**: `mindshelf-web`
- **Environment**: `Docker`
- **Region**: Choose closest to your users
- **Branch**: `deploy`
- **Root Directory**: Leave empty (uses root)

**Build & Deploy Settings:**
- **Build Command**: `cd MindShelf_PL/MindShelf_PL && dotnet publish -c Release -o out`
- **Start Command**: `cd MindShelf_PL/MindShelf_PL/out && dotnet MindShelf_PL.dll`

### Step 4: Set Environment Variables

In Render dashboard, go to Environment tab and add:

**Required Variables:**
```
ASPNETCORE_ENVIRONMENT=Production
```

**Database Connection:**
```
ConnectionStrings__Cs=<your_production_database_connection_string>
```

**Authentication (Google OAuth):**
```
Authentication__Google__ClientId=<your_google_client_id>
Authentication__Google__ClientSecret=<your_google_client_secret>
```

**Stripe (if using payments):**
```
Stripe__SecretKey=<your_stripe_secret_key>
Stripe__PublishableKey=<your_stripe_publishable_key>
```

**Note:** The application automatically detects the PORT environment variable set by Render and configures itself accordingly. No hardcoded URLs are used.

### Step 5: Database Setup

**Option 1: Use Render PostgreSQL (Recommended)**
1. In Render dashboard, create a new PostgreSQL database
2. Copy the connection string and use it as `ConnectionStrings__Cs`
3. The database will be automatically created when the app starts

**Option 2: Use External Database**
- Update the connection string to point to your external database

### Step 6: Deploy

1. Click "Create Web Service"
2. Render will automatically build and deploy your application
3. Monitor the build logs for any issues
4. Once deployed, you'll get a URL like `https://mindshelf-web.onrender.com`

### Step 7: Configure Google OAuth

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create/select your project
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add your Render URL to authorized redirect URIs:
   - `https://your-app-name.onrender.com/signin-google`
6. Update the environment variables in Render with your Google credentials

### Troubleshooting

**Common Issues:**

1. **Build Fails**: Check build logs for missing dependencies
2. **Database Connection Issues**: Verify connection string format
3. **Static Files Not Loading**: Ensure `wwwroot` folder is included
4. **Authentication Issues**: Check Google OAuth configuration

**Logs:**
- Check Render dashboard logs for detailed error information
- Use `Console.WriteLine()` for debugging in production

### Environment-Specific Notes

- **Development**: 
  - Uses local SQL Server (unchanged from your current setup)
  - Runs on localhost:5068 (unchanged)
  - Uses HTTPS redirection (unchanged)
  - All your existing local configuration remains intact
- **Production**: 
  - Uses PostgreSQL (Render's managed database)
  - Automatically detects PORT environment variable
  - No HTTPS redirection (handled by Render)
  - File uploads stored in `/app/wwwroot/Images/` directory

### Cost Considerations

- **Free Tier**: 750 hours/month, sleeps after 15 minutes of inactivity
- **Paid Plans**: Start at $7/month for always-on service
- **Database**: Free PostgreSQL up to 1GB

### Security Notes

- Never commit sensitive data to repository
- Use environment variables for all secrets
- Enable HTTPS (automatic with Render)
- Regularly update dependencies

### Monitoring

- Use Render's built-in monitoring
- Set up alerts for downtime
- Monitor database usage
- Track application performance

For more help, check [Render Documentation](https://render.com/docs) or contact support.
