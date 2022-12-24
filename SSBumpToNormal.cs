using System.Drawing;
using System.Numerics;

public class SSBumpToNormal
{
    private static float OO_SQRT_3 = 0.57735025882720947f;
    static Vector3[] bumpBasisTranspose = new Vector3[]{
        new Vector3( 0.81649661064147949f, -0.40824833512306213f, -0.40824833512306213f ),
        new Vector3(  0.0f, 0.70710676908493042f, -0.7071068286895752f ),
        new Vector3(  OO_SQRT_3, OO_SQRT_3, OO_SQRT_3 )
    };

    public static void Main(string[] args)
    {
        Process(args);
    }

    private static void Process(string[] args)
    {
        List<string> files = new List<string>(args);

        if (files.Count == 0)
        {
            Console.WriteLine("[ssbump to normal by rob5300] Enter path for files to convert:");
            files.Add(Console.ReadLine());
        }

        List<Task> tasks = new List<Task>();
        foreach (var fileList in files.Chunk(50))
        {
            var taskFiles = fileList;
            tasks.Add(Task.Run(() =>
            {
                foreach (var f in taskFiles)
                {
                    try
                    {
                        ConvertToNormal(f.Replace("\"", ""));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to process file '{f}'. {e.Message}");
                    }
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        Console.WriteLine("Done!");
        Console.ReadKey();
    }

    private static void ConvertToNormal(string path)
    {
        Bitmap image = new Bitmap(path);
        Bitmap convertedImage = new Bitmap(image.Width, image.Height);
        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                var pixel = image.GetPixel(x, y);
                var readVector = new Vector3(pixel.R / 255f, pixel.G / 255f, pixel.B / 255f);
                Color newColor = Color.FromArgb(
                    ConvertVector(ref readVector, 0),
                    ConvertVector(ref readVector, 1),
                    ConvertVector(ref readVector, 2)
                    );
                convertedImage.SetPixel(x, y, newColor);
            }
        }
        image.Dispose();

        string fileName = Path.GetFileNameWithoutExtension(path);
        string newFileName = Path.Combine(Path.GetDirectoryName(path), fileName + "_normal" + Path.GetExtension(path));
        convertedImage.Save(newFileName, convertedImage.RawFormat);
        Console.WriteLine($"Converted '{path}' to tangent normal map.");
    }

    private static int ConvertVector(ref Vector3 vecIn, int index)
    {
        float newColor = Vector3.Dot(vecIn, bumpBasisTranspose[index]) * 0.5f + 0.5f;
        return (int)Math.Floor(Math.Clamp(newColor, 0f, 1f) * 255f);
    }
}