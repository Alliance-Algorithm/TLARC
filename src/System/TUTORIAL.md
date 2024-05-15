# 声明物体
## 格式
```[json]
{
    "name": "sentry_target",
    "theta": "0",
    "y": "0",
    "x": "0"
}
```

## 参数说明
- name 名字
- theta 初始角度
- y y坐标
- x x坐标

# 声明组件群
## 格式
```[json]
{
    "list": [
        {
            comp1
        },
        {
            comp2
        }
        ...
    ]
}

```
# 声明组件
声明物体
## 格式
```[json]
{
    "this_id": "1",
    "input_id": [],
    "assembly": "AllianceDM.StdComponent",
    "type": "Transform2D",
    "arg": [
        "sentry"
    ]
}
```

## 参数说明

- _this_id_ ： 组件唯一标识符，不可重复
- _input_id_ ： 输入组件使用的id
- _assembly_ : 组件所属命名空间
- _type_ : 组件名（类名）
- _arg_ : 组件自定义消息

# 组件库约定: 在仓库根目录放置 Compoents.md 进行可用组件说明，格式参考本仓库