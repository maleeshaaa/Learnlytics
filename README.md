# Learnlytics

**Learnlytics** is an AI-powered skill assessment and personalized learning platform built with ASP.NET Core and React.  
This repository contains both the backend API and frontend web application.

---

## Tech Stack

- Backend: ASP.NET Core Web API (.NET 8), C#, Entity Framework Core  
- Frontend: React (Create React App), Axios  
- API Documentation: Swagger  
- Authentication: JWT (planned)  
- Hosting: Local development (future Azure/Render deployment)  

---

## Getting Started (Local Development)

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  
- [Node.js (v16 or v18 LTS)](https://nodejs.org/en/download/)  
- npm (comes with Node.js)  

---

### Backend Setup

1. Navigate to the backend folder:

   ```bash
   cd backend/Learnlytics.API

2. Restore dependencies and run the API:

   ```bash
   dotnet restore
   dotnet run
   
3. The API should be running at: https://localhost:7020 (or your port)
4. Swagger UI is available at https://localhost:7020/swagger



