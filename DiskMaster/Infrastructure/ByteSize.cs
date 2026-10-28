namespace DiskMaster.Infrastructure;

public static class ByteSize
{
    public static string Format(long bytes)
    {
        string[] units = ["o", "Kio", "Mio", "Gio", "Tio"];
        double value = bytes;
        int index = 0;
        while (value >= 1024 && index < units.Length - 1) { value /= 1024; index++; }
        return $"{value:0.##} {units[index]}";
    }
}
