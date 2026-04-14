# WMSLite SaaS Backend (ASP.NET Core + JSON Files)

## 1) Folder structure

```text
WMSLite.Api/
  Controllers/
    AuthController.cs
    BillingController.cs
    InventoryController.cs
    ItemsController.cs
    LocationsController.cs
    OrdersController.cs
    TenantController.cs
    UsersController.cs
  DTOs/
  Middleware/
    TenantResolverMiddleware.cs
    SubscriptionValidationMiddleware.cs
  Models/
  Repositories/
    IJsonRepository.cs
    JsonRepository.cs
  Services/
  Data/
    tenants.json
    users.json
    items.json
    locations.json
    inventory.json
    orders.json
    subscriptions.json
  Program.cs
  appsettings.json
```

## 2) Multi-tenant model notes

- All operational entities include `TenantId` (`User`, `Item`, `Location`, `InventoryRecord`, `Order`, `Subscription`).
- Tenant isolation is enforced in service/controller queries using tenant ID from JWT.

## 3) Generic JSON repository

- Async CRUD methods:
  - `GetAllAsync`
  - `GetByIdAsync`
  - `InsertAsync`
  - `UpdateAsync`
  - `DeleteAsync`
- Auto creates data files when missing.
- Uses a per-file `SemaphoreSlim` lock for thread-safe writes/reads.

## 4) Middleware

1. **TenantResolverMiddleware**
   - Reads `tenantId`, `sub`, `role` from JWT claims.
   - Stores values in scoped `ITenantContext`.

2. **SubscriptionValidationMiddleware**
   - For authenticated write requests (`POST/PUT/PATCH/DELETE`), verifies billing state.
   - Excludes `/api/auth/*` and `/api/billing/*`.
   - Returns `402 Payment Required` if trial/subscription inactive.

## 5) Controllers delivered

- Auth: `/api/auth/signup`, `/api/auth/login`
- Tenant: `/api/tenant/details`
- Users: `/api/users` add/remove/list (Admin role)
- Inventory: `/api/inventory` view/update
- Items: `/api/items`
- Locations: `/api/locations`
- Orders: `/api/orders`, `/api/orders/{id}/dispatch`
- Billing: `/api/billing/subscribe`, `/api/billing/status`
- Root web endpoint `/` returns a simple HTML status page so the service is directly usable as a web application.

## 6) Billing logic

- Trial starts on signup and ends in 16 days.
- If trial expired and no active subscription:
  - write APIs blocked with HTTP 402.
- Seat-based billing:
  - after trial, new user creation allowed only while current users `< purchased seats`.

## 7) Working flow (signup → trial → expiry → payment)

1. `POST /api/auth/signup`
   - Creates tenant + admin user.
   - Sets `TrialEndsAtUtc = UtcNow + 16 days`.
2. Use token for normal WMS writes during trial.
3. After trial expiry, write APIs return `402` until subscription exists.
4. `POST /api/billing/subscribe` (Admin)
   - Creates active subscription with seats and end date.
   - Write APIs re-enabled.
5. User growth controlled by purchased seats.

## Run notes

```bash
cd WMSLite.Api
# dotnet restore
# dotnet run
```

(If `dotnet` SDK is unavailable in your environment, install .NET 8 SDK first.)

## Docker run (no local .NET install)

From repository root:

```bash
docker compose up --build
```

Then open:
- `http://localhost:8080/`
- `http://localhost:8080/swagger`

Stop:

```bash
docker compose down
```
