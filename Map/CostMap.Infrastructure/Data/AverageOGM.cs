using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using g4;
using Kernel.Contract;
using Kernel.Contract.Navigation;

namespace CostMap.Infrastructure.Data;

public class AverageOGM : IMap2D
{
    public required GridMap2DDescription   _description;
    public required float[,]               _ogm;
    public required int[,]                 _k;
    public sbyte[]?                        _data;
    
    private AverageOGM(){}

    public static explicit operator GridMap2DData(AverageOGM map)
    {
        map._data ??= new sbyte[map._description.Width * map._description.Height];
        for(int i = 0; i < map._description.Width;  i++)
        for(int j = 0; j < map._description.Height; j++)
        {
            var index = i + j * map._description.Width;
            map._data[index] = (sbyte)((map._ogm[i,j] > 0.3) ? 100 : ((map._ogm[i,j] < -0.3) ? 0 : -1)); 
        }
        return new GridMap2DData()
        {
            Header = map._description,
            Data = map._data
        };
    }

    private bool Check(int x, int y)
    {
        return x >= 0 && y >= 0 && x < _description.Width && y < _description.Height;
    }
    private Vector2i Index(Vector2 p2, Func<Vector3,Vector3> cast)
    {
        var p1 = cast(new(p2, 0));
        var p = new Vector2(p1.X, p1.Y);
        var Resolution  = _description.Resolution;
        var posInMap    = p - _description.Origin;
        return new ((int) Math.Round(posInMap.X / Resolution),(int)Math.Round(posInMap.Y / Resolution));
    }
    public void Update<Map2dT>(Map2dT map, Func<Vector3,Vector3> cast)where Map2dT : IEnumableGridMap
    {
        map.All(IEnumableGridMap.StateEnum.Free | IEnumableGridMap.StateEnum.Occu, (p, s) =>
        {
            var i = Index(p, cast);
            var x = i.x;
            var y = i.y;
            if(!Check(x,y)) return;
            if(s == IEnumableGridMap.StateEnum.Occu)
            {   
                _k[x,y]++;
                _ogm[x,y] = (_ogm[x,y] * _k[x,y] + 1) / (_k[x,y] + 1);
            }
            if(s == IEnumableGridMap.StateEnum.Free)
            {   
                _k[x,y]++;
                _ogm[x,y] = (_ogm[x,y] * _k[x,y] - 1) / (_k[x,y] + 1);
            }
        },true);  
    }

    public static AverageOGM CreateEmpty(GridMap2DDescription description) =>
        new (){
            _description    = description,
            _k              = new int[description.Width, description.Height],
            _ogm            = new float[description.Width, description.Height],
        };

    public bool IsMoveAble(Vector2 from, Vector2 to)
    {
        throw new NotImplementedException();
    }

    public bool IsMoveAble(Vector2 position)
    {
        throw new NotImplementedException();
    }
}