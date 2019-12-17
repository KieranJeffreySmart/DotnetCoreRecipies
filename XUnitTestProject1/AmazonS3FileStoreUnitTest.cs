using Amazon.S3;
using Amazon.S3.Transfer;
using NSubstitute;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject1
{
    public class AmazonS3FileStoreUnitTest
    {
        IFileStore sut;
        private IAmazonS3 amazonMock = Substitute.For<IAmazonS3>();
        private ITransferUtility transferUtilityMock = Substitute.For<ITransferUtility>();

        public AmazonS3FileStoreUnitTest()
        {
            sut = new S3FileStore(amazonMock, (client) => transferUtilityMock);
        }

        [Fact]
        public async Task CreateSubDirectoryThatExists()
        {
            string bucketName = "BucketName";
            amazonMock.DoesS3BucketExistAsync(Arg.Is<string>(a => a == bucketName)).Returns(true);
            await sut.CreateSubDirectory(bucketName);
            amazonMock.Received().DoesS3BucketExistAsync(Arg.Is<string>(a => a == bucketName));
            amazonMock.DidNotReceive().PutBucketAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task CreateSubDirectoryThatDoesntExist()
        {
            string bucketName = "BucketName";
            amazonMock.DoesS3BucketExistAsync(Arg.Is<string>(a => a == bucketName)).Returns(false);
            await sut.CreateSubDirectory(bucketName);
            amazonMock.Received().DoesS3BucketExistAsync(Arg.Is<string>(a => a == bucketName));
            amazonMock.Received().PutBucketAsync(Arg.Is<string>(a => a == bucketName));
        }

        [Fact]
        public async Task WriteFile()
        {
            var stream = new MemoryStream(new byte[] {1, 2, 3});
            string bucketName = "BucketName";
            string fileName = "myfile.bin";
            await sut.WriteFile(stream, bucketName, fileName);
            
            transferUtilityMock.Received().UploadAsync(Arg.Is<Stream>(s => StreamsAreEqual(s, stream)), Arg.Is<string>(a => a == bucketName), Arg.Is<string>(a => a == fileName));
        }

        [Fact]
        public async Task WriteTextFile()
        {
            var text = "1, 2, 3";
            string bucketName = "BucketName";
            string fileName = "myfile.bin";
            await sut.WriteTextFile(text, bucketName, fileName);

            transferUtilityMock.Received().UploadAsync(Arg.Is<Stream>(s => StreamTextIsEqual(s, text)), Arg.Is<string>(a => a == bucketName), Arg.Is<string>(a => a == fileName));
        }

        [Fact]
        public async Task ReadFile()
        {
            var stream = new MemoryStream();
            
            string bucketName = "BucketName";
            string fileName = "myfile.bin";

            var file = await sut.ReadFile(bucketName, fileName);

            amazonMock.Received().PutBucketAsync(Arg.Is<string>(a => a == bucketName));
        }

        [Fact]
        public async Task ReadFileAsText()
        {
            var stream = new MemoryStream();
            string bucketName = "BucketName";
            string fileName = "myfile.bin";

            var file = await sut.ReadFileAsText(bucketName, fileName);

            amazonMock.Received().PutBucketAsync(Arg.Is<string>(a => a == bucketName));
        }

        private bool StreamTextIsEqual(Stream stream, string text)
        {
            var streamReader = new StreamReader(stream);
            var streamText = streamReader.ReadToEnd();

            Assert.Equal(streamText, text);
            return true;
        }

        private bool StreamsAreEqual(Stream streamA, Stream streamB)
        {
            var bufferA = new byte[streamA.Length];
            streamA.Read(bufferA, 0, (int)streamA.Length);

            var bufferB = new byte[streamB.Length];
            streamA.Read(bufferB, 0, (int)streamA.Length);

            Assert.Equal(bufferA, bufferB);
            return true;
        }
    }
}
