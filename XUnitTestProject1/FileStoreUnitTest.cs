using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject1
{
    public class FileStoreUnitTest
    {
        IFileStore sut;
        private IAmazonS3 amazonMock = Substitute.For<IAmazonS3>();

        public FileStoreUnitTest()
        {
            sut = new S3FileStore(amazonMock);
        }

        [Fact]
        public async Task CreateSubDirectoryThatExists()
        {
            string bucketName = "BucketName";
            amazonMock.GetBucketPolicyStatusAsync(Arg.Is<GetBucketPolicyStatusRequest>(a => a.BucketName == bucketName)).Returns(new GetBucketPolicyStatusResponse());
            await sut.CreateSubDirectory(bucketName);
            amazonMock.Received().GetBucketPolicyStatusAsync(Arg.Is<GetBucketPolicyStatusRequest>(a => a.BucketName == bucketName));
            amazonMock.DidNotReceive().PutBucketAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task CreateSubDirectoryThatDoesntExist()
        {
            string bucketName = "BucketName";
            amazonMock.When(m => m.GetBucketPolicyStatusAsync(Arg.Is<GetBucketPolicyStatusRequest>(a => a.BucketName == bucketName))).Throw(new AmazonS3Exception("", ErrorType.Receiver, "", "", System.Net.HttpStatusCode.NotFound));
            await sut.CreateSubDirectory(bucketName);
            amazonMock.Received().GetBucketPolicyStatusAsync(Arg.Is<GetBucketPolicyStatusRequest>(a => a.BucketName == bucketName));
            amazonMock.Received().PutBucketAsync(Arg.Is<string>(a => a == bucketName));
        }

        [Fact]
        public async Task FailToCreateSubDirectory()
        {
            string bucketName = "BucketName";
            amazonMock.When(m => m.GetBucketPolicyStatusAsync(Arg.Is<GetBucketPolicyStatusRequest>(a => a.BucketName == bucketName))).Throw(new AmazonS3Exception("", ErrorType.Receiver, "", "", System.Net.HttpStatusCode.BadGateway));
            Func<Task> createDirectory = () => sut.CreateSubDirectory(bucketName);
            Assert.ThrowsAsync<AmazonS3Exception>(createDirectory);
        }
    }
}
