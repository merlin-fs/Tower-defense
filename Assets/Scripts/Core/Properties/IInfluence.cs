namespace TowerDefense.Core
{
    public interface IInfluence : ISlice
    {
        void Apply(IUnit target);
    }
}