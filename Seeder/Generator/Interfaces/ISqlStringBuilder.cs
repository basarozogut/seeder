namespace Seeder.Generator.Interfaces
{
    public interface ISqlStringBuilder
    {
        void Append(string value);
        void AppendLine();
        void AppendLine(string value);
        void AppendCommentLine(string value);
        void EndStatement();
    }
}