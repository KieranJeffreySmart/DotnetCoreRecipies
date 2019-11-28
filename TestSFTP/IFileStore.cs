using System.Threading.Tasks;

public interface IFileStore
{
    Task CreateSubDirectory(string directoryName);

    Task<bool> SubDirectoryExists(string directoryName);
}