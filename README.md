# Auth Middleware Demo (Attribute Filter + JSON Check UI)

This project demonstrates authentication checks using **controller attributes + `IAsyncActionFilter`**, not runtime auth middleware.

## Current Auth Approach

- Active runtime auth path: **Filters on controllers**
- Inactive (legacy only): `CookieMiddleware`, `SessionMiddleware`, `JwtTokenMiddleware`
- Why: only controller endpoints are checked, static files are not scanned by custom auth logic.

## Filters Used

- `CookieAuthFilter` -> checks `Request.Cookies["email"]`
- `SessionAuthFilter` -> checks `HttpContext.Session.GetString("email")`
- `JwtCookieAuthFilter` -> checks `jwt_token` cookie, validates claims, and refreshes token cookie

Applied by controller attributes:

- `[CookieAuth]` on `CookieTestController`
- `[SessionAuth]` on `SessionTestController`
- `[JwtAuth]` on `JwtTestController`

`[AllowAnonymous]` is still respected for setup/login endpoints.

## JSON Demo Contract

Protected endpoints return JSON (success case):

```json
{
  "success": true,
  "filter": "cookie|session|jwt",
  "message": "...",
  "timestampUtc": "2026-03-15T...Z"
}
```

Filter failure for demo checks returns `401` JSON (no redirect):

```json
{
  "success": false,
  "filter": "cookie|session|jwt",
  "message": "Beginner-friendly reason",
  "statusCode": 401,
  "timestampUtc": "2026-03-15T...Z"
}
```

## Demo Endpoints

### Cookie
- `GET /CookieTest/SetupCookie` (`[AllowAnonymous]`)
- `GET /CookieTest/Protected` (`[CookieAuth]`)

### Session
- `GET /SessionTest/SetupSession` (`[AllowAnonymous]`)
- `GET /SessionTest/Protected` (`[SessionAuth]`)

### JWT
- `GET /JwtTest/SetupJwt` (`[AllowAnonymous]`)
- `GET /JwtTest/Protected` (`[JwtAuth]`)

## Home Dashboard UI (Tailwind)

The Home page includes three interactive buttons:

- **Try Cookie Filter**
- **Try Session Filter**
- **Try JWT Filter**

Behavior:

1. Click a button -> page calls `fetch()` with `Accept: application/json`.
2. Response is parsed for both success (`200`) and failure (`401`).
3. Status card updates with:
   - Green success or red failure color
   - SVG success/fail icon
   - Message from JSON
   - Last checked time/filter/status
4. **Retry** button reruns the most recent check.
5. Buttons are temporarily disabled while request is running.

No full-page navigation is used for these checks.

## Quick Start

1. Run app:
   ```bash
   dotnet run --project AuthMiddlware/AuthMiddlware.csproj
   ```
2. Open sign-in page:
   - `/SignIn/Index`
3. Use one-click demo sign-in (prefilled credentials).
4. On Home, click Try Filter buttons and observe JSON-driven status card.

## Tests

Run integration tests:

```bash
dotnet test AuthMiddlware.Tests/AuthMiddlware.Tests.csproj --no-restore
```

What is validated:

- Protected checks return `401` JSON before setup
- Setup then protected returns `200` JSON
- JWT protected check refreshes `jwt_token` cookie
- Static file `/css/site.css` is served normally

## Notes

- Target framework is currently `net7.0` (out-of-support warning appears during build/test).
- Middleware files are kept intentionally for legacy/reference and rollback history.
