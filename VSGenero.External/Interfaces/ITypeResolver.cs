namespace VSGenero.External.Interfaces
{
    public interface ITypeResolver
    {
        ITypeResult GetGeneroType(string variableName, string filename, int lineNumber);
        string GetQuickInfoTargetFullName();
    }
}
