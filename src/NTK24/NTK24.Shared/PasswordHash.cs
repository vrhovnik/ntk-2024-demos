﻿using System.Security.Cryptography;

namespace NTK24.Shared;

/// <summary>
/// Salted password hashing with PBKDF2-SHA1.
/// Author: havoc AT defuse.ca
/// www: http://crackstation.net/hashing-security.htm
/// Compatibility: .NET 3.0 and later.
/// </summary>
public class PasswordHash
{
    // The following constants may be changed without breaking existing hashes.
    /// <summary>
    /// The sal t_ byt e_ size
    /// </summary>
    public const int SALT_BYTE_SIZE = 24;
    /// <summary>
    /// The has h_ byt e_ size
    /// </summary>
    public const int HASH_BYTE_SIZE = 24;
    /// <summary>
    /// The PBKD F2_ iterations
    /// </summary>
    public const int PBKDF2_ITERATIONS = 1000;

    /// <summary>
    /// The iteratio n_ index
    /// </summary>
    public const int ITERATION_INDEX = 0;
    /// <summary>
    /// The sal t_ index
    /// </summary>
    public const int SALT_INDEX = 1;
    /// <summary>
    /// The PBKD F2_ index
    /// </summary>
    public const int PBKDF2_INDEX = 2;

    /// <summary>
    /// Creates a salted PBKDF2 hash of the password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hash of the password.</returns>
    public static string CreateHash(string password)
    {
        var randomNumberGenerator = RandomNumberGenerator.Create();
        var salt = new byte[SALT_BYTE_SIZE];
        randomNumberGenerator?.GetBytes(salt);

        // Hash the password and encode the parameters
        var hash = PBKDF2(password, salt, PBKDF2_ITERATIONS, HASH_BYTE_SIZE);
        return $"{PBKDF2_ITERATIONS}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Validates a password given a hash of the correct one.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <param name="correctHash">A hash of the correct password.</param>
    /// <returns>True if the password is correct. False otherwise.</returns>
    public static bool ValidateHash(string password, string correctHash)
    {
        // Extract the parameters from the hash
        char[] delimiter = [':'];
        var split = correctHash.Split(delimiter);
        var iterations = int.Parse(split[ITERATION_INDEX]);
        var salt = Convert.FromBase64String(split[SALT_INDEX]);
        var hash = Convert.FromBase64String(split[PBKDF2_INDEX]);

        var testHash = PBKDF2(password, salt, iterations, hash.Length);
        return SlowEquals(hash, testHash);
    }

    /// <summary>
    /// Compares two byte arrays in length-constant time. This comparison
    /// method is used so that password hashes cannot be extracted from
    /// on-line systems using a timing attack and then attacked off-line.
    /// </summary>
    /// <param name="a">The first byte array.</param>
    /// <param name="b">The second byte array.</param>
    /// <returns>True if both byte arrays are equal. False otherwise.</returns>
    private static bool SlowEquals(IReadOnlyList<byte> a, IReadOnlyList<byte> b)
    {
        var diff = (uint)a.Count ^ (uint)b.Count;
        for (var i = 0; i < a.Count && i < b.Count; i++)
            diff |= (uint)(a[i] ^ b[i]);
        return diff == 0;
    }

    /// <summary>
    /// Computes the PBKDF2-SHA1 hash of a password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="iterations">The PBKDF2 iteration count.</param>
    /// <param name="outputBytes">The length of the hash to generate, in bytes.</param>
    /// <returns>A hash of the password.</returns>
    private static byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
    {
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
        return pbkdf2.GetBytes(outputBytes);
    }
}