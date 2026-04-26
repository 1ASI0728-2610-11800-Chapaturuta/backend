using Frock_backend.shared.Interface.REST.Resources;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.shared.Interface.REST;

[ApiController]
[Route("api/config")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Config")]
public class MapController(IConfiguration configuration) : ControllerBase
{
    [HttpGet("map")]
    [SwaggerOperation(
        Summary = "Get map configuration",
        Description = "Returns tile server URL, OSM attribution, zoom limits and Peru bounding box for frontend map initialization.",
        OperationId = "GetMapConfig")]
    [SwaggerResponse(200, "Map configuration", typeof(MapConfigResource))]
    public ActionResult<MapConfigResource> GetMapConfig()
    {
        var tilesUrl = configuration["OsmTiles:PublicUrl"]
            ?? "http://localhost:8088/tile/{z}/{x}/{y}.png";

        return Ok(new MapConfigResource(
            TilesUrl: tilesUrl,
            Attribution: "© OpenStreetMap contributors",
            MinZoom: 5,
            MaxZoom: 19,
            BoundingBox: new BoundingBoxResource(-18.35, -0.03, -81.33, -68.65)
        ));
    }
}
