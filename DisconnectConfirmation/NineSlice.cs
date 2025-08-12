using System.Text.Json.Serialization;
using OnixRuntime.Api;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Rendering;

namespace DisconnectConfirmation;

public class NineSlice {
    public NineSlice(TexturePath texture, string customPath = "", string? customJsonData = null) {
        Texture = texture;
        CustomPath = customPath == "" ? texture.Path : customPath;
        if (customJsonData != null) {
            NineSliceJson? jsonData = System.Text.Json.JsonSerializer.Deserialize<NineSliceJson>(customJsonData);
            NineSliceData = jsonData ?? throw new Exception($"Failed to deserialize NineSlice JSON data for texture: {Texture}");
        }
    }

    public TexturePath Texture { get; set; }
    public string CustomPath { get; set; }

    public class NineSliceJson {
        [JsonPropertyName("nineslice_size")] public int NineSliceSize { get; init; } = 4;
        [JsonPropertyName("base_size")] public int[] BaseSizeArray { get; init; } = [16, 16];

        [JsonIgnore]public Vec2 BaseSize => new(BaseSizeArray[0], BaseSizeArray[1]);
    }

    private NineSliceJson? NineSliceData { get; set; } = null;

    public NineSliceJson GetNineSliceData() {
        if (NineSliceData != null) {
            return NineSliceData;
        }
        
        try {
            string jsonPath = CustomPath == "" ? Texture.Path : CustomPath + ".json";
            byte[] jsonBytes = Onix.Game.PackManager.LoadContent(new TexturePath(jsonPath, Texture.Base));
            string jsonString = System.Text.Encoding.UTF8.GetString(jsonBytes);
            
            NineSliceJson? jsonData = System.Text.Json.JsonSerializer.Deserialize<NineSliceJson>(jsonString);
            NineSliceData = jsonData ?? throw new Exception($"Failed to deserialize NineSlice JSON data for texture: {Texture}");
            
            return NineSliceData;
        }
        catch {
            NineSliceData = new NineSliceJson();
            return NineSliceData;
        }
    }

    public void SetNineSliceData(NineSliceJson jsonData) {
        NineSliceData = jsonData;
    }

    public void SetNineSliceData(string jsonString) {
        NineSliceJson? jsonData = System.Text.Json.JsonSerializer.Deserialize<NineSliceJson>(jsonString);
        NineSliceData = jsonData ?? throw new Exception($"Failed to deserialize NineSlice JSON data for texture: {Texture}");
    }

    public void Render(RendererCommon gfx, Rect rect, float opacity = 1, Rect? uv = null) {
        NineSliceJson jsonData = GetNineSliceData();
        int sliceSize = jsonData.NineSliceSize;
        Vec2 baseSize = jsonData.BaseSize;

        float leftX = rect.X;
        float centerX = rect.X + sliceSize;
        float rightX = rect.Z - sliceSize;
        float topY = rect.Y;
        float centerY = rect.Y + sliceSize;
        float bottomY = rect.W - sliceSize;

        float centerWidth = rightX - centerX;
        float centerHeight = bottomY - centerY;
        
        Rect baseUv = uv ?? Rect.FromSize(0, 0, baseSize.X, baseSize.Y).NormalizeWith(baseSize);
        
        float uvSliceSizeX = sliceSize / baseSize.X * baseUv.Width;
        float uvSliceSizeY = sliceSize / baseSize.Y * baseUv.Height;
        
        float uvLeftX = baseUv.X;
        float uvCenterX = baseUv.X + uvSliceSizeX;
        float uvRightX = baseUv.Z - uvSliceSizeX;
        float uvTopY = baseUv.Y;
        float uvCenterY = baseUv.Y + uvSliceSizeY;
        float uvBottomY = baseUv.W - uvSliceSizeY;
        
        Rect topLeftUv = new(uvLeftX, uvTopY, uvCenterX, uvCenterY);
        Rect topRightUv = new(uvRightX, uvTopY, baseUv.Z, uvCenterY);
        Rect bottomLeftUv = new(uvLeftX, uvBottomY, uvCenterX, baseUv.W);
        Rect bottomRightUv = new(uvRightX, uvBottomY, baseUv.Z, baseUv.W);
        
        Rect topEdgeUv = new(uvCenterX, uvTopY, uvRightX, uvCenterY);
        Rect bottomEdgeUv = new(uvCenterX, uvBottomY, uvRightX, baseUv.W);
        Rect leftEdgeUv = new(uvLeftX, uvCenterY, uvCenterX, uvBottomY);
        Rect rightEdgeUv = new(uvRightX, uvCenterY, baseUv.Z, uvBottomY);
        
        Rect centerUv = new(uvCenterX, uvCenterY, uvRightX, uvBottomY);
        
        Rect topLeft = Rect.FromSize(leftX, topY, sliceSize, sliceSize);
        Rect topRight = Rect.FromSize(rightX, topY, sliceSize, sliceSize);
        Rect bottomLeft = Rect.FromSize(leftX, bottomY, sliceSize, sliceSize);
        Rect bottomRight = Rect.FromSize(rightX, bottomY, sliceSize, sliceSize);
        
        Rect topEdge = Rect.FromSize(centerX, topY, centerWidth, sliceSize);
        Rect bottomEdge = Rect.FromSize(centerX, bottomY, centerWidth, sliceSize);
        Rect leftEdge = Rect.FromSize(leftX, centerY, sliceSize, centerHeight);
        Rect rightEdge = Rect.FromSize(rightX, centerY, sliceSize, centerHeight);
        
        Rect center = Rect.FromSize(centerX, centerY, centerWidth, centerHeight);
        
        gfx.RenderTexture(topLeft, Texture, opacity, topLeftUv);
        gfx.RenderTexture(topRight, Texture, opacity, topRightUv);
        gfx.RenderTexture(bottomLeft, Texture, opacity, bottomLeftUv);
        gfx.RenderTexture(bottomRight, Texture, opacity, bottomRightUv);
        
        gfx.RenderTexture(topEdge, Texture, opacity, topEdgeUv);
        gfx.RenderTexture(bottomEdge, Texture, opacity, bottomEdgeUv);
        gfx.RenderTexture(leftEdge, Texture, opacity, leftEdgeUv);
        gfx.RenderTexture(rightEdge, Texture, opacity, rightEdgeUv);
        
        gfx.RenderTexture(center, Texture, opacity, centerUv);
    }
    public void Render(RendererCommon gfx, Rect rect, ColorF color = default, Rect? uv = null) {
        NineSliceJson jsonData = GetNineSliceData();
        int sliceSize = jsonData.NineSliceSize;
        Vec2 baseSize = jsonData.BaseSize;

        float leftX = rect.X;
        float centerX = rect.X + sliceSize;
        float rightX = rect.Z - sliceSize;
        float topY = rect.Y;
        float centerY = rect.Y + sliceSize;
        float bottomY = rect.W - sliceSize;

        float centerWidth = rightX - centerX;
        float centerHeight = bottomY - centerY;
        
        Rect baseUv = uv ?? Rect.FromSize(0, 0, baseSize.X, baseSize.Y).NormalizeWith(baseSize);
        
        float uvSliceSizeX = sliceSize / baseSize.X * baseUv.Width;
        float uvSliceSizeY = sliceSize / baseSize.Y * baseUv.Height;
        
        float uvLeftX = baseUv.X;
        float uvCenterX = baseUv.X + uvSliceSizeX;
        float uvRightX = baseUv.Z - uvSliceSizeX;
        float uvTopY = baseUv.Y;
        float uvCenterY = baseUv.Y + uvSliceSizeY;
        float uvBottomY = baseUv.W - uvSliceSizeY;
        
        Rect topLeftUv = new(uvLeftX, uvTopY, uvCenterX, uvCenterY);
        Rect topRightUv = new(uvRightX, uvTopY, baseUv.Z, uvCenterY);
        Rect bottomLeftUv = new(uvLeftX, uvBottomY, uvCenterX, baseUv.W);
        Rect bottomRightUv = new(uvRightX, uvBottomY, baseUv.Z, baseUv.W);
        
        Rect topEdgeUv = new(uvCenterX, uvTopY, uvRightX, uvCenterY);
        Rect bottomEdgeUv = new(uvCenterX, uvBottomY, uvRightX, baseUv.W);
        Rect leftEdgeUv = new(uvLeftX, uvCenterY, uvCenterX, uvBottomY);
        Rect rightEdgeUv = new(uvRightX, uvCenterY, baseUv.Z, uvBottomY);
        
        Rect centerUv = new(uvCenterX, uvCenterY, uvRightX, uvBottomY);
        
        Rect topLeft = Rect.FromSize(leftX, topY, sliceSize, sliceSize);
        Rect topRight = Rect.FromSize(rightX, topY, sliceSize, sliceSize);
        Rect bottomLeft = Rect.FromSize(leftX, bottomY, sliceSize, sliceSize);
        Rect bottomRight = Rect.FromSize(rightX, bottomY, sliceSize, sliceSize);
        
        Rect topEdge = Rect.FromSize(centerX, topY, centerWidth, sliceSize);
        Rect bottomEdge = Rect.FromSize(centerX, bottomY, centerWidth, sliceSize);
        Rect leftEdge = Rect.FromSize(leftX, centerY, sliceSize, centerHeight);
        Rect rightEdge = Rect.FromSize(rightX, centerY, sliceSize, centerHeight);
        
        Rect center = Rect.FromSize(centerX, centerY, centerWidth, centerHeight);
        
        gfx.RenderTexture(topLeft, Texture, color, topLeftUv);
        gfx.RenderTexture(topRight, Texture, color, topRightUv);
        gfx.RenderTexture(bottomLeft, Texture, color, bottomLeftUv);
        gfx.RenderTexture(bottomRight, Texture, color, bottomRightUv);
        
        gfx.RenderTexture(topEdge, Texture, color, topEdgeUv);
        gfx.RenderTexture(bottomEdge, Texture, color, bottomEdgeUv);
        gfx.RenderTexture(leftEdge, Texture, color, leftEdgeUv);
        gfx.RenderTexture(rightEdge, Texture, color, rightEdgeUv);
        
        gfx.RenderTexture(center, Texture, color, centerUv);
    }
}