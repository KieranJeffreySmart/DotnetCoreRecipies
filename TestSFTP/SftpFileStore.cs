using Renci.SshNet;
using System.IO;

public class SftpFileStore : IFileStore
{
    ISftpClient client;

    public SftpFileStore(ISftpClient client)
    {
        this.client = client;
    }

    public void CreateSubDirectory(string directoryName)
    {
        client.Connect();

        if (!client.Exists(directoryName))
        {
            client.CreateDirectory(directoryName);
        }

        client.Disconnect();
    }

    public Stream ReadFile(string directoryName, string fileName)
    {
        var downloadStream = new MemoryStream();

        client.Connect();
        client.DownloadFile($"{directoryName}/{fileName}", downloadStream);
        downloadStream.Position = 0;
        client.Disconnect();

        return downloadStream;
    }

    public string ReadFileAsText(string directoryName, string fileName)
    {
        var stream = ReadFile(directoryName, fileName);
        var sr = new StreamReader(stream);
        return sr.ReadToEnd();
    }

    public void WriteFile(Stream stream, string directoryName, string fileName)
    {
        client.Connect();
        client.UploadFile(stream, $"{directoryName}/{fileName}");
        client.Disconnect();
    }

    public void WriteTextFile(string text, string directoryName, string fileName)
    {
        var stream = new MemoryStream();
        var sw = new StreamWriter(stream);
        sw.Write(text);
        sw.Flush();
        stream.Position = 0;
        WriteFile(stream, directoryName, fileName);
    }
}

public interface ISftpClient
{
    void Connect();
    bool Exists(string path);
    void CreateDirectory(string path);
    void Disconnect();
    void DownloadFile(string path, Stream stream);
    void UploadFile(Stream stream, string path);
}

public class SshNetSftpClientAdaptor : ISftpClient
{
    SftpClient client;

    public SshNetSftpClientAdaptor(ConnectionInfo connectionInfo)
    {
        this.client = new SftpClient(connectionInfo);
    }

    public void Connect()
    {
        client.Connect();
    }

    public void CreateDirectory(string path)
    {
        client.CreateDirectory(path);
    }

    public void Disconnect()
    {
        client.Disconnect();
    }

    public void DownloadFile(string path, Stream stream)
    {
        client.DownloadFile(path, stream);
    }

    public bool Exists(string path)
    {
        return client.Exists(path);
    }

    public void UploadFile(Stream stream, string path)
    {
        client.UploadFile(stream, path);
    }
}