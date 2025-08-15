// See https://aka.ms/new-console-template for more information


using MapServer;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Sensor;

Console.WriteLine(
    @"
_______  _        ______   ______   ______ @                                       
  | |   | |      | |  | | | |  | \ | |                                             
  | |   | |   _  | |__| | | |__| | | |                                             
  |_|   |_|__|_| |_|  |_| |_|  \_\ |_|____                                         
                                                                                   
 _________   ______   ______   ______   ______  ______   _     _   ______  ______  
| | | | | \ | |  | | | |  | \ / |      | |     | |  | \ | |   | | | |     | |  | \ 
| | | | | | | |__| | | |__|_/ '------. | |---- | |__| | \ \   / / | |---- | |__| | 
|_| |_| |_| |_|  |_| |_|       ____|_/ |_|____ |_|  \_\  \_\_/_/  |_|____ |_|  \_\ 
                                                                                   
"
);

const string mapSaverExe = "map_saver";
const string mapSaverOptionPath = "map_path";

const string helpString = @$"
===================================================================================
helps:
    <executable> [options=value]
example: 
    {mapSaverExe} map_path='path\to\map\dir'
";

bool MapSaver(string[] args)
{
    foreach (var arg in args)
    {
        var optionValuePair = arg.Split("=", StringSplitOptions.RemoveEmptyEntries);
        if (optionValuePair.Length != 2)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[option=value] Format Fault : {arg}");
            Console.WriteLine(helpString);
            Console.ResetColor();
            return false;
        }

        switch (optionValuePair[0])
        {
            case mapSaverOptionPath:
                SaveMap.MapSavePath = optionValuePair[1];
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Fatal]: [option] name fault : {arg} \n exe {mapSaverExe}");
                Console.ResetColor();
                Console.WriteLine(helpString);
                return false;
        }
    }

    SaveMap.MapSavePath += DateTime.Now.ToString("yyyyMMddHHmmss");

    var ros = TlarcRosBridge.Domain.RosBridge.Build("TlarcRecast");
    ros.Recast<OccupancyGrid>("/rmcs_map/map/grid", "/tlarc/recast/map/grid", "car_link",
        TlarcRosBridge.Infrastructure.DataProcess.Recast.OccupancyGrid);
    ros.Recast<PointCloud2>("/rmcs_map/segmentation_part", "/tlarc/recast/map/segmentation_part", "car_link",
        TlarcRosBridge.Infrastructure.DataProcess.Recast.PointCloud2);

    Console.WriteLine("输入 exit 退出或者其他任何字符串保存地图");
    SaveMap.Build();
    while (Console.ReadLine() is not "exit")
    {
        SaveMap.SaveTrigger();
        Console.WriteLine($"Save To {SaveMap.MapSavePath} \n ~~~~~~~~~~~~~~~ \n");
    }

    return true;
}


CommandIn:
while (args.Length == 0)
{
    Console.WriteLine(helpString);
    args = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
}

switch (args[0])
{
    case mapSaverExe:
        if (MapSaver(args[1..]))
            break;
        args = [];
        break;
    default:
        args = [];
        break;
}

if (args.Length == 0)
    goto CommandIn;