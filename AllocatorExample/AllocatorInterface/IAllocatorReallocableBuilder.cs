namespace AllocatorInterface
{
    public interface IAllocatorReallocableBuilder : IAllocatorBuilder
    {
        IAllocatorReallocable BuildReallocable();
    }
}
