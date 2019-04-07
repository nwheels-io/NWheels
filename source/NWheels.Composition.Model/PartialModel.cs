namespace NWheels.Composition.Model
{
    public abstract class PartialModel<TConfig>
    {
        protected PartialModel(TConfig config)
        {
            Config = config;
        }

        public TConfig Config { get; }
    }
}