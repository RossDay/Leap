using Leap;

namespace Vyrolan.VMCS
{
    internal interface IFrameUpdater
    {
        void RegisterForFrameUpdates(IFrameUpdate item);
        void UnregisterForFrameUpdates(IFrameUpdate item);
    }

    internal interface IFrameUpdate
    {
        bool Update(Frame frame);
    }
}
