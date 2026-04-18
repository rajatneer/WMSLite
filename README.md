# WMSLite (Multi-tenant SaaS Lite WMS Backend)

ASP.NET Core backend using JSON files (no database) for multi-tenant WMS + billing/trial enforcement.

## Folder Structure

- `Controllers/` - API endpoints (`Auth`, `Tenant`, `Users`, `Items`, `Inventory`, `Orders`, `Billing`)
- `Services/` - business logic (`AuthService`, `BillingService`)
- `Repositories/` - generic async JSON repository with file locking
- `Middleware/` - tenant resolver and subscription validator
- `Models/` - tenant-aware entities
- `DTOs/` - request/response payload models
- `Data/` - JSON persistence files

## Trial + Billing Flow

1. `POST /api/auth/signup`
   - Creates Tenant + Admin User + Subscription trial (16 days).
2. During trial (`TrialEndsAtUtc` not reached)
   - All write APIs are allowed.
3. After trial expires
   - Write APIs (`POST/PUT/PATCH/DELETE`) are blocked by middleware with HTTP `402`.
4. `POST /api/billing/subscribe`
   - Admin activates paid subscription and seat count.
5. With active paid subscription
   - Write APIs are re-enabled until `PaidEndsAtUtc`.
6. Seat enforcement
   - `POST /api/users` blocks if current users exceed purchased seats (post-trial paid mode).

## Storage Files

- `Data/tenants.json`
- `Data/users.json`
- `Data/items.json`
- `Data/inventory.json`
- `Data/orders.json`
- `Data/subscriptions.json`

## Notes

- `TenantId` is present on all entities.
- JWT carries `tenant_id` + role claims.
- Middleware resolves tenant from JWT and enforces subscription policy.
- Repository auto-creates JSON files and serializes asynchronously with write locking.
