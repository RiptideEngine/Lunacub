using Caxivitual.Lunacub.Importing;
using Microsoft.Extensions.Logging;
using Silk.NET.WebGPU;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Caxivitual.Lunacub.Examples.TextureViewer;

public sealed class Texture2DDeserializer(Renderer renderer) : Deserializer<Texture2D> {
    protected override async Task<Texture2D> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(dataStream, cancellationToken);

        Texture2D outputTexture = new Texture2D(renderer, (uint)image.Width, (uint)image.Height, TextureFormat.Rgba8Unorm);

        try {
            image.ProcessPixelRows(accessor => {
                unsafe {
                    TextureDataLayout layout = new() {
                        Offset = 0,
                        BytesPerRow = (uint)sizeof(Rgba32) * (uint)accessor.Width,
                        RowsPerImage = (uint)accessor.Height,
                    };
                    Extent3D writeSize = new() {
                        Width = (uint)accessor.Width,
                        Height = 1,
                        DepthOrArrayLayers = 1
                    };

                    for (int y = 0; y < accessor.Height; y++) {
                        var row = accessor.GetRowSpan(y);

                        fixed (Rgba32* pRow = row) {
                            ImageCopyTexture dest = new() {
                                Aspect = TextureAspect.All,
                                MipLevel = 0,
                                Texture = outputTexture.Texture,
                                Origin = new() {
                                    X = 0,
                                    Y = (uint)y,
                                    Z = 0
                                },
                            };

                            renderer.WebGPU.QueueWriteTexture(renderer.RenderingDevice.Queue, &dest, pRow, (nuint)sizeof(Rgba32) * (uint)accessor.Width, &layout, &writeSize);
                        }
                    }
                }
            });
            
            return outputTexture;
        } catch {
            outputTexture.Dispose();
            throw;
        }
    }
}