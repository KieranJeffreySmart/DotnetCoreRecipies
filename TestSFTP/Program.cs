using Renci.SshNet;
using System;
using System.IO;
using System.Threading;

namespace TestSFTP
{
    class Program
    {
        private static string SftpHost = "127.0.0.1";
        private static string SftpUser = "TestUser1";
        private static string sftpPassword = "Password1";
        private static string sftpPrivateKey = @"C:/Users/kezza/.ssh/azurePasswordlessSshKey";
        private static string sftpRemotePath = $"./testData/"; //$"/home/{SftpUser}/SftpTest/TabSeperatedData.txt"

        static void Main(string[] args)
        { 
            // PrivateKeyFile keyFile = new PrivateKeyFile(sftpPrivateKey);
            var sftpClient = new SftpClient(SftpHost, SftpUser, sftpPassword);

            var fileContent = File.ReadAllText("./SftpData/TabSeperatedData.txt");

            var uploadStream = new MemoryStream();
            var sw = new StreamWriter(uploadStream);
            sw.Write(fileContent);
            sw.Flush();
            uploadStream.Position = 0;

            sftpClient.Connect();
            sftpClient.UploadFile(uploadStream, sftpRemotePath+ "TabSeperatedData.txt");
            sftpClient.Disconnect();

            var downloadStream = new MemoryStream();

            sftpClient.Connect();
            sftpClient.DownloadFile(sftpRemotePath + "TabSeperatedData.txt", downloadStream);

            downloadStream.Position = 0;
            var sr = new StreamReader(downloadStream);
            var downloadedContent = sr.ReadToEnd();

            downloadStream.Close();
            sftpClient.Disconnect();

            Console.WriteLine(downloadedContent);

            Thread.Sleep(5000);
        }
    }

    public class AdeptraFile
    {
        public string FileName { get; }
        public string FileExtension { get; }
        private Stream Stream { get; }
    }
}
