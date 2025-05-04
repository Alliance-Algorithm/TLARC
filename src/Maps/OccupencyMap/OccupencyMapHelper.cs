using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using Accord.IO;
using BEPUphysics.BroadPhaseEntries.Events;
using Emgu.CV;
using Newtonsoft.Json;

namespace Maps;

static class OccupancyGridMapHelper
{
  public static OccupancyMapData Init(string path)
  {
    if (path.EndsWith("tlm"))
    {
      return Internal_Init(TlarcSystem.MapPath + path);
    }
    else
    {
      return Internal_Init();
    }
  }
  public static void Save(OccupancyMapData data, string path)
  {
    OccupancyMapData.Description description = new() { SizeX = data.SizeX, SizeY = data.SizeY, Resolution = data.Resolution };
    string json = JsonConvert.SerializeObject(description);
    Emgu.CV.Mat mat = new(data.SizeX, data.SizeY, Emgu.CV.CvEnum.DepthType.Cv8S, 1);

    unsafe
    {
      sbyte* dataPtr = (sbyte*)mat.DataPointer.ToPointer();
      for (int i = 0; i < data.SizeX; i++)
        for (int j = 0; j < data.SizeY; j++)
        {
          dataPtr[i * data.SizeY + j] = data[i, j];
        }
    }
    mat.Save("/tmp/tmp.png");
    using FileStream zipStream = new(TlarcSystem.MapPath + path, FileMode.Create);

    using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);
    ZipArchiveEntry imageEntry = archive.CreateEntry("map.png", CompressionLevel.Optimal);
    using Stream entryStream = imageEntry.Open();
    using FileStream imageStream = File.OpenRead("/tmp/tmp.png");
    imageStream.CopyTo(entryStream);
    entryStream.Close();

    ZipArchiveEntry textEntry = archive.CreateEntry("description.json", CompressionLevel.Optimal);
    using StreamWriter writer = new(textEntry.Open(), Encoding.UTF8);
    writer.Write(json);
    writer.Close();
  }
  static OccupancyMapData Load(string path)
  {
    OccupancyMapData data;
    OccupancyMapData.Description description = new();
    Mat mat = new();
    using FileStream zipStream = new(path, FileMode.Open);
    using ZipArchive archive = new(zipStream, ZipArchiveMode.Read);
    foreach (ZipArchiveEntry entry in archive.Entries)
    {
      using StreamReader stream = new(entry.Open(), Encoding.UTF8);
      if (entry.Name == "description.json")
        description = JsonConvert.DeserializeObject<OccupancyMapData.Description>(stream.ReadToEnd());
      else
      {
        using Stream entryStream = entry.Open();
        using FileStream imageStream = File.OpenWrite("/tmp/tmp.png");
        entryStream.CopyTo(imageStream);

        var tmat = CvInvoke.Imread("/tmp/tmp.png", Emgu.CV.CvEnum.ImreadModes.Grayscale);
        tmat.ConvertTo(mat, Emgu.CV.CvEnum.DepthType.Cv8S);
      }

    }
    data = new(description.SizeX, description.SizeY, description.Resolution);
    unsafe
    {
      sbyte* dataPtr = (sbyte*)mat.DataPointer.ToPointer();
      for (int i = 0; i < data.SizeX; i++)
        for (int j = 0; j < data.SizeY; j++)
        {
          data[i, j] = Math.Clamp(dataPtr[i * data.SizeY + j], (sbyte)0, (sbyte)100);
        }
    }
    return data;
  }
  private static OccupancyMapData Internal_Init(string tlm_path) => Load(tlm_path);
  private static OccupancyMapData Internal_Init()
  {
    return new OccupancyMapData(301, 201, 0.1f);
  }
}
