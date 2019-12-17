using NSubstitute;
using System.IO;
using Xunit;

namespace XUnitTestProject1
{
    public class SftpFileStoreUnitTest
    {
        IFileStore sut;
        private ISftpClient sshClientMock = Substitute.For<ISftpClient>();

        public SftpFileStoreUnitTest()
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
