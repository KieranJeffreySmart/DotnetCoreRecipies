using Amazon.S3;
using Amazon.S3.Model;
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
        IAsyncFileStore sut;
        private IAmazonS3 amazonMock = Substitute.For<IAmazonS3>();
        private ITransferUtility transferUtilityMock = Substitute.For<ITransferUtility>();


        public AmazonS3FileStoreUnitTest()
        {
            sut = new S3FileStore(amazonMock, TransferutilityCreate);
        }

        private ITransferUtility TransferutilityCreate(IAmazonS3 client)
        {
            return transferUtilityMock;
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
            var actualText = string.Empty;

            transferUtilityMock.When(tum => tum.UploadAsync(Arg.Is<Stream>(s => StreamTextIsEqual(s, text)), Arg.Is<string>(a => a == bucketName), Arg.Is<string>(a => a == fileName)))
                .Do(cb => actualText = ExtractTextFromStream(cb.Arg<Stream>()));

            await sut.WriteTextFile(text, bucketName, fileName);

            Assert.Equal(text, actualText);
        }

        private string ExtractTextFromStream(Stream stream)
        {
            stream.Position = 0;
            var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        [Fact]
        public async Task ReadFile()
        {
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            GetObjectResponse response = new GetObjectResponse();
            response.ResponseStream = stream;

            string bucketName = "BucketName";
            string fileName = "myfile.bin";

            amazonMock.GetObjectAsync(Arg.Is<string>(s => s == bucketName), Arg.Is<string>(s => s == fileName)).Returns(response);

            var file = await sut.ReadFile(bucketName, fileName);

            Assert.True(StreamsAreEqual(file, stream));
        }

        [Fact]
        public async Task ReadFileAsText()
        {
            var content = "My String Content";
            var stream = new MemoryStream();
            var sw = new StreamWriter(stream);
            sw.Write(content);
            sw.Flush();
            stream.Position = 0;

            string bucketName = "BucketName";
            string fileName = "myfile.bin";

            GetObjectResponse response = new GetObjectResponse();
            response.ResponseStream = stream;
            amazonMock.GetObjectAsync(Arg.Is<string>(s => s == bucketName), Arg.Is<string>(s => s == fileName)).Returns(response);
            
            var file = await sut.ReadFileAsText(bucketName, fileName);

            Assert.Equal(file, content);
        }

        private bool StreamTextIsEqual(Stream stream, string text)
        {
            Assert.Equal(ExtractTextFromStream(stream), text);
            return true;
        }

        private bool StreamsAreEqual(Stream streamA, Stream streamB)
        {
            streamA.Position = 0;
            var bufferA = new byte[streamA.Length];
            streamA.Read(bufferA, 0, (int)streamA.Length);

            streamB.Position = 0;
            var bufferB = new byte[streamB.Length];
            streamA.Read(bufferB, 0, (int)streamB.Length);

            Assert.Equal(bufferA, bufferB);
            return true;
        }
    }


    public class SshNetFileStoreUnitTest
    {
        IFileStore sut;
        private ISftpClient sshClientMock = Substitute.For<ISftpClient>();

        public SshNetFileStoreUnitTest()
        {
            sut = new SftpFileStore(sshClientMock);
        }

        [Fact]
        public void CreateSubDirectoryThatExists()
        {
            string bucketName = "BucketName";
            sshClientMock.Exists(Arg.Is<string>(a => a == bucketName)).Returns(true);
            sut.CreateSubDirectory(bucketName);
            sshClientMock.Received().Exists(Arg.Is<string>(a => a == bucketName));
            sshClientMock.DidNotReceive().CreateDirectory(Arg.Any<string>());
        }

        [Fact]
        public void CreateSubDirectoryThatDoesntExist()
        {
            string bucketName = "BucketName";
            sshClientMock.Exists(Arg.Is<string>(a => a == bucketName)).Returns(false);
            sut.CreateSubDirectory(bucketName);
            sshClientMock.Received().Exists(Arg.Is<string>(a => a == bucketName));
            sshClientMock.Received().CreateDirectory(Arg.Is<string>(a => a == bucketName));
        }

        [Fact]
        public void WriteFile()
        {
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            string bucketName = "BucketName";
            string fileName = "myfile.bin";
            sut.WriteFile(stream, bucketName, fileName);

            sshClientMock.Received().UploadFile(Arg.Is<Stream>(s => StreamsAreEqual(s, stream)), Arg.Is<string>(a => a == $"{bucketName}/{fileName}"));
        }

        [Fact]
        public void WriteTextFile()
        {
            var text = "1, 2, 3";
            string bucketName = "BucketName";
            string fileName = "myfile.bin";
            sut.WriteTextFile(text, bucketName, fileName);

            sshClientMock.Received().UploadFile(Arg.Is<Stream>(s => StreamTextIsEqual(s, text)), Arg.Is<string>(a => a == $"{bucketName}/{fileName}"));
        }

        [Fact]
        public void ReadFile()
        {
            var stream = new MemoryStream();

            string bucketName = "BucketName";
            string fileName = "myfile.bin";

            var file = sut.ReadFile(bucketName, fileName);

            sshClientMock.Received().DownloadFile(Arg.Is<string>(a => a == $"{bucketName}/{fileName}"), Arg.Any<Stream>());
        }

        [Fact]
        public void ReadFileAsText()
        {
            var stream = new MemoryStream();
            string bucketName = "BucketName";
            string fileName = "myfile.bin";

            var file = sut.ReadFileAsText(bucketName, fileName);

            sshClientMock.Received().DownloadFile(Arg.Is<string>(a => a == $"{bucketName}/{fileName}"), Arg.Any<Stream>());
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
            streamA.Position = 0;
            var bufferA = new byte[streamA.Length];
            streamA.Read(bufferA, 0, (int)streamA.Length);

            streamB.Position = 0;
            var bufferB = new byte[streamB.Length];
            streamA.Read(bufferB, 0, (int)streamB.Length);

            Assert.Equal(bufferA, bufferB);
            return true;
        }
    }
}
