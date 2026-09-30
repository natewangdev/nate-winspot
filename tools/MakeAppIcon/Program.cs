using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: MakeAppIcon <output.ico>");
    return 1;
}

var path = args[0];
Directory.CreateDirectory(Path.GetDirectoryName(path)!);
int[] sizes = [16, 32, 48, 256];
using var ms = new MemoryStream();
using (var bw = new BinaryWriter(ms))
{
    bw.Write((short)0);
    bw.Write((short)1);
    bw.Write((short)sizes.Length);

    var images = new byte[sizes.Length][];
    for (var i = 0; i < sizes.Length; i++)
    {
        using var bmp = DrawW(sizes[i]);
        images[i] = EncodePng(bmp);
    }

    var offset = 6 + 16 * sizes.Length;
    for (var i = 0; i < sizes.Length; i++)
    {
        var s = sizes[i];
        bw.Write((byte)(s >= 256 ? 0 : s));
        bw.Write((byte)(s >= 256 ? 0 : s));
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((short)1);
        bw.Write((short)32);
        bw.Write(images[i].Length);
        bw.Write(offset);
        offset += images[i].Length;
    }

    foreach (var image in images)
    {
        bw.Write(image);
    }
}

File.WriteAllBytes(path, ms.ToArray());
Console.WriteLine($"Wrote {path} ({new FileInfo(path).Length} bytes)");
return 0;

static Bitmap DrawW(int size)
{
    var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
    using var g = Graphics.FromImage(bmp);
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
    g.Clear(Color.FromArgb(255, 31, 42, 55));
    var fontSize = size * 0.62f;
    using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
    using var brush = new SolidBrush(Color.White);
    var rect = new RectangleF(0, size * 0.02f, size, size);
    using var sf = new StringFormat
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center
    };
    g.DrawString("W", font, brush, rect, sf);
    return bmp;
}

static byte[] EncodePng(Bitmap bmp)
{
    using var ms = new MemoryStream();
    bmp.Save(ms, ImageFormat.Png);
    return ms.ToArray();
}
