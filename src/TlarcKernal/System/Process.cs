using System.Collections;
using System.Timers;
using Rcl;
using Rcl.Logging;
using Tlarc.Init;
using Tlarc.IO;

namespace Tlarc;

class Process
{
    public required int fps { get; init; }
    public required Dictionary<uint, ComponentCell> Components { get; init; }
    List<List<ComponentCell>> UpdateFuncs = new();
    uint PoolDim = 0;
    uint TasksId = 0;
    readonly object _lock = new();
    bool _lockWasTaken = false;
    List<Task>[] tasks = [];
    public void Start()
    {
        double delay_time = 1000.0 / fps;

        StageConstruct();

        Awake();

        System.Timers.Timer timer = new System.Timers.Timer
        {
            Enabled = true,
            Interval = delay_time //执行间隔时间,单位为毫秒; 这里实际间隔为10分钟  
        };
        timer.Elapsed += new ElapsedEventHandler(LifeCycle);
        timer.Start();
    }
    void Awake()
    {
        tasks = new List<Task>[PoolDim + 1];
        for (int i = 0; i < PoolDim + 1; i++)
            tasks[i] = [];
        for (int i = 1; i < PoolDim; i++)
        {
            foreach (var a in UpdateFuncs[i])
            {
                if (!a.Image)
                    tasks[a.Early].Add(Task.Run(a.Start));
            }
            TasksId = (uint)i;

            Task.WaitAll([.. tasks[i]]);
        }
    }

    void LifeCycle(object? source, ElapsedEventArgs e)
    {
        if (_lockWasTaken)
        {
            Ros2Def.node.Logger.LogWarning("Warning: did not fix in fps : " + fps + "At Tasks :" + TasksId.ToString());
        }
        lock (_lock)
        {
            _lockWasTaken = true;
            InputUpdate();
            Update();
            OutputUpdate();
            _lockWasTaken = false;
        }
    }

    void InputUpdate()
    {
        ((IOManager)Components[0].Component).Input();
    }

    void Update()
    {
        for (int i = 0; i < PoolDim + 1; i++)
            tasks[i] = [];
        for (int i = 1; i < PoolDim; i++)
        {
            foreach (var a in UpdateFuncs[i])
            {
                tasks[a.Early].Add(Task.Run(a.Update));
            }
            TasksId = (uint)i;

            Task.WaitAll([.. tasks[i]]);
        }
    }
    void OutputUpdate()
    {
        ((IOManager)Components[0].Component).Output();
    }

    void FindPath(ref ComponentCell cell, in Hashtable colored)
    {
        try
        {
            if (colored.ContainsKey(cell.ID))
                throw new Exception("There is a loop,path is:");
            colored.Add(cell.ID, null);
            uint max = 0;
            for (var i = 0; i < cell.Forward.Count; i++)
            {
                ComponentCell c = cell.Forward[i];
                if (c.Dim == 0)
                    FindPath(ref c, in colored);
                max = Math.Max(c.Dim, max);
            }
            cell.Dim = max + 1;
            cell.Early = max + 1;
            if (max == 0)
                return;
            for (var i = 0; i < cell.Forward.Count; i++)
            {
                if (cell.Forward[i].Flag)
                    cell.Forward[i].Dim = Math.Min(max, cell.Forward[i].Dim);
                else
                {
                    cell.Forward[i].Dim = max;
                    cell.Forward[i].Flag = true;
                }
            }
            return;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message + "<-" + cell.ID.ToString());
        }
    }
    public void StageConstruct()
    {
        var l = Components.Values.ToArray();

        for (int k = 0; k < l.Length; k++)
        {
            // try
            {
                foreach (var i in l[k].ReceiveID)
                {
                    if (i.Value == 0)
                    {
                        Ros2Def.node.Logger.LogWarning("0 should not be inputID" + "@:" + l[k].ID.ToString());
                        continue;
                    }
                    if (!Components[i.Value].Image)
                        l[k].Forward.Add(Components[i.Value]);
                }
            }
            // catch (Exception e)
            // {
            //     Ros2Def.node.Logger.LogFatal(e.Message + "\twhen:Set ID:" + l[k].ID + " Forward node At Program.cs");
            //     Environment.Exit(-1);
            // }
        }
        for (int k = 0; k < l.Length; k++)
        {
            if (l[k].Dim != 0)
                continue;
            Hashtable colored = [];
            FindPath(ref l[k], in colored);
            PoolDim = Math.Max(l[k].Dim, PoolDim);
        }
        foreach (var i in l)
            if (!i.Flag)
                i.Dim = PoolDim;
        PoolDim += 1;
        for (int i = 0; i < PoolDim; i++)
            UpdateFuncs.Add([]);
        Components[0].Dim = 0;
        foreach (var i in l)
            if (!i.Image)
                UpdateFuncs[(int)i.Dim].Add(i);
    }

    //=====================
    public T GetComponentWithUID<T>(uint id) where T : Component
    {
        return Components[id].Component as T ?? throw new Exception("uuid:" + id.ToString());
    }
}