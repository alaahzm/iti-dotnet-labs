namespace lab15.Services
{
    public class AlaaStampService : IAlaaStampService
    {
        public string Stamp { get; }
        public string Owner => "Alaa Hazem Helmy";

        public AlaaStampService()
        {
            Stamp = Guid.NewGuid().ToString().Substring(0, 8);
        }
    }
}