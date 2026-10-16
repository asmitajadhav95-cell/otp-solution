# One-Time Password (OTP) Solution

.NET 10 Web API (Clean Architecture) + React 19 front end.
Generates a 6-digit OTP from User Id + DateTime, valid for 30 seconds, one-time use, max 3 wrong attempts.

## Run the API
```bash
dotnet watch --project src/Otp.Api
```
Swagger: http://localhost:5000/swagger

## Run the front end
```bash
cd otp-client
npm install
npm run dev
```
App: http://localhost:5173

## Run tests
```bash
dotnet test
```

## Assumptions
- DateTime must be within the last 30 seconds (future times rejected)
- Validation needs only User Id + OTP
- OTP is invalid after one successful use or 3 wrong attempts
- In-memory storage (swap for Redis in production)