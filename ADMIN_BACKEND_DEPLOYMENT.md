# Admin backend deployment

## Required Supabase migration

Run `Database/Migrations/20260907_add_user_is_active.sql` in the Supabase SQL editor before deploying the updated API. The migration keeps every existing user enabled by using `NOT NULL DEFAULT true`.

Run `Database/Migrations/20261007_building_coordinates_double_precision.sql` as well. It widens `building.latitude/longitude` from `real` (about 7 significant digits, so longitude kept only about 4 decimals and could be several metres off) to `double precision`, which matches the `double?` properties in the API. Existing values stay as they are, so re-pick buildings on the admin map to get full precision.

## Bootstrap the first Admin

The public registration API intentionally continues to create only `Customer` accounts. After creating the first account through the existing registration flow, promote that account from a trusted Supabase SQL session:

```sql
UPDATE public."user"
SET role = 'Admin', is_active = true
WHERE email = '<admin-email>';
```

Do not expose an unauthenticated "create admin" endpoint.

## Environment variables / Azure App Settings

Required:

- `Supabase__Url`
- `Supabase__ServiceRoleKey`

Configure each production web origin as an indexed setting:

- `Cors__AllowedOrigins__0`
- `Cors__AllowedOrigins__1`

Existing optional integrations continue to use:

- `ChatbotUrl`
- `ChatbotApiKey`
- `GmailSender`
- `GmailAppPassword`

Legacy keys `supabaseUrl`, `supabaseApiKey`, and `SupabaseAdminKey` are still read as a temporary backward-compatible fallback. New deployments should use the nested environment keys above.

## Mobile account endpoints

`Supabase__ServiceRoleKey` is also required by the mobile account flows: registration creates the Auth user
already confirmed (the email was proven by OTP), and password reset / email change update the Auth user through
the admin API. `GmailSender` and `GmailAppPassword` must be set for any OTP to be delivered.

| Endpoint | Auth | Purpose |
|---|---|---|
| `POST /api/v1/users/register/request-otp` | – | Send a registration code (409 if the email is taken) |
| `POST /api/v1/users/verify-otp` | – | Check a `register`/`reset` code without consuming it |
| `POST /api/v1/users/create-customer` | – | Create a customer; `otpCode` is now required |
| `POST /api/v1/users/forgot-password` | – | Send a reset code (same answer for unknown emails) |
| `POST /api/v1/users/reset-password` | – | Set a new password with the reset code |
| `POST /api/v1/users/request-email-change` | Bearer | Send a code to the new address |
| `POST /api/v1/users/change-email` | Bearer | Change the email with that code |
| `POST /api/v1/users/refresh-token` | – | Exchange a refresh token for a new session |
| `GET /api/v1/campus/map` | Bearer | Buildings → floors → rooms for any active user |

OTP rules: 4 digits from a cryptographic RNG, valid 5 minutes, 60 s resend cooldown (HTTP 429 with
`Retry-After`), single use, 5 wrong guesses invalidate the code.

Tests: `dotnet test Tests/CO4029_BE.Tests` (in-memory fakes, no Supabase or SMTP needed).

## Credential rotation

The previous Supabase service-role credential was committed to source control. Removing it from the current file does not remove it from Git history. Rotate that credential in Supabase, update Azure App Settings, and revoke the old credential.

Development permits any CORS origin only when no origins are configured. Non-development environments allow cross-origin requests only from configured origins.
