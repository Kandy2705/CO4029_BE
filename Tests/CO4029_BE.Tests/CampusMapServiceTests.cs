using CO4029_BE.Application.Services;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.Tests.Fakes;

namespace CO4029_BE.Tests;

public class CampusMapServiceTests
{
    private readonly TestBed bed = new();
    private readonly InMemoryBuildingRepository buildings = new();
    private readonly InMemoryFloorRepository floors = new();
    private readonly InMemoryRoomRepository rooms = new();

    private CampusMapService Service => new(buildings, floors, rooms, bed.CurrentUser, bed.Time);

    [Fact]
    public async Task Customer_gets_buildings_with_sorted_floors_and_rooms()
    {
        var token = TestBed.TokenFor(bed.AddUser("a@hcmut.edu.vn"));
        await buildings.CreateAsync(new Building { ID = "b10", Name = "B10", Latitude = 10.7731, Longitude = 106.6595 });
        await buildings.CreateAsync(new Building { ID = "b9", Name = "B9" });
        await buildings.CreateAsync(new Building { ID = "a1", Name = "A1" });
        await floors.CreateAsync(new Floor { Id = "f2", BuildingId = "b9", FloorNumber = 2, Name = "Tầng 2" });
        await floors.CreateAsync(new Floor { Id = "f1", BuildingId = "b9", FloorNumber = 1, Name = "Tầng 1" });
        await rooms.CreateAsync(new Room { Id = "r2", FloorId = "f1", RoomCode = "B9-102" });
        await rooms.CreateAsync(new Room { Id = "r1", FloorId = "f1", RoomCode = "B9-101", LocalX = 1.5f });
        await rooms.CreateAsync(new Room { Id = "orphan", FloorId = "missing", RoomCode = "X" });

        var map = await Service.GetMapAsync(token);

        Assert.Equal(new[] { "A1", "B10", "B9" }, map.Buildings.Select(b => b.Name));
        Assert.Equal(10.7731, map.Buildings[1].Latitude);
        var b9 = map.Buildings[2];
        Assert.Equal(new[] { 1, 2 }, b9.Floors.Select(f => f.FloorNumber));
        Assert.Equal(new[] { "B9-101", "B9-102" }, b9.Floors[0].Rooms.Select(r => r.RoomCode));
        Assert.Equal(1.5f, b9.Floors[0].Rooms[0].LocalX);
        Assert.Empty(b9.Floors[1].Rooms);
        Assert.Empty(map.Buildings[0].Floors);
        Assert.Equal(bed.Time.GetUtcNow().UtcDateTime, map.GeneratedAt);
    }

    [Fact]
    public async Task Map_requires_an_active_signed_in_user()
    {
        var off = bed.AddUser("off@hcmut.edu.vn", active: false);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service.GetMapAsync("garbage"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service.GetMapAsync(TestBed.TokenFor(off)));
    }
}

public class RepositoryQueryBuilderTests
{
    private sealed class Probe : Repository<OtpCode>
    {
        public Probe(Supabase.Client client) : base(client) { }
        public object Builder() => Table;
    }

    [Fact]
    public void Each_query_starts_from_a_fresh_builder()
    {
        // A shared builder would carry the filters of an earlier query into the next one.
        var probe = new Probe(new Supabase.Client("http://localhost", "test-key"));
        Assert.NotSame(probe.Builder(), probe.Builder());
    }
}
