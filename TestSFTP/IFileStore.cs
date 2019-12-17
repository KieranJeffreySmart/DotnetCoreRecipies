﻿using System.IO;
using System.Threading.Tasks;

public interface IAsyncFileStore: IFileWriter, IFileReader
{
    Task CreateSubDirectory(string directoryName);
}

public interface IFileWriter
{
    Task WriteFile(Stream stream, string directoryName, string fileName);

    Task WriteTextFile(string text, string directoryName, string fileName);
}

public interface IFileReader
{
    Task<Stream> ReadFile(string directoryName, string name);

    Task<string> ReadFileAsText(string directoryName, string fileName);
}


public interface ISynchronousFileStore: ISynchronousFileWriter, ISynchronousFileReader
{
    void CreateSubDirectory(string directoryName);
}

public interface ISynchronousFileWriter
{
    void WriteFile(Stream stream, string directoryName, string fileName);

    void WriteTextFile(string text, string directoryName, string fileName);
}

public interface ISynchronousFileReader
{
    Stream ReadFile(string directoryName, string name);

    string ReadFileAsText(string directoryName, string fileName);
}