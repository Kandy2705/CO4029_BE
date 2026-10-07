-- Run this migration in the Supabase SQL editor before deploying the updated API.
-- `real` (float4) keeps only ~7 significant digits, so a longitude such as 106.6583104
-- was stored as 106.6583 (or less) — an error of several metres. `double precision`
-- keeps ~15 digits. Widening the type is lossless for existing rows (their values are
-- already rounded and must be re-picked in the admin map to regain precision).
ALTER TABLE public.building
  ALTER COLUMN latitude  TYPE double precision,
  ALTER COLUMN longitude TYPE double precision;

COMMENT ON COLUMN public.building.latitude  IS 'WGS 84 latitude in decimal degrees (double precision).';
COMMENT ON COLUMN public.building.longitude IS 'WGS 84 longitude in decimal degrees (double precision).';

-- Values converted from real carry binary noise (10.773479 -> 10.7734794616699).
-- real only held ~6 decimals here, so rounding to 6 restores the values that were typed.
UPDATE public.building
SET latitude  = round(latitude::numeric, 6)::double precision,
    longitude = round(longitude::numeric, 6)::double precision
WHERE latitude IS NOT NULL OR longitude IS NOT NULL;
