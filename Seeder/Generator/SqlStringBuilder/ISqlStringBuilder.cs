namespace Seeder.Generator.SqlStringBuilder
{
    interface ISqlStringBuilder
    {
        void Append(string value);
        void AppendLine();
        void AppendLine(string value);
        void AppendCommentLine(string value);
        void EndStatement();
    }
}