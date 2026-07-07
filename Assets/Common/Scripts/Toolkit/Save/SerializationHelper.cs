using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace OctoberStudio.Save
{
    public static class SerializationHelper
    {
        public static readonly string persistentDataPath = Application.persistentDataPath + "/";

        public static T DeserializePersistent<T>(string fileName, bool isJson = false, string secureKey = "", bool useLogs = true) where T : new()
        {
            if(isJson)
            {
                return JsonDeserializeFromPath<T>(persistentDataPath + fileName, secureKey, useLogs);
            } else
            {
                return BinaryDeserializeFromPath<T>(persistentDataPath + fileName, useLogs);
            }
        }

        public static T BinaryDeserializeFromPath<T>(string absolutePath, bool useLogs = true) where T : new()
        {
            if (File.Exists(absolutePath))
            {
                var formatter = new BinaryFormatter();
                var file = File.Open(absolutePath, FileMode.Open);

                try
                {
                    return (T)formatter.Deserialize(file);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    return new T();
                }
                finally
                {
                    file.Close();
                }
            }
            else
            {
                if (useLogs) 
                    Debug.LogWarning($"File '{absolutePath}' does not exist.");

                return new T();
            }
        }

        public static T JsonDeserializeFromPath<T>(string absolutePath, string encriptionKey = "", bool useLogs = true) where T : new()
        {
            if (File.Exists(absolutePath))
            {
                var file = File.Open(absolutePath, FileMode.Open);

                using (var reader = new StreamReader(file))
                {
                    var jsonObject = reader.ReadToEnd();
                    reader.Dispose();

                    if (!string.IsNullOrEmpty(encriptionKey))
                        jsonObject = Decrypt(jsonObject, encriptionKey);

                    try
                    {
                        var deserializedObject = JsonUtility.FromJson<T>(jsonObject);
                        return deserializedObject;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                        return new T();
                    }
                    finally
                    {
                        file.Close();
                    }
                }
            }
            else
            {
                if (useLogs)
                {
                    Debug.LogError("File at path : \"" + absolutePath + "\" does not exist.");
                }
                return new T();
            }
        }

        public static void SerializePersistent<T>(T objectToSerialize, string fileName, bool isJson = false, string secureKey = "")
        {
            if (isJson)
            {
                JsonSerializeToPath(objectToSerialize, persistentDataPath + fileName, secureKey);
            } else
            {
                BinarySerializeToPath(objectToSerialize, persistentDataPath + fileName);
            }
        }

        public static void BinarySerializeToPath<T>(T objectToSerialize, string absolutePath)
        {
            var formatter = new BinaryFormatter();
            using (FileStream file = File.Open(absolutePath, FileMode.Create))
            {
                formatter.Serialize(file, objectToSerialize);
            }
        }

        public static void JsonSerializeToPath<T>(T objectToSerialize, string absolutePath, string secureKey = "")
        {
            var jsonObject = JsonUtility.ToJson(objectToSerialize);

            if (!string.IsNullOrEmpty(secureKey))
                jsonObject = Encrypt(jsonObject, secureKey);

            var stream = File.Open(absolutePath, FileMode.Create);
            var writer = new StreamWriter(stream);
            writer.Write(jsonObject);
            writer.Flush();

            writer.Close();
            stream.Close();
        }

        public static void DeletePersistent(string fileName)
        {
            File.Delete(persistentDataPath + fileName);
        }

        public static string Encrypt(string clearText, string EncryptionKey)
        {
            var clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText, string EncryptionKey)
        {
            cipherText = cipherText.Replace(" ", "+");
            var cipherBytes = Convert.FromBase64String(cipherText);
            using (var encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static bool IsFileLocked(string fileName)
        {
            string absolutePath = persistentDataPath + fileName;

            if (!File.Exists(absolutePath)) return false;

            try
            {
                var file = new FileInfo(absolutePath);
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }
    }
}