namespace EBusTGXImporter.Logger
{
    public interface ILogService
    {
        void Fatal(string message);
        void Error(string message);
        void Warn(string message);
        void Info(string message);
        void Debug(string message);
    }
}
