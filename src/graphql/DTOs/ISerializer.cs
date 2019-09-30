namespace Tanka.GraphQL.DTOs
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T obj);
        T Deserialize<T>(byte[] json);
    }
}