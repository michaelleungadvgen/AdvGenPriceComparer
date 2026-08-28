using System;
using System.Security.Cryptography;
using System.IO;

public class Test {
    public static void Main() {
        var path = "test_hash.cs";
        var fileBytes = File.ReadAllBytes(path);
        var expectedHash = "TESTHASH";

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(fileBytes);
        var actualHash = Convert.ToHexString(hashBytes);

        Console.WriteLine($"Actual hash: {actualHash}");
        Console.WriteLine($"Match: {string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase)}");
    }
}
