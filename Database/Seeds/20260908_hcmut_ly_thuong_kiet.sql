-- Idempotent seed for HCMUT campus 1 (Ly Thuong Kiet).
-- Sources are official HCMUT/Centre for Continuing Education/OISP/SIM pages.
-- Do not treat this as a complete room inventory: only verified rooms are included.

with admin_user as (
  select id from public."user"
  where lower(email)=lower('man.ngotrieuman27@hcmut.edu.vn')
  limit 1
), seed(name, content, latitude, longitude) as (
  values
    ('A3','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Nhà A3.',null::real,null::real),
    ('A4','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Nhà A4.',null::real,null::real),
    ('A5','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Nhà A5.',null::real,null::real),
    ('B1','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Dãy nhà B1.',null::real,null::real),
    ('B3','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Dãy nhà B3.',null::real,null::real),
    ('B4','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Dãy nhà B4.',null::real,null::real),
    ('B6','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Dãy nhà B6.',null::real,null::real),
    ('B9','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Dãy nhà B9.',10.773479::real,106.660469::real),
    ('B10','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Dãy nhà B10.',null::real,null::real),
    ('C4','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Dãy nhà C4.',null::real,null::real),
    ('C5','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Dãy nhà C5.',null::real,null::real),
    ('C6','Cơ sở 1 HCMUT - 268 Lý Thường Kiệt. Nhà C6.',null::real,null::real),
    ('Thư viện','Thư viện trường tại cơ sở 1 HCMUT - 268 Lý Thường Kiệt.',null::real,null::real),
    ('Căn tin B4','Căn tin khu B4 tại cơ sở 1 HCMUT.',null::real,null::real),
    ('Sân vận động','Sân vận động tại cơ sở 1 HCMUT.',null::real,null::real),
    ('C1 - Xưởng','Xưởng C1 - cơ sở 1 HCMUT, theo sơ đồ chính thức.',null::real,null::real),
    ('Công viên','Khu công viên tại cơ sở 1 HCMUT.',null::real,null::real),
    ('Bãi xe B3','Bãi xe B3 - cơ sở 1 HCMUT, theo sơ đồ chính thức.',null::real,null::real),
    ('Sân bóng rổ','Sân bóng rổ - cơ sở 1 HCMUT, theo sơ đồ chính thức.',null::real,null::real),
    ('Sân tennis','Sân tennis - cơ sở 1 HCMUT, theo sơ đồ chính thức.',null::real,null::real),
    ('Bãi xe máy B10','Bãi xe máy B10 - cơ sở 1 HCMUT, theo sơ đồ chính thức.',null::real,null::real),
    ('Bến xe bus','Bến xe bus - cơ sở 1 HCMUT, theo sơ đồ chính thức.',null::real,null::real)
)
insert into public.building(name,content,latitude,longitude,user_id)
select s.name,s.content,s.latitude,s.longitude,a.id
from seed s cross join admin_user a
where not exists (select 1 from public.building b where lower(b.name)=lower(s.name));

with floor_seed(building_name,floor_number,name,source_url,verified) as (
  values
    ('A4',3,'Tầng 3','https://oisp.hcmut.edu.vn/cac-truong-doi-tac/kinh-nghiem-du-hoc-bo-ich-tu-dai-dien-griffith-university.html',true),
    ('A4',5,'Tầng 5','https://oisp.hcmut.edu.vn/tuyen-sinh/danh-sach-thi-sinh-tham-du-ky-thi-thu-mon-toan-ngay-26-3-2017.html',true),
    ('B1',2,'Tầng 2','https://sim.hcmut.edu.vn/hoc-cung-doanh-nghiep-10/',true),
    ('B1',3,'Tầng 3','https://sim.hcmut.edu.vn/hoc-cung-doanh-nghiep-vai-tro-cua-cong-tac-quan-ly-va-nha-quan-ly-trong-hoat-dong-cua-to-chuc/',true),
    ('B9',2,'Tầng 2','https://sim.hcmut.edu.vn/hoi-thao-tong-quan-ve-quan-tri-nguon-nhan-luc-trong-thuc-tien-va-dinh-huong-nghe-nghiep-nganh-nhan-su/',true),
    ('C6',0,'Tầng trệt','https://cce.hcmut.edu.vn/image/catalog/doc/Sodo_Diadiemthi_C6.pdf',true),
    ('C6',4,'Tầng 4','https://cce.hcmut.edu.vn/image/catalog/doc/Sodo_Diadiemthi_C6.pdf',true),
    ('C6',5,'Tầng 5','https://cce.hcmut.edu.vn/image/catalog/doc/Sodo_Diadiemthi_C6.pdf',true)
)
insert into public.floor(building_id,floor_number,name,source_url,verified)
select b.id,s.floor_number,s.name,s.source_url,s.verified
from floor_seed s join public.building b on lower(b.name)=lower(s.building_name)
where not exists (select 1 from public.floor f where f.building_id=b.id and f.floor_number=s.floor_number);

with room_seed(building_name,floor_number,room_code,name,room_type,source_url,verified) as (
  values
    ('A4',3,'306','Phòng 306','Office / service','https://oisp.hcmut.edu.vn/cac-truong-doi-tac/kinh-nghiem-du-hoc-bo-ich-tu-dai-dien-griffith-university.html',true),
    ('A4',5,'506','Phòng 506','Classroom','https://oisp.hcmut.edu.vn/tuyen-sinh/danh-sach-thi-sinh-tham-du-ky-thi-thu-mon-toan-ngay-26-3-2017.html',true),
    ('A4',5,'507','Phòng 507','Classroom','https://oisp.hcmut.edu.vn/tuyen-sinh/danh-sach-thi-sinh-tham-du-ky-thi-thu-mon-toan-ngay-26-3-2017.html',true),
    ('A4',5,'508','Phòng 508','Classroom','https://oisp.hcmut.edu.vn/tuyen-sinh/danh-sach-thi-sinh-tham-du-ky-thi-thu-mon-toan-ngay-26-3-2017.html',true),
    ('A4',5,'509','Phòng 509','Classroom','https://oisp.hcmut.edu.vn/tuyen-sinh/danh-sach-thi-sinh-tham-du-ky-thi-thu-mon-toan-ngay-26-3-2017.html',true),
    ('A4',5,'510','Phòng 510','Classroom','https://oisp.hcmut.edu.vn/tuyen-sinh/danh-sach-thi-sinh-tham-du-ky-thi-thu-mon-toan-ngay-26-3-2017.html',true),
    ('B1',2,'212','Phòng B1-212','Classroom','https://sim.hcmut.edu.vn/hoc-cung-doanh-nghiep-10/',true),
    ('B1',3,'313','Phòng B1-313','Classroom','https://sim.hcmut.edu.vn/hoc-cung-doanh-nghiep-vai-tro-cua-cong-tac-quan-ly-va-nha-quan-ly-trong-hoat-dong-cua-to-chuc/',true),
    ('B9',2,'202','Phòng B9-202','Classroom','https://sim.hcmut.edu.vn/hoi-thao-tong-quan-ve-quan-tri-nguon-nhan-luc-trong-thuc-tien-va-dinh-huong-nghe-nghiep-nganh-nhan-su/',true),
    ('C6',0,'102','Phòng 102','Computer / exam room','https://cce.hcmut.edu.vn/image/catalog/doc/Sodo_Diadiemthi_C6.pdf',true),
    ('C6',0,'103','Phòng 103','Computer room','https://cce.hcmut.edu.vn/image/catalog/doc/Sodo_Diadiemthi_C6.pdf',true),
    ('C6',0,'104','Phòng 104','Computer / exam room','https://cce.hcmut.edu.vn/image/catalog/doc/Sodo_Diadiemthi_C6.pdf',true),
    ('C6',4,'509','Phòng 509','Classroom','https://cce.hcmut.edu.vn/image/catalog/doc/Sodo_Diadiemthi_C6.pdf',true),
    ('C6',5,'PM1','Phòng máy PM1','Computer room','https://cce.hcmut.edu.vn/image/catalog/doc/Sodo_Diadiemthi_C6.pdf',true),
    ('C6',5,'PM2','Phòng máy PM2','Computer room','https://cce.hcmut.edu.vn/image/catalog/doc/Sodo_Diadiemthi_C6.pdf',true),
    ('C6',5,'PM3','Phòng máy PM3','Computer room','https://cce.hcmut.edu.vn/image/catalog/doc/Sodo_Diadiemthi_C6.pdf',true),
    ('C6',5,'PM5','Phòng máy PM5','Computer room','https://cce.hcmut.edu.vn/image/catalog/doc/Sodo_Diadiemthi_C6.pdf',true)
)
insert into public.room(floor_id,room_code,name,room_type,source_url,verified)
select f.id,s.room_code,s.name,s.room_type,s.source_url,s.verified
from room_seed s
join public.building b on lower(b.name)=lower(s.building_name)
join public.floor f on f.building_id=b.id and f.floor_number=s.floor_number
where not exists (select 1 from public.room r where r.floor_id=f.id and lower(r.room_code)=lower(s.room_code));
