# ══════════════════════════════════════════════════════
#  TaskFlow — Multi-Stage Production Dockerfile
#  Stage 1: Build React frontend
#  Stage 2: Build .NET backend
#  Stage 3: Lightweight runtime
# ══════════════════════════════════════════════════════

# ── Stage 1: Build Frontend ──
FROM node:22-alpine AS frontend-build
WORKDIR /app/frontend

ARG VITE_GOOGLE_CLIENT_ID
ENV VITE_GOOGLE_CLIENT_ID=$VITE_GOOGLE_CLIENT_ID

# Install dependencies first (layer caching)
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci --silent

# Build production bundle
COPY frontend/ ./
RUN npm run build

# ── Stage 2: Build Backend ──
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /app

# Restore dependencies first (layer caching)
COPY backend/ToDoApp.sln ./
COPY backend/ToDoApp.Api/ToDoApp.Api.csproj ./ToDoApp.Api/
RUN dotnet restore ./ToDoApp.Api/ToDoApp.Api.csproj

# Build and publish
COPY backend/ ./
RUN dotnet publish ./ToDoApp.Api/ToDoApp.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 3: Runtime ──
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Create data directory for SQLite volume mount
RUN mkdir -p /data

# Copy published backend
COPY --from=backend-build /app/publish ./

# Copy frontend build output into wwwroot for static file serving
COPY --from=frontend-build /app/frontend/dist ./wwwroot

# Fly.io expects port 8080 by default
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/data/todos.db"

EXPOSE 8080

ENTRYPOINT ["dotnet", "ToDoApp.Api.dll"]
