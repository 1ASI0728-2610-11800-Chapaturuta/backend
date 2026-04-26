namespace Frock_backend.shared.Interface.REST.Resources;

public record BoundingBoxResource(double MinLat, double MaxLat, double MinLng, double MaxLng);

public record MapConfigResource(
    string TilesUrl,
    string Attribution,
    int MinZoom,
    int MaxZoom,
    BoundingBoxResource BoundingBox
);
