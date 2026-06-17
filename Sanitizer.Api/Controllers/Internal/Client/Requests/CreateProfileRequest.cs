using Sanitizer.Api.Models;
using Sanitizer.Api.Models.Strategy;

namespace Sanitizer.Api.Controllers.Internal.Client.Requests;

public record CreateProfileRequest(string Name, Dictionary<DetectorType, StrategyConfig> Rules);