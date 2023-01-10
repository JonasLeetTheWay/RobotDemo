using Common.Protos;
using System.Reflection;

namespace Common.LoggerHelper;
public static class LoggerHelper
{
    public static string GetMethodData(MethodBase methodBase)
    {
        return $"{methodBase?.DeclaringType.Name} {methodBase?.Name}->\n\t\t\t\t\t\t\t\t";
    }
    public static string GetString_LocationResponse(LocationResponse locationResponse)
    {
        return "id: " + locationResponse.Id + ", name:" + locationResponse.Name + ", x:" + locationResponse.X + ", y:" + locationResponse.Y + ", robotIds:" + string.Join(",", locationResponse.RobotIds);
    }
}
