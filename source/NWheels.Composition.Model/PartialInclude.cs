namespace NWheels.Composition.Model
{
    public abstract class PartialInclude<TConfig>
    {
        protected PartialInclude(TConfig config)
        {
            Config = config;
        }

        public TConfig Config { get; }
    }
}