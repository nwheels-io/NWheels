namespace NWheels.Composition.Model.Metadata
{
    public interface IModelParserRegistry
    {
        void Add<TCompiled, TParser>() 
            where TParser : ModelParser<TCompiled>;

        void AddRoot<TCompiled, TParser>() 
            where TParser : ModelParser<TCompiled>, IRootModelParser<TCompiled>;
    }
}