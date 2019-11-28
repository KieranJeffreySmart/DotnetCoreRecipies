using Amazon.S3;
using System.Threading.Tasks;

public class S3FileStore : IFileStore
{
    private readonly IAmazonS3 client;

    public S3FileStore(IAmazonS3 client)
    {
        this.client = client;
    }

    public async Task CreateSubDirectory(string directoryName)
    {
        if (!(await SubDirectoryExists(directoryName)))
            await this.client.PutBucketAsync(directoryName);
    }

    public async Task<bool> SubDirectoryExists(string directoryName)
    {
        try
        {
            var response = await client.GetBucketPolicyStatusAsync(new Amazon.S3.Model.GetBucketPolicyStatusRequest() { BucketName = directoryName });
            return true;
        }
        catch (Amazon.S3.AmazonS3Exception ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            throw;
        }
    }
}