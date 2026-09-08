create table if not exists public.floor (
    id text primary key default (gen_random_uuid())::text,
    building_id text not null references public.building(id) on delete cascade,
    floor_number integer not null,
    name text,
    floor_plan_url text,
    source_url text,
    verified boolean not null default false,
    constraint floor_building_number_unique unique (building_id, floor_number)
);

create index if not exists idx_floor_building_id on public.floor(building_id);

create table if not exists public.room (
    id text primary key default (gen_random_uuid())::text,
    floor_id text not null references public.floor(id) on delete cascade,
    room_code text not null,
    name text,
    description text,
    room_type text,
    local_x real,
    local_y real,
    local_z real,
    source_url text,
    verified boolean not null default false,
    constraint room_floor_code_unique unique (floor_id, room_code)
);

create index if not exists idx_room_floor_id on public.room(floor_id);
create index if not exists idx_room_code on public.room(room_code);
