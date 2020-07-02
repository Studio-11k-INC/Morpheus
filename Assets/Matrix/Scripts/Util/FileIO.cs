using System.IO;

public class FileIO
{
    public static string OpenFile(string file)
    {
        string retVal = "";
        StreamReader reader;

        if (File.Exists(file))
        {
            FileStream stream = File.Open(file, FileMode.Open);
            reader = new StreamReader(stream);
            retVal = reader.ReadToEnd();
            reader.Close();
            stream.Close();            
        }        

        return retVal;
    }

    public static void WriteFile(string file, byte[] bytes)
    {
        FileStream stream = File.Open(file, FileMode.Create);        
        stream.Write(bytes, 0, bytes.Length);
        stream.Close();
    }
}

