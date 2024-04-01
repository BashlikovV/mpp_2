

namespace faker {

    public class Program
    {
        public static void Main(){ 
            var faker = new FakerImpl();
            var filledObj = faker.create<FillableClass>();
            Console.WriteLine(filledObj.getI());
        }
    }

    class FillableClass(int i = 0)
    {
        public int getI() {
            return i;
        }
    }

}