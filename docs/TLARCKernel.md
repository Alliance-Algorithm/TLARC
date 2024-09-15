# The Top Layer Controller System With ROS:humble and Dotnet C#
## 简介
TLARC Framework 是基于ROS2 和 C#12 开发的组件化编程架构，用户可以编写类似游戏引擎的代码进行机器人项目构建，项目本身具有声明周期和组件依赖的概念，可以自动进行组件依赖分析，并行化处理，多线程不同频率运行组件，架构保证了每个组件在Update中依赖的数据不会改变，对Ros接口进行了重新编写，以适应上述要求

## 特性
- 组件化编程
- 多个不同频率组件同时运行
- 架构自动完成线程安全
- 高效率，运行时信息传递使用表达式树自动生成
- 管线依赖的外部信息值为组件生命周期开始时的值且该周期内不会改变
- 无需关心组件之间数据传递
- 依赖组件在同一生命周期内必然先执行完成
- 组件之间自动分析并并行执行无依赖关系的组件
- 兼容ROS
- 原生.Net环境，可使用Nuget生态
## 安装
[项目地址](https://github.com/Alliance-Algorithm/TLARC-Framwork/)，新建c#[RCLNet](https://github.com/noelex/rclnet)项目，注意选择正确的版本，然后拉取本库到任意地方即可

具体步骤可以看下文

- 新建控制台项目
```
dotnet new install Rcl.NET.Templates
mkdir your_project_name && cd your_project_name
dotnet new ros2-node
```
- 删除目录下的.cs文件，（确保控制台工作区在your_project_name）
```
rm -f ./*.cs
```

将项目拉取于C#项目的任意地方，建议放置于```src```文件夹
```
mkdir src && git clone -b main https://github.com/Alliance-Algorithm/TLARC-Framwork/
```

- 启用调试模式：```VSCode or VS```,安装相应插件后```f5```, 启用部署模式
```
source /opt/ros/humble/setup.sh && colcon build
```

## 使用
### 概念说明
#### 组件
组件是一个基本的执行单位，继承自```Component```类，例：
```
class PathGenerator : Component
```
```Component```有2个主要的虚函数
> ```void Start()```:继承类中该函数的数据会在最开始的时候执行一次，依然有依赖分析，依赖的组件会先执行，但是跨管线（管线的概念会在之后提出）的并无此约束

>```void Update()```继承类中每次生命周期执行一次，执行频率由管线设置

组件的字段和属性有独立含义
>```字段```：字段是组件的数据存放处，需要跨管线的数据储存于Public中，不需要的储存于private中，字段数据可以在```Start()```之前被赋值为外部管线描述文件内规定的值，但是public的字段开销会更大，至于为何不用反射而需要public才能在管线中传递数据，这是为了性能考虑

> ```属性```:public的属性代表仅在管线内传递的值，开销小于public的字段，除非跨管线，否则请使用public的属性 
#### 管线
管线是一个工作流的总称，在管线内有多个组件组成，每个管线由一份描述文件给出，在一个管线内的组件会按顺序执行Update函数，在管线开始和结束由IOManager 获取到该管线需要的Ros信息或者其他管线的组件信息

#### 生命周期
生命周期指管线从接受数据开始，依次执行管线中的Update函数，到向外传递信息的过程

### 示例
#### 组件编写示例
```[c#]
class EngineerTargetPreDictor : Component // 组件需要继承自Component
{
    // 向外传递的数据
    public bool Found => unitInfo.Found[VehicleCode];
    public bool Locked { get; private set; }

    // 依赖的组件
    Transform2D sentry;

    // 私有数据
    float[] angles_ = { -MathF.PI, 0 };

    // 主要的执行函数
    public override void Update()
    {
        // do what you want to do in this component
    }
}
```

#### 管线编写示例
管线描述文件为一个json
```[json]
{
    "fps": 10,    //管线执行速率
    "list": [
        // 组件描述
        {
            "this_id": 201, //ID
            "input_id": {// 组件的依赖组件ID
                "FSM": 202  // “类中字段名称”：目标组件ID
            },
            "assembly": "Tlarc.ALPlanner", // 组件类的命名空间
            "type": "ALPlannerDecisionMaker", // 组件的类名
            "arg": {
                // “类中字段名称”：目标值
                "minEquivalentHpLimitToReturn": 300, 
                } // 组件内非组件字段初始值
        }
    ]
}
```

#### 调试
安装好相对应的C#调试插件，使用自带的调试器即可

#### 部署
```
source /opt/ros/humble/setup.sh && colcon build
```