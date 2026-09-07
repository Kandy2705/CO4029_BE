-- Run this migration in the Supabase SQL editor before deploying the updated API.
-- Existing rows remain enabled because the column is NOT NULL with DEFAULT true.
ALTER TABLE public."user"
ADD COLUMN IF NOT EXISTS is_active boolean NOT NULL DEFAULT true;

COMMENT ON COLUMN public."user".is_active IS
'Application-level account status. Disabled users are rejected even when their Supabase token is valid.';
