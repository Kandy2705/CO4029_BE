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

## Credential rotation

The previous Supabase service-role credential was committed to source control. Removing it from the current file does not remove it from Git history. Rotate that credential in Supabase, update Azure App Settings, and revoke the old credential.

Development permits any CORS origin only when no origins are configured. Non-development environments allow cross-origin requests only from configured origins.
