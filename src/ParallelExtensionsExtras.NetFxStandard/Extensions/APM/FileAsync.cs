﻿//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: FileAsync.cs
//
//--------------------------------------------------------------------------

using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace System.IO;

/// <summary>Provides asynchronous counterparts to members of the File class.</summary>
public static class FileAsync
{
    private const int BUFFER_SIZE = 0x2000;

    /// <summary>Opens an existing file for asynchronous reading.</summary>
    /// <param name="path">The path to the file to be opened for reading.</param>
    /// <returns>A read-only <see cref="FileStream"/> on the specified <paramref name="path"/>.</returns>
    public static FileStream OpenRead(string path)
    {
        // Open a file stream for reading and that supports asynchronous I/O
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true);
    }

    /// <summary>Opens an existing file for asynchronous writing.</summary>
    /// <param name="path">The path to the file to be opened for writing.</param>
    /// <returns>An unshared <see cref="FileStream"/> on the specified <paramref name="path"/> with access for writing.</returns>
    public static FileStream OpenWrite(string path)
    {
        // Open a file stream for writing and that supports asynchronous I/O
        return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, BUFFER_SIZE, true);
    }

    /// <summary>
    /// Opens a binary file for asynchronous operation, reads the contents of the file into a byte array, and then closes the file.
    /// </summary>
    /// <param name="path">The path to the file to be read.</param>
    /// <returns>A <see cref="Task"/> that will contain the contents of the file.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
    [Obsolete("This API is superseded by System.IO.File.ReadAllBytesAsync().")]
#endif
    public static Task<byte[]> ReadAllBytes(string path)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
        return File.ReadAllBytesAsync(path);
#else
        // Open the file for reading
        var fs = OpenRead(path);

        // Read all of its contents
        var asyncRead = fs.ReadAllBytesAsync();

        // When we're done reading its contents, close the file and propagate the file's contents
        var closedFile = asyncRead.ContinueWith(t =>
        {
            fs.Close();
            return t.Result;
        }, TaskContinuationOptions.ExecuteSynchronously);

        // Return the task that represents the entire operation being complete and that returns the
        // file's contents
        return closedFile;
#endif
    }

    /// <summary>
    /// Opens a binary file for asynchronous operation, writes the contents of the byte array into the file, and then closes the file.
    /// </summary>
    /// <param name="path">The path to the file to be written.</param>
    /// <param name="bytes">An array of bytes to be written.</param>
    /// <returns>A <see cref="Task"/> that will signal the completion of the operation.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
    [Obsolete("This API is superseded by System.IO.File.WriteAllBytesAsync().")]
#endif
    public static Task WriteAllBytes(string path, byte[] bytes)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
        return File.WriteAllBytesAsync(path, bytes);
#else
        // Open the file for writing
        var fs = OpenWrite(path);

        // Write the contents to the file
        var asyncWrite = fs.WriteAsync(bytes, 0, bytes.Length);

        // When complete, close the file and propagate any exceptions
        var closedFile = asyncWrite.ContinueWith(t => 
        {
            var e = t.Exception;
            fs.Close();
            if (e != null) throw e;
        }, TaskContinuationOptions.ExecuteSynchronously);

        // Return a task that represents the operation having completed
        return closedFile;
#endif
    }

    /// <summary>
    /// Opens a text file for asynchronous operation, reads the contents of the file into a <see cref="string"/>, and then closes the file.
    /// </summary>
    /// <param name="path">The path to the file to be read.</param>
    /// <returns>A <see cref="Task"/> that will contain the contents of the file.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
    [Obsolete("This API is superseded by System.IO.File.ReadAllTextAsync().")]
#endif
    public static Task<string> ReadAllText(string path)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
        return File.ReadAllTextAsync(path);
#else
        // Create a StringBuilder to store the text from the file and an encoding object to decode the
        // contents of the file
        var text = new StringBuilder();
        var encoding = new UTF8Encoding();

        // Open the file for reading
        var fs = OpenRead(path);

        // Continually read buffers from the file, decoding them and storing the results into the StringBuilder
        var asyncRead = fs.ReadBuffersAsync(BUFFER_SIZE, (buffer, count) => text.Append(encoding.GetString(buffer, 0, count)));

        // When done, close the file, propagate any exceptions, and return the decoded text
        return asyncRead.ContinueWith(t =>
        {
            var e = t.Exception;
            fs.Close();
            if (e != null) throw e;
            return text.ToString();
        }, TaskContinuationOptions.ExecuteSynchronously);
#endif
    }

    /// <summary>
    /// Opens a text file for asynchronous operation, writes <paramref name="contents"/> into the file, and then closes the file.
    /// </summary>
    /// <param name="path">The path to the file to be written.</param>
    /// <param name="contents">The string to write to the file.</param>
    /// <returns>A <see cref="Task"/> that will signal the completion of the operation.</returns>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
    [Obsolete("This API is superseded by System.IO.File.WriteAllTextAsync().")]
#endif
    public static Task WriteAllText(string path, string contents)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
        return File.WriteAllTextAsync(path, contents);
#else
        // First encode the string contents into a byte array
        var encoded = Task.Factory.StartNew(
            state => Encoding.UTF8.GetBytes((string?)state!),
            contents);

        // When encoding is done, write all of the contents to the file.  Return
        // a task that represents the completion of that write.
        return encoded.ContinueWith(t => WriteAllBytes(path, t.Result)).Unwrap();
#endif
    }
}
