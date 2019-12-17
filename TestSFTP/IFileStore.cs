using System.IO;
using System.Threading.Tasks;

public interface IFileStore: IFileWriter, IFileReader
{
    void CreateSubDirectory(string directoryName);
}

public interface IFileWriter
{
    void WriteFile(Stream stream, string directoryName, string fileName);

    void WriteTextFile(string text, string directoryName, string fileName);
}

public interface IFileReader
{
    Stream ReadFile(string directoryName, string name);

    string ReadFileAsText(string directoryName, string fileName);
}