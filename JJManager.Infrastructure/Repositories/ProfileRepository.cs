using JJManager.Core.Database.Entities;
using JJManager.Core.Interfaces.Repositories;
using JJManager.Core.Profile;
using JJManager.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Text.Json.Nodes;

namespace JJManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for profile operations
/// </summary>
public class ProfileRepository : IProfileRepository
{
    private readonly JJManagerDbContext _context;

    #region private functions
    private static DeviceProfile MapToDomain(ProfileEntity entity)
    {

        var profile = new DeviceProfile
        {
            Id = entity.Id,
            Name = entity.Name,
            ProductId = entity.ProductId,
            Data = string.IsNullOrEmpty(entity.Data)
                ? new JsonObject()
                : JsonNode.Parse(entity.Data)?.AsObject() ?? new JsonObject()
        };

        foreach (var input in entity.Inputs)
        {
            profile.Inputs.Add(new ProfileInput
            {
                Index = input.Index,
                Mode = Enum.TryParse<InputMode>(input.Mode, out var mode) ? mode : InputMode.None,
                Type = Enum.TryParse<InputType>(input.Type, out var type) ? type : InputType.Digital,
                Configuration = string.IsNullOrEmpty(input.Configuration)
                    ? null
                    : JsonNode.Parse(input.Configuration)?.AsObject()
            });
        }

        foreach (var output in entity.Outputs)
        {

            var profileOutput = new ProfileOutput
            {
                Index = output.Index,
                Mode = Enum.TryParse<OutputMode>(output.Mode, out var mode) ? mode : OutputMode.None
            };

            if (!string.IsNullOrEmpty(output.Configuration))
            {
                var config = JsonNode.Parse(output.Configuration)?.AsObject();
                profileOutput.Configuration = config;

                if (profileOutput.Mode == OutputMode.Leds)
                {
                    profileOutput.Led = DeviceProfile.JsonToLedConfig(config);
                }
            }

            profile.Outputs.Add(profileOutput);
        }


        return profile;
    }
    #endregion

    public ProfileRepository(JJManagerDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceProfile?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Profiles
            .Include(p => p.Inputs)
            .Include(p => p.Outputs)
            .FirstOrDefaultAsync(p => p.Id == id);

        return entity == null ? null : MapToDomain(entity);
    }
    public async Task<DeviceProfile?> GetByConnIdAsync(string connId)
    {
        // First, check if UserProduct exists for this connId
        var userProduct = await _context.UserProducts
            .FirstOrDefaultAsync(up => up.ConnectionId == connId);

        if (userProduct == null || userProduct.ProfileId == null || userProduct.ProfileId == Guid.Empty)
        {
            return null;
        }

        var entity = await _context.Profiles
            .Include(p => p.Inputs)
            .Include(p => p.Outputs)
            .FirstOrDefaultAsync(p => p.Id == userProduct.ProfileId);

        if (entity == null)
        {
            return null;
        }

        return MapToDomain(entity);
    }

    public async Task<List<DeviceProfile>> GetAllAsync()
    {
        var entities = await _context.Profiles
            .Include(p => p.Inputs)
            .Include(p => p.Outputs)
            .ToListAsync();

        return [.. entities.Select(MapToDomain)];
    }

    public async Task<List<DeviceProfile>> GetByProductIdAsync(Guid productId)
    {

        var entities = await _context.Profiles
            .Include(p => p.Inputs)
            .Include(p => p.Outputs)
            .Where(p => p.ProductId == productId)
            .ToListAsync();

        foreach (var e in entities)
        {
        }

        var result = entities.Select(MapToDomain).ToList();

        foreach (var p in result)
        {
        }

        return result;
    }

    public async Task<DeviceProfile> SaveAsync(
    DeviceProfile? profile = null,
    string? connId = null,
    Guid? productId = null,
    string name = "Perfil Padrão")
    {
        try
        {
            // 1️⃣ Resolve ProductId
            Guid resolvedProductId;

            if (productId.HasValue)
            {
                resolvedProductId = productId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(connId))
            {
                var userProduct = await _context.UserProducts
                    .FirstOrDefaultAsync(up => up.ConnectionId == connId)
                    ?? throw new InvalidOperationException(
                        $"No product found for connection ID '{connId}'.");

                resolvedProductId = userProduct.ProductId;
            }
            else if (profile != null)
            {
                resolvedProductId = profile.ProductId;
            }
            else
            {
                throw new ArgumentException(
                    "Either profile, productId or connId must be provided.");
            }

            // 2️⃣ Se profile for null, cria novo
            profile ??= new DeviceProfile
            {
                Id = Guid.Empty,
                Name = name,
                ProductId = resolvedProductId
            };

            // Garante ProductId correto
            profile.ProductId = resolvedProductId;

            // 3️⃣ Busca entidade existente (se update)
            ProfileEntity? entity = null;

            if (profile.Id != Guid.Empty)
            {
                entity = await _context.Profiles
                    .Include(p => p.Inputs)
                    .Include(p => p.Outputs)
                    .FirstOrDefaultAsync(p => p.Id == profile.Id);
            }

            var all = await _context.Profiles.ToListAsync();
            Console.WriteLine(all.Count);

            if (entity == null)
            {
                // 🔹 CREATE
                entity = new ProfileEntity
                {
                    Id = Guid.NewGuid(),
                    ProductId = profile.ProductId,
                    Name = profile.Name,
                    Data = profile.Data?.ToJsonString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };


                _context.Profiles.Add(entity);
            }
            else
            {
                // 🔹 UPDATE

                entity.Name = profile.Name;
                entity.Data = profile.Data?.ToJsonString();
                entity.UpdatedAt = DateTime.UtcNow;

                var inputs = await _context.Inputs
                    .Where(i => i.ProfileId == entity.Id)
                    .ToListAsync();

                if (inputs.Count > 0)
                {
                    _context.Inputs.RemoveRange(inputs);
                }

                var outputs = await _context.Outputs
                    .Where(o => o.ProfileId == entity.Id)
                    .ToListAsync();
                
                if (outputs.Count > 0)
                {
                    _context.Outputs.RemoveRange(outputs);
                }

                if (inputs.Count > 0 || outputs.Count > 0)
                {
                    await _context.SaveChangesAsync();
                }

            }

            // 4️⃣ Recria Inputs
            foreach (var input in profile.Inputs)
            {
                _context.Inputs.Add(new InputEntity
                {
                    Id = Guid.NewGuid(),
                    ProfileId = entity.Id,
                    Index = input.Index,
                    Mode = input.Mode.ToString(),
                    Type = input.Type.ToString(),
                    Configuration = input.Configuration?.ToJsonString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // 5️⃣ Recria Outputs

            // Recria filhos explicitamente
            foreach (var output in profile.Outputs)
            {
                _context.Outputs.Add(new OutputEntity
                {
                    Id = Guid.NewGuid(),
                    ProfileId = entity.Id,
                    Index = output.Index,
                    Mode = output.Mode.ToString(),
                    Configuration = BuildOutputConfigJson(output),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            profile.Id = entity.Id;

            return profile;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                Console.WriteLine("---- CONCURRENCY ERROR ----");
                Console.WriteLine($"Entity: {entry.Entity.GetType().Name}");
                Console.WriteLine($"State: {entry.State}");

                var dbValues = await entry.GetDatabaseValuesAsync();

                if (dbValues == null)
                {
                    Console.WriteLine("❌ Não existe mais no banco.");
                }
                else
                {
                    Console.WriteLine("⚠ Existe no banco, mas foi modificado.");
                }
            }

            throw;
        }
    }

    public async Task<DeviceProfile> GetDefaultProfileAsync(string connId)
    {
        var profile = await GetByConnIdAsync(connId);
        
        if (profile != null)
        {
            return profile;
        }

        var productId = await _context.UserProducts
            .Where(up => up.ConnectionId == connId)
            .Select(up => up.ProductId)
            .FirstOrDefaultAsync();

        if (productId == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"No product found for connection ID '{connId}'.");
        }

        var profiles = await GetByProductIdAsync(productId);
        
        profile = profiles.FirstOrDefault();

        if (profile != null)
        {
            // Set default profile for connection
            await SetDefaultProfileAsync(profile.Id, connId);
        }
        else
        {
            // if doesn't exist, create default profile
            profile = await SaveAsync(connId: connId);
        }

        return profile;
    }

    public async Task<bool> SetDefaultProfileAsync(Guid id, string connId)
    {
        var userProduct = await _context.UserProducts
            .FirstOrDefaultAsync(up => up.ConnectionId == connId);

        if (userProduct == null)
        {
            return false;
        }

        userProduct.ProfileId = id;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Profiles.FindAsync(id);
        if (entity != null)
        {
            _context.Profiles.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Profiles.AnyAsync(p => p.Id == id);
    }

    public async Task<DeviceProfile> GetAsync(string? connId = null, Guid? productId = null, string name = "Perfil Padrão")
    {
        DeviceProfile? profile = null;
        
        if (!string.IsNullOrWhiteSpace(connId))
        {
            profile = await GetByConnIdAsync(connId);
        }
        else if (productId.HasValue)
        {
            profile = await GetByProductIdAsync(productId.Value).ContinueWith(x => {
                return x.Result.First(p => p.Name == name) ?? null;
            });
        }

        if (profile == null)
        {
            profile = await SaveAsync(productId: productId, name: name);
        }

        return profile;
    }

    #region Private Methods

    private static string BuildOutputConfigJson(ProfileOutput output)
    {
        var json = new JsonObject();

        // Copy existing configuration
        if (output.Configuration != null)
        {
            foreach (var prop in output.Configuration)
            {
                json[prop.Key] = prop.Value?.DeepClone();
            }
        }

        // Add LED configuration if present
        if (output.Led != null)
        {
            json["property"] = output.Led.Property;
            json["property_name"] = output.Led.PropertyName;
            json["color"] = output.Led.Color;
            json["order"] = output.Led.Order;
            json["mode"] = output.Led.Mode;
            json["mode_if_enabled"] = output.Led.ModeIfEnabled;
            json["brightness"] = output.Led.Brightness;
            json["blink_speed"] = output.Led.BlinkSpeed;
            json["pulse_delay"] = output.Led.PulseDelay;
            json["value_to_activate"] = output.Led.ValueToActivate;
            json["comparative"] = output.Led.Comparative.ToString();
            json["var_type"] = output.Led.VarType.ToString();

            var ledsArray = new JsonArray();
            if (output.Led.LedsGrouped != null)
            {
                foreach (var ledIdx in output.Led.LedsGrouped)
                {
                    ledsArray.Add(ledIdx);
                }
            }
            json["leds"] = ledsArray;
        }

        return json.ToJsonString();
    }

    #endregion
}
