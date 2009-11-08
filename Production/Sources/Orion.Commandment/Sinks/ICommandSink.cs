
namespace Orion.Commandment
{
    public interface ICommandSink
    {
        void Feed(Command command);
        void EndFeed();
    }
}
