using System.Linq.Expressions;
using System.Reflection;

namespace Tlarc.IO.TlarcMsgs;

public class CopyWithExpressions
{
    Action<object, object>? _copyDelegate = null;
    Func<object, object, object>? _newCopyDelegate = null;
    object _tmp;

    public void CopyFrom(object src) => NewCopy(src, _tmp);
    public void CopyTo(object dst) => Copy(_tmp, dst);
    public void Copy(object src, object dst)
    {
        if (_copyDelegate == null)
        {
            var sourceType = src.GetType();
            var targetType = dst.GetType();

            var sourceParameter = Expression.Parameter(typeof(object), "source");
            var targetParameter = Expression.Parameter(typeof(object), "target");

            var sourceVariable = Expression.Variable(sourceType, "typedSource");
            var targetVariable = Expression.Variable(targetType, "typedTarget");
            var loopVariable = Expression.Variable(typeof(int), "i");

            var assignSource = Expression.Assign(sourceVariable, Expression.Convert(sourceParameter, sourceType));
            var assignTarget = Expression.Assign(targetVariable, Expression.Convert(targetParameter, targetType));

            var bindings = new List<Expression>
            {
                assignSource,
                assignTarget
            };

            foreach (var sourceFiled in sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {

                var targetFiled = targetType.GetField(sourceFiled.Name, BindingFlags.Public | BindingFlags.Instance);

                var sourceValue = Expression.Field(sourceVariable, sourceFiled);
                var targetValue = Expression.Field(targetVariable, targetFiled);
                if (sourceFiled.FieldType.IsArray)
                {
                    var elementType = sourceFiled.FieldType.GetElementType();
                    var arrayLength = Expression.ArrayLength(sourceValue);
                    var newArray = Expression.NewArrayBounds(elementType, arrayLength);
                    var assignArray = Expression.Assign(targetValue, newArray);

                    var breakLabel = Expression.Label("LoopBreak");

                    var loopBody = Expression.Block(
                        Expression.IfThenElse(
                            Expression.LessThan(loopVariable, arrayLength),
                            Expression.Block(
                                Expression.Assign(
                                    Expression.ArrayAccess(targetValue, loopVariable),
                                    Expression.ArrayAccess(sourceValue, loopVariable)
                                ),
                                Expression.PostIncrementAssign(loopVariable)
                            ),
                            Expression.Break(breakLabel)
                        )
                    );

                    var loop = Expression.Loop(loopBody, breakLabel);

                    bindings.Add(assignArray);
                    bindings.Add(Expression.Assign(loopVariable, Expression.Constant(0)));
                    bindings.Add(loop);
                }
                else
                {
                    var assign = Expression.Assign(targetValue, sourceValue);
                    bindings.Add(assign);
                }

            }

            var body = Expression.Block(new[] { sourceVariable, targetVariable, loopVariable }, bindings);
            var lambda = Expression.Lambda<Action<object, object>>(body, sourceParameter, targetParameter);
            _copyDelegate = lambda.Compile();
        }

        _copyDelegate(src, dst);
    }
    public void NewCopy(object src, object dst)
    {
        if (_newCopyDelegate == null)
        {
            var sourceType = src.GetType();
            var targetType = dst.GetType();

            var sourceParameter = Expression.Parameter(typeof(object), "source");
            var targetParameter = Expression.Parameter(typeof(object), "target");

            var sourceVariable = Expression.Variable(sourceType, "typedSource");
            var targetVariable = Expression.Variable(targetType, "typedTarget");
            var loopVariable = Expression.Variable(typeof(int), "i");

            var assignSource = Expression.Assign(sourceVariable, Expression.Convert(sourceParameter, sourceType));
            var assignTarget = Expression.Assign(targetVariable, Expression.New(dst.GetType()));

            var bindings = new List<Expression>
            {
                assignSource,
                assignTarget
            };

            foreach (var sourceFiled in sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {

                var targetFiled = targetType.GetField(sourceFiled.Name, BindingFlags.Public | BindingFlags.Instance);

                var sourceValue = Expression.Field(sourceVariable, sourceFiled);
                var targetValue = Expression.Field(targetVariable, targetFiled);
                if (sourceFiled.FieldType.IsArray)
                {
                    var elementType = sourceFiled.FieldType.GetElementType();
                    var arrayLength = Expression.ArrayLength(sourceValue);
                    var newArray = Expression.NewArrayBounds(elementType, arrayLength);
                    var assignArray = Expression.Assign(targetValue, newArray);

                    var breakLabel = Expression.Label("LoopBreak");

                    var loopBody = Expression.Block(
                        Expression.IfThenElse(
                            Expression.LessThan(loopVariable, arrayLength),
                            Expression.Block(
                                Expression.Assign(
                                    Expression.ArrayAccess(targetValue, loopVariable),
                                    Expression.ArrayAccess(sourceValue, loopVariable)
                                ),
                                Expression.PostIncrementAssign(loopVariable)
                            ),
                            Expression.Break(breakLabel)
                        )
                    );

                    var loop = Expression.Loop(loopBody, breakLabel);

                    bindings.Add(assignArray);
                    bindings.Add(Expression.Assign(loopVariable, Expression.Constant(0)));
                    bindings.Add(loop);
                }
                else
                {
                    var assign = Expression.Assign(targetValue, sourceValue);
                    bindings.Add(assign);
                }

            }

            var body = Expression.Block([sourceVariable, targetVariable, loopVariable], [.. bindings, targetVariable]);
            var lambda = Expression.Lambda<Func<object, object, object>>(body, sourceParameter, targetParameter);
            _newCopyDelegate = lambda.Compile();
        }

        _tmp = _newCopyDelegate(src, dst);
    }
}