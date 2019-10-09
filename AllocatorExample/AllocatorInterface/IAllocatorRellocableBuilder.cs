namespace AllocatorInterface
{
    public interface IAllocatorRellocableBuilder : IAllocatorBuilder
    {
        IAllocatorReallocable BuildRellocable();
    }
}
