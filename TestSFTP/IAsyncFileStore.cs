using System.IO;
using System.Threading.Tasks;

public interface IAsyncFileStore: IAsyncFileWriter, IAsyncFileReader
{
    Task CreateSubDirectory(string directoryName);
}

public interface IAsyncFileWriter
{
    Task WriteFile(Stream stream, string directoryName, string fileName);

    Task WriteTextFile(string text, string directoryName, string fileName);
}

public interface IAsyncFileReader
{
    Task<Stream> ReadFile(string directoryName, string name);

    Task<string> ReadFileAsText(string directoryName, string fileName);
}
