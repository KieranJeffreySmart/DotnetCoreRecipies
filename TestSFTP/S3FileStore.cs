using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.Threading.Tasks;

public class S3FileStore : IAsyncFileStore
{
    private readonly IAmazonS3 client;
    private Func<IAmazonS3, ITransferUtility> transferUtilityFactory;

    public S3FileStore(IAmazonS3 client, Func<IAmazonS3, ITransferUtility> transferUtilityFactory)
    {
        this.client = client;
    }

    public async Task CreateSubDirectory(string directoryName)
    {
        if (!(await client.DoesS3BucketExistAsync(directoryName)))
            await this.client.PutBucketAsync(directoryName);
    }

    public async Task<Stream> ReadFile(string directoryName, string fileName)
    {
        var response = await client.GetObjectAsync(directoryName, fileName);
        return response.ResponseStream;
    }

    public async Task<string> ReadFileAsText(string directoryName, string fileName)
    {
        var streamReader = new StreamReader(await ReadFile(directoryName, fileName));
        return await streamReader.ReadToEndAsync();
    }

    public async Task WriteFile(Stream stream, string directoryName, string fileName)
    {
        stream.Position = 0;
        var transferUtility = transferUtilityFactory(client);
        await transferUtility.UploadAsync(stream, directoryName, fileName);
    }

    public async Task WriteTextFile(string text, string directoryName, string fileName)
    {
        var stream = new MemoryStream();
        var streamWriter = new StreamWriter(stream);
        streamWriter.Write(text);

        await WriteFile(stream, directoryName, fileName);
    }
}

public class SftpFileStore: IFileStore
{

}