using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentWinner.Api.Data;
using TournamentWinner.Api.DTOs;
using TournamentWinner.Api.Models;

namespace TournamentWinner.Api.Controllers;

public class CreateCommunityRequestDto
{
    public string Name { get; set; }
    public string? Slug { get; set; }
    public string Description { get; set; }
    public string Country { get; set; }
    public string RegionState { get; set; }
    public string? City { get; set; }
    public string OwnerId { get; set; }
}
[ApiController]
public class CommunityController : ControllerBase
{

    private CommunityContext _context;
    private readonly ILogger<CommunityController> _logger;

    public CommunityController(CommunityContext context, ILogger<CommunityController> logger)
    {
        this._context = context;
        this._logger = logger;
    }

    [HttpGet("/api/[controller]")]
    public IEnumerable<Community>? GetCommunities(int p = 1, int r = 20)
    {
        var toSkip = (p - 1) * r;
        return this._context.Communities
        .Include(c => c.Users.Where(u => u.Role == CommunityRoleType.Owner))
        .Take(r)
        .Skip(toSkip)
        .OrderByDescending(c => c.InsertDate);
    }

    [HttpGet("/api/[controller]/{id}/games")]
    public async Task<IEnumerable<CommunityGame>?> GetGames(string id)
    {
        IQueryable<CommunityGame> query;
        if (int.TryParse(id, out var communityActualId))
        {
            query = this._context.CommunityGames
                .Where(c => c.Community.Id == communityActualId);
        }
        else
        {
            query = this._context.CommunityGames
            .Where(c => c.Community.Slug == id);
        }
        return await query.Select(cg =>
            new CommunityGame
            {
                CommunityId = cg.CommunityId,
                GameId = cg.GameId,
                Community = new Community
                {
                    Description = cg.Community.Description,
                    Id = cg.Community.Id,
                    Name = cg.Community.Name,
                    Slug = cg.Community.Slug,
                    InsertDate = cg.Community.InsertDate,
                },
                Id = cg.Id,
                Game = new Game
                {
                    BannerImage = cg.Game.BannerImage,
                    Description = cg.Game.Description,
                    Id = cg.Game.Id,
                    IconImage = cg.Game.IconImage,
                    Name = cg.Game.Name,
                    InsertDate = cg.Game.InsertDate,
                    ReleaseDate = cg.Game.ReleaseDate,
                    Slug = cg.Game.Slug,
                },
                InsertDate = cg.InsertDate,
            }).ToListAsync();
    }

    [HttpGet("/api/[controller]/{id}/games/{gameId}")]
    public async Task<CommunityGameDto?> GetGame(string id, string gameId)
    {
        var gameIdParsed = int.TryParse(gameId, out var gameIdNumber);
        Community? community = await SearchCommunity(id)
            .Include(c => c.CommunityGames.Where(cg => gameIdParsed ? cg.Game.Id == gameIdNumber : cg.Game.Slug == gameId))
                .ThenInclude(cg => cg.Game)
            .FirstOrDefaultAsync();

        return CommunityGameDto.GetDto(community?.CommunityGames.FirstOrDefault());
    }


    [HttpGet("/api/[controller]/{id}/users")]
    public async Task<IEnumerable<UserDto?>> GetCommunityUsers(string id, [FromQuery] IEnumerable<CommunityRoleType> roleTypes) {
        if(!roleTypes.Any()){
            return new List<UserDto>();
        }

        return await this.SearchCommunity(id)
            .SelectMany(x => x.Users.Select(y => UserDto.GetDto(y.User)))
            .ToListAsync();
    }

    private IQueryable<Community> SearchCommunity(string id)
    {
        if (int.TryParse(id, out var communityActualId))
        {
            return this._context.Communities.Where(c => c.Id == communityActualId);
        }
        return this._context.Communities.Where(c => c.Slug == id);
    }

    private async Task<Game?> GetGame(string gameId)
    {
        if (int.TryParse(gameId, out var gameActualId))
        {
            return await this._context.Games.FirstOrDefaultAsync(g => g.Id == gameActualId);
        }
        return await this._context.Games.FirstOrDefaultAsync(g => g.Slug == gameId);
    }

    [HttpGet("/api/[controller]/{id}")]
    public Task<Community?> Get(string id)
    {
        return this.SearchCommunity(id).SingleOrDefaultAsync();
    }

    [HttpGet("/api/[controller]/{id}/players")]
    public IEnumerable<Player> GetPlayers(string id)
    {
        //id can be the slag or actual id, resolve to one
        if (int.TryParse(id, out var communityActualId))
        {
            return this._context.Communities
                .FirstOrDefault(c => c.Id == communityActualId)?.CommunityGames.SelectMany(cg => cg.CommunityGamePlayers.Select(cgp => cgp.Player))
                ?? new List<Player>();
        }

        return this._context.Communities
            .FirstOrDefault(c => c.Slug == id)?.CommunityGames.SelectMany(cg => cg.CommunityGamePlayers.Select(cgp => cgp.Player))
            ?? new List<Player>();

    }

    [HttpPost("/api/[controller]")]
    public async Task<Community?> CreateCommunity(CreateCommunityRequestDto dto)
    {
        if (!this.ModelState.IsValid)
        {
            this.HttpContext.Response.StatusCode = 422;
            return null;
        }
        var community = new Community()
        {
            City = dto.City,
            Name = dto.Name,
            Slug = dto.Slug,
            Users = new List<CommunityUser>(){
                new CommunityUser(){
                    Role = CommunityRoleType.Owner,
                    UserId = dto.OwnerId,
                }
            },
            Country = dto.Country,
            Description = dto.Description,
            RegionState = dto.RegionState,
        };
        await this._context.AddAsync(community);
        await this._context.SaveChangesAsync();
        return community;
    }

    [HttpPatch("/api/[controller]")]
    public void Patch(Community community)
    {

    }

    [HttpDelete("/api/[controller]")]
    public void Delete(Community community)
    {

    }
}
