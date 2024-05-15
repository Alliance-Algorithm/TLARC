# Transform2D

## 格式
```[json]
{
    "this_id": "2",
    "input_id": [],
    "assembly": "AllianceDM.StdComponent",
    "type": "Transform2D",
    "arg": [
        "sentry_target",
       "W",
        "/sentry/target/publish"
    ]
},
```

## args参数说明
- _arg[0]_: 目标游戏物体名
- _arg[1]_: 读写模式：读为“R”，无需_arg[3]_,仅读取该物体，写为“W”，从ROS接受消息并写入
- _arg[3]_: 写模式下的TopicName