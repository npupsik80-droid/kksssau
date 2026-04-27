using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing.Rendering;

namespace KkmFactory;

public class ImageBarCode
{
	public int Width;

	public int Height;

	public byte[] Pixels = new byte[0];

	public Image<Rgba32> Image;

	public PixelData Bitmap;

	public string PngBase64;
}
