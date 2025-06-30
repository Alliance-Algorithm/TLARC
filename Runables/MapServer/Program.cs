// See https://aka.ms/new-console-template for more information

using Map;
using Kernel.Core.EventBus;
using Kernel.Core.Messages;
using Kernel.Core.TransformTree;
using Kernel.DataInterfaces.Geometry;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Sensor;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Sensor;
using Kernel.DataInterfaces.Tf;
using TlarcRosBridge.Infrastructure.Messages.Tf2;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

Console.WriteLine(
    @"
 ________ __
|        |  \
 \$$$$$$$| $$ ______   ______   _______
   | $$  | $$|      \ /      \ /       \
   | $$  | $$ \$$$$$$|  $$$$$$|  $$$$$$$
   | $$  | $$/      $| $$   \$| $$
   | $$  | $|  $$$$$$| $$     | $$_____
   | $$  | $$\$$    $| $$      \$$     \
 __ \$$   __$ \$$$$$$$\$$       ______$$
|  \     /  \                  /      \
| $$\   /  $$ ______   ______ |  $$$$$$\ ______   ______ __     __  ______   ______
| $$$\ /  $$$|      \ /      \| $$___\$$/      \ /      |  \   /  \/      \ /      \
| $$$$\  $$$$ \$$$$$$|  $$$$$$\\$$    \|  $$$$$$|  $$$$$$\$$\ /  $|  $$$$$$|  $$$$$$\
| $$\$$ $$ $$/      $| $$  | $$_\$$$$$$| $$    $| $$   \$$\$$\  $$| $$    $| $$   \$$
| $$ \$$$| $|  $$$$$$| $$__/ $|  \__| $| $$$$$$$| $$       \$$ $$ | $$$$$$$| $$
| $$  \$ | $$\$$    $| $$    $$\$$    $$\$$     | $$        \$$$   \$$     | $$
 \$$      \$$ \$$$$$$| $$$$$$$  \$$$$$$  \$$$$$$$\$$         \$     \$$$$$$$\$$
                     | $$
                     | $$
                      \$$
"
);

#region Parameters 调好了，改这里就好

// TF
/*
 * tf tree:
 * car_init
 * |
 * L car_link
 * |    |
 * |    L lidar_link
 * |
 * L cost_map_link
 * |
 * L lidar_init
 */
Quaternion QuaternionFromYawPitchRoll(float yaw = 0, float pitch = 0, float roll = 0) =>
    Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(yaw   / 180 * Math.PI)) *
    Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(pitch / 180 * Math.PI)) *
    Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(roll  / 180 * Math.PI));

const string tfCostMapLinkName      = "cost_map_link";
const string tfCarLinkName          = "car_link";
const string tfCarInitName          = "car_init";
const string tfLidarLinkName        = "lidar_link";
const string tfLidarInitName        = "lidar_init";
var          tfCostMapLinkTranslate = new Vector3(-5, -10, 0);
var          tfCostMapLinkRotation  = Quaternion.Identity;
var          tfLidarLinkTranslate   = new Vector3(0.14f, 0.12f, 0.27f);
var          tfLidarLinkRotation    = QuaternionFromYawPitchRoll(-49.7f, roll: -50.0f);

// Events
const string eventPointCloudInputName  = "/tlarc/map_server/point_cloud";
const string eventRobotPositionName    = "/tlarc/map_server/sensor_pose";
const string eventGridMapName          = "/tlarc/map/cost_map_with_pcd";
const string eventTfName               = "/tlarc/tf/publish";
const string eventPointCloudOutputName = "/tlarc/point_cloud";
const string eventSaveMapName          = "/tlarc/save_map";

// Ros
const string rosNodeName                         = "TlarcMapServer";
const string rosSubRegisteredPointCloudTopicName = "/rmcs_slam/cloud_registered_world";
const string rosSubRobotPosTopicName             = "/rmcs_slam/pose";
const string rosPubDebugTlarcGridMapTopicName    = "/tlarc/map/cost_map_with_pcd";
const string rosPubDebugTlarcTfTopicName         = "/tlarc_tf";
const string rosPubTlarcPointCloudTopicName      = "/tlarc/point_cloud";

// Others
const string pointCloudInputId         = tfCarInitName;
const string pointCloudOutputId        = tfCostMapLinkName;
const string pointCloudCostMapSensorId = tfLidarLinkName;
const string robotPositionInputTfId    = tfCarLinkName;

var mapSavePath = Environment.ProcessPath + "Output/TestMap";

#endregion

#region TFSetupHere

Tf.AddTfNode(tfCostMapLinkName, tfCarInitName);
Tf.AddTfNode(tfCarLinkName,     tfCarInitName);
Tf.AddTfNode(tfLidarInitName,   tfCarInitName);
Tf.AddTfNode(tfLidarLinkName,   tfCarLinkName);
Tf.SetTfNode(tfCostMapLinkName, tfCostMapLinkTranslate, tfCostMapLinkRotation);
Tf.SetTfNode(tfLidarLinkName,   tfLidarLinkTranslate,   tfLidarLinkRotation);

#endregion

#region ROS Setup Here

var ros = TlarcRosBridge.Domain.RosBridge.Build(rosNodeName);

ros.Subscript<PointCloud2, IPointCloud>(
    rosSubRegisteredPointCloudTopicName, eventPointCloudInputName,
    TlarcRosBridge.Infrastructure.DataProcess.Subscriber.FastLioRegistered);
ros.Subscript<PoseStamped, IPose>(
    rosSubRobotPosTopicName, eventRobotPositionName,
    TlarcRosBridge.Infrastructure.DataProcess.Subscriber.RawPoseFromPoseStamped);
ros.Publish<IGridMap2DData, OccupancyGrid>(
    eventGridMapName,
    rosPubDebugTlarcGridMapTopicName,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
ros.Publish<ITfCollection, TFMessage>(
    eventTfName,
    rosPubDebugTlarcTfTopicName,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.TfCollectionToTfMessage);
ros.Publish<IPointCloud, PointCloud2>(
    eventPointCloudOutputName,
    rosPubTlarcPointCloudTopicName,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPointCloud);

#endregion

#region Domain Setup Here

var pcdCostMap =
    PointCloudCostMap.DefaultNew
        .SetInput_PointCloudTopicName(eventPointCloudInputName)
        .SetOutput_DataStructure(750, 450, resolution: 0.04f, topZ: 0.4f, bottomZ: -0.5f, lossFree: 0.7f,
            lossOccu: -0.9f)
        .SetId_PointCloud(pointCloudInputId)
        .SetId_Sensor(pointCloudCostMapSensorId)
        .BuildOccupancyHighMap();
var saver =
    MapSaver.DefaultNew
        .SetInput_CostMapTopicName(eventGridMapName)
        .SetTrigger_SaveTriggerTopicName(eventSaveMapName)
        .Build();

#endregion

GC.KeepAlive(pcdCostMap);
GC.KeepAlive(saver);

#region MainLogics

EventBus<IPose>.Instance.Subscribe(eventRobotPositionName,
    data =>
    {
        Tf.SetTfNode(robotPositionInputTfId, data.Position, data.Orientation);
        EventBus<ITfCollection>.Instance.Publish(eventTfName, Tf.GetTree());
    });

EventBus<IPointCloud>.Instance.Subscribe(eventPointCloudInputName,
    data =>
    {
        Kernel.DataInterfaces.Sensor.PointCloud pointCloud = new()
        {
            Points = Tf.Cast(pointCloudInputId, pointCloudOutputId, data.Points),
            Identifier = pointCloudOutputId
        };
        EventBus<IPointCloud>.Instance.Publish(eventPointCloudOutputName, pointCloud);
    });


Console.WriteLine("Enter to save");
Console.ReadLine();

EventBus<StringMessage>.Instance.Publish(eventSaveMapName,
    StringMessage.Build(mapSavePath));

Console.WriteLine($"Map save to {mapSavePath}");

#endregion