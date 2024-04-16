namespace faker
{

    public class FakerImpl : IFaker
    {
        public FakerImpl() { }


        public T create<T>()
        {
            var type = typeof(T);
            var obj = Generators.generateDto(type);
            return (T)obj;
        }

        public object createLab5(Type type)
        {
            var obj = Generators.generateDto(type);
            return obj;
        }
    }

}