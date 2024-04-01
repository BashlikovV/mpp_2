
namespace faker {

    public interface IFakerConfig
    {
        void add<TClass, TField, TICustomGenerator>(Func<TClass, TField> fieldSelector);
    }

}